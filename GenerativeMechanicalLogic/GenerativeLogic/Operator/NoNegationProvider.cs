using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Operator
{
    /// <summary>
    /// Represents the lack of negation.
    /// </summary>
    public class NoNegationProvider : INegationProvider
    {
        public bool GetNegation()
        {
            return false;
        }
    }
}
