using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

using JetBrains.Annotations;

using Whetstone.Contracts;

namespace FFSharp.Native
{
    /// <summary>
    /// Reference type that safely wraps a reference to a <see cref="Movable{T}"/> that originates
    /// from either managed or unmanaged code.
    /// </summary>
    /// <typeparam name="T">The pointed-to struct type.</typeparam>
    /// <remarks>
    /// Decouples object lifetimes for shared and unique ownership of unmanaged structs by providing
    /// a mechanism for wrapping and safely disposing or propagating the disposal of both. If a
    /// <see cref="SmartRef{T}"/> is owning, it will couple the <see cref="Movable{T}"/> lifetime to
    /// itself and optionally execute a cleanup action on it on dispose. If it is non-owning, it
    /// will simply store the pointer and gate access to it as long as it is not disposed.
    /// </remarks>
    // ReSharper disable errors
    internal unsafe class SmartRef<T> : Disposable
        where T : unmanaged
    {
        static Movable<T> AllocMovable()
        {
            Movable<T> alloc = Marshal.AllocHGlobal(sizeof(void*));
            alloc.SetTarget(null);

            return alloc;
        }

        [CanBeNull]
        readonly Action<Movable<T>> FCleanup;
        Movable<T> FMovable;
        [NotNull]
        readonly List<WeakReference<IDisposable>> FHolders = new List<WeakReference<IDisposable>>();

        /// <summary>
        /// Create an owning <see cref="SmartRef{T}"/>.
        /// </summary>
        /// <param name="ACleanup">The optional cleanup <see cref="Action{T}"/>.</param>
        /// <exception cref="OutOfMemoryException">Out of memory.</exception>
        public SmartRef([CanBeNull] Action<Movable<T>> ACleanup = null)
        {
            FCleanup = ACleanup;

            FMovable = AllocMovable();
            IsOwning = true;
        }
        /// <summary>
        /// Create a shared <see cref="SmartRef{T}"/>.
        /// </summary>
        /// <param name="AShared">The shared <see cref="Movable{T}"/>.</param>
        /// <exception cref="ArgumentException">Shared Movable may not be null.</exception>
        public SmartRef(Movable<T> AShared)
        {
            if (AShared.IsNull)
                throw new ArgumentException("Shared Movable may not be null.", nameof(AShared));

            FMovable = AShared;
            IsOwning = false;
        }

        /// <summary>
        /// Link a holder to this <see cref="SmartRef{T}"/>.
        /// </summary>
        /// <param name="AHolder">The <see cref="IDisposable"/> holder.</param>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        public void Link([NotNull] IDisposable AHolder)
        {
            ThrowIfDisposed();

            Debug.Assert(
                AHolder != null,
                "Disposable is null.",
                "This indicates a contract violation."
            );

            for (var I = 0; I < FHolders.Count; ++I)
            {
                if (FHolders[I].TryGetTarget(out var test))
                {
                    if (ReferenceEquals(test, AHolder)) return;
                    continue;
                }

                FHolders.RemoveAt(I);
                --I;
            }

            FHolders.Add(new WeakReference<IDisposable>(AHolder));
        }
        /// <summary>
        /// Unlink a holder from this <see cref="SmartRef{T}"/>.
        /// </summary>
        /// <param name="AHolder">The <see cref="IDisposable"/> holder.</param>
        /// <remarks>
        /// Will dispose the <see cref="SmartRef{T}"/> if there are no more linked holders left.
        /// </remarks>
        public void Unlink([NotNull] IDisposable AHolder)
        {
            if (IsDisposed) return;

            Debug.Assert(
                AHolder != null,
                "Disposable is null.",
                "This indicates a contract violation."
            );

            int I;
            for (I = 0; I < FHolders.Count; ++I)
            {
                if (FHolders[I].TryGetTarget(out var test))
                {
                    if (ReferenceEquals(test, AHolder))
                    {
                        FHolders.RemoveAt(I);
                        if (!IsLinked) Dispose();
                        return;
                    }
                    continue;
                }

                FHolders.RemoveAt(I);
                --I;
            }
        }

        /// <summary>
        /// Get a value indicating whether this <see cref="SmartRef{T}"/> is linked.
        /// </summary>
        public bool IsLinked
        {
            get
            {
                for (var I = 0; I < FHolders.Count; ++I)
                {
                    if (FHolders[I].TryGetTarget(out _))
                        return true;

                    FHolders.RemoveAt(I);
                    --I;
                }

                return false;
            }
        }

        #region Disposable overrides
        /// <inheritdoc />
        protected override void Dispose(bool ADisposing)
        {
            var holders = FHolders
                .Select(X => X.TryGetTarget(out var target) ? target : null)
                .Where(X => X != null)
                .ToArray();

            FHolders.Clear();
            foreach (var holder in holders)
                holder.Dispose();

            if (IsOwning)
            {
                FCleanup?.Invoke(FMovable);
                Marshal.FreeHGlobal(FMovable);
            }

            FMovable = null;
            base.Dispose(ADisposing);
        }
        #endregion

        /// <summary>
        /// Get the underlying <see cref="Movable{T}"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        public Movable<T> Movable
        {
            get
            {
                ThrowIfDisposed();
                Debug.Assert(
                    !FMovable.IsNull,
                    "Movable is null.",
                    "This indicates a severe logic error in the code."
                );

                return FMovable;
            }
        }
        /// <summary>
        /// Get a value indicating whether this <see cref="SmartRef{T}"/> is owning.
        /// </summary>
        public bool IsOwning { get; }

        /// <summary>
        /// Implicitly convert a <see cref="SmartRef{T}"/> to it's
        /// !<see cref="Disposable.IsDisposed"/>.
        /// </summary>
        /// <param name="ASmartRef">The <see cref="SmartRef{T}"/>.</param>
        public static implicit operator bool(SmartRef<T> ASmartRef) => !ASmartRef.IsDisposed;

        /// <summary>
        /// Implicitly convert a <see cref="SmartRef{T}"/> to it's <see cref="Movable"/>.
        /// </summary>
        /// <param name="ASmartRef">The <see cref="SmartRef{T}"/>.</param>
        public static implicit operator Movable<T>(SmartRef<T> ASmartRef) => ASmartRef.Movable;
        /// <summary>
        /// Implicitly convert a <see cref="SmartRef{T}"/> to it's <see cref="Fixed{T}"/> target.
        /// </summary>
        /// <param name="ASmartRef">The <see cref="SmartRef{T}"/>.</param>
        public static implicit operator Fixed<T>(SmartRef<T> ASmartRef) => ASmartRef.Movable.Target;
    }
    // ReSharper restore errors
}
