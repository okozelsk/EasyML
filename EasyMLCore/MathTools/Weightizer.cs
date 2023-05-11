using EasyMLCore.Extensions;
using System;
using System.Collections.Generic;

namespace EasyMLCore.MathTools
{
    /// <summary>
    /// Implements the simple weghting.
    /// </summary>
    /// <remarks>
    /// The values to be weightized must be nonnegative.
    /// </remarks>
    [Serializable]
    public class Weightizer : SerializableObject
    {
        //Attributes
        public readonly List<double> _natValues;

        //Constructor
        /// <summary>
        /// Creates an unitialized instance.
        /// </summary>
        public Weightizer()
        {
            _natValues = new List<double>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="values">Values of the same metric to be weighted with each other.</param>
        public Weightizer(IEnumerable<double> values)
            : this()
        {
            foreach (double value in values)
            {
                Add(value);
            }
            return;
        }

        //Methods
        /// <summary>
        /// Cleares already added values.
        /// </summary>
        public void Reset()
        {
            _natValues.Clear();
            return;
        }

        /// <summary>
        /// Gets already added values.
        /// </summary>
        public double[] GetValues()
        {
            return _natValues.ToArray();
        }

        /// <summary>
        /// Adds the next value.
        /// </summary>
        /// <param name="value">A value.</param>
        public void Add(double value)
        {
            if (value < 0d)
            {
                throw new ArgumentException("Invalid value. Value must be nonnegative.", nameof(value));
            }
            _natValues.Add(value);
            return;
        }

        /// <summary>
        /// Sets the values.
        /// </summary>
        /// <param name="values">Values to be set.</param>
        public void Set(IEnumerable<double> values)
        {
            Reset();
            foreach (double value in values)
            {
                Add(value);
            }
            return;
        }

        /// <summary>
        /// Gets weightized values.
        /// </summary>
        /// <param name="power">The power to be applied after the normalization.</param>
        /// <param name="inverse">Specifies whether to inverse values before weightization.</param>
        public double[] GetWeights(double power = 1d, bool inverse = false)
        {
            double[] result = _natValues.ToArray();
            if (inverse)
            {
                result.Inverse();
            }
            double max = result.Max();
            if (max != 0)
            {
                result.Scale(1d / max);
                if (power != 1d)
                {
                    result.Power(power);
                    result.Scale(1d / result.Max());
                }
            }
            return result;
        }

        /// <summary>
        /// Gets weightized values in softmax fashion.
        /// </summary>
        /// <param name="inverse">Specifies whether to inverse values before weightization.</param>
        public double[] GetSoftmaxWeights(bool inverse = false)
        {
            double[] result = _natValues.ToArray();
            if (inverse)
            {
                result.Inverse();
            }
            double max = result.Max();
            double sum = 0d;
            if (max != 0)
            {
                for(int i = 0; i < result.Length; i++)
                {
                    result[i] = Math.Exp(result[i] - max);
                    sum += result[i];
                }
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] /= sum;
                }
            }
            return result;
        }

    }//Weightizer
}//Namespace
