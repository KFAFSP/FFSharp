using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace FFSharp.Managed
{
    /// <summary>
    /// Extensions for the <see cref="IEnumerator{T}"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class EnumeratorExtensions
    {
        /// <summary>
        /// Create a <see cref="DelegateMapEnumerator{TIn, TOut}"/> for the specified input.
        /// </summary>
        /// <typeparam name="TIn">The input type.</typeparam>
        /// <typeparam name="TOut">The output type.</typeparam>
        /// <param name="AIn">The input <see cref="IEnumerator{T}"/>.</param>
        /// <param name="AMap">The map <see cref="Func{T, TResult}"/>.</param>
        /// <returns>A mapped <see cref="IEnumerator{T}"/>.</returns>
        [NotNull]
        public static IEnumerator<TOut> Map<TIn, TOut>(
            [NotNull] this IEnumerator<TIn> AIn,
            [NotNull] Func<TIn, TOut> AMap
        ) => new DelegateMapEnumerator<TIn, TOut>(AIn, AMap);

        /// <summary>
        /// Copy all items from an <see cref="IEnumerator{T}"/> into a new <see cref="List{T}"/>.
        /// </summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="AEnumerator">The input <see cref="IEnumerator{T}"/>.</param>
        /// <returns>The new <see cref="List{T}"/> of copied items.</returns>
        [NotNull]
        public static List<T> ToList<T>(
            [NotNull] this IEnumerator<T> AEnumerator
        )
        {
            try
            {
                var result = new List<T>();

                while (AEnumerator.MoveNext())
                    result.Add(AEnumerator.Current);

                return result;
            }
            finally
            {
                AEnumerator.Dispose();
            }
        }

        /// <summary>
        /// Copy items from an <see cref="IEnumerator{T}"/> to an array.
        /// </summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="AEnumerator">The input <see cref="IEnumerator{T}"/>.</param>
        /// <param name="AArray">The output array.</param>
        /// <param name="AArrayIndex">The start index in the output array.</param>
        /// <returns>Number of items copied.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="AArrayIndex"/> is out of range.
        /// </exception>
        /// <exception cref="ArgumentException">Array is too small.</exception>
        public static int CopyTo<T>(
            [NotNull] this IEnumerator<T> AEnumerator,
            [NotNull] T[] AArray,
            int AArrayIndex
        )
        {
            Debug.Assert(
                AEnumerator != null,
                "Enumerator is null.",
                "This indicates a contract violation."
            );
            Debug.Assert(
                AArray != null,
                "Array is null.",
                "This indicates a contract violation."
            );

            try
            {
                if (AArrayIndex < 0 || AArrayIndex >= AArray.Length)
                    throw new ArgumentOutOfRangeException(nameof(AArrayIndex));

                var I = AArrayIndex;
                while (AEnumerator.MoveNext())
                {
                    if (I >= AArray.Length)
                        throw new ArgumentException("Array is too small.");

                    AArray[I] = AEnumerator.Current;
                    ++I;
                }

                return AArrayIndex - I;
            }
            finally
            {
                AEnumerator.Dispose();
            }
        }
    }
}