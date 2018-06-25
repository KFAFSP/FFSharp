using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace FFSharp.Managed
{
    /// <summary>
    /// Extensions for the <see cref="IEnumerator{T}"/> class.
    /// </summary>
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
        )
        {
            return new DelegateMapEnumerator<TIn, TOut>(AIn, AMap);
        }
    }
}