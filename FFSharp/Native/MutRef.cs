using System;
using System.Diagnostics;

using JetBrains.Annotations;

namespace FFSharp.Native
{
    /// <summary>
    /// Wraps a pointer based relocatable reference to a struct.
    /// </summary>
    /// <typeparam name="T">The pointed-to type.</typeparam>
    /// <remarks>
    /// Use this instead of a <c>T**</c> pointer to better represent the intention and to statically
    /// check contracts.
    /// </remarks>
    // ReSharper disable errors
    internal unsafe struct MutRef<T> :
        IEquatable<MutRef<T>>
        where T : unmanaged
    {
        /// <summary>
        /// Initialize a <see cref="MutRef{T}"/>.
        /// </summary>
        /// <param name="AMutPtr">The target pointer pointer.</param>
        public MutRef([CanBeNull] T** AMutPtr)
        {
            MutPtr = AMutPtr;
        }

        #region IEquatable<MutRef<T>>
        /// <inheritdoc/>
        public bool Equals(MutRef<T> ARef)
        {
            return MutPtr == ARef.MutPtr;
        }
        #endregion

        #region System.Object overrides
        /// <inheritdoc/>
        public override bool Equals(object AObject)
        {
            switch (AObject)
            {
                case MutRef<T> wrapped:
                    return Equals(wrapped);

                default:
                    return false;
            }
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Address.GetHashCode();
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"<Ref:0x{Address.ToInt64():X16}>";
        }
        #endregion

        /// <summary>
        /// Get the pointer to the pointer to the struct.
        /// </summary>
        [CanBeNull]
        public T** MutPtr { get; }
        /// <summary>
        /// Get the address of <see cref="MutPtr"/> as an <see cref="IntPtr"/>.
        /// </summary>
        public IntPtr Address => (IntPtr)MutPtr;

        /// <summary>
        /// Assert that <see cref="MutPtr"/> is not <see langword="null"/>.
        /// </summary>
        [DebuggerHidden]
        [Conditional("DEBUG")]
        public void AssertNotNull()
        {
            Debug.Assert(
                !IsNull,
                "MutPtr is null.",
                "This indicates a severe logic error in the code."
            );
        }
        /// <summary>
        /// Assert that <see cref="MutPtr"/> and <see cref="Ptr"/> are not <see langword="null"/>.
        /// </summary>
        [DebuggerHidden]
        [Conditional("DEBUG")]
        public void AssertNotAbsent()
        {
            AsRef.AssertNotNull();
        }

        /// <summary>
        /// Get or set the pointer to the struct.
        /// </summary>
        /// <remarks>
        /// This property is only safe to use if <see cref="IsNull"/> is <see langword="false"/>.
        /// </remarks>
        [CanBeNull]
        public T* Ptr
        {
            get
            {
                AssertNotNull();
                return *MutPtr;
            }
            set
            {
                AssertNotNull();
                *MutPtr = value;
            }
        }

        /// <summary>
        /// Get a value indicating whether <see cref="MutPtr"/> is <see langword="null"/>.
        /// </summary>
        public bool IsNull => MutPtr == null;
        /// <summary>
        /// Get a value indicating whether the pointed-to struct is absent.
        /// </summary>
        /// <remarks>
        /// The target is considered absent if either <see cref="MutPtr"/> or <see cref="Ptr"/> is
        /// <see langword="null"/>.
        /// </remarks>
        public bool IsAbsent => IsNull || Ptr == null;

        /// <summary>
        /// Get or set the underlying pointer as a <see cref="Ref{T}"/>.
        /// </summary>
        /// <remarks>
        /// This property is only safe to use if <see cref="IsNull"/> is <see langword="false"/>.
        /// </remarks>
        public Ref<T> AsRef
        {
            get => new Ref<T>(Ptr);
            set => Ptr = value.Ptr;
        }

        #region Operator overloads
        public static bool operator ==(MutRef<T> ALhs, MutRef<T> ARhs) => ALhs.Equals(ARhs);
        public static bool operator !=(MutRef<T> ALhs, MutRef<T> ARhs) => !ALhs.Equals(ARhs);

        public static bool operator ==(MutRef<T> ALhs, T** ARhs) => ALhs.MutPtr == ARhs;
        public static bool operator !=(MutRef<T> ALhs, T** ARhs) => ALhs.MutPtr != ARhs;

        public static implicit operator MutRef<T>([CanBeNull] T** AMutPtr)
            => new MutRef<T>(AMutPtr);
        [CanBeNull]
        public static implicit operator T** (MutRef<T> ARef) => ARef.MutPtr;
        #endregion
    }

    /// <summary>
    /// Helper class for the <see cref="MutRef{T}"/> struct.
    /// </summary>
    internal static class MutRef
    {
        /// <summary>
        /// Initialize a <see cref="MutRef{T}"/>.
        /// </summary>
        /// <typeparam name="T">The pointed-to type.</typeparam>
        /// <param name="AMutPtr">The pointer.</param>
        /// <returns>A <see cref="MutRef{T}"/> wrapping <paramref name="AMutPtr"/>.</returns>
        public static unsafe MutRef<T> Of<T>([CanBeNull] T** AMutPtr)
            where T : unmanaged
        {
            return new MutRef<T>(AMutPtr);
        }
    }
    // ReSharper restore errors
}
