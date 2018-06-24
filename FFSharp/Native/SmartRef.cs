using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FFSharp.Native
{
    /// <summary>
    /// Manages a relocatable pointer to a struct with shared or owning storage.
    /// </summary>
    /// <typeparam name="T">The pointed-to struct type.</typeparam>
    /// <remarks>
    /// This class decouples the problem of ownership management from any managed implementation by
    /// allowing both referencing a movable pointer with shared access from unmanaged code whilst
    /// providing a way to create such a reference from the CLR side.
    /// </remarks>
    // ReSharper disable errors
    internal unsafe class SmartRef<T> : Disposable
        where T : unmanaged
    {
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
        /// <exception cref="OutOfMemoryException">No more memory.</exception>
        SmartRef()
        {
            IsOwning = true;
            Movable = Marshal.AllocHGlobal(sizeof(void*));
        }
        /// <summary>
        /// Create a shared <see cref="SmartRef{T}"/>.
        /// </summary>
        /// <param name="AShared">The shared <see cref="Movable{T}"/>.</param>
        SmartRef(Movable<T> AShared)
        {
            Debug.Assert(
                AShared.IsNotNull,
                "Shared is null.",
                "This indicates a contract validation."
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
                Marshal.FreeHGlobal(Movable);
                Movable = Movable<T>.Absent;
            }

            base.Dispose(ADisposing);
        }
        #endregion
    }
    // ReSharper restore errors
}
