using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Operand
{
    /// <summary>
    /// A series of operands.
    /// </summary>
    public class VariableOperandSeries : ISeries<VariableOperand>, ISeries<IOperandStateProvider>
    {
        /// <summary>
        /// The name of the series.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The prefix of the variables.
        /// </summary>
        public string VariablePrefix { get; private set; }

        /// <summary>
        /// Gets the variable at the indicated index.
        /// </summary>
        /// <param name="index">The index at which to get the variable.</param>
        /// <returns>The variable at the indicated index.</returns>
        public VariableOperand GetValue(int index)
        {
            if (!operands.ContainsKey(index))
            {
                operands.Add(index,
                    new VariableOperand(
                        String.Format("{0}{1}", this.VariablePrefix, index))
                    { Value = this.defaultValue });
            }
            return operands[index];
        }

        IOperandStateProvider ISeries<IOperandStateProvider>.GetValue(int index)
        {
            return GetValue(index);
        }

        /// <summary>
        /// A container for the operands.
        /// </summary>
        private Dictionary<int, VariableOperand> operands = new Dictionary<int, VariableOperand>();

        /// <summary>
        /// The default value for the operands.
        /// </summary>
        private bool defaultValue;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of the series.</param>
        /// <param name="variablePrefix">The prefix used to name the operands.</param>
        /// <param name="defaultValue">The default value for the operands.</param>
        public VariableOperandSeries(string name, string variablePrefix, bool defaultValue = false)
        {
            this.Name = name;
            this.VariablePrefix = variablePrefix;
            this.defaultValue = defaultValue;
        }

        /// <summary>
        /// Creates a finite series using this series.
        /// </summary>
        /// <param name="indices">The indices to use in the finite series.</param>
        /// <returns>A finite series using this series.</returns>
        public IFiniteSeries<IOperandStateProvider> CreateFiniteSeries(IEnumerable<int> indices)
        {
            var finiteSeries = new FiniteSeries<IOperandStateProvider>(this, indices);
            return finiteSeries;
        }

        /// <summary>
        /// Creates a finite series using this series.
        /// </summary>
        /// <param name="range">The range of values in the series.</param>
        /// <returns>A finite series using this series.</returns>
        public IFiniteSeries<IOperandStateProvider> CreateFiniteSeries(IRange<int> range)
        {
            var finiteSeries = new FiniteSeries<IOperandStateProvider>(this, range);
            return finiteSeries;
        }
    }
}
