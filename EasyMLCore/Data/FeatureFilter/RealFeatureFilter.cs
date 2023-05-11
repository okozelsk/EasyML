using EasyMLCore.Extensions;
using EasyMLCore.MathTools;
using System;

namespace EasyMLCore.Data
{
    /// <summary>
    /// Implements filter of a real number feature.
    /// </summary>
    [Serializable]
    public class RealFeatureFilter : FeatureFilterBase
    {
        public BasicStat StdSamplesStat { get; }
        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="featureUse">Feature use.</param>
        public RealFeatureFilter(FeatureUse featureUse)
            : base(FeatureValueType.Real, featureUse)
        {
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance</param>
        public RealFeatureFilter(RealFeatureFilter source)
            : base(source)
        {
            return;
        }

        //Methods
        /// <summary>
        /// Gets interval of standardized data.
        /// </summary>
        private Interval GetStdInterval()
        {
            return new Interval((SamplesStat.Min - SamplesStat.ArithAvg) / (SamplesStat.StdDev == 0d ? 1d : SamplesStat.StdDev),
                                (SamplesStat.Max - SamplesStat.ArithAvg) / (SamplesStat.StdDev == 0d ? 1d : SamplesStat.StdDev)
                                );
        }
        /// <inheritdoc/>
        public override double ApplyFilter(double value)
        {
            if (!value.IsValid())
            {
                throw new ArgumentException("Value argument is not a valid (computable) double value.", nameof(value));
            }
            if(SamplesStat.Span == 0d)
            {
                //All sample values are the same
                return 1d;
            }
            //Standardize
            value -= SamplesStat.ArithAvg;
            value /= SamplesStat.StdDev;
            //Normalize
            value = Interval.IntN1P1.Rescale(value, GetStdInterval());
            if(!value.IsValid())
            {
                throw new ApplicationException("Filter ApplyFilter leads to unusable value.");
            }
            return value;
        }

        /// <inheritdoc/>
        public override double ApplyReverse(double value)
        {
            if (!value.IsValid())
            {
                throw new ArgumentException("Value argument is not a valid (computable) double value.", nameof(value));
            }
            if (SamplesStat.Span == 0d)
            {
                //All sample values are the same
                return SamplesStat.ArithAvg;
            }
            //Denormalize
            value = GetStdInterval().Rescale(value, Interval.IntN1P1);
            //Destandardize
            value *= SamplesStat.StdDev;
            value += SamplesStat.ArithAvg;
            if (!value.IsValid())
            {
                throw new ApplicationException("Filter ApplyReverse leads to unusable value.");
            }
            return value;
        }

        /// <inheritdoc/>
        public override FeatureFilterBase DeepClone()
        {
            return new RealFeatureFilter(this);
        }


    }//RealFeatureFilter

}//Namespace
