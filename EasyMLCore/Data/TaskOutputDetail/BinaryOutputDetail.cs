using EasyMLCore.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace EasyMLCore.Data
{
    /// <summary>
    /// Provides detailed information about an output of binary task computation.
    /// </summary>
    [Serializable]
    public class BinaryOutputDetail : TaskOutputDetailBase
    {
        //Attribute properties
        /// <summary>
        /// Contains resulting binary data derived from raw data.
        /// </summary>
        public int[] BinarizedData { get; }

        /// <summary>
        /// Contains mapped pairs Feature name - Feature binary value
        /// </summary>
        public List<Tuple<string, int>> MappedBinaryData { get; }

        /// <summary>
        /// Contains mapped pairs Feature name - Feature binary value
        /// </summary>
        public List<Tuple<string, string>> MappedTextualData { get; }

        //Attributes
        private readonly int _textualDataMaxLength;

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="featureNames">Names of output features.</param>
        /// <param name="rawData">Vector of corresponding values.</param>
        public BinaryOutputDetail(List<string> featureNames, double[] rawData)
            : this(featureNames, rawData, false)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="featureNames">Names of output features.</param>
        /// <param name="rawData">Vector of corresponding values.</param>
        protected BinaryOutputDetail(List<string> featureNames, double[] rawData, bool categorical)
            : base(featureNames, rawData)
        {
            BinarizedData = rawData.BinarizeProbabilities(categorical);
            MappedBinaryData = new List<Tuple<string, int>>(MappedRawData.Count);
            MappedTextualData = new List<Tuple<string, string>>(MappedRawData.Count);
            _textualDataMaxLength = 0;
            for(int i = 0; i < MappedRawData.Count; i++)
            {
                string binValText = BinarizedData[i] == 1 ? "true" : "false";
                int binValTextLength = binValText.Length;
                _textualDataMaxLength = Math.Max(_textualDataMaxLength, binValTextLength);
                MappedBinaryData.Add(new Tuple<string, int>(MappedRawData[i].Item1, BinarizedData[i]));
                MappedTextualData.Add(new Tuple<string, string>(MappedRawData[i].Item1, binValText));
            }
            return;
        }

        /// <inheritdoc/>
        public override string GetTextInfo(int margin = 0)
        {
            int decPlaces = 3;
            string numFormat = "N" + decPlaces.ToString();
            List<string> titles = new List<string>() { "Feature", "Value", "Binary", "Textual" };
            int titlePad = titles.MaxLength();
            string indentation = margin > 0 ? new string(' ', margin) : string.Empty;
            StringBuilder featureRow = new StringBuilder(indentation + titles[0].PadLeft(titlePad) + " ");
            StringBuilder valueRow = new StringBuilder(indentation + titles[1].PadLeft(titlePad) + " ");
            StringBuilder binaryRow = new StringBuilder(indentation + titles[2].PadLeft(titlePad) + " ");
            StringBuilder textualRow = new StringBuilder(indentation + titles[3].PadLeft(titlePad) + " ");
            for (int i = 0; i < MappedRawData.Count; i++)
            {
                string featureName = MappedRawData[i].Item1;
                string featureValue = MappedRawData[i].Item2.ToString(numFormat, CultureInfo.InvariantCulture);
                string binaryValue = MappedBinaryData[i].Item2.ToString(CultureInfo.InvariantCulture);
                string textualValue = MappedTextualData[i].Item2;
                int maxLength = Math.Max(featureName.Length, featureValue.Length);
                maxLength = Math.Max(maxLength, textualValue.Length);
                featureRow.Append(" | " + featureName.PadRight(maxLength));
                valueRow.Append(" | " + featureValue.PadRight(maxLength));
                binaryRow.Append(" | " + binaryValue.PadRight(maxLength));
                textualRow.Append(" | " + textualValue.PadRight(maxLength));
            }
            featureRow.Append(Environment.NewLine);
            valueRow.Append(Environment.NewLine);
            binaryRow.Append(Environment.NewLine);
            textualRow.Append(Environment.NewLine);
            StringBuilder rows = new StringBuilder(featureRow.ToString());
            rows.Append(valueRow);
            rows.Append(binaryRow);
            rows.Append(textualRow);
            return rows.ToString();
        }

    }//BinaryOutputDetail

}//Namespace
