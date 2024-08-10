using GenerativeLogic.Operand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeLogic.Helper
{  /// <summary>
   /// A static class that provides utility functions that would otherwise be constructed inline in other functions repeatedly.
   /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Returns an array of the specified type with the specified default value.
        /// </summary>
        /// <param name="length">The length of the array.</param>
        /// <param name="defaultValue">The default value to place in the array.</param>
        /// <returns>An array of the type specified type with the specified values.</returns>
        public static ArrayType[] GetArrayOfType<ArrayType>(int length, ArrayType defaultValue)
        {
            ArrayType[] array = new ArrayType[length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = defaultValue;
            }
            return array;
        }

        /// <summary>
        /// Returns the maximum address given an address size.
        /// </summary>
        /// <param name="addressSize">The size of the address.</param>
        /// <returns>The maximum address give the address size.</returns>
        public static long GetMaximumAddress(int addressSize)
        {
            return (1 << addressSize) - 1;
        }

        /// <summary>
        /// Sets the value of the operand series to the value of the given integer.
        /// </summary>
        /// <param name="series">The series to set the value of.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="startingIndex">The index of the least significant bit.</param>
        /// <param name="length">The length of the values to set.</param>
        public static void SetVariableOperandSeries(VariableOperandSeries series, int value, int startingIndex, int length)
        {
            var bits = Conversion.ConvertIntToBoolArray(value, length);
            for(int i=0;i<bits.Length;i++)
            {
                series.GetValue(i + startingIndex).Value = bits[i];
            }
        }

        /// <summary>
        /// Converts a series values to an int.
        /// </summary>
        /// <param name="series">The series that provides the values to convert.</param>
        /// <param name="startingIndex">The starting index of the series to convert.</param>
        /// <param name="length">The length of the series to convert.</param>
        /// <returns>An interger value of the series.</returns>
        public static int GetSeriesValue(ISeries<IOperandStateProvider> series, int startingIndex, int length)
        {
            bool[] values = new bool[length];
            for(int i=0;i<length;i++)
            {
                values[i] = series.GetValue(i + startingIndex).Provide();
            }
            int result = Conversion.ConvertBoolArrayToInt(values);
            return result;
        }


    }
}
