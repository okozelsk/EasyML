using System.Globalization;
using System.Runtime.CompilerServices;

namespace EasyMLCore.Extensions
{
    /// <summary>
    /// Implements extensions of the int type.
    /// </summary>
    public static class ExtensionsInt
    {
        /// <summary>
        /// Converts an int value to left-padded string of length corresponding to number of digits of specified reference value.
        /// </summary>
        /// <param name="refValue">A reference value determinimg an output string length.</param>
        /// <param name="paddingChar">Padding character.</param>
        /// <param name="value">Int value.</param>
        /// <returns>A left-padded string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToLeftPaddedString(this int value, int refValue, char paddingChar = ' ')
        {
            string maxValueString = refValue.ToString(CultureInfo.InvariantCulture);
            string valueString = value.ToString(CultureInfo.InvariantCulture);
            if (maxValueString.Length <= valueString.Length)
            {
                return valueString;
            }
            else
            {
                return valueString.PadLeft(maxValueString.Length, paddingChar);
            }
        }


    }//ExtensionsInt

}//Namespace
