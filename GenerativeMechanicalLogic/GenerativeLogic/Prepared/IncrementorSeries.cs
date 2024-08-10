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
    /// Create the incrementor series.
    /// </summary>
    public class IncrementorSeries : GenericSeriesCreator
    {

        /// <summary>
        /// The type of this series.
        /// </summary>
        public static SeriesType SeriesType = new SeriesType()
        {
            SeriesKey = Guid.Parse("092DD99E-AB17-499A-A8C6-93D14BE6AB4E"),
            Name = typeof(IncrementorSeries).Name,
            Description = "A series that performs the Incrementation operation."
        };

        /// <summary>
        /// The parameters used for each value in the incrementor function.
        /// </summary>
        public enum IncrementorParameter
        {
            /// <summary>
            /// The only parameter of the incrementor.
            /// </summary>
            Input = 1
        }

        /// <summary>
        /// The parameter describing the input.
        /// </summary>
        public IParameter InputParameter
        {
            get
            {
                var parameter = new Parameter();
                parameter.Index = (int)IncrementorParameter.Input;
                parameter.Name = IncrementorParameter.Input.ToString();
                parameter.Type = typeof(IncrementorParameter);
                parameter.Description = "The input series to the incrementor.";
                return parameter;
            }
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="operandWrapper">An optional wrapper for series values.</param>
        public IncrementorSeries(IHomogeneousWrapper<IOperandStateProvider> operandWrapper = null)
        {
            this.dynamicSeries.OperandWrapper = operandWrapper;
            this.dynamicSeries.InputParameters = new List<IParameter>() { this.InputParameter };
            this.dynamicSeries.SeriesType = SeriesType;
            SetupDynamicSeries();
        }

        /// <summary>
        /// Creates an incrementor whose value is the incremented input.
        /// </summary>
        /// <param name="input">The series to increment.</param>
        /// <returns>An incrementor whose value is the incremented input.</returns>
        public ISeries<IOperandStateProvider> CreateSeries(ISeries<IOperandStateProvider> input)
        {
            var inputSpecification = new SeriesInputSpecification();
            var inputParameter = new SeriesInputParameter();
            inputParameter.Parameter = this.InputParameter;
            inputParameter.Series = input;
            inputSpecification.Parameters = new List<SeriesInputParameter>() { inputParameter };
            return this.dynamicSeries.CreateSeries(inputSpecification);
        }

        /// <summary>
        /// Creates an incrementor whose value is the incremented input.
        /// </summary>
        /// <param name="input">The series to increment.</param>
        /// <returns>An incrementor whose value is the incremented input.</returns>
        public IFiniteSeries<IOperandStateProvider> CreateSeries(IFiniteSeries<IOperandStateProvider> input)
        {
            var inputSpecification = new FiniteSeriesInputSpecification();
            var inputParameter = new FiniteSeriesInputParameter();
            inputParameter.Parameter = this.InputParameter;
            inputParameter.Series = input;
            inputSpecification.Parameters = new List<FiniteSeriesInputParameter>() { inputParameter };
            return this.dynamicSeries.CreateSeries(inputSpecification, input.Indices);
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
            var inputFunctionRelation = new SeriesIndexRelation();
            inputFunctionRelation.Index = 0;
            inputFunctionRelation.Parameter = this.InputParameter;
            inputFunctionRelation.Type = SeriesIndexRelation.RelationType.Parameter;

            var inputRecursionRelation = new SeriesIndexRelation();
            inputRecursionRelation.Index = -1;
            inputRecursionRelation.Parameter = this.InputParameter;
            inputRecursionRelation.Type = SeriesIndexRelation.RelationType.Parameter;
            
            Parameter recursionParameter = new Parameter();
            recursionParameter.Name = "Recursion";
            recursionParameter.Index = 0;
            recursionParameter.Description = "Recursion for the Incrementor.";

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
            
            template = new SeriesTemplate();
            templates.Add(template);
            template.Key = recursionTemplateKey;
            template.Name = "Incrementor Recursion";
            template.Constants.Add(1, ConstantOperand.One);
            template.Expression = new LogicalExpression();

            //Create the recursion series.
            //r[i] = a[i-1] AND r[i-1].  r[0] = 1.
            node = new LogicalExpression.ExpressionNode(0, null, BinaryOperator.AND, isRoot: true);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(1, 0, inputRecursionRelation, LogicalExpression.ExpressionParameter.Parameter1);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(2, 0, recursionSelfRelation, LogicalExpression.ExpressionParameter.Parameter2);
            template.Expression.AddNode(node);


            template = new SeriesTemplate();
            templates.Add(template);
            template.Key = functionTemplateKey;
            template.Name = "Incrementor Function";
            template.Expression = new LogicalExpression();

            //Create the lead function.
            // f[i] = a[i] XOR r[i]
            node = new LogicalExpression.ExpressionNode(0, null, BinaryOperator.XOR, isRoot: true);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(1, 0, inputFunctionRelation, LogicalExpression.ExpressionParameter.Parameter1);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(2, 0, recursionFunctionRelation, LogicalExpression.ExpressionParameter.Parameter2);
            template.Expression.AddNode(node);

            //Set the templates.
            buildSpecification.Templates = templates;
            //Indicate that the function is the output.
            buildSpecification.OutputKey = functionTemplateKey;

            this.dynamicSeries.BuildSpecification = buildSpecification;
        }
    }
}
