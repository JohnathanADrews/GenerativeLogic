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
    /// Determines whether the left series is greater than the right series.
    /// </summary>
    public class GreaterThanSeries : GenericComparisonCreator
    {

        /// <summary>
        /// The expression that will be used in the comparison.
        /// </summary>
        public override LogicalExpression Expression
        {
            get
            {
                LogicalExpression.ExpressionNode node;
                LogicalExpression expression = new LogicalExpression();
                //Create the lead function.
                // f[i] = ( a[i] AORNB b[i] ) OR ( ( a[i] XNOR b[i] ) AND f[i-1] )
                node = new LogicalExpression.ExpressionNode(0, null, BinaryOperator.OR, isRoot: true);
                expression.AddNode(node);
                node = new LogicalExpression.ExpressionNode(1, 0, BinaryOperator.ANB, LogicalExpression.ExpressionParameter.Parameter1);
                expression.AddNode(node);
                node = new LogicalExpression.ExpressionNode(2, 0, BinaryOperator.AND, LogicalExpression.ExpressionParameter.Parameter2);
                expression.AddNode(node);
                node = new LogicalExpression.ExpressionNode(3, 1, this.LeftRelation, LogicalExpression.ExpressionParameter.Parameter1);
                expression.AddNode(node);
                node = new LogicalExpression.ExpressionNode(4, 1, this.RightRelation, LogicalExpression.ExpressionParameter.Parameter2);
                expression.AddNode(node);
                node = new LogicalExpression.ExpressionNode(5, 2, BinaryOperator.XNOR, LogicalExpression.ExpressionParameter.Parameter1);
                expression.AddNode(node);
                node = new LogicalExpression.ExpressionNode(6, 5, this.LeftRelation, LogicalExpression.ExpressionParameter.Parameter1);
                expression.AddNode(node);
                node = new LogicalExpression.ExpressionNode(7, 5, this.RightRelation, LogicalExpression.ExpressionParameter.Parameter2);
                expression.AddNode(node);
                node = new LogicalExpression.ExpressionNode(8, 2, this.FunctionRelation, LogicalExpression.ExpressionParameter.Parameter2);
                expression.AddNode(node);
                return expression;
            }
        }

        /// <summary>
        /// The constant operand to use at the zero index.
        /// </summary>
        public override ConstantOperand BoundaryOperand { get { return ConstantOperand.Zero; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="operandWrapper">The optional wrapper for the individual operands.</param>
        public GreaterThanSeries(IHomogeneousWrapper<IOperandStateProvider> operandWrapper = null)
            : base(operandWrapper)
        {
        }
        

    }
}
