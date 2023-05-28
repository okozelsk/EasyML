using EasyMLCore.MathTools;
using System;

namespace EasyMLCore.Data
{
    /// <summary>
    /// Implements the base class of all feature filters.
    /// </summary>
    [Serializable]
    public abstract class FeatureFilterBase : SerializableObject
    {
        //Enumerations
        /// <summary>
        /// Feature value type.
        /// </summary>
        public enum FeatureValueType
        {
            /// <summary>
            /// Feature represents a binary value.
            /// </summary>
            Binary,
            /// <summary>
            /// Feature represents a real number.
            /// </summary>
            Real
        }

        /// <summary>
        /// Feature use.
        /// </summary>
        public enum FeatureUse
        {
            /// <summary>
            /// Used as an input feature.
            /// </summary>
            Input,
            /// <summary>
            /// Used as an output feature.
            /// </summary>
            Output
        }

        //Attribute properties
        /// <inheritdoc cref="FeatureValueType"/>
        public FeatureValueType ValueType { get; }

        /// <inheritdoc cref="FeatureUse"/>
        public FeatureUse Use { get; }

        /// <summary>
        /// The statistics of the samples.
        /// </summary>
        public BasicStat SamplesStat { get; }


        //Constructor
        /// <summary>
        /// Protected constructor.
        /// </summary>
        /// <param name="valueType">Feature value type.</param>
        /// <param name="featureUse">Feature use.</param>
        protected FeatureFilterBase(FeatureValueType valueType, FeatureUse featureUse)
        {
            ValueType = valueType;
            Use = featureUse;
            SamplesStat = new BasicStat();
            return;
        }

        /// <summary>
        /// Protected copy constructor.
        /// </summary>
        /// <param name="source">The source instance</param>
        protected FeatureFilterBase(FeatureFilterBase source)
        {
            ValueType = source.ValueType;
            Use = source.Use;
            SamplesStat = new BasicStat(source.SamplesStat);
            return;
        }

        //Methods
        /// <summary>
        /// Resets the filter to its initial state.
        /// </summary>
        public virtual void Reset()
        {
            SamplesStat.Reset();
            return;
        }

        /// <summary>
        /// Updates inner statistics.
        /// </summary>
        /// <param name="sample">The sample.</param>
        public virtual void Update(double sample)
        {
            SamplesStat.AddSample(sample);
            return;
        }

        /// <summary>
        /// Applies the filter.
        /// </summary>
        /// <param name="value">A value in original range.</param>
        /// <param name="centered">Specifies whether to center value between -1 an 1, so min value is -1 and max value is 1. If false, 0 is not the interval center but represents the average value and -1 or 1 represents the magnitude.</param>
        /// <returns>A value in normalized range.</returns>
        public abstract double ApplyFilter(double value, bool centered);

        /// <summary>
        /// Applies the filter reverse.
        /// </summary>
        /// <param name="value">A value in normalized range.</param>
        /// <param name="centered">Specifies whether ApplyFilter centered the value.</param>
        /// <returns>A value in original range.</returns>
        public abstract double ApplyReverse(double value, bool centered);

        /// <summary>
        /// Creates a deep clone.
        /// </summary>
        public abstract FeatureFilterBase DeepClone();

    }//FeatureFilterBase

}//Namespace
