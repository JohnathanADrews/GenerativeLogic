﻿using GenerativeLogic.Foundation;
using GenerativeLogic.Operand;
using GenerativeLogic.Operator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Prepared
{
    /// <summary>
    /// Determines whether the left series is equal to the right series.
    /// </summary>
    public class EqualSeries : GenericComparisonCreator
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
                // f[i] = ( a[i] XNOR b[i] ) AND f[i-1] 
                node = new LogicalExpression.ExpressionNode(0, null, BinaryOperator.AND, isRoot: true);
                expression.AddNode(node);
                node = new LogicalExpression.ExpressionNode(1, 0, BinaryOperator.XNOR, LogicalExpression.ExpressionParameter.Parameter1);
                expression.AddNode(node);
                node = new LogicalExpression.ExpressionNode(2, 0, this.FunctionRelation, LogicalExpression.ExpressionParameter.Parameter2);
                expression.AddNode(node);
                node = new LogicalExpression.ExpressionNode(3, 1, this.LeftRelation, LogicalExpression.ExpressionParameter.Parameter1);
                expression.AddNode(node);
                node = new LogicalExpression.ExpressionNode(4, 1, this.RightRelation, LogicalExpression.ExpressionParameter.Parameter2);
                expression.AddNode(node);
                return expression;
            }
        }

        /// <summary>
        /// The constant operand to use at the zero index.
        /// </summary>
        public override ConstantOperand BoundaryOperand { get { return ConstantOperand.One; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="operandWrapper">The optional wrapper for the individual operands.</param>
        public EqualSeries(IHomogeneousWrapper<IOperandStateProvider> operandWrapper = null)
            : base(operandWrapper)
        {
        }
    }
}