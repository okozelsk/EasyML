using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace EasyMLCore.Data
{
    /// <summary>
    /// Implements the single row of delimited string values (csv format).
    /// </summary>
    [Serializable]
    public class DelimitedStringValues : SerializableObject
    {
        //Constants
        //Delimiters
        /// <summary>
        /// The semicolon delimiter.
        /// </summary>
        public const char SemicolonDelimiter = ';';
        /// <summary>
        /// The comma delimiter.
        /// </summary>
        public const char CommaDelimiter = ',';
        /// <summary>
        /// The tabelator delimiter.
        /// </summary>
        public const char TabDelimiter = '\t';
        /// <summary>
        /// Default delimiter.
        /// </summary>
        public const char DefaultDelimiter = SemicolonDelimiter;

        //Attribute properties
        /// <summary>
        /// Current delimiter.
        /// </summary>
        public char Delimiter { get; private set; }
        /// <summary>
        /// Collection of string values.
        /// </summary>
        public List<string> StringValueCollection { get; }

        //Constructor
        /// <summary>
        /// Creates an empty instance.
        /// </summary>
        /// <param name="delimiter">String values delimiter.</param>
        public DelimitedStringValues(char delimiter = DefaultDelimiter)
        {
            Delimiter = delimiter;
            StringValueCollection = new List<string>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="data">A data row consisting of delimited values.</param>
        /// <param name="delimiter">String values delimiter.</param>
        public DelimitedStringValues(string data, char delimiter)
        {
            Delimiter = delimiter;
            StringValueCollection = new List<string>();
            LoadFromString(data, false, false);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="data">A data row consisting of delimited values.</param>
        public DelimitedStringValues(string data)
        {
            StringValueCollection = new List<string>();
            LoadFromString(data, false, true);
            return;
        }

        //Properties
        /// <summary>
        /// Number of stored string values.
        /// </summary>
        public int NumOfStringValues { get { return StringValueCollection.Count; } }

        //Methods
        //Static methods
        /// <summary>
        /// Tries to recognize a delimiter used in the sample data.
        /// </summary>
        /// <param name="sampleDelimitedData">Row of sample data.</param>
        /// <returns>The recognized delimiter or the default delimiter.</returns>
        public static char RecognizeDelimiter(string sampleDelimitedData)
        {
            //Check of the presence of candidate chars
            //Is "tab" char the candidate?
            if (sampleDelimitedData.IndexOf(TabDelimiter) != -1)
            {
                //If tab is present then it is the most probable delimiter
                return TabDelimiter;
            }
            //Is "semicolon" char the candidate?
            if (sampleDelimitedData.IndexOf(SemicolonDelimiter) != -1)
            {
                //If semicolon is present then it is the next most probable delimiter
                return SemicolonDelimiter;
            }
            //Recognize a floating point char
            char floatingPointChar = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
            if (sampleDelimitedData.IndexOf('.') != -1)
            {
                int index = sampleDelimitedData.IndexOf('.');
                if (index > 0 && index < sampleDelimitedData.Length - 1)
                {
                    char charBefore = sampleDelimitedData[index - 1];
                    if (charBefore >= '0' && charBefore <= '9')
                    {
                        char charAfter = sampleDelimitedData[index + 1];
                        if (charAfter >= '0' && charAfter <= '9')
                        {
                            floatingPointChar = '.';
                        }
                    }
                }
            }
            //Is "comma" char the candidate?
            if (sampleDelimitedData.IndexOf(CommaDelimiter) != -1 && floatingPointChar != CommaDelimiter)
            {
                //Comma is the probable delimiter
                return CommaDelimiter;
            }
            else
            {
                //Remaining default delimiter
                return DefaultDelimiter;
            }
        }

        /// <summary>
        /// Builds a single string consisting of delimited string values.
        /// </summary>
        /// <param name="stringValueCollection">A collection of alone string values.</param>
        /// <param name="delimiter">Delimiter to be used.</param>
        /// <returns>Built string.</returns>
        public static string ToString(IEnumerable<string> stringValueCollection,
                                      char delimiter = DefaultDelimiter
                                      )
        {
            StringBuilder output = new StringBuilder();
            bool firstVal = true;
            foreach (string value in stringValueCollection)
            {
                if (!firstVal)
                {
                    output.Append(delimiter);
                }
                output.Append(value);
                firstVal = false;
            }
            return output.ToString();
        }

        /// <summary>
        /// Splits a data row consisting of delimited values.
        /// </summary>
        /// <param name="data">A data row consisting of delimited values.</param>
        /// <param name="delimiter">Used delimiter.</param>
        /// <returns>Collection of alone string values.</returns>
        public static List<string> ToList(string data, char delimiter = DefaultDelimiter)
        {
            List<string> values = new List<string>();
            if (data.Length > 0)
            {
                char[] allowedDelims = new char[1];
                allowedDelims[0] = delimiter;
                values.AddRange(data.Split(allowedDelims, StringSplitOptions.None));
            }
            return values;
        }

        //Instance methods
        /// <summary>
        /// Clears the internal collection of string values.
        /// </summary>
        public void Reset()
        {
            StringValueCollection.Clear();
            return;
        }

        /// <summary>
        /// Changes the string values delimiter.
        /// </summary>
        /// <param name="delimiter">New delimiter to be used.</param>
        public void ChangeDelimiter(char delimiter)
        {
            Delimiter = delimiter;
            return;
        }

        /// <summary>
        /// Adds a string value into the internal collection of string values.
        /// </summary>
        /// <param name="value">A string value to be added.</param>
        /// <returns>The resulting number of string values in the internal collection.</returns>
        public int AddValue(string value)
        {
            StringValueCollection.Add(value);
            return StringValueCollection.Count;
        }

        /// <summary>
        /// Removes a string value at the specified position from the internal collection.
        /// </summary>
        /// <param name="index">The zero-based index of a string value to be removed.</param>
        /// <returns>The resulting number of string values in the internal collection.</returns>
        public int RemoveAt(int index)
        {
            StringValueCollection.RemoveAt(index);
            return StringValueCollection.Count;
        }

        /// <summary>
        /// Removes trailing blank values from the internal collection.
        /// </summary>
        /// <remarks>
        /// A string value is considered blank if it is zero length or contains only "white" characters.
        /// </remarks>
        /// <returns>The resulting number of string values in the internal collection.</returns>
        public int RemoveTrailingWhites()
        {
            while (StringValueCollection.Count > 0 && StringValueCollection[StringValueCollection.Count - 1].Trim() == string.Empty)
            {
                StringValueCollection.RemoveAt(StringValueCollection.Count - 1);
            }
            return StringValueCollection.Count;
        }

        /// <summary>
        /// Loads string values into the internal collection.
        /// </summary>
        /// <param name="data">A data row consisting of delimited values.</param>
        /// <param name="reset">Specifies whether to clear the internal collection before the load.</param>
        /// <param name="recognizeDelimiter">Specifies whether to try to recognize data delimiter used in data row. If not then current delimiter is used.</param>
        /// <returns>The resulting number of string values in the internal collection.</returns>
        public int LoadFromString(string data, bool reset = true, bool recognizeDelimiter = false)
        {
            if (reset)
            {
                Reset();
            }
            Delimiter = recognizeDelimiter ? RecognizeDelimiter(data) : Delimiter;
            StringValueCollection.AddRange(ToList(data, Delimiter));
            return StringValueCollection.Count;
        }

        /// <summary>
        /// Gets a string value from the internal collection at specified zero-based position (an index).
        /// </summary>
        /// <param name="idx">Zero-based position (an index).</param>
        /// <returns>A string value at requested position.</returns>
        public string GetValueAt(int idx)
        {
            return StringValueCollection[idx];
        }

        ///<inheritdoc cref="List{T}.IndexOf(T)"/>
        public int IndexOf(string item)
        {
            return StringValueCollection.IndexOf(item);
        }

        ///<inheritdoc/>
        public override string ToString()
        {
            return ToString(StringValueCollection, Delimiter);
        }

    }//DelimitedStringValues

}//Namespace

