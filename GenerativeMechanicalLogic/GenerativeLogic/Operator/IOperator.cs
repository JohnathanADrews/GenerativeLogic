using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Operator
{
    /// <summary>
    /// An interface for operators.
    /// </summary>
    public interface IOperator
    {
        /// <summary>
        /// Evaluates the parameters against the implementing operator.
        /// </summary>
        /// <param name="a">The first (or left) operand.</param>
        /// <param name="b">The second (or right) operand.</param>
        /// <returns>The result of the evaluation.</returns>
        bool Evaluate(bool a, bool b);

    }
}
