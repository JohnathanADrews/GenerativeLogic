using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Operator
{
    /// <summary>
    /// Used to determine whether there is a negation of a value.
    /// </summary>
    public interface INegationProvider
    {
        /// <summary>
        /// Gets the negation associated with an object.
        /// </summary>
        /// <returns>The negation of the object.</returns>
        bool GetNegation();
    }
}
