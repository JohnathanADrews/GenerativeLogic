using GenerativeLogic.Foundation;
using GenerativeLogic.Helper;
using GenerativeLogic.Operand;
using GenerativeLogic.Operator;
using GenerativeLogic.Series;
using HularionCore.Pattern.Functional;
using HularionCore.Pattern.Set;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Prepared
{
    /// <summary>
    /// Performs arithmatic addition on two series.
    /// </summary>
    public class AdderSeries : GenericSeriesCreator
    {

        /// <summary>
        /// The type of this series.
        /// </summary>
        public static SeriesType SeriesType = new SeriesType()
        {
            SeriesKey = Guid.Parse("A25EC5CB-B277-469B-AFDE-3DD2B4719A90"),
            Name = typeof(AdderSeries).Name,
            Description = "A series that performs the Addition operation."
        };

        /// <summary>
        /// The parameters used for each value in the incrementor function.
        /// </summary>
        public enum AdderParameter
        {
            /// <summary>
            /// One of the two series being added.
            /// </summary>
            SeriesA = 1,
            /// <summary>
            /// One of the two series being added.
            /// </summary>
            SeriesB = 2
        }

        /// <summary>
        /// The parameter describing series A.
        /// </summary>
        public IParameter SeriesAParameter
        {
            get
            {
                var parameter = new Parameter();
                parameter.Index = (int)AdderParameter.SeriesA;
                parameter.Name = AdderParameter.SeriesA.ToString();
                parameter.Type = typeof(AdderParameter);
                parameter.Description = "One of the two series being added.";
                return parameter;
            }
        }

        /// <summary>
        /// The parameter describing series B.
        /// </summary>
        public IParameter SeriesBParameter
        {
            get
            {
                var parameter = new Parameter();
                parameter.Index = (int)AdderParameter.SeriesB;
                parameter.Name = AdderParameter.SeriesB.ToString();
                parameter.Type = typeof(AdderParameter);
                parameter.Description = "One of the two series being added.";
                return parameter;
            }
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="operandWrapper">The optional wrapper for the individual operands.</param>
        public AdderSeries(IHomogeneousWrapper<IOperandStateProvider> operandWrapper = null)
        {
            this.dynamicSeries.OperandWrapper = operandWrapper;
            this.dynamicSeries.InputParameters = new List<IParameter>() { this.SeriesAParameter, this.SeriesBParameter };
            this.dynamicSeries.SeriesType = SeriesType;
            SetupDynamicSeries();
        }

        /// <summary>
        /// Performs arithmatic addition on seriesA and seriesB
        /// </summary>
        /// <param name="seriesA">One of the series to add.</param>
        /// <param name="seriesB">One of the series to add.</param>
        /// <returns>The series that provides the arithmetic addition.</returns>
        public ISeries<IOperandStateProvider> CreateSeries(ISeries<IOperandStateProvider> seriesA,
            ISeries<IOperandStateProvider> seriesB)
        {
            var series = this.dynamicSeries.CreateSeries();
            series.SetInput<IOperandStateProvider>(this.SeriesAParameter, seriesA);
            series.SetInput<IOperandStateProvider>(this.SeriesBParameter, seriesB);
            return series;
        }

        /// <summary>
        /// Creates a finite Adder.
        /// </summary>
        /// <param name="seriesA">One of the series to add.</param>
        /// <param name="seriesB">One of the series to add.</param>
        /// <param name="indices">The indices specified in the series.</param>
        /// <returns>A finite adder.</returns>
        public IFiniteSeries<IOperandStateProvider> CreateSeries(IFiniteSeries<IOperandStateProvider> seriesA,
            IFiniteSeries<IOperandStateProvider> seriesB,
            IEnumerable<int> indices)
        {
            var series = this.dynamicSeries.CreateSeries(indices);
            series.SetInput<IOperandStateProvider>(this.SeriesAParameter, seriesA);
            series.SetInput<IOperandStateProvider>(this.SeriesBParameter, seriesB);
            return series;
        }

        /// <summary>
        /// Creates a finite Adder.
        /// </summary>
        /// <param name="seriesA">One of the series to add.</param>
        /// <param name="seriesB">One of the series to add.</param>
        /// <param name="range">The range of finite values.</param>
        /// <returns>A finite adder.</returns>
        public IFiniteSeries<IOperandStateProvider> CreateSeries(IFiniteSeries<IOperandStateProvider> seriesA,
            IFiniteSeries<IOperandStateProvider> seriesB,
            IRange<int> range)
        {
            var series = this.dynamicSeries.CreateSeries(range);
            series.SetInput<IOperandStateProvider>(this.SeriesAParameter, seriesA);
            series.SetInput<IOperandStateProvider>(this.SeriesBParameter, seriesB);
            return series;
        }
                
        /// <summary>
        /// Sets up the expressions for the dynamic series, which encapsulates the logic for creating the result series.
        /// </summary>
        protected override void SetupDynamicSeries()
        {
            var buildSpecification = new SeriesBuildSpecification();
            var templates = new List<SeriesTemplate>();
            SeriesTemplate template;
            LogicalExpression.ExpressionNode node;
            int recursionTemplateKey = 1;
            int functionTemplateKey = 2;

            //Setup the series relations.
            var seriesAFunctionRelation = new SeriesIndexRelation();
            seriesAFunctionRelation.Index = 0;
            seriesAFunctionRelation.Parameter = this.SeriesAParameter;
            seriesAFunctionRelation.Type = SeriesIndexRelation.RelationType.Parameter;

            var seriesARecursionRelation = new SeriesIndexRelation();
            seriesARecursionRelation.Index = -1;
            seriesARecursionRelation.Parameter = this.SeriesAParameter;
            seriesARecursionRelation.Type = SeriesIndexRelation.RelationType.Parameter;

            var seriesBFunctionRelation = new SeriesIndexRelation();
            seriesBFunctionRelation.Index = 0;
            seriesBFunctionRelation.Parameter = this.SeriesBParameter;
            seriesBFunctionRelation.Type = SeriesIndexRelation.RelationType.Parameter;

            var seriesBRecursionRelation = new SeriesIndexRelation();
            seriesBRecursionRelation.Index = -1;
            seriesBRecursionRelation.Parameter = this.SeriesBParameter;
            seriesBRecursionRelation.Type = SeriesIndexRelation.RelationType.Parameter;

            Parameter recursionParameter = new Parameter();
            recursionParameter.Name = "Recursion";
            recursionParameter.Index = 0;
            recursionParameter.Description = "Recursion for the Adder.";

            var recursionFunctionRelation = new SeriesIndexRelation();
            recursionFunctionRelation.Index = 0;
            recursionFunctionRelation.Parameter = recursionParameter;
            recursionFunctionRelation.TemplateKey = recursionTemplateKey;
            recursionFunctionRelation.Type = SeriesIndexRelation.RelationType.Internal;

            var recursionSelfRelation = new SeriesIndexRelation();
            recursionSelfRelation.Index = -1;
            recursionSelfRelation.Parameter = recursionParameter;
            recursionSelfRelation.TemplateKey = recursionTemplateKey;
            recursionSelfRelation.Type = SeriesIndexRelation.RelationType.Internal;

            //Setup the templates.
            template = new SeriesTemplate();
            templates.Add(template);
            template.Key = recursionTemplateKey;
            template.Name = "Adder Recursion";
            template.Constants.Add(1, ConstantOperand.Zero);
            template.Expression = new LogicalExpression();
            
            //Create the recursion series.
            //r[i] = ( a[i-1] AND b[i-1] ) OR ( ( a[i-1] XOR b[i-1] ) AND r[i-1] ).  r[0] = 0.
            node = new LogicalExpression.ExpressionNode(0, null, BinaryOperator.OR, isRoot: true);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(1, 0, BinaryOperator.AND, LogicalExpression.ExpressionParameter.Parameter1);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(2, 1, seriesARecursionRelation, LogicalExpression.ExpressionParameter.Parameter1);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(3, 1, seriesBRecursionRelation, LogicalExpression.ExpressionParameter.Parameter2);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(4, 0, BinaryOperator.AND, LogicalExpression.ExpressionParameter.Parameter2);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(5, 4, BinaryOperator.XOR, LogicalExpression.ExpressionParameter.Parameter1);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(6, 5, seriesARecursionRelation, LogicalExpression.ExpressionParameter.Parameter1);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(7, 5, seriesBRecursionRelation, LogicalExpression.ExpressionParameter.Parameter2);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(8, 4, recursionSelfRelation, LogicalExpression.ExpressionParameter.Parameter2);
            template.Expression.AddNode(node);            
            
            //Create the lead function.
            // f[i] = ( a[i] XOR b[i] ) XOR r[i]
            template = new SeriesTemplate();
            templates.Add(template);
            template.Key = functionTemplateKey;
            template.Name = "Adder Function";
            template.Expression = new LogicalExpression();
            node = new LogicalExpression.ExpressionNode(0, null, BinaryOperator.XOR, isRoot: true);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(1, 0, BinaryOperator.XOR, LogicalExpression.ExpressionParameter.Parameter1);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(2, 1, seriesAFunctionRelation, LogicalExpression.ExpressionParameter.Parameter1);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(3, 1, seriesBFunctionRelation, LogicalExpression.ExpressionParameter.Parameter2);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(4, 0, recursionFunctionRelation, LogicalExpression.ExpressionParameter.Parameter2);
            template.Expression.AddNode(node);

            //Set the templates.
            buildSpecification.Templates = templates;
            //Indicate that the function is the output.
            buildSpecification.OutputKey = functionTemplateKey;

            this.dynamicSeries.BuildSpecification = buildSpecification;
        }
    }
}
