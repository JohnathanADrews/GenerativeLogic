using GenerativeLogic.Foundation;
using GenerativeLogic.Operand;
using GenerativeLogic.Operator;
using GenerativeLogic.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Prepared
{
    /// <summary>
    /// Creates majority series.
    /// </summary>
    public class MajoritySeries : GenericSeriesCreator
    {

        /// <summary>
        /// The type of this series.
        /// </summary>
        public static SeriesType SeriesType = new SeriesType()
        {
            SeriesKey = Guid.Parse("2D32DEFB-DF5D-4B43-8CA5-A1182F338713"),
            Name = typeof(MajoritySeries).Name,
            Description = "A series that performs the bitwise Majority operation."
        };

        /// <summary>
        /// The parameters used for each value in the majority function.
        /// </summary>
        public enum MajorityParameter
        {
            /// <summary>
            /// A parameter in the majority function.
            /// </summary>
            SeriesA = 1,
            /// <summary>
            /// A parameter in the majority function.
            /// </summary>
            SeriesB = 2,
            /// <summary>
            /// A parameter in the majority function.
            /// </summary>
            SeriesC = 3
        }

        /// <summary>
        /// The parameter describing the A series.
        /// </summary>
        public IParameter SeriesAParameter
        {
            get
            {
                var parameter = new Parameter();
                parameter.Index = (int)MajorityParameter.SeriesA;
                parameter.Name = MajorityParameter.SeriesA.ToString();
                parameter.Type = typeof(MajorityParameter);
                parameter.Description = "The A series to the majority function.";
                return parameter;
            }
        }

        /// <summary>
        /// The parameter describing the B series.
        /// </summary>
        public IParameter SeriesBParameter
        {
            get
            {
                var parameter = new Parameter();
                parameter.Index = (int)MajorityParameter.SeriesB;
                parameter.Name = MajorityParameter.SeriesB.ToString();
                parameter.Type = typeof(MajorityParameter);
                parameter.Description = "The B series to the majority function.";
                return parameter;
            }
        }

        /// <summary>
        /// The parameter describing the C series.
        /// </summary>
        public IParameter SeriesCParameter
        {
            get
            {
                var parameter = new Parameter();
                parameter.Index = (int)MajorityParameter.SeriesC;
                parameter.Name = MajorityParameter.SeriesC.ToString();
                parameter.Type = typeof(MajorityParameter);
                parameter.Description = "The C series to the majority function.";
                return parameter;
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="operandWrapper">An optional wrapper for series value.</param>
        public MajoritySeries(IHomogeneousWrapper<IOperandStateProvider> operandWrapper = null)
        {
            this.dynamicSeries.OperandWrapper = operandWrapper;
            this.dynamicSeries.InputParameters = new List<IParameter>() { this.SeriesAParameter, this.SeriesBParameter, this.SeriesCParameter };
            this.dynamicSeries.SeriesType = SeriesType;
            SetupDynamicSeries();
        }

        /// <summary>
        /// Creates a majority function given the provided series.
        /// </summary>
        /// <param name="seriesA">A series in the majority function.</param>
        /// <param name="seriesB">A series in the majority function.</param>
        /// <param name="seriesC">A series in the majority function.</param>
        /// <returns>A series that performs the majority function.</returns>
        public ISeries<IOperandStateProvider> CreateSeries(ISeries<IOperandStateProvider> seriesA,
            ISeries<IOperandStateProvider> seriesB,
            ISeries<IOperandStateProvider> seriesC)
        {
            var series = this.dynamicSeries.CreateSeries();
            series.SetInput<IOperandStateProvider>(this.SeriesAParameter, seriesA);
            series.SetInput<IOperandStateProvider>(this.SeriesBParameter, seriesB);
            series.SetInput<IOperandStateProvider>(this.SeriesCParameter, seriesC);
            return series;
        }

        /// <summary>
        /// Creates a majority function.
        /// </summary>
        /// <param name="seriesA">The series selected at bits where choiceSeries is 0.</param>
        /// <param name="seriesB">The series selected at bits where choiceSeries is 1.</param>
        /// <param name="seriesC">The series that chooses bit by bit the low Series or highSeries.</param>
        /// <returns>A series that performs the choice function.</returns>
        public IFiniteSeries<IOperandStateProvider> CreateSeries(IFiniteSeries<IOperandStateProvider> seriesA,
            IFiniteSeries<IOperandStateProvider> seriesB,
            IFiniteSeries<IOperandStateProvider> seriesC)
        {
            var series = this.dynamicSeries.CreateSeries(seriesA.Indices);
            series.SetInput<IOperandStateProvider>(this.SeriesAParameter, seriesA);
            series.SetInput<IOperandStateProvider>(this.SeriesBParameter, seriesB);
            series.SetInput<IOperandStateProvider>(this.SeriesCParameter, seriesC);
            return series;
        }

        /// <summary>
        /// Sets up the expressions for the dynamic series, which encapsulates the logic for creating the result series.
        /// </summary>
        protected override void SetupDynamicSeries()
        {
            var buildSpecification = new SeriesBuildSpecification();
            SeriesTemplate template;
            LogicalExpression.ExpressionNode node;
            int functionTemplateKey = 1;

            //Setup the series relations.
            var seriesAFunctionRelation = new SeriesIndexRelation();
            seriesAFunctionRelation.Index = 0;
            seriesAFunctionRelation.Parameter = this.SeriesAParameter;
            seriesAFunctionRelation.Type = SeriesIndexRelation.RelationType.Parameter;

            var seriesBFunctionRelation = new SeriesIndexRelation();
            seriesBFunctionRelation.Index = 0;
            seriesBFunctionRelation.Parameter = this.SeriesBParameter;
            seriesBFunctionRelation.Type = SeriesIndexRelation.RelationType.Parameter;

            var seriesCFunctionRelation = new SeriesIndexRelation();
            seriesCFunctionRelation.Index = 0;
            seriesCFunctionRelation.Parameter = this.SeriesCParameter;
            seriesCFunctionRelation.Type = SeriesIndexRelation.RelationType.Parameter;


            //Setup the templates.
            template = new SeriesTemplate();
            template.Key = functionTemplateKey;
            template.Name = "Majority";
            template.Expression = new LogicalExpression();

            //Create the lead function.
            // f[i] = ( a[i] AND b[i] ) XOR ( ( a[i] AND c[i] ) XOR ( b[i] AND c[i] ) )
            node = new LogicalExpression.ExpressionNode(0, null, BinaryOperator.XOR, isRoot: true);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(1, 0, BinaryOperator.AND, LogicalExpression.ExpressionParameter.Parameter1);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(2, 0, BinaryOperator.XOR, LogicalExpression.ExpressionParameter.Parameter2);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(3, 2, BinaryOperator.AND, LogicalExpression.ExpressionParameter.Parameter1);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(4, 2, BinaryOperator.AND, LogicalExpression.ExpressionParameter.Parameter2);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(5, 1, seriesAFunctionRelation, LogicalExpression.ExpressionParameter.Parameter1);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(6, 1, seriesBFunctionRelation, LogicalExpression.ExpressionParameter.Parameter2);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(7, 3, seriesAFunctionRelation, LogicalExpression.ExpressionParameter.Parameter1);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(8, 3, seriesCFunctionRelation, LogicalExpression.ExpressionParameter.Parameter2);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(9, 4, seriesBFunctionRelation, LogicalExpression.ExpressionParameter.Parameter1);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(10, 4, seriesCFunctionRelation, LogicalExpression.ExpressionParameter.Parameter2);
            template.Expression.AddNode(node);


            //Set the templates.
            buildSpecification.Templates = new List<SeriesTemplate>() { template };
            //Indicate that the function is the output.
            buildSpecification.OutputKey = functionTemplateKey;

            this.dynamicSeries.BuildSpecification = buildSpecification;
        }
    }
}
