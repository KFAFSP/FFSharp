using System;
using JetBrains.Annotations;

namespace FFSharp.Native
{
    /// <summary>
    /// Wrapper type for an unmanaged pointer.
    /// </summary>
    /// <typeparam name="T">The pointed-to type.</typeparam>
    /// <remarks>
    /// This type makes basic pointer queries like null, comparison and conversion to
    /// <see cref="IntPtr"/> available to safe code segments and introduces a common way of storing
    /// these references.
    /// </remarks>
    // ReSharper disable errors
    internal unsafe struct Ref<T> :
        IEquatable<Ref<T>>
        where T : unmanaged
    {
        /// <summary>
        /// Represents a <see cref="Ref{T}"/> to the <see langword="null"/> pointer.
        /// </summary>
        public static readonly Ref<T> Null = default;

        /// <summary>
        /// Initialize a <see cref="Ref{T}"/>.
        /// </summary>
        /// <param name="APtr">The underlying pointer.</param>
        public Ref(T* APtr)
        {
            Ptr = APtr;
        }

        #region IEquatable<Ref<T>>
        /// <inheritdoc />
        public bool Equals(Ref<T> ARef)
        {
            return Ptr == ARef.Ptr;
        }
        #endregion

        #region System.Object overrides
        /// <inheritdoc />
        public override bool Equals(object AOther)
        {
            switch (AOther)
            {
                case Ref<T> wrapped:
                    return Equals(wrapped);

                default:
                    return false;
            }
        }
        /// <inheritdoc />
        public override int GetHashCode() => Address.GetHashCode();
        #endregion

        /// <summary>
        /// Cast the reference to a different type.
        /// </summary>
        /// <typeparam name="U">The target type.</typeparam>
        /// <returns>A <see cref="Ref{T}"/> for the target type.</returns>
        /// <remarks>
        /// This cast cannot fail, but is not safe as it cannot be dynamically checked for
        /// correctness.
        /// </remarks>
        public Ref<U> Cast<U>()
            where U : unmanaged
        {
            return new Ref<U>((U*)Ptr);
        }

        /// <summary>
        /// Get the underlying pointer.
        /// </summary>
        [CanBeNull]
        public T* Ptr { get; }
        /// <summary>
        /// Get the address of the underlying pointer.
        /// </summary>
        public IntPtr Address => (IntPtr) Ptr;

        /// <summary>
        /// Get a value indicating whether this reference is <see cref="Null"/>.
        /// </summary>
        public bool IsNull => Ptr == null;

        #region Operator overloads
        /// <summary>
        /// Implicitly get the pointer of a <see cref="Ref{T}"/>.
        /// </summary>
        /// <param name="ARef">The <see cref="Ref{T}"/>.</param>
        public static implicit operator T*(Ref<T> ARef)
        {
            return ARef.Ptr;
        }
        /// <summary>
        /// Implicitly get a <see cref="Ref{T}"/> for a pointer.
        /// </summary>
        /// <param name="APtr">The pointer.</param>
        public static implicit operator Ref<T>(T* APtr)
        {
            return new Ref<T>(APtr);
        }

        /// <summary>
        /// Compare two <see cref="Ref{T}"/> for equality.
        /// </summary>
        /// <param name="ALhs">The left hand side.</param>
        /// <param name="ARhs">The right hand side.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="ALhs"/> and <paramref name="ARhs"/> point to the same
        /// object; otherwise <c>false</c>.
        /// </returns>
        public static bool operator ==(Ref<T> ALhs, Ref<T> ARhs)
        {
            return ALhs.Ptr == ARhs.Ptr;
        }
        /// <summary>
        /// Compare two <see cref="Ref{T}"/> for inequality.
        /// </summary>
        /// <param name="ALhs">The left hand side.</param>
        /// <param name="ARhs">The right hand side.</param>
        /// <returns>
        /// <c>false</c> if <paramref name="ALhs"/> and <paramref name="ARhs"/> point to the same
        /// object; otherwise <c>true</c>.
        /// </returns>
        public static bool operator !=(Ref<T> ALhs, Ref<T> ARhs)
        {
            return ALhs.Ptr != ARhs.Ptr;
        }
        #endregion
    }
    // ReSharper restore errors
}
