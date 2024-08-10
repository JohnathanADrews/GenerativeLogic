using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Helper
{
    /// <summary>
    /// A class containing useful conversions for logical values.
    /// </summary>
    public class Conversion
    {
        /// <summary>
        /// Converts an array of boolean values to a string given a replacement mapping.
        /// </summary>
        /// <param name="boolArray">The array of boolean values to be converted.</param>
        /// <param name="falseValue">The value to replace a boolean false.</param>
        /// <param name="trueValue">The value to replace a boolean true.</param>
        /// <returns>A string with FalseValue and TrueValue string at positions corresponding to false and true in the array.</returns>
        public static string ConvertBoolArrayToString(bool[] boolArray, string falseValue = "0", string trueValue = "1")
        {
            string s = string.Empty;
            for (int i = 0; i < boolArray.Length; i++) s += boolArray[i] ? trueValue : falseValue;
            return s;
        }

        /// <summary>
        /// Converts the bool array to an integer using the zero index as the most significant bit.
        /// </summary>
        /// <param name="boolArray">The array of boolean values to convert to an integer.</param>
        /// <param name="zeroIsLSB">If true, the zero index of BoolArray is treated as LSB.  If false, zero is treated as MSB.</param>
        /// <returns>An integer representation of the bool array.</returns>
        public static int ConvertBoolArrayToInt(bool[] boolArray, bool zeroIsLSB = true)
        {
            int returnValue = 0;
            if (zeroIsLSB)
            {
                for (int i = boolArray.Length - 1; i >= 0; i--)
                {
                    returnValue = returnValue << 1;
                    returnValue += boolArray[i] ? 1 : 0;
                }
            }
            else
            {
                for (int i = 0; i < boolArray.Length; i++)
                {
                    returnValue = returnValue << 1;
                    returnValue += boolArray[i] ? 1 : 0;
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Converts the provided integer value to a bool array.
        /// </summary>
        /// <param name="value">The value of the integer to convert.</param>
        /// <param name="minSize">The minimum size of the array to return.</param>
        /// <param name="zeroIsLSB">If true, the zero index of BoolArray is treated as LSB.  If false, zero is treated as MSB.</param>
        /// <returns>The bool array representation of the integer</returns>
        public static bool[] ConvertIntToBoolArray(int value, int minSize, bool zeroIsLSB = true)
        {
            List<bool> values = new List<bool>();
            int iterations = 8 * sizeof(int);
            minSize = minSize - 1;
            for (int i = 0; i < iterations; i++)
            {
                if (value == 0 && i > minSize)
                {
                    break;
                }
                values.Add((value & 1) == 1 ? true : false);
                value = value >> 1;
            }
            if (zeroIsLSB)
            {
                return values.ToArray();
            }
            else
            {
                values.Reverse();
                return values.ToArray();
            }
        }

        /// <summary>
        /// Transforms a range of integers to an IEnmuerable of integers.
        /// </summary>
        public static ITransform<IRange<int>, IEnumerable<int>> RangeTransform = new TransformFunction<IRange<int>, IEnumerable<int>>(input =>
        {
            var indices = new List<int>();
            for(int i=input.Low;i<= input.High;i++)
            {
                indices.Add(i);
            }
            return indices;
        });
    }
}
