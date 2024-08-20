using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Operator
{
    /// <summary>
    /// Represents an operator that can be changed.
    /// </summary>
    public class VariableOperator : IOperator
    {
        /// <summary>
        /// The operator to use.
        /// </summary>
        public IOperator Operator { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public VariableOperator()
        {
            this.Operator = null;
        }


        #region IOperator


        public bool Evaluate(bool a, bool b)
        {
            return this.Operator.Evaluate(a, b);
        }

        #endregion


    }
}
