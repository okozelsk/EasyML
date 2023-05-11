using System;
using System.Globalization;

namespace EasyMLCore.Extensions
{
    /// <summary>
    /// Implements extensions of the string type.
    /// </summary>
    public static class ExtensionsString
    {
        /// <summary>
        /// Indents a multiline or sigleline text in string by specified indentation.
        /// </summary>
        /// <param name="indentation">A string to be used as indentation.</param>
        /// <param name="str">A string.</param>
        /// <returns>Indented text as a string.</returns>
        public static string Indent(this string str, string indentation)
        {
            if (indentation.Length > 0)
            {
                string newLineEndingString = Environment.NewLine + indentation;
                string indentedString = (indentation + str.ReplaceLineEndings(newLineEndingString));
                if (indentedString.EndsWith(newLineEndingString, StringComparison.Ordinal))
                {
                    indentedString = indentedString.Substring(0, indentedString.Length - indentation.Length);
                }
                return indentedString;
            }
            return str;
        }

        /// <summary>
        /// Indents a multiline or sigleline text in string by specified number of spaces.
        /// </summary>
        /// <param name="margin">Number of indentation spaces.</param>
        /// <param name="str">A string.</param>
        /// <returns>Indented text in string.</returns>
        public static string Indent(this string str, int margin)
        {
            if (margin > 0)
            {
                return Indent(str, new string(' ', margin));
            }
            return str;
        }

        /// <summary>
        /// Parses a double value.
        ///</summary>
        /// <remarks>
        /// Function tries to parse a value using the CultureInfo.InvariantCulture and if it fails, tries to use the CultureInfo.CurrentCulture.
        /// If it fails at all, function's behavior then depends on specified parameters.
        /// It can throw the InvalidOperationException exception or return the double.NaN value.
        /// </remarks>
        /// <param name="throwEx">Specifies whether to throw the InvalidOperationException exception in case the parsing fails.</param>
        /// <param name="exText">Specific text of the InvalidOperationException exception.</param>
        /// <param name="str">A string.</param>
        public static double ParseDouble(this string str, bool throwEx, string exText = "")
        {
            if (!double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                if (!double.TryParse(str, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
                {
                    if (throwEx)
                    {
                        throw new ArgumentException(exText);
                    }
                    else
                    {
                        value = double.NaN;
                    }
                }
            }
            return value;
        }


        /// <summary>
        /// Parses a DateTime value.
        ///</summary>
        /// <remarks>
        /// Function tries to parse a value using the CultureInfo.InvariantCulture and if it fails, tries to use the CultureInfo.CurrentCulture.
        /// If it fails at all, function's behavior then depends on specified parameters.
        /// It can throw the InvalidOperationException exception or return the DateTime.MinValue.
        /// </remarks>
        /// <param name="throwEx">Specifies whether to throw the InvalidOperationException exception in case the parsing fails.</param>
        /// <param name="exText">Specific text of the InvalidOperationException exception.</param>
        /// <param name="str">A string.</param>
        public static DateTime ParseDateTime(this string str, bool throwEx, string exText = "")
        {
            if (!DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime value))
            {
                if (!DateTime.TryParse(str, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out value))
                {
                    if (throwEx)
                    {
                        throw new InvalidOperationException(exText);
                    }
                    else
                    {
                        value = DateTime.MinValue;
                    }
                }
            }
            return value;
        }


        /// <summary>
        /// Parses an int value.
        ///</summary>
        /// <remarks>
        /// Function tries to parse a value using the CultureInfo.InvariantCulture and if it fails, tries to use the CultureInfo.CurrentCulture.
        /// If it fails at all, function's behavior then depends on specified parameters.
        /// It can throw the InvalidOperationException exception or return the int.MinValue.
        /// </remarks>
        /// <param name="throwEx">Specifies whether to throw the InvalidOperationException exception in case the parsing fails.</param>
        /// <param name="exText">Specific text of the InvalidOperationException exception.</param>
        /// <param name="str">A string.</param>
        public static int ParseInt(this string str, bool throwEx, string exText = "")
        {
            if (!int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                if (!int.TryParse(str, NumberStyles.Integer, CultureInfo.CurrentCulture, out value))
                {
                    if (throwEx)
                    {
                        throw new InvalidOperationException(exText);
                    }
                    else
                    {
                        value = int.MinValue;
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// Parses a bool value.
        ///</summary>
        /// <remarks>
        /// Function tries to parse a bool value.
        /// If it fails, function's behavior then depends on specified parameters.
        /// It can throw the InvalidOperationException exception or return the false value.
        /// </remarks>
        /// <param name="throwEx">Specifies whether to throw the InvalidOperationException exception in case the parsing fails.</param>
        /// <param name="exText">Specific text of the InvalidOperationException exception.</param>
        /// <param name="str">A string.</param>
        public static bool ParseBool(this string str, bool throwEx, string exText = "")
        {
            if (!bool.TryParse(str, out bool value))
            {
                if (throwEx)
                {
                    throw new InvalidOperationException(exText);
                }
                else
                {
                    value = false;
                }
            }
            return value;
        }

    }//ExtensionsString

}//Namespace

