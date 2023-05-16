using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace EasyMLCore.Data
{
    /// <summary>
    /// Implements the single row of delimited string values.
    /// </summary>
    [Serializable]
    public class DelimitedStringValues : SerializableObject
    {
        //Constants
        public const int DefaultExpectedNumOfValues = 1000;
        //Attribute properties
        /// <summary>
        /// Collection of string values.
        /// </summary>
        public List<string> StringValueCollection { get; }

        //Constructor
        /// <summary>
        /// Creates an empty instance.
        /// </summary>
        /// <param name="expectedNumOfValues">Expected number of values.</param>
        public DelimitedStringValues(int expectedNumOfValues = DefaultExpectedNumOfValues)
        {
            StringValueCollection = new List<string>(expectedNumOfValues);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="data">A data row consisting of delimited values.</param>
        /// <param name="delimiter">Values delimiter.</param>
        public DelimitedStringValues(string data, char delimiter)
        {
            StringValueCollection = new List<string>(data.Split(new char[] { delimiter }, StringSplitOptions.None));
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="data">Collection of data.</param>
        public DelimitedStringValues(IEnumerable<string> data)
        {
            StringValueCollection = new List<string>(data);
            return;
        }

        //Properties
        /// <summary>
        /// Number of stored string values.
        /// </summary>
        public int NumOfStringValues { get { return StringValueCollection.Count; } }

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

        /// <summary>
        /// Builds single row from inner collection of values.
        /// </summary>
        /// <param name="delimiter">Values delimiter.</param>
        public string ToSingleRow(char delimiter)
        {
            StringBuilder output = new StringBuilder();
            bool firstVal = true;
            foreach (string value in StringValueCollection)
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

    }//DelimitedStringValues

}//Namespace

