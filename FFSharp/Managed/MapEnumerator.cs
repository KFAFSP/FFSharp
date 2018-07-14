using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

using Whetstone.Contracts;

namespace FFSharp.Managed
{
    /// <summary>
    /// Utility base class for implementing an <see cref="IEnumerator{T}"/> that performs a map.
    /// </summary>
    /// <typeparam name="TIn">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    [ExcludeFromCodeCoverage]
    internal abstract class MapEnumerator<TIn, TOut> : Disposable,
        IEnumerator<TOut>
    {
        [NotNull]
        readonly IEnumerator<TIn> FParent;

        bool FComputed;
        TOut FCurrent;

        /// <summary>
        /// Create a new <see cref="MapEnumerator{TIn,TOut}"/> instance.
        /// </summary>
        /// <param name="AParent">The parent <see cref="IEnumerator{T}"/>.</param>
        protected MapEnumerator([NotNull] IEnumerator<TIn> AParent)
        {
            Debug.Assert(
                AParent != null,
                "Parent is null.",
                "This indicates a contract violation."
            );

            FParent = AParent;
            FComputed = false;
        }

        /// <summary>
        /// Map an input to an output element.
        /// </summary>
        /// <param name="AIn">The input element.</param>
        /// <returns>The output element.</returns>
        /// <remarks>
        /// Guaranteed to be performed only once for each input element during a non-resetted
        /// enumeration.
        /// </remarks>
        protected abstract TOut Map(TIn AIn);

        #region Disposable overrides
        /// <inheritdoc />
        protected override void Dispose(bool ADisposing)
        {
            FParent.Dispose();

            base.Dispose(ADisposing);
        }
        #endregion

        #region IEnumerator<TOut>
        /// <inheritdoc />
        public bool MoveNext()
        {
            if (!FParent.MoveNext())
                return false;

            FComputed = false;
            return true;
        }
        /// <inheritdoc />
        public void Reset()
        {
            FParent.Reset();
            FComputed = false;
        }

        /// <inheritdoc />
        public TOut Current
        {
            get
            {
                if (!FComputed)
                {
                    FCurrent = Map(FParent.Current);
                    FComputed = true;
                }

                return FCurrent;
            }
        }
        #endregion

        #region IEnumerator
        object IEnumerator.Current => Current;
        #endregion
    }
}
