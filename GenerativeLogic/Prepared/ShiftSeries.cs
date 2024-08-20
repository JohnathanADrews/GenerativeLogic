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
    /// Creates a bit shift series.
    /// </summary>
    public class ShiftSeries : GenericSeriesCreator
    {

        /// <summary>
        /// The type of this series.
        /// </summary>
        public static SeriesType SeriesType = new SeriesType()
        {
            SeriesKey = Guid.Parse("351B6D33-190C-408B-A0C2-BD272862C1C7"),
            Name = typeof(ChoiceSeries).Name,
            Description = "A series that performs the Shift operation."
        };

        /// <summary>
        /// The direction to shift the bits.
        /// </summary>
        public enum ShiftDirection
        {
            /// <summary>
            /// Shift the bits to the left.
            /// </summary>
            Left,
            /// <summary>
            /// Shift the bits to the right.
            /// </summary>
            Right
        }


        /// <summary>
        /// The parameters used for each value in the function.
        /// </summary>
        public enum ShiftParameter
        {
            /// <summary>
            /// The only shift parameter.
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
                parameter.Index = (int)ShiftParameter.Input;
                parameter.Name = ShiftParameter.Input.ToString();
                parameter.Type = typeof(ShiftParameter);
                parameter.Description = "The input series.";
                return parameter;
            }
        }

        /// <summary>
        /// The direction to shift the bits.
        /// </summary>
        public ShiftDirection Direction { get; set; }

        /// <summary>
        /// The number of bits to shift.
        /// </summary>
        public int ShiftAmount { get; set; }

        /// <summary>
        /// The value to use for indices without a source in a finite series.
        /// </summary>
        public ConstantOperand FillOperand { get; set; }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="direction">The direction to shift the bits.</param>
        /// <param name="shiftAmount">The number of bits to shift.</param>
        /// <param name="fillOperand">The value to use for indices without a source in a finite series.</param>
        /// <param name="operandWrapper">An optional wrapper for series values.</param>
        public ShiftSeries(ShiftDirection direction,
            int shiftAmount,
            ConstantOperand fillOperand,
            IHomogeneousWrapper<IOperandStateProvider> operandWrapper = null)
        {
            this.Direction = direction;
            this.ShiftAmount = shiftAmount;
            this.FillOperand = fillOperand;
            this.dynamicSeries.OperandWrapper = operandWrapper;
            this.dynamicSeries.InputParameters = new List<IParameter>() { this.InputParameter };
            this.dynamicSeries.SeriesType = SeriesType;
            SetupDynamicSeries();
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
            int functionTemplateKey = 1;

            //Setup the series relations.
            var inputFunctionRelation = new SeriesIndexRelation();
            inputFunctionRelation.Index = this.ShiftAmount * (this.Direction == ShiftDirection.Left ? -1 : 1);
            inputFunctionRelation.Parameter = this.InputParameter;
            inputFunctionRelation.Type = SeriesIndexRelation.RelationType.Parameter;
            
            template = new SeriesTemplate();
            templates.Add(template);
            template.Key = functionTemplateKey;
            template.Name = "Shift";
            template.Expression = new LogicalExpression();

            //Create the lead function.
            // f[i] = a[i + shift]
            node = new LogicalExpression.ExpressionNode(0, null, inputFunctionRelation, isRoot: true);
            template.Expression.AddNode(node);
            
            //Set the templates.
            buildSpecification.Templates = new List<SeriesTemplate>() { template };
            //Indicate that the function is the output.
            buildSpecification.OutputKey = functionTemplateKey;

            this.dynamicSeries.BuildSpecification = buildSpecification;
        }

    }
}
