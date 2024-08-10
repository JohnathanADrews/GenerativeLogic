using GenerativeLogic.Foundation;
using GenerativeLogic.Operand;
using GenerativeLogic.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Prepared
{
    /// <summary>
    /// Contains most of the information required to create the comparison series.
    /// </summary>
    public abstract class GenericComparisonCreator : GenericSeriesCreator
    {

        /// <summary>
        /// The parameters used in the function.
        /// </summary>
        public enum ComparisonParameter
        {
            /// <summary>
            /// The series on the left of the comparison.
            /// </summary>
            LeftSeries,
            /// <summary>
            /// The series on the right of the comparison.
            /// </summary>
            RightSeries
        }

        /// <summary>
        /// The parameter describing the left series.
        /// </summary>
        public IParameter LeftSeriesParameter
        {
            get
            {
                var parameter = new Parameter();
                parameter.Index = (int)ComparisonParameter.LeftSeries;
                parameter.Name = ComparisonParameter.LeftSeries.ToString();
                parameter.Type = typeof(ComparisonParameter);
                parameter.Description = "The left series in the comparison.";
                return parameter;
            }
        }

        /// <summary>
        /// The parameter describing the right series.
        /// </summary>
        public IParameter RightSeriesParameter
        {
            get
            {
                var parameter = new Parameter();
                parameter.Index = (int)ComparisonParameter.RightSeries;
                parameter.Name = ComparisonParameter.RightSeries.ToString();
                parameter.Type = typeof(ComparisonParameter);
                parameter.Description = "The right series in the comparison.";
                return parameter;
            }
        }

        /// <summary>
        /// The expression that will be used in the comparison.
        /// </summary>
        public abstract LogicalExpression Expression { get; }

        /// <summary>
        /// The constant operand to use at the zero index.
        /// </summary>
        public abstract ConstantOperand BoundaryOperand { get; }

        /// <summary>
        /// The relation for the left series.
        /// </summary>
        protected SeriesIndexRelation LeftRelation = new SeriesIndexRelation();

        /// <summary>
        /// The relation for the right series.
        /// </summary>
        protected SeriesIndexRelation RightRelation = new SeriesIndexRelation();

        /// <summary>
        /// The relation for the series function.
        /// </summary>
        protected SeriesIndexRelation FunctionRelation = new SeriesIndexRelation();

        /// <summary>
        /// The template key of the function.
        /// </summary>
        private int functionTemplateKey = 1;

        /// <summary>
        /// The optional wrapper for the individual operands.
        /// </summary>
        private IHomogeneousWrapper<IOperandStateProvider> operandWrapper;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="operandWrapper">The optional wrapper for the individual operands.</param>
        public GenericComparisonCreator(IHomogeneousWrapper<IOperandStateProvider> operandWrapper = null)
        {
            //Setup the series relations.
            this.LeftRelation = new SeriesIndexRelation();
            this.LeftRelation.Index = 0;
            this.LeftRelation.Parameter = this.LeftSeriesParameter;
            this.LeftRelation.Type = SeriesIndexRelation.RelationType.Parameter;

            this.RightRelation = new SeriesIndexRelation();
            this.RightRelation.Index = 0;
            this.RightRelation.Parameter = this.RightSeriesParameter;
            this.RightRelation.Type = SeriesIndexRelation.RelationType.Parameter;

            Parameter recursionParameter = new Parameter();
            recursionParameter.Name = "Function Recursion";
            recursionParameter.Index = 0;
            recursionParameter.Description = "Recursion for a comparison.";

            this.FunctionRelation = new SeriesIndexRelation();
            this.FunctionRelation.Index = -1;
            this.FunctionRelation.Parameter = recursionParameter;
            this.FunctionRelation.TemplateKey = functionTemplateKey;
            this.FunctionRelation.Type = SeriesIndexRelation.RelationType.Internal;

            SetupDynamicSeries();
        }

        /// <summary>
        /// Creates a series that calculates the aggregate comparison.
        /// </summary>
        /// <param name="leftSeries">The series representing a number on the left of the comparison.</param>
        /// <param name="rightSeries">The series representing a number on the right of the comparison.</param>
        /// <returns>The series that calculates the comparison aggregate result.</returns>
        public ISeries<IOperandStateProvider> CreateSeries(ISeries<IOperandStateProvider> leftSeries,
            ISeries<IOperandStateProvider> rightSeries)
        {
            var series = this.dynamicSeries.CreateSeries();
            series.SetInput<IOperandStateProvider>(this.LeftSeriesParameter, leftSeries);
            series.SetInput<IOperandStateProvider>(this.RightSeriesParameter, rightSeries);
            return series;
        }

        /// <summary>
        /// Creates a series that calculates the aggregate comparison.
        /// </summary>
        /// <param name="leftSeries">The series representing a number on the left of the comparison.</param>
        /// <param name="rightSeries">The series representing a number on the right of the comparison.</param>
        /// <param name="indices">The indices specified in the series.</param>
        /// <returns>A comparison series.</returns>
        public IFiniteSeries<IOperandStateProvider> CreateSeries(IFiniteSeries<IOperandStateProvider> leftSeries,
            IFiniteSeries<IOperandStateProvider> rightSeries,
            IEnumerable<int> indices)
        {
            var series = this.dynamicSeries.CreateSeries(indices);
            series.SetInput<IOperandStateProvider>(this.LeftSeriesParameter, leftSeries);
            series.SetInput<IOperandStateProvider>(this.RightSeriesParameter, rightSeries);
            return series;
        }

        /// <summary>
        /// Creates a series that calculates the aggregate comparison.
        /// </summary>
        /// <param name="leftSeries">The series representing a number on the left of the comparison.</param>
        /// <param name="rightSeries">The series representing a number on the right of the comparison.</param>
        /// <param name="range">The range of indices specified in the series.</param>
        /// <returns>A comparison series.</returns>
        public IFiniteSeries<IOperandStateProvider> CreateSeries(IFiniteSeries<IOperandStateProvider> leftSeries,
            IFiniteSeries<IOperandStateProvider> rightSeries,
            IRange<int> range)
        {
            var series = this.dynamicSeries.CreateSeries(range);
            series.SetInput<IOperandStateProvider>(this.LeftSeriesParameter, leftSeries);
            series.SetInput<IOperandStateProvider>(this.RightSeriesParameter, rightSeries);
            return series;
        }


        /// <summary>
        /// Sets up the expressions for the dynamic series.
        /// </summary>
        protected override void SetupDynamicSeries()
        {
            var buildSpecification = new SeriesBuildSpecification();
            SeriesTemplate template;

            //Setup the templates.
            template = new SeriesTemplate();
            template.Key = functionTemplateKey;
            template.Name = "Comparison";
            template.Expression = this.Expression;
            template.Constants.Add(0, this.BoundaryOperand);

            //Set the templates.
            buildSpecification.Templates = new List<SeriesTemplate>() { template };
            //Indicate that the function is the output.
            buildSpecification.OutputKey = functionTemplateKey;

            this.dynamicSeries.BuildSpecification = buildSpecification;
        }
    }
}
