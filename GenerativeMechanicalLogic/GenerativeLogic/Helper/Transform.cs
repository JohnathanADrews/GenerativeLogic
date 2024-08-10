using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Helper
{
    /// <summary>
    /// A static class used to perform standard binary operations. 
    /// </summary>
    public static class Transform
    {

        #region Incrementation

        /// <summary>
        /// Increments the provided bool array.
        /// </summary>
        /// <param name="argument">The bool array to increment.</param>
        /// <param name="carry">true if the incremented value has false at each value.  false otherwise.</param>
        public static void Increment(bool[] argument, out bool carry)
        {
            carry = false;
            int i;
            for (i = 0; i < argument.Length; i++)
            {
                argument[i] = argument[i] ^ true;
                if (argument[i])
                {
                    break;
                }
            }
            if (i == argument.Length) carry = true;
        }

        /// <summary>
        /// Increments the provided bool array.
        /// </summary>
        /// <param name="argument">The bool array to increment.</param>
        public static void Increment(bool[] argument)
        {
            bool carry;
            Increment(argument, out carry);
        }

        #endregion

    }
}
