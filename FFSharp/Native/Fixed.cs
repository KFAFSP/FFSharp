using System;
using System.Diagnostics;

using JetBrains.Annotations;

namespace FFSharp.Native
{
    /// <summary>
    /// Value type wrapping a native pointer.
    /// </summary>
    /// <remarks>
    /// Use this instead of a <c>void*</c> pointer to better represent the intention and to
    /// statically check contracts. Allows passing pointers through safe contexts.
    /// </remarks>
    internal readonly unsafe struct Fixed :
        IEquatable<Fixed>
        // cannot implement IEquatable<void*>
    {
        /// <summary>
        /// The <see langword="null"/> pointer <see cref="Fixed"/>.
        /// </summary>
        public static readonly Fixed Null = default;

        /// <summary>
        /// Initialize a <see cref="Fixed"/> with a pointer.
        /// </summary>
        /// <param name="ARaw">The target pointer.</param>
        public Fixed([CanBeNull] void* ARaw)
        {
            Raw = ARaw;
        }

        /// <summary>
        /// Get the non-<see langword="null"/> value of this or a default.
        /// </summary>
        /// <param name="ADefault">The default <see cref="Fixed"/>.</param>
        /// <returns>
        /// This if <see cref="IsNull"/> is <see langword="false"/>; otherwise
        /// <paramref name="ADefault"/>.
        /// </returns>
        [Pure]
        public Fixed Or(Fixed ADefault) => !IsNull ? this : ADefault;
        /// <summary>
        /// Cast to a different pointed-to struct type.
        /// </summary>
        /// <typeparam name="TTo">The struct type to cast to.</typeparam>
        /// <returns>A casted <see cref="Fixed{T}"/>.</returns>
        /// <remarks>
        /// This cast can never fail, but cannot be checked for semantic correctness. Use only when
        /// dynamical correctness is known.
        /// </remarks>
        [Pure]
        public Fixed<TTo> Cast<TTo>() where TTo : unmanaged => (TTo*)Raw;

        #region IEquatable<Fixed>
        /// <inheritdoc />
        [Pure]
        public bool Equals(Fixed AFixed) => Raw == AFixed.Raw;
        #endregion

        /// <summary>
        /// Check whether this <see cref="Fixed"/> is equal to the specified pointer.
        /// </summary>
        /// <param name="APtr">The pointer.</param>
        /// <returns>
        /// <see langword="true"/> if <see cref="Raw"/> is equal to <paramref name="APtr"/>;
        /// otherwise <see langword="false"/>.
        /// </returns>
        [Pure]
        public bool Equals(void* APtr) => Raw == APtr;

        #region System.Object overrides
        /// <inheritdoc />
        public override bool Equals(object AObject)
        {
            switch (AObject)
            {
                case Fixed @fixed:
                    return Equals(@fixed);

                default:
                    return false;
            }
        }
        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Address.GetHashCode();
        }
        /// <inheritdoc />
        public override string ToString()
        {
            return $"Fixed<void>(0x{Address.ToUInt64():X16})";
        }
        #endregion

        /// <summary>
        /// Get the underlying pointer.
        /// </summary>
        [CanBeNull]
        public void* Raw { get; }
        /// <summary>
        /// Get the address of <see cref="Raw"/> as an <see cref="IntPtr"/>.
        /// </summary>
        public IntPtr Address => (IntPtr)Raw;

        /// <summary>
        /// Get a value indicating whether <see cref="Raw"/> is <see langword="null"/>.
        /// </summary>
        public bool IsNull => Raw == null;

        /// <summary>
        /// Check pointer equality for two <see cref="Fixed"/> structs.
        /// </summary>
        /// <param name="ALhs">The left hand side.</param>
        /// <param name="ARhs">The right hand side.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="ALhs"/> and <paramref name="ARhs"/> point
        /// to the same struct; otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator ==(Fixed ALhs, Fixed ARhs) => ALhs.Equals(ARhs);
        /// <summary>
        /// Check pointer inequality for two <see cref="Fixed"/> structs.
        /// </summary>
        /// <param name="ALhs">The left hand side.</param>
        /// <param name="ARhs">The right hand side.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="ALhs"/> and <paramref name="ARhs"/> point
        /// to a different struct; otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator !=(Fixed ALhs, Fixed ARhs) => !ALhs.Equals(ARhs);

        /// <summary>
        /// Check pointer equality for a <see cref="Fixed"/> struct and a pointer.
        /// </summary>
        /// <param name="ALhs">The left hand side.</param>
        /// <param name="ARhs">The right hand side.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="ALhs"/> and <paramref name="ARhs"/> point
        /// to the same struct; otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator ==(Fixed ALhs, [CanBeNull] void* ARhs) => ALhs.Equals(ARhs);
        /// <summary>
        /// Check pointer inequality for a <see cref="Fixed"/> struct and a pointer.
        /// </summary>
        /// <param name="ALhs">The left hand side.</param>
        /// <param name="ARhs">The right hand side.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="ALhs"/> and <paramref name="ARhs"/> point
        /// to a different struct; otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator !=(Fixed ALhs, [CanBeNull] void* ARhs) => !ALhs.Equals(ARhs);

        /// <summary>
        /// Implicitly convert a <see cref="Fixed"/> to it's !<see cref="IsNull"/>.
        /// </summary>
        /// <param name="AFixed">The <see cref="Fixed"/>.</param>
        public static implicit operator bool(Fixed AFixed) => !AFixed.IsNull;

        /// <summary>
        /// Implicitly convert a pointer to a <see cref="Fixed"/>.
        /// </summary>
        /// <param name="APtr">The pointer.</param>
        public static implicit operator Fixed([CanBeNull] void* APtr) => new Fixed(APtr);
        /// <summary>
        /// Implicitly convert a <see cref="Fixed"/> to a pointer.
        /// </summary>
        /// <param name="AFixed">The <see cref="Fixed"/>.</param>
        [CanBeNull]
        public static implicit operator void* (Fixed AFixed) => AFixed.Raw;

        /// <summary>
        /// Implicitly convert an <see cref="IntPtr"/> to a <see cref="Fixed"/>.
        /// </summary>
        /// <param name="AAddress">The address.</param>
        public static implicit operator Fixed(IntPtr AAddress) => new Fixed((void*)AAddress);
        /// <summary>
        /// Implicitly convert a <see cref="Fixed"/> to it's <see cref="Address"/>.
        /// </summary>
        /// <param name="AFixed">The <see cref="Fixed"/>.</param>
        public static implicit operator IntPtr(Fixed AFixed) => AFixed.Address;
    }

    /// <summary>
    /// Value type wrapping a pointer to a native struct.
    /// </summary>
    /// <typeparam name="T">The pointed-to struct type.</typeparam>
    /// <remarks>
    /// Use this instead of a <c>T*</c> pointer to better represent the intention and to statically
    /// check contracts. Allows passing pointers through safe contexts.
    /// </remarks>
    // ReSharper disable errors
    internal readonly unsafe struct Fixed<T> :
        IEquatable<Fixed<T>>,
        IEquatable<Fixed>
        // cannot implement IEquatable<T*>
        where T : unmanaged
    {
        /// <summary>
        /// The <see langword="null"/> pointer <see cref="Fixed{T}"/>.
        /// </summary>
        public static readonly Fixed<T> Null = default;

        /// <summary>
        /// Initialize a <see cref="Fixed{T}"/> with a pointer.
        /// </summary>
        /// <param name="ARaw">The target pointer.</param>
        public Fixed([CanBeNull] T* ARaw)
        {
            Raw = ARaw;
        }

        /// <summary>
        /// Get the non-<see langword="null"/> value of this or a default.
        /// </summary>
        /// <param name="ADefault">The default <see cref="Fixed{T}"/>.</param>
        /// <returns>
        /// This if <see cref="IsNull"/> is <see langword="false"/>; otherwise
        /// <paramref name="ADefault"/>.
        /// </returns>
        [Pure]
        public Fixed<T> Or(Fixed<T> ADefault) => !IsNull ? this : ADefault;
        /// <summary>
        /// Cast to a different pointed-to struct type.
        /// </summary>
        /// <typeparam name="TTo">The struct type to cast to.</typeparam>
        /// <returns>A casted <see cref="Fixed{T}"/>.</returns>
        /// <remarks>
        /// This cast can never fail, but cannot be checked for semantic correctness. Use only when
        /// dynamical correctness is known.
        /// </remarks>
        [Pure]
        public Fixed<TTo> Cast<TTo>() where TTo : unmanaged => (TTo*)Raw;

        #region IEquatable<Fixed<T>>
        /// <inheritdoc />
        [Pure]
        public bool Equals(Fixed<T> AFixed) => Raw == AFixed.Raw;
        #endregion

        #region IEquatable<Fixed>
        /// <inheritdoc />
        [Pure]
        public bool Equals(Fixed AFixed) => Raw == AFixed.Raw;
        #endregion

        /// <summary>
        /// Check whether this <see cref="Fixed{T}"/> is equal to the specified pointer.
        /// </summary>
        /// <param name="APtr">The pointer.</param>
        /// <returns>
        /// <see langword="true"/> if <see cref="Raw"/> is equal to <paramref name="APtr"/>;
        /// otherwise <see langword="false"/>.
        /// </returns>
        [Pure]
        public bool Equals(T* APtr) => Raw == APtr;

        #region System.Object overrides
        /// <inheritdoc />
        public override bool Equals(object AObject)
        {
            switch (AObject)
            {
                case Fixed<T> @fixedT:
                    return Equals(@fixedT);

                case Fixed @fixed:
                    return Equals(@fixed);

                default:
                    return false;
            }
        }
        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Address.GetHashCode();
        }
        /// <inheritdoc />
        public override string ToString()
        {
            return $"Fixed<{typeof(T).Name}>(0x{Address.ToUInt64():X16})";
        }
        #endregion

        /// <summary>
        /// Get the underlying pointer.
        /// </summary>
        [CanBeNull]
        public T* Raw { get; }
        /// <summary>
        /// Get the address of <see cref="Raw"/> as an <see cref="IntPtr"/>.
        /// </summary>
        public IntPtr Address => (IntPtr)Raw;

        /// <summary>
        /// Get a value indicating whether <see cref="Raw"/> is <see langword="null"/>.
        /// </summary>
        public bool IsNull => Raw == null;

        /// <summary>
        /// Get a modifiable reference to the pointed-to struct.
        /// </summary>
        /// <remarks>
        /// Getting this property when <see cref="IsNull"/> is <see langword="true"/> results in
        /// undefined behaviour!
        /// </remarks>
        public ref T AsRef
        {
            get
            {
                Debug.Assert(
                    !IsNull,
                    "Fixed is null.",
                    "This indicates a severe logic error in the code."
                );

                return ref *Raw;
            }
        }

        /// <summary>
        /// Check pointer equality for two <see cref="Fixed{T}"/> structs.
        /// </summary>
        /// <param name="ALhs">The left hand side.</param>
        /// <param name="ARhs">The right hand side.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="ALhs"/> and <paramref name="ARhs"/> point
        /// to the same struct; otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator ==(Fixed<T> ALhs, Fixed<T> ARhs) => ALhs.Equals(ARhs);
        /// <summary>
        /// Check pointer inequality for two <see cref="Fixed{T}"/> structs.
        /// </summary>
        /// <param name="ALhs">The left hand side.</param>
        /// <param name="ARhs">The right hand side.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="ALhs"/> and <paramref name="ARhs"/> point
        /// to a different struct; otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator !=(Fixed<T> ALhs, Fixed<T> ARhs) => !ALhs.Equals(ARhs);

        /// <summary>
        /// Check pointer equality for a <see cref="Fixed{T}"/> and a <see cref="Fixed"/> struct.
        /// </summary>
        /// <param name="ALhs">The left hand side.</param>
        /// <param name="ARhs">The right hand side.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="ALhs"/> and <paramref name="ARhs"/> point
        /// to the same struct; otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator ==(Fixed<T> ALhs, Fixed ARhs) => ALhs.Equals(ARhs);
        /// <summary>
        /// Check pointer inequality for a <see cref="Fixed{T}"/> and a <see cref="Fixed"/> struct.
        /// </summary>
        /// <param name="ALhs">The left hand side.</param>
        /// <param name="ARhs">The right hand side.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="ALhs"/> and <paramref name="ARhs"/> point
        /// to a different struct; otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator !=(Fixed<T> ALhs, Fixed ARhs) => !ALhs.Equals(ARhs);

        /// <summary>
        /// Check pointer equality for a <see cref="Fixed{T}"/> struct and a pointer.
        /// </summary>
        /// <param name="ALhs">The left hand side.</param>
        /// <param name="ARhs">The right hand side.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="ALhs"/> and <paramref name="ARhs"/> point
        /// to the same struct; otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator ==(Fixed<T> ALhs, [CanBeNull] T* ARhs) => ALhs.Equals(ARhs);
        /// <summary>
        /// Check pointer inequality for a <see cref="Fixed{T}"/> struct and a pointer.
        /// </summary>
        /// <param name="ALhs">The left hand side.</param>
        /// <param name="ARhs">The right hand side.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="ALhs"/> and <paramref name="ARhs"/> point
        /// to a different struct; otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator !=(Fixed<T> ALhs, [CanBeNull] T* ARhs) => !ALhs.Equals(ARhs);

        /// <summary>
        /// Implicitly convert a <see cref="Fixed{T}"/> to it's !<see cref="IsNull"/>.
        /// </summary>
        /// <param name="AFixed">The <see cref="Fixed{T}"/>.</param>
        public static implicit operator bool(Fixed<T> AFixed) => !AFixed.IsNull;

        /// <summary>
        /// Implicitly convert a <see cref="Fixed{T}"/> to an untyped <see cref="Fixed"/>.
        /// </summary>
        /// <param name="AFixed">The <see cref="Fixed{T}"/>.</param>
        public static implicit operator Fixed(Fixed<T> AFixed) => new Fixed(AFixed.Raw);

        /// <summary>
        /// Implicitly convert a pointer to a <see cref="Fixed{T}"/>.
        /// </summary>
        /// <param name="APtr">The pointer.</param>
        public static implicit operator Fixed<T>([CanBeNull] T* APtr) => new Fixed<T>(APtr);
        /// <summary>
        /// Implicitly convert a <see cref="Fixed{T}"/> to a pointer.
        /// </summary>
        /// <param name="AFixed">The <see cref="Fixed{T}"/>.</param>
        [CanBeNull]
        public static implicit operator T*(Fixed<T> AFixed) => AFixed.Raw;

        /// <summary>
        /// Implicitly convert an <see cref="IntPtr"/> to a <see cref="Fixed{T}"/>.
        /// </summary>
        /// <param name="AAddress">The address.</param>
        public static implicit operator Fixed<T>(IntPtr AAddress) => new Fixed<T>((T*)AAddress);
        /// <summary>
        /// Implicitly convert a <see cref="Fixed{T}"/> to it's <see cref="Address"/>.
        /// </summary>
        /// <param name="AFixed">The <see cref="Fixed{T}"/>.</param>
        public static implicit operator IntPtr(Fixed<T> AFixed) => AFixed.Address;
    }
    // ReSharper restore errors
}
