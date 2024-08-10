using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Operand
{

    /// <summary>
    /// Provides constant operands.
    /// </summary>
    public class ConstantOperand : IOperandStateProvider
    {
        /// <summary>
        /// The value of the operand.
        /// </summary>
        public bool Value { get; private set; }

        /// <summary>
        /// There are no dependencies, so use an empty list.
        /// </summary>
        public IEnumerable<IOperandStateProvider> Connections { get { return connections;  } }

        /// <summary>
        /// Provides the value of the operand.
        /// </summary>
        /// <returns>The value of the operand.</returns>
        public bool Provide()
        {
            return this.Value;
        }

        /// <summary>
        /// Provides the zero/false operand.
        /// </summary>
        public static ConstantOperand Zero = new ConstantOperand(false);

        /// <summary>
        /// Provides the one/true operand.
        /// </summary>
        public static ConstantOperand One = new ConstantOperand(true);

        /// <summary>
        /// There are no dependencies, so use an empty list.
        /// </summary>
        private List<IOperandStateProvider> connections = new List<IOperandStateProvider>(); 

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">The value of the operand.</param>
        private ConstantOperand(bool value)
        {
            this.Value = value;
        }

        /// <summary>
        /// override ToString.
        /// </summary>
        /// <returns>A string representation of the constant.</returns>
        public override string ToString()
        {
            return String.Format("({0})", this.Value ? 1 : 0);
        }

    }
}
