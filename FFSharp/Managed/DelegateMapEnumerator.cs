using System;
using System.Collections.Generic;
using System.Diagnostics;

using JetBrains.Annotations;

namespace FFSharp.Managed
{
    /// <summary>
    /// Implementation of <see cref="MapEnumerator{TIn, TOut}"/> that uses a
    /// <see cref="Func{T, TResult}"/> delegate.
    /// </summary>
    /// <typeparam name="TIn">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    internal sealed class DelegateMapEnumerator<TIn, TOut> : MapEnumerator<TIn, TOut>
    {
        [NotNull]
        readonly Func<TIn, TOut> FMap;

        /// <summary>
        /// Create a new <see cref="DelegateMapEnumerator{TIn, TOut}"/> instance.
        /// </summary>
        /// <param name="AParent">The parent <see cref="IEnumerator{T}"/>.</param>
        /// <param name="AMap">The map <see cref="Func{T, TResult}"/>.</param>
        public DelegateMapEnumerator(
            [NotNull] IEnumerator<TIn> AParent,
            [NotNull] Func<TIn, TOut> AMap
        ) : base(AParent)
        {
            Debug.Assert(
                AMap != null,
                "Map is null.",
                "This indicates a contract violation."
            );

            FMap = AMap;
        }

        #region MapEnumerator<TIn, TOut> overrides
        /// <inheritdoc />
        protected override TOut Map(TIn AIn) => FMap(AIn);
        #endregion
    }
}