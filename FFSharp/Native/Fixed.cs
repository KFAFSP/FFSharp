using System;
using System.Diagnostics;

using JetBrains.Annotations;

namespace FFSharp.Native
{
    /// <summary>
    /// Non-generic helper class for the <see cref="Fixed{T}"/> struct.
    /// </summary>
    // ReSharper disable errors
    internal static class Fixed
    {
        /// <summary>
        /// Initialize a <see langword="null"/> <see cref="Fixed{T}"/>.
        /// </summary>
        /// <typeparam name="T">The pointed-tp type.</typeparam>
        /// <returns>A <see langword="null"/> <see cref="Fixed{T}"/>.</returns>
        public static Fixed<T> Null<T>() where T : unmanaged => default;

        /// <summary>
        /// Initialize a <see cref="Fixed{T}"/>.
        /// </summary>
        /// <typeparam name="T">The pointed-to type.</typeparam>
        /// <param name="APtr">The pointer.</param>
        /// <returns>A <see cref="Fixed{T}"/> wrapping <paramref name="APtr"/>.</returns>
        public static unsafe Fixed<T> Of<T>([CanBeNull] T* APtr)
            where T : unmanaged
        {
            return new Fixed<T>(APtr);
        }
    }
    // ReSharper restore errors

    /// <summary>
    /// Wraps a fixed pointer to a native struct.
    /// </summary>
    /// <typeparam name="T">The pointed-to struct type.</typeparam>
    /// <remarks>
    /// Use this instead of a <c>T*</c> pointer to better represent the intention and to statically
    /// check contracts.
    /// </remarks>
    // ReSharper disable errors
    internal readonly unsafe struct Fixed<T> :
        IEquatable<Fixed<T>>
        where T : unmanaged
    {
        /// <summary>
        /// The <see langword="null"/> pointer <see cref="Fixed{T}"/>.
        /// </summary>
        public static readonly Fixed<T> Null = default;

        /// <summary>
        /// Initialize a <see cref="Fixed{T}"/>.
        /// </summary>
        /// <param name="ARaw">The target pointer.</param>
        public Fixed([CanBeNull] T* ARaw)
        {
            Raw = ARaw;
        }

        #region IEquatable<Fixed<T>>
        /// <inheritdoc/>
        public bool Equals(Fixed<T> AFixed)
        {
            return Raw == AFixed.Raw;
        }
        #endregion

        #region System.Object overrides
        /// <inheritdoc/>
        public override bool Equals(object AObject)
        {
            switch (AObject)
            {
                case Fixed<T> @fixed:
                    return Equals(@fixed);

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
            return $"<Fixed:0x{Address.ToInt64():X16}>";
        }
        #endregion

        #region Convenience
        /// <summary>
        /// Get the non-<see langword="null"/> value of this or a default.
        /// </summary>
        /// <param name="ADefault">The default <see cref="Fixed{T}"/>.</param>
        /// <returns>
        /// This if <see cref="IsNull"/> is <see langword="false"/>; otherwise
        /// <paramref name="ADefault"/>.
        /// </returns>
        [Pure]
        public Fixed<T> Or(Fixed<T> ADefault)
        {
            return !IsNull ? this : ADefault;
        }
        /// <summary>
        /// Cast to a different underlying struct type.
        /// </summary>
        /// <typeparam name="U">The struct type to cast to.</typeparam>
        /// <returns>A casted <see cref="Fixed{T}"/>.</returns>
        /// <remarks>
        /// This cast can never fail, but cannot be checked for semantic correctness. Use only when
        /// dynamical correctness is known.
        /// </remarks>
        [Pure]
        public Fixed<U> Cast<U>()
            where U : unmanaged
        {
            return (U*)Raw;
        }
        #endregion

        /// <summary>
        /// Get the pointer to the struct.
        /// </summary>
        [CanBeNull]
        public T* Raw { get; }
        /// <summary>
        /// Get the address of <see cref="Raw"/> as an <see cref="IntPtr"/>.
        /// </summary>
        public IntPtr Address => (IntPtr)Raw;

        #region Assertions
        /// <summary>
        /// Assert that <see cref="Raw"/> is not <see langword="null"/>.
        /// </summary>
        [DebuggerHidden]
        [Conditional("DEBUG")]
        public void AssertNotNull()
        {
            Debug.Assert(
                !IsNull,
                "Fixed is null.",
                "This indicates a severe logic error in the code."
            );
        }
        #endregion

        /// <summary>
        /// Get a value indicating whether <see cref="Raw"/> is <see langword="null"/>.
        /// </summary>
        public bool IsNull => Raw == null;

        /// <summary>
        /// Get the pointed-to struct as a C# reference.
        /// </summary>
        /// <remarks>
        /// Only access this property if <see cref="IsNull"/> is <see langword="false"/>!
        /// </remarks>
        public ref T AsRef
        {
            get
            {
                AssertNotNull();
                return ref *Raw;
            }
        }

        #region Operator overloads
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
        /// Check pointer equality for a <see cref="Fixed{T}"/> struct.
        /// </summary>
        /// <param name="ALhs">The left hand side.</param>
        /// <param name="ARhs">The right hand side.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="ALhs"/> and <paramref name="ARhs"/> point
        /// to the same struct; otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator ==(Fixed<T> ALhs, [CanBeNull] T* ARhs) => ALhs.Raw == ARhs;
        /// <summary>
        /// Check pointer inequality for a <see cref="Fixed{T}"/> struct.
        /// </summary>
        /// <param name="ALhs">The left hand side.</param>
        /// <param name="ARhs">The right hand side.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="ALhs"/> and <paramref name="ARhs"/> point
        /// to a different struct; otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator !=(Fixed<T> ALhs, [CanBeNull] T* ARhs) => ALhs.Raw != ARhs;

        /// <summary>
        /// Implicitly convert a <see cref="Fixed{T}"/> to it's !<see cref="IsNull"/>.
        /// </summary>
        /// <param name="AFixed">The <see cref="Fixed{T}"/>.</param>
        public static implicit operator bool(Fixed<T> AFixed) => !AFixed.IsNull;

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
        #endregion
    }
    // ReSharper restore errors
}
