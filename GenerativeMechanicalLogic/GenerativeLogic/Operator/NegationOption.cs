using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Operator
{

    /// <summary>
    /// Provides the negation options.
    /// </summary>
    public static class NegationOption
    {
        /// <summary>
        /// The option indicating that a value is not negated.
        /// </summary>
        public static INegationProvider NoNegation = new NoNegationProvider();
        
        /// <summary>
        /// The option indicating that a value is negated.
        /// </summary>
        public static INegationProvider Negation = new NegationProvider();

        /// <summary>
        /// Converts a boolean value to a negation value.
        /// </summary>
        /// <param name="value">returns Negation iff the value is true.</param>
        /// <returns>The negation value corresponding to the boolean value.</returns>
        public static INegationProvider GetFromValue(bool value)
        {
            if (value)
            {
                return Negation;
            }
            return NoNegation;
        }

    }
}
