using System;
using System.Collections.Generic;
using System.Globalization;

namespace EasyMLCore.Data
{
    /// <summary>
    /// Base class for task specific output details.
    /// </summary>
    [Serializable]
    public abstract class TaskOutputDetailBase : SerializableObject
    {
        //Attribute properties
        /// <summary>
        /// Contains original data.
        /// </summary>
        public double[] RawData { get; }

        /// <summary>
        /// Contains mapped pairs Feature name - Feature raw value
        /// </summary>
        public List<Tuple<string, double>> MappedRawData { get; }

        //Attributes
        protected int _featureNameMaxLength;
        protected int _dataValueIntegerPartMaxLength;

        /// <summary>
        /// Base constructor prepares RawData a MappedRawData.
        /// </summary>
        /// <param name="featureNames">Names of output features.</param>
        /// <param name="rawData">Vector of corresponding values.</param>
        protected TaskOutputDetailBase(List<string> featureNames, double[] rawData)
        {
            if(featureNames == null)
            {
                throw new ArgumentNullException(nameof(featureNames));
            }
            if (rawData == null)
            {
                throw new ArgumentNullException(nameof(rawData));
            }
            if(featureNames.Count == 0)
            {
                throw new ArgumentException("Feature names can not be empty.", nameof(featureNames));
            }
            if(featureNames.Count != rawData.Length)
            {
                throw new ArgumentException("Number of feature names does not correspond to length of raw data.", nameof(featureNames));
            }
            RawData = (double[])rawData.Clone();
            MappedRawData = new List<Tuple<string, double>>(RawData.Length);
            _featureNameMaxLength = 0;
            _dataValueIntegerPartMaxLength = 0;
            for (int i = 0; i < RawData.Length; i++)
            {
                int featureNameLength = featureNames[i].Length;
                if(featureNameLength == 0)
                {
                    throw new ArgumentException("Feature names contain one or more zero-length name(s).", nameof(featureNames));
                }
                _featureNameMaxLength = Math.Max(_featureNameMaxLength, featureNameLength);
                int integerPartMaxLength = ((int)Math.Floor(RawData[i])).ToString(CultureInfo.InvariantCulture).Length;
                _dataValueIntegerPartMaxLength = Math.Max(_dataValueIntegerPartMaxLength, integerPartMaxLength);
                MappedRawData.Add(new Tuple<string, double>(featureNames[i], RawData[i]));
            }
            return;
        }

        /// <summary>
        /// Gets formatted text describing an output.
        /// </summary>
        /// <param name="margin">Specifies left margin to be applied.</param>
        /// <returns>Formatted text describing an output.</returns>
        public abstract string GetTextInfo(int margin = 0);



    }//TaskOutputDetailBase

}//Namespace
