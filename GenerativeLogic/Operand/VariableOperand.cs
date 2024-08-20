using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Operand
{
    /// <summary>
    /// Represents an independent operand.
    /// </summary>
    public class VariableOperand : IOperand
    {
        /// <summary>
        /// The name of this operand.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The value of this operand.
        /// </summary>
        public bool Value { get; set; }

        /// <summary>
        /// There are no dependencies, so use an empty list.
        /// </summary>
        public IEnumerable<IOperandStateProvider> Connections { get { return connections; } }

        /// <summary>
        /// There are no dependencies, so use an empty list.
        /// </summary>
        private List<IOperandStateProvider> connections = new List<IOperandStateProvider>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of this operand.</param>
        public VariableOperand(string name)
        {
            this.Name = name;
        }

        public bool Provide()
        {
            return this.Value;
        }        

        public override string ToString()
        {
            return this.Name;
        }
    }
}
