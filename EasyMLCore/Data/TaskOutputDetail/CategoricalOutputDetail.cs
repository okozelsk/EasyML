using EasyMLCore.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMLCore.Data
{
    /// <summary>
    /// Provides detailed information about an output of categorical task computation.
    /// </summary>
    [Serializable]
    public class CategoricalOutputDetail : BinaryOutputDetail
    {
        //Attribute properties
        /// <summary>
        /// Resulting class name.
        /// </summary>
        public string ResultingClassName { get; }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="featureNames">Names of output features.</param>
        /// <param name="rawData">Vector of corresponding values.</param>
        public CategoricalOutputDetail(List<string> featureNames, double[] rawData)
            : base(featureNames, rawData, true)
        {
            ResultingClassName = MappedBinaryData[BinarizedData.IndexOfMax(out _)].Item1;
            return;
        }

        /// <inheritdoc/>
        public override string GetTextInfo(int margin = 0)
        {
            string indentation = margin > 0 ? new string(' ', margin) : string.Empty;
            StringBuilder sb = new StringBuilder($"{indentation}Resulting class name is <");
            sb.Append(ResultingClassName);
            sb.Append('>');
            sb.Append(Environment.NewLine);
            sb.Append(base.GetTextInfo(margin));
            return sb.ToString();
        }

    }//CategoricalOutputDetail

}//Namespace
