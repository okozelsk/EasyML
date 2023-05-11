using System;
using System.Runtime.CompilerServices;

namespace EasyMLCore.Extensions
{
    /// <summary>
    /// Implements extensions of the Array.
    /// </summary>
    public static class ExtensionsArray
    {
        /// <summary>
        /// Extracts part of this array.
        /// </summary>
        /// <param name="fromIdx">Where to start in this array.</param>
        /// <param name="count">Number of items to be copied.</param>
        /// <param name="array">This array.</param>
        /// <returns>A new instance of an array containing extracted items.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Extract<T>(this T[] array, int fromIdx, int count)
        {
            if (array.Length == 0)
            {
                throw new InvalidOperationException("Array has no items.");
            }
            T[] result = new T[count];
            Array.Copy(array, fromIdx, result, 0, count);
            return result;
        }

    }//ExtensionsArray

}//Namespace

