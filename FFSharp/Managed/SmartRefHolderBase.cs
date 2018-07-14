using System;
using System.Diagnostics;

using FFSharp.Native;

using JetBrains.Annotations;

using Whetstone.Contracts;

namespace FFSharp.Managed
{
    /// <summary>
    /// Base class for implementing managed types that refer to underlying <see cref="SmartRef{T}"/>
    /// instances.
    /// </summary>
    /// <typeparam name="T">The pointed-to struct type.</typeparam>
    // ReSharper disable errors
    [PublicAPI]
    public abstract class SmartRefHolderBase<T> : Disposable
        where T : unmanaged
    {
        /// <summary>
        /// Get the <see cref="SmartRef{T}"/> to the underlying <typeparamref name="T"/>.
        /// </summary>
        [CanBeNull]
        internal SmartRef<T> SmartRef { get; private set; }

        /// <summary>
        /// Create a new <see cref="SmartRefHolderBase{T}"/> instance.
        /// </summary>
        /// <param name="ASmartRef">The associated <see cref="SmartRef{T}"/>.</param>
        internal SmartRefHolderBase([NotNull] SmartRef<T> ASmartRef)
        {
            SmartRef = ASmartRef ?? throw new ArgumentNullException(nameof(ASmartRef));
            SmartRef.Link(this);
        }

        #region Disposable overrides
        /// <inheritdoc />
        protected override void Dispose(bool ADisposing)
        {
            SmartRef?.Unlink(this);
            SmartRef = null;

            base.Dispose(ADisposing);
        }
        #endregion

        /// <summary>
        /// Get the underlying <see cref="Movable{T}"/>.
        /// </summary>
        internal Movable<T> Movable
        {
            get
            {
                ThrowIfDisposed();
                Debug.Assert(SmartRef != null);

                return SmartRef.Movable;
            }
        }
        /// <summary>
        /// Get or set the underlying <see cref="Fixed{T}"/>.
        /// </summary>
        internal Fixed<T> Fixed
        {
            get => Movable.Target;
            set => Movable.SetTarget(value);
        }
    }
    // ReSharper restore errors
}
