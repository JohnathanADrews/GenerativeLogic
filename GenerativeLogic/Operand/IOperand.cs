using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Operand
{
    /// <summary>
    /// Represents an operand.
    /// </summary>
    public interface IOperand : IOperandStateProvider
    {
        /// <summary>
        /// The name of the operand.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The value of the operand.
        /// </summary>
        bool Value { get; set; }
    }
}
