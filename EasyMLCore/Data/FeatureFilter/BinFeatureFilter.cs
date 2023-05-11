using EasyMLCore.Extensions;
using EasyMLCore.MathTools;
using System;

namespace EasyMLCore.Data
{
    /// <summary>
    /// Implements filter of a binary feature.
    /// </summary>
    [Serializable]
    public class BinFeatureFilter : FeatureFilterBase
    {
        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="featureUse">Feature use.</param>
        public BinFeatureFilter(FeatureUse featureUse = FeatureUse.Output)
            : base(FeatureValueType.Binary, featureUse)
        {
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance</param>
        public BinFeatureFilter(BinFeatureFilter source)
            : base(source)
        {
            return;
        }

        //Static methods
        /// <summary>
        /// Gets the binary border value of normalized feature depending on specified feature use.
        /// </summary>
        /// <param name="featureUse">Feature use.</param>
        /// <returns>The binary border.</returns>
        public static double GetBinaryBorder(FeatureUse featureUse)
        {
            if (featureUse == FeatureUse.Input)
            {
                return 0d;
            }
            else
            {
                return 0.5d;
            }
        }

        //Methods
        /// <inheritdoc/>
        public override void Update(double sample)
        {
            if (sample != Interval.IntZP1.Min && sample != Interval.IntZP1.Max)
            {
                throw new ArgumentException($"Sample value {sample} is not allowed. Sample value must be {Interval.IntZP1.Min} or {Interval.IntZP1.Max}.", nameof(sample));
            }
            base.Update(sample);
            return;
        }

        /// <inheritdoc/>
        public override double ApplyFilter(double value)
        {
            if (!value.IsValid())
            {
                throw new ArgumentException("Value argument is not a valid (computable) double value.", nameof(value));
            }
            if (Use == FeatureUse.Input)
            {
                return value == 0d ? -1d : 1d;
            }
            else
            {
                return value;
            }
        }

        /// <inheritdoc/>
        public override double ApplyReverse(double value)
        {
            if (!value.IsValid())
            {
                throw new ArgumentException("Value argument is not a valid (computable) double value.", nameof(value));
            }
            if (Use == FeatureUse.Input)
            {
                return value == -1d ? 0d : 1d;
            }
            else
            {
                return value;
            }
        }

        /// <inheritdoc/>
        public override FeatureFilterBase DeepClone()
        {
            return new BinFeatureFilter(this);
        }

    }//BinFeatureFilter

}//Namespace
