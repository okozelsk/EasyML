using EasyMLCore.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace EasyMLCore.Data
{
    /// <summary>
    /// Provides detailed information about an output of regression task computation.
    /// </summary>
    [Serializable]
    public class RegressionOutputDetail : TaskOutputDetailBase
    {

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="featureNames">Names of output features.</param>
        /// <param name="rawData">Vector of corresponding values.</param>
        public RegressionOutputDetail(List<string> featureNames, double[] rawData)
            : base(featureNames, rawData)
        {
            return;
        }

        /// <inheritdoc/>
        public override string GetTextInfo(int margin = 0)
        {
            int decPlaces = 5;
            string numFormat = "N" + decPlaces.ToString();
            List<string> titles = new List<string>() { "Feature", "Value" };
            int titlePad = titles.MaxLength();
            string indentation = margin > 0 ? new string(' ', margin) : string.Empty;
            StringBuilder featureRow = new StringBuilder(indentation + titles[0].PadLeft(titlePad) + " ");
            StringBuilder valueRow = new StringBuilder(indentation + titles[1].PadLeft(titlePad) + " ");
            for (int i = 0; i < MappedRawData.Count; i++)
            {
                string featureName = MappedRawData[i].Item1;
                string featureValue = MappedRawData[i].Item2.ToString(numFormat, CultureInfo.InvariantCulture);
                int maxLength = Math.Max(featureName.Length, featureValue.Length);
                featureRow.Append(" | " + featureName.PadRight(maxLength));
                valueRow.Append(" | " + featureValue.PadRight(maxLength));
            }
            featureRow.Append(Environment.NewLine);
            valueRow.Append(Environment.NewLine);
            StringBuilder rows = new StringBuilder(featureRow.ToString());
            rows.Append(valueRow);
            return rows.ToString();
        }

    }//RegressionOutputDetail

}//Namespace
