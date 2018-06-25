using System;
using System.Diagnostics;

using FFSharp.Native;

using JetBrains.Annotations;

namespace FFSharp.Managed
{
    /// <summary>
    /// Base class for correctly implementing the <see cref="SmartRef{T}"/> holder pattern.
    /// </summary>
    /// <typeparam name="T">The pointed-to struct type.</typeparam>
    /// <remarks>
    /// If you inherit from this class, you don't have to do anything more.
    /// </remarks>
    // ReSharper disable errors
    [PublicAPI]
    public abstract class SmartRefHolderBase<T> : Disposable
        where T : unmanaged
    {
        /// <summary>
        /// Get the <see cref="SmartRef{T}"/> used by this holder.
        /// </summary>
        [CanBeNull]
        internal SmartRef<T> Ref { get; private set; }
        /// <summary>
        /// Safely get the <see cref="Movable{T}"/> of <see cref="Ref"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        internal Movable<T> Movable
        {
            get
            {
                ThrowIfDisposed();

                Debug.Assert(
                    Ref != null,
                    "Ref is null.",
                    "This indicates a severe logic error in the code."
                );
                Ref.Movable.AssertPresent();

                return Ref.Movable;
            }
        }
        /// <summary>
        /// Safely get the <see cref="Fixed{T}"/> of <see cref="Ref"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        internal Fixed<T> Fixed
        {
            get
            {
                ThrowIfDisposed();

                return Movable.AsFixed;
            }
        }

        /// <summary>
        /// Create a new <see cref="SmartRefHolderBase{T}"/> instance.
        /// </summary>
        /// <param name="ASmartRef">The <see cref="SmartRef{T}"/>.</param>
        internal SmartRefHolderBase([NotNull] SmartRef<T> ASmartRef)
        {
            Debug.Assert(
                ASmartRef != null,
                "SmartRef is null.",
                "This indicates a contract violation."
            );

            ASmartRef.Movable.AssertPresent();

            Ref = ASmartRef;
            Ref.Acquire(this);
        }

        #region Disposable overrides
        /// <inheritdoc/>
        protected override void Dispose(bool ADisposing)
        {
            Ref?.Release(this);
            Ref = null;
            base.Dispose(ADisposing);
        }
        #endregion
    }
    // ReSharper restore errors
}
