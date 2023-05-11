using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EasyMLCore.Extensions
{
    /// <summary>
    /// Implements extensions of IEnumerable.
    /// </summary>
    public static class ExtensionsIEnumerable
    {
        //Methods
        /// <summary>
        /// Returns the length of the string having the max length.
        /// </summary>
        /// <param name="enumerable">This IEnumerable of strings.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MaxLength(this IEnumerable<string> enumerable)
        {
            int maxLength = 0;
            foreach(string str in enumerable)
            {
                maxLength = Math.Max(str.Length, maxLength);
            }
            return maxLength;
        }

        /// <summary>
        /// Returns the length of the string having the min length.
        /// </summary>
        /// <param name="enumerable">This IEnumerable of strings.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MinLength(this IEnumerable<string> enumerable)
        {
            int maxLength = 0;
            foreach (string str in enumerable)
            {
                maxLength = Math.Min(str.Length, maxLength);
            }
            return maxLength;
        }

        /// <summary>
        /// Sequentially concates this IEnumerable of arrays of type T into a single flat array of type T.
        /// </summary>
        /// <param name="enumerable">This IEnumerable of arrays.</param>
        /// <remarks>Method skippes null references in IEnumerable collection.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Flattenize<T>(this IEnumerable<T[]> enumerable)
        {
            int length = 0;
            foreach (T[] vector in enumerable)
            {
                if (vector != null)
                {
                    length += vector.Length;
                }
            }
            T[] resultVector = new T[length];
            if (length > 0)
            {
                int idx = 0;
                foreach (T[] vector in enumerable)
                {
                    if (vector != null)
                    {
                        vector.CopyTo(resultVector, idx);
                        idx += vector.Length;
                    }
                }
            }
            return resultVector;
        }

        /// <summary>
        /// Gets a vector of averages.
        /// </summary>
        /// <param name="enumerable">This enumerable of vectors.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[] GetAverages(this IEnumerable<double[]> enumerable)
        {
            int count = 0;
            double[] averageVector = null;
            foreach (double[] vector in enumerable)
            {
                if (vector != null)
                {
                    if (averageVector == null)
                    {
                        averageVector = (double[])vector.Clone();
                    }
                    else
                    {
                        for (int i = 0; i < averageVector.Length; i++)
                        {
                            averageVector[i] += vector[i];
                        }
                    }
                    ++count;
                }
            }
            for (int i = 0; i < averageVector.Length; i++)
            {
                averageVector[i] /= count;
            }
            return averageVector;
        }

        /// <summary>
        /// Checks if IEnumerable contains only unique items (uses Equals).
        /// </summary>
        /// <param name="enumerable">This enumerable.</param>
        /// <returns>Boolean indicator whether IEnumerable contains only unique items.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsOnlyUniques<T>(this IEnumerable<T> enumerable)
        {
            HashSet<T> uniques = new HashSet<T>();
            foreach (T item in enumerable)
            {
                if(!uniques.Add(item))
                {
                    return false;
                }
            }
            return uniques.Count > 0;
        }

        /// <summary>
        /// Computes the Median on IEnumerable of double.
        /// </summary>
        /// <param name="enumerable">This double enumerable.</param>
        /// <returns>The median value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Median(this IEnumerable<double> enumerable)
        {
            double[] numbers = enumerable.ToArray();
            if (numbers.Length == 0)
            {
                throw new ArgumentException("No numbers.", nameof(enumerable));
            }
            //Sort the numbers
            Array.Sort(numbers);
            //Return the median
            int middleIdx = numbers.Length / 2;
            return (numbers.Length % 2 != 0) ? numbers[middleIdx] : (numbers[middleIdx] + numbers[middleIdx - 1]) / 2d;
        }

    }//ExtensionsIEnumerable

}//Namespace

