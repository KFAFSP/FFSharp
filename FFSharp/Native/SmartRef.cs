using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using JetBrains.Annotations;

using Whetstone.Contracts;

namespace FFSharp.Native
{
    /// <summary>
    /// Manages a relocatable pointer to a struct with shared or owning storage.
    /// </summary>
    /// <typeparam name="T">The pointed-to struct type.</typeparam>
    /// <remarks>
    /// <para>
    /// This class decouples the problem of ownership management from any managed implementation by
    /// allowing both referencing a movable pointer with shared access from unmanaged code whilst
    /// providing a way to create such a reference from the CLR side.
    /// </para>
    /// <para>
    /// For owning <see cref="SmartRef{T}"/> instances a cleanup <see cref="Action{T}"/> may be
    /// supplied which is invoked on cleanup. This action must not throw as it is also run in the
    /// finalizer if necessary!
    /// </para>
    /// <para>
    /// To use <see cref="SmartRef{T}"/> properly, the following things should be noted:
    /// <list type="bullet">
    /// <item><description>
    /// Any holder of <see cref="SmartRef{T}"/> must derive from <see cref="IDisposable"/> and
    /// cease all use of the reference on <see cref="IDisposable.Dispose()"/> which will be called
    /// when the <see cref="SmartRef{T}"/> ceases to exist because of outside interference.
    /// </description></item>
    /// <item><description>
    /// Any holder of <see cref="SmartRef{T}"/> must call
    /// <see cref="SmartRef{T}.Acquire(IDisposable)"/> when it initially gets the
    /// <see cref="SmartRef{T}"/> to indicate it's usage of it. When the holder is disposed, it
    /// should call <see cref="SmartRef{T}.Release(IDisposable)"/> to signal that it no longer needs
    /// to be informed of the reference's death.
    /// </description></item>
    /// <item><description>
    /// Any holder of <see cref="SmartRef{T}"/> shall keep the reference to the object around as
    /// long as it is needed and unroot it once it is no longer needed to allow it to be garbage-
    /// collected.
    /// </description></item>
    /// </list>
    /// </para>
    /// </remarks>
    // ReSharper disable errors
    internal unsafe class SmartRef<T> : Disposable
        where T : unmanaged
    {
        [CanBeNull]
        Action<Movable<T>> FCleanup;
        /// <summary>
        /// Get a value indicating whether this <see cref="SmartRef{T}"/> is owning.
        /// </summary>
        public bool IsOwning { get; }

        /// <summary>
        /// Get the <see cref="Movable{T}"/> of this <see cref="SmartRef{T}"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="IsOwning"/> is <see langword="false"/> the unmanaged code may have
        /// invalidates this pointer and accessing it may have become invalid. It is in the
        /// responsibility of the caller to verify that this reference is still valid.
        /// </remarks>
        public Movable<T> Movable { get; private set; }

        /// <summary>
        /// Create an owning <see cref="SmartRef{T}"/>.
        /// </summary>
        /// <param name="ACleanup">A cleanup <see cref="Action{T}"/>.</param>
        /// <exception cref="OutOfMemoryException">No more memory.</exception>
        public SmartRef([CanBeNull] Action<Movable<T>> ACleanup = null)
        {
            IsOwning = true;
            Movable = Marshal.AllocHGlobal(sizeof(void*));
            Movable.SetTarget(null);
            FCleanup = ACleanup;
        }
        /// <summary>
        /// Create a shared <see cref="SmartRef{T}"/>.
        /// </summary>
        /// <param name="AShared">The shared <see cref="Movable{T}"/>.</param>
        public SmartRef(Movable<T> AShared)
        {
            Debug.Assert(
                AShared.IsPresent,
                "Shared is absent.",
                "This indicates a contract violation."
            );

            IsOwning = false;
            Movable = AShared;
        }

        #region Disposable
        /// <inheritdoc/>
        protected override void Dispose(bool ADisposing)
        {
            if (IsOwning)
            {
                FCleanup?.Invoke(Movable);
                Marshal.FreeHGlobal(Movable);
            }

            Movable = Movable<T>.Null;

            if (ADisposing)
            {
                PropagateDispose();
            }

            base.Dispose(ADisposing);
        }
        #endregion

        #region Weak event pattern
        [NotNull]
        readonly List<WeakReference<IDisposable>> FReferences =
            new List<WeakReference<IDisposable>>();

        void PropagateDispose()
        {
            // This will speed up the Release() calls that will happen now.
            var refs = FReferences.ToArray();
            FReferences.Clear();

            foreach (var weakRef in refs)
            {
                if (weakRef.TryGetTarget(out var target))
                {
                    target.Dispose();
                }
            }
        }

        /// <summary>
        /// Acquire a reference to this <see cref="Movable{T}"/>.
        /// </summary>
        /// <param name="ADisposable">The acquiring <see cref="IDisposable"/>.</param>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        public void Acquire([NotNull] IDisposable ADisposable)
        {
            Debug.Assert(
                ADisposable != null,
                "Disposable is null.",
                "This indicates a contract violation."
            );

            ThrowIfDisposed();

            FReferences.Add(new WeakReference<IDisposable>(ADisposable));
        }
        /// <summary>
        /// Release an acquired reference to this <see cref="Movable{T}"/>.
        /// </summary>
        /// <param name="ADisposable">The releasing <see cref="IDisposable"/>.</param>
        public void Release([NotNull] IDisposable ADisposable)
        {
            Debug.Assert(
                ADisposable != null,
                "Disposable is null.",
                "This indicates a contract violation."
            );

            for (int I = 0; I < FReferences.Count; ++I)
            {
                if (!FReferences[I].TryGetTarget(out var target))
                {
                    FReferences.RemoveAt(I);
                    --I;
                    continue;
                }

                if (ReferenceEquals(target, ADisposable))
                {
                    FReferences.RemoveAt(I);
                    return;
                }
            }
        }
        #endregion
    }
    // ReSharper restore errors
}
