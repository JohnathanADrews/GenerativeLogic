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
    /// Creates choice series.
    /// </summary>
    public class ChoiceSeries : GenericSeriesCreator
    {

        /// <summary>
        /// The type of this series.
        /// </summary>
        public static SeriesType SeriesType = new SeriesType()
        {
            SeriesKey = Guid.Parse("4F6FAE5C-EAF8-463C-B2B4-71D8C11A6085"),
            Name = typeof(ChoiceSeries).Name,
            Description = "A series that performs the bitwise Choice operation."
        };

        /// <summary>
        /// The parameters used for each value in the choice function.
        /// </summary>
        public enum ChoiceParameter
        {
            /// <summary>
            /// The parameter selected when the choice series is 0.
            /// </summary>
            Low = 1,
            /// <summary>
            /// The parameter selected when the choice series is 1.
            /// </summary>
            High = 2,
            /// <summary>
            /// The series that selects whether to provide values from the low or high series.
            /// </summary>
            Choice = 3
        }

        /// <summary>
        /// The parameter describing the low input series.
        /// </summary>
        public IParameter LowInputParameter
        {
            get
            {
                var parameter = new Parameter();
                parameter.Index = (int)ChoiceParameter.Low;
                parameter.Name = ChoiceParameter.Low.ToString();
                parameter.Type = typeof(ChoiceParameter);
                parameter.Description = "The low input series to the choice function.";
                return parameter;
            }
        }

        /// <summary>
        /// The parameter describing the high input series.
        /// </summary>
        public IParameter HighInputParameter
        {
            get
            {
                var parameter = new Parameter();
                parameter.Index = (int)ChoiceParameter.High;
                parameter.Name = ChoiceParameter.High.ToString();
                parameter.Type = typeof(ChoiceParameter);
                parameter.Description = "The high input series to the choice function.";
                return parameter;
            }
        }

        /// <summary>
        /// The parameter describing the low input series.
        /// </summary>
        public IParameter ChoiceInputParameter
        {
            get
            {
                var parameter = new Parameter();
                parameter.Index = (int)ChoiceParameter.Choice;
                parameter.Name = ChoiceParameter.Choice.ToString();
                parameter.Type = typeof(ChoiceParameter);
                parameter.Description = "The choice input series to the choice function.";
                return parameter;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="operandWrapper">An optional wrapper for series value.</param>
        public ChoiceSeries(IHomogeneousWrapper<IOperandStateProvider> operandWrapper = null)
        {
            this.dynamicSeries.OperandWrapper = operandWrapper;
            this.dynamicSeries.InputParameters = new List<IParameter>() { this.LowInputParameter, this.HighInputParameter, this.ChoiceInputParameter };
            this.dynamicSeries.SeriesType = SeriesType;
            SetupDynamicSeries();
        }

        /// <summary>
        /// Creates a choice function that has the value of lowSeries at bits where choiceSeries is 0 and highSeries at bts where choiceSeries is 1.
        /// </summary>
        /// <param name="lowSeries">The series selected at bits where choiceSeries is 0.</param>
        /// <param name="highSeries">The series selected at bits where choiceSeries is 1.</param>
        /// <param name="choiceSeries">The series that chooses bit by bit the low Series or highSeries.</param>
        /// <returns>A series that performs the choice function.</returns>
        public ISeries<IOperandStateProvider> CreateSeries(ISeries<IOperandStateProvider> lowSeries,
            ISeries<IOperandStateProvider> highSeries,
            ISeries<IOperandStateProvider> choiceSeries)
        {
            var series = this.dynamicSeries.CreateSeries();
            series.SetInput<IOperandStateProvider>(this.LowInputParameter, lowSeries);
            series.SetInput<IOperandStateProvider>(this.HighInputParameter, highSeries);
            series.SetInput<IOperandStateProvider>(this.ChoiceInputParameter, choiceSeries);
            return series;         
        }

        /// <summary>
        /// Creates a choice function that has the value of lowSeries at bits where choiceSeries is 0 and highSeries at bts where choiceSeries is 1.
        /// </summary>
        /// <param name="lowSeries">The series selected at bits where choiceSeries is 0.</param>
        /// <param name="highSeries">The series selected at bits where choiceSeries is 1.</param>
        /// <param name="choiceSeries">The series that chooses bit by bit the low Series or highSeries.</param>
        /// <returns>A series that performs the choice function.</returns>
        public IFiniteSeries<IOperandStateProvider> CreateSeries(IFiniteSeries<IOperandStateProvider> lowSeries,
            IFiniteSeries<IOperandStateProvider> highSeries,
            IFiniteSeries<IOperandStateProvider> choiceSeries)
        {
            var series = this.dynamicSeries.CreateSeries(choiceSeries.Indices);
            series.SetInput<IOperandStateProvider>(this.LowInputParameter, lowSeries);
            series.SetInput<IOperandStateProvider>(this.HighInputParameter, highSeries);
            series.SetInput<IOperandStateProvider>(this.ChoiceInputParameter, choiceSeries);
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
            var highFunctionRelation = new SeriesIndexRelation();
            highFunctionRelation.Index = 0;
            highFunctionRelation.Parameter = this.HighInputParameter;
            highFunctionRelation.Type = SeriesIndexRelation.RelationType.Parameter;

            var lowFunctionRelation = new SeriesIndexRelation();
            lowFunctionRelation.Index = 0;
            lowFunctionRelation.Parameter = this.LowInputParameter;
            lowFunctionRelation.Type = SeriesIndexRelation.RelationType.Parameter;

            var choiceFunctionRelation = new SeriesIndexRelation();
            choiceFunctionRelation.Index = 0;
            choiceFunctionRelation.Parameter = this.ChoiceInputParameter;
            choiceFunctionRelation.Type = SeriesIndexRelation.RelationType.Parameter;
            

            //Setup the templates.
            template = new SeriesTemplate();
            template.Key = functionTemplateKey;
            template.Name = "Choice";
            template.Expression = new LogicalExpression();

            //Create the lead function.
            // f[i] = ( a[i] AND c[i] ) OR ( b[i] AND !c[i] )
            node = new LogicalExpression.ExpressionNode(0, null, BinaryOperator.OR, isRoot: true);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(1, 0, BinaryOperator.AND, LogicalExpression.ExpressionParameter.Parameter1);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(2, 0, BinaryOperator.AND, LogicalExpression.ExpressionParameter.Parameter2);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(3, 1, highFunctionRelation, LogicalExpression.ExpressionParameter.Parameter1);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(4, 1, choiceFunctionRelation, LogicalExpression.ExpressionParameter.Parameter2);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(5, 2, lowFunctionRelation, LogicalExpression.ExpressionParameter.Parameter1);
            template.Expression.AddNode(node);
            node = new LogicalExpression.ExpressionNode(6, 2, choiceFunctionRelation, LogicalExpression.ExpressionParameter.Parameter2, negation: NegationOption.Negation);
            template.Expression.AddNode(node);


            //Set the templates.
            buildSpecification.Templates = new List<SeriesTemplate>() { template };
            //Indicate that the function is the output.
            buildSpecification.OutputKey = functionTemplateKey;

            this.dynamicSeries.BuildSpecification = buildSpecification;
        }

    }
}
