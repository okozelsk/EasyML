using EasyMLCore.MathTools;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EasyMLCore.Extensions
{
    /// <summary>
    /// Implements extensions of IList.
    /// </summary>
    public static class ExtensionsIList
    {
        //Methods
        /// <summary>
        /// Multiplies all numbers in a collection by the specified coefficient.
        /// </summary>
        /// <param name="coeff">Coefficient by which to multiply all numbers.</param>
        /// <param name="collection">A collection supporting IList interface.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Scale(this IList<double> collection, double coeff)
        {
            if (collection.Count == 0)
            {
                throw new InvalidOperationException("Operation is not defined on an empty collection.");
            }
            for (int idx = 0; idx < collection.Count; idx++)
            {
                collection[idx] *= coeff;
            }
            return;
        }

        /// <summary>
        /// Proportionally scales all numbers in a collection to be within the specified range.
        /// </summary>
        /// <param name="newRange">New range (min max interval). If null, defaults to range 0..1.</param>
        /// <param name="collection">A collection supporting IList interface.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Rescale(this IList<double> collection, Interval newRange = null)
        {
            if (collection.Count == 0)
            {
                throw new InvalidOperationException("Operation is not defined on an empty collection.");
            }
            newRange ??= Interval.IntZP1;
            if (newRange.Min == newRange.Max)
            {
                throw new ArgumentException("Required new range interval has the same min and max borders.", nameof(newRange));
            }
            Interval orgMinMax = new Interval(collection);
            if (orgMinMax.Min == orgMinMax.Max)
            {
                throw new InvalidOperationException("Operation is not defined for a collection of numbers all having the same value.");
            }
            for (int i = 0; i < collection.Count; i++)
            {
                collection[i] = newRange.Rescale(collection[i], orgMinMax);
            }
            return;
        }

        /// <summary>
        /// Returns the max number value.
        /// </summary>
        /// <param name="collection">A collection supporting IList interface.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Max(this IList<double> collection)
        {
            if (collection.Count == 0)
            {
                throw new InvalidOperationException("Operation is not defined on an empty collection.");
            }
            double max = double.MinValue;
            for (int i = 0; i < collection.Count; i++)
            {
                if (max < collection[i])
                {
                    max = collection[i];
                }
            }
            return max;
        }

        /// <summary>
        /// Returns the max number value.
        /// </summary>
        /// <param name="collection">A collection supporting IList interface.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Max(this IList<int> collection)
        {
            if (collection.Count == 0)
            {
                throw new InvalidOperationException("Operation is not defined on an empty collection.");
            }
            int max = int.MinValue;
            for (int i = 0; i < collection.Count; i++)
            {
                if (max < collection[i])
                {
                    max = collection[i];
                }
            }
            return max;
        }

        /// <summary>
        /// Returns the max value first index.
        /// </summary>
        /// <param name="count">Number of the same max values within the collection.</param>
        /// <param name="collection">A collection supporting IList interface.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfMax(this IList<double> collection, out int count)
        {
            if (collection.Count == 0)
            {
                throw new InvalidOperationException("Operation is not defined on an empty collection.");
            }
            double max = collection[0];
            int maxIdx = 0;
            count = 1;
            for (int i = 1; i < collection.Count; i++)
            {
                if (max < collection[i])
                {
                    max = collection[i];
                    maxIdx = i;
                    count = 1;
                }
                else if (max == collection[i])
                {
                    ++count;
                }
            }
            return maxIdx;
        }

        /// <summary>
        /// Returns the max value first index.
        /// </summary>
        /// <param name="count">Number of the same max values within the collection.</param>
        /// <param name="collection">A collection supporting IList interface.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfMax(this IList<int> collection, out int count)
        {
            if (collection.Count == 0)
            {
                throw new InvalidOperationException("Operation is not defined on an empty collection.");
            }
            int max = collection[0];
            int maxIdx = 0;
            count = 1;
            for (int i = 1; i < collection.Count; i++)
            {
                if (max < collection[i])
                {
                    max = collection[i];
                    maxIdx = i;
                    count = 1;
                }
                else if (max == collection[i])
                {
                    ++count;
                }
            }
            return maxIdx;
        }

        /// <summary>
        /// Returns the min number value.
        /// </summary>
        /// <param name="collection">A collection supporting IList interface.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Min(this IList<double> collection)
        {
            if (collection.Count == 0)
            {
                throw new InvalidOperationException("Operation is not defined on an empty collection.");
            }
            double min = double.MaxValue;
            for (int i = 0; i < collection.Count; i++)
            {
                if (min > collection[i])
                {
                    min = collection[i];
                }
            }
            return min;
        }

        /// <summary>
        /// Returns the min number value.
        /// </summary>
        /// <param name="collection">A collection supporting IList interface.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Min(this IList<int> collection)
        {
            if (collection.Count == 0)
            {
                throw new InvalidOperationException("Operation is not defined on an empty collection.");
            }
            int min = int.MaxValue;
            for (int i = 0; i < collection.Count; i++)
            {
                if (min > collection[i])
                {
                    min = collection[i];
                }
            }
            return min;
        }

        /// <summary>
        /// Returns the min value first index.
        /// </summary>
        /// <param name="count">Number of the same min values within the collection.</param>
        /// <param name="collection">A collection supporting IList interface.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfMin(this IList<double> collection, out int count)
        {
            if (collection.Count == 0)
            {
                throw new InvalidOperationException("Operation is not defined on an empty collection.");
            }
            double min = collection[0];
            int minIdx = 0;
            count = 1;
            for (int i = 1; i < collection.Count; i++)
            {
                if (min > collection[i])
                {
                    min = collection[i];
                    minIdx = i;
                    count = 1;
                }
                else if (min == collection[i])
                {
                    ++count;
                }
            }
            return minIdx;
        }

        /// <summary>
        /// Returns the min value first index.
        /// </summary>
        /// <param name="count">Number of the same min values within the collection.</param>
        /// <param name="collection">A collection supporting IList interface.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfMin(this IList<int> collection, out int count)
        {
            if (collection.Count == 0)
            {
                throw new InvalidOperationException("Operation is not defined on an empty collection.");
            }
            double min = collection[0];
            int minIdx = 0;
            count = 1;
            for (int i = 1; i < collection.Count; i++)
            {
                if (min > collection[i])
                {
                    min = collection[i];
                    minIdx = i;
                    count = 1;
                }
                else if (min == collection[i])
                {
                    ++count;
                }
            }
            return minIdx;
        }

        /// <summary>
        /// Returns the max absolute value (Magnitude).
        /// </summary>
        /// <param name="collection">A collection supporting IList interface.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Magnitude(this IList<double> collection)
        {
            if (collection.Count == 0)
            {
                throw new InvalidOperationException("Operation is not defined on an empty collection.");
            }
            double mag = 0d;
            for (int i = 0; i < collection.Count; i++)
            {
                double absVal = Math.Abs(collection[i]);
                if (mag < absVal)
                {
                    mag = absVal;
                }
            }
            return mag;
        }

        /// <summary>
        /// Returns the max absolute value (Magnitude).
        /// </summary>
        /// <param name="collection">A collection supporting IList interface.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Magnitude(this IList<int> collection)
        {
            if (collection.Count == 0)
            {
                throw new InvalidOperationException("Operation is not defined on an empty collection.");
            }
            int mag = 0;
            for (int i = 0; i < collection.Count; i++)
            {
                int absVal = Math.Abs(collection[i]);
                if (mag < absVal)
                {
                    mag = absVal;
                }
            }
            return mag;
        }

        /// <summary>
        /// Returns the sum of all numbers.
        /// </summary>
        /// <param name="abs">If true then the sum of absolute values is returned.</param>
        /// <param name="collection">A collection supporting IList interface.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Sum(this IList<double> collection, bool abs = false)
        {
            if (collection.Count == 0)
            {
                throw new InvalidOperationException("Operation is not defined on an empty collection.");
            }
            double sum = 0d;
            if (abs)
            {
                for (int i = 0; i < collection.Count; i++)
                {
                    sum += Math.Abs(collection[i]);
                }
            }
            else
            {
                for (int i = 0; i < collection.Count; i++)
                {
                    sum += collection[i];
                }
            }
            return sum;
        }

        /// <summary>
        /// Returns the sum of all numbers.
        /// </summary>
        /// <param name="abs">If true then the sum of absolute values is returned.</param>
        /// <param name="collection">A collection supporting IList interface.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sum(this IList<int> collection, bool abs = false)
        {
            if (collection.Count == 0)
            {
                throw new InvalidOperationException("Operation is not defined on an empty collection.");
            }
            int sum = 0;
            if (abs)
            {
                for (int i = 0; i < collection.Count; i++)
                {
                    sum += Math.Abs(collection[i]);
                }
            }
            else
            {
                for (int i = 0; i < collection.Count; i++)
                {
                    sum += collection[i];
                }
            }
            return sum;
        }

        /// <summary>
        /// Increments numbers by specified value.
        /// </summary>
        /// <param name="value">Increment value.</param>
        /// <param name="collection">A collection supporting IList interface.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Increment(this IList<double> collection, double value)
        {
            if (collection.Count == 0)
            {
                throw new InvalidOperationException("Operation is not defined on an empty collection.");
            }
            for (int i = 0; i < collection.Count; i++)
            {
                collection[i] += value;
            }
            return;
        }

        /// <summary>
        /// Increments numbers by specified value.
        /// </summary>
        /// <param name="value">Increment value.</param>
        /// <param name="collection">A collection supporting IList interface.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Increment(this IList<int> collection, int value)
        {
            if (collection.Count == 0)
            {
                throw new InvalidOperationException("Operation is not defined on an empty collection.");
            }
            for (int i = 0; i < collection.Count; i++)
            {
                collection[i] += value;
            }
            return;
        }

        /// <summary>
        /// Raises numbers to the specified power.
        /// </summary>
        /// <param name="power">A power.</param>
        /// <param name="collection">A collection supporting IList interface.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Power(this IList<double> collection, double power)
        {
            if (collection.Count == 0)
            {
                throw new InvalidOperationException("Operation is not defined on an empty collection.");
            }
            for (int i = 0; i < collection.Count; i++)
            {
                collection[i] = Math.Pow(collection[i], power);
            }
            return;
        }

        /// <summary>
        /// Raises numbers to the specified power.
        /// </summary>
        /// <param name="power">A power.</param>
        /// <param name="collection">A collection supporting IList interface.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Power(this IList<double> collection, uint power)
        {
            if (collection.Count == 0)
            {
                throw new InvalidOperationException("Operation is not defined on an empty collection.");
            }
            for (int i = 0; i < collection.Count; i++)
            {
                collection[i] = collection[i].Power(power);
            }
            return;
        }

        /// <summary>
        /// Scales numbers in the way their sum equals to the specified value.
        /// </summary>
        /// <param name="newSum">The new sum to be achieved.</param>
        /// <param name="abs">If true, new sum will be the sum of absolute values.</param>
        /// <param name="collection">A collection supporting IList interface.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScaleToNewSum(this IList<double> collection, double newSum = 1d, bool abs = false)
        {
            if (collection.Count == 0)
            {
                throw new InvalidOperationException("Operation is not defined on an empty collection.");
            }
            if (abs && newSum < 0)
            {
                throw new ArgumentException("New sum can not be LT 0 when abs arg is true.", nameof(newSum));
            }
            //Scale
            double sum = Sum(collection, abs);
            if (sum != newSum)
            {
                if (sum != 0d)
                {
                    collection.Scale(newSum / sum);
                }
                else
                {
                    if (newSum == 0d)
                    {
                        for(int i = 0; i < collection.Count; i++)
                            collection[i] = 0d;
                    }
                    else
                    {
                        collection.Increment(newSum / collection.Count);
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Changes numbers proportionally according to the rule that the new min is original max and the new max is the original min.
        /// </summary>
        /// <param name="collection">A collection supporting IList interface.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Inverse(this IList<double> collection)
        {
            if (collection.Count == 0)
            {
                throw new InvalidOperationException("Operation is not defined on an empty collection.");
            }
            double max = collection.Max();
            double min = collection.Min();
            double mid = min + ((max - min) / 2d);
            for (int i = 0; i < collection.Count; i++)
            {
                collection[i] = (-(collection[i] - mid)) + mid;
            }
            return;
        }

        /// <summary>
        /// Converts probabilities to 0/1 int binaries.
        /// </summary>
        /// <param name="categorical">Specifies whether probabilities are related to a categorical decision (true) or to a set of independent binary decisions (false).</param>
        /// <param name="collection">A collection supporting IList interface.</param>
        /// <remarks>
        /// When categorical then only the first number having max value is considered as 1.
        /// </remarks>
        /// <returns>Int array containing corresponding binary 0/1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int[] BinarizeProbabilities(this IList<double> collection, bool categorical = false)
        {
            if (collection.Count == 0)
            {
                throw new InvalidOperationException("Operation is not defined on an empty collection.");
            }
            int[] output = new int[collection.Count];
            if(categorical)
            {
                output[collection.IndexOfMax(out _)] = 1;
            }
            else
            {
                for(int i = 0; i < collection.Count; i++)
                {
                    output[i] = Common.GetBinary(collection[i]);
                }
            }
            return output;
        }

        /// <summary>
        /// Fills a collection with the indices.
        /// </summary>
        /// <param name="collection">A collection supporting IList interface.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Indices(this IList<int> collection)
        {
            if (collection.Count == 0)
            {
                throw new InvalidOperationException("Operation is not defined on an empty collection.");
            }
            for (int i = 0; i < collection.Count; i++)
            {
                collection[i] = i;
            }
            return;
        }


    }//ExtensionsIList

}//Namespace

