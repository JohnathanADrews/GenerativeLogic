using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Operator
{
    /// <summary>
    /// Represents a negation.
    /// </summary>
    public class NegationProvider : INegationProvider
    {
        public bool GetNegation()
        {
            return true;
        }
    }
}
