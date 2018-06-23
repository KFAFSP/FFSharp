using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using JetBrains.Annotations;

namespace FFSharp.Native
{
    /// <summary>
    /// Utility class for implementing <see cref="IEnumerable{T}"/> by factory delegate.
    /// </summary>
    /// <typeparam name="T">The enumerated item type.</typeparam>
    internal sealed class FactoryEnumerable<T> :
        IEnumerable<T>
    {
        [NotNull]
        readonly Func<IEnumerator<T>> FFactory;

        /// <summary>
        /// Create a new <see cref="FactoryEnumerable{T}"/> instance.
        /// </summary>
        /// <param name="AFactory">The <see cref="IEnumerator{T}"/> factory.</param>
        public FactoryEnumerable([NotNull] Func<IEnumerator<T>> AFactory)
        {
            Debug.Assert(
                AFactory != null,
                "Factory is null.",
                "This indicates a severe logic error."
            );

            FFactory = AFactory;
        }

        #region IEnumerable<T>
        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator() => FFactory();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
