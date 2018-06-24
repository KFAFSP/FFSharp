using System;
using System.Diagnostics;

using JetBrains.Annotations;

namespace FFSharp.Native
{
    /// <summary>
    /// Non-generic helper class for the <see cref="Movable{T}"/> struct.
    /// </summary>
    // ReSharper disable errors
    internal static class Movable
    {
        /// <summary>
        /// Initialize an absent <see cref="Movable{T}"/>.
        /// </summary>
        /// <typeparam name="T">The pointed-tp type.</typeparam>
        /// <returns>An absent <see cref="Movable{T}"/>.</returns>
        public static Movable<T> Absent<T>() where T : unmanaged => default;

        /// <summary>
        /// Initialize a <see cref="Movable{T}"/>.
        /// </summary>
        /// <typeparam name="T">The pointed-to type.</typeparam>
        /// <param name="APtr">The pointer.</param>
        /// <returns>A <see cref="Movable{T}"/> wrapping <paramref name="APtr"/>.</returns>
        public static unsafe Movable<T> Of<T>([CanBeNull] T** APtr)
            where T : unmanaged
        {
            return new Movable<T>(APtr);
        }
    }
    // ReSharper restore errors

    /// <summary>
    /// Wraps a relocatable pointer to a native struct.
    /// </summary>
    /// <typeparam name="T">The pointed-to struct type.</typeparam>
    /// <remarks>
    /// Use this instead of a <c>T**</c> pointer to better represent the intention and to statically
    /// check contracts.
    /// </remarks>
    // ReSharper disable errors
    internal unsafe struct Movable<T> :
        IEquatable<Movable<T>>
        where T : unmanaged
    {
        /// <summary>
        /// The absent <see cref="Movable{T}"/>.
        /// </summary>
        public static readonly Movable<T> Absent = default;

        /// <summary>
        /// Initialize a <see cref="Movable{T}"/>.
        /// </summary>
        /// <param name="ARaw">The target pointer pointer.</param>
        public Movable([CanBeNull] T** ARaw)
        {
            Raw = ARaw;
        }

        #region IEquatable<Movable<T>>
        /// <inheritdoc/>
        public bool Equals(Movable<T> ARef)
        {
            return Raw == ARef.Raw;
        }
        #endregion

        #region System.Object overrides
        /// <inheritdoc/>
        public override bool Equals(object AObject)
        {
            switch (AObject)
            {
                case Movable<T> movable:
                    return Equals(movable);

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
            var target = !IsAbsent
                ? $" -> 0x{(IntPtr)(*Raw)}"
                : "";

            return $"<Movable:0x{Address.ToInt64():X16}{target}>";
        }
        #endregion

        #region Convenience
        /// <summary>
        /// Get the present value of this or a default.
        /// </summary>
        /// <param name="ADefault">The default <see cref="Movable{T}"/>.</param>
        /// <returns>
        /// This if <see cref="IsAbsent"/> is <see langword="false"/>; otherwise
        /// <paramref name="ADefault"/>.
        /// </returns>
        [Pure]
        public Movable<T> Or(Movable<T> ADefault)
        {
            return !IsAbsent ? this : ADefault;
        }
        /// <summary>
        /// Cast to a different underlying struct type.
        /// </summary>
        /// <typeparam name="U">The struct type to cast to.</typeparam>
        /// <returns>A casted <see cref="Movable{T}"/>.</returns>
        /// <remarks>
        /// This cast can never fail, but cannot be checked for semantic correctness. Use only when
        /// dynamical correctness is known.
        /// </remarks>
        [Pure]
        public Movable<U> Cast<U>()
            where U : unmanaged
        {
            return (U**)Raw;
        }
        #endregion

        /// <summary>
        /// Get the pointer to the pointer to the struct.
        /// </summary>
        [CanBeNull]
        public T** Raw { get; }
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
        public void AssertPresent()
        {
            Debug.Assert(
                !IsAbsent,
                "Movable is absent.",
                "This indicates a severe logic error in the code."
            );
        }
        /// <summary>
        /// Assert that <see cref="Raw"/> and <see cref="Target"/> are not <see langword="null"/>.
        /// </summary>
        [DebuggerHidden]
        [Conditional("DEBUG")]
        public void AssertNotNull()
        {
            Debug.Assert(
                IsNotNull,
                "Movable target is null.",
                "This indicates a severe logic error in the code."
            );
        }
        #endregion

        /// <summary>
        /// Get or set the target pointer to the struct.
        /// </summary>
        /// <remarks>
        /// This property is only safe to use if <see cref="IsAbsent"/> is <see langword="false"/>.
        /// </remarks>
        [CanBeNull]
        public T* Target
        {
            get
            {
                AssertPresent();
                return *Raw;
            }
            set
            {
                AssertPresent();
                *Raw = value;
            }
        }

        /// <summary>
        /// Get a value indicating whether <see cref="Raw"/> is <see langword="null"/>.
        /// </summary>
        public bool IsAbsent => Raw == null;
        /// <summary>
        /// Get a value indicating whether the target pointer is not null.
        /// </summary>
        /// <remarks>
        /// Only <see langword="true"/> if not <see cref="IsAbsent"/> and <see cref="Target"/> not
        /// <see langword="null"/>.
        /// </remarks>
        public bool IsNotNull => IsAbsent && Target != null;

        /// <summary>
        /// Get or set the target pointer as a <see cref="Fixed{T}"/>.
        /// </summary>
        /// <remarks>
        /// This property is only safe to use if <see cref="IsAbsent"/> is <see langword="false"/>.
        /// </remarks>
        public Fixed<T> AsFixed
        {
            get => new Fixed<T>(Target);
            set => Target = value.Raw;
        }

        #region Operator overloads
        /// <summary>
        /// Check pointer equality for two <see cref="Movable{T}"/> structs.
        /// </summary>
        /// <param name="ALhs">The left hand side.</param>
        /// <param name="ARhs">The right hand side.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="ALhs"/> and <paramref name="ARhs"/> point
        /// to the same target pointer; otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator ==(Movable<T> ALhs, Movable<T> ARhs) => ALhs.Equals(ARhs);
        /// <summary>
        /// Check pointer inequality for two <see cref="Movable{T}"/> structs.
        /// </summary>
        /// <param name="ALhs">The left hand side.</param>
        /// <param name="ARhs">The right hand side.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="ALhs"/> and <paramref name="ARhs"/> point
        /// to a different target pointer; otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator !=(Movable<T> ALhs, Movable<T> ARhs) => !ALhs.Equals(ARhs);

        /// <summary>
        /// Check pointer equality for a <see cref="Movable{T}"/> struct and a pointer.
        /// </summary>
        /// <param name="ALhs">The left hand side.</param>
        /// <param name="ARhs">The right hand side.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="ALhs"/> and <paramref name="ARhs"/> point
        /// to the same target pointer; otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator ==(Movable<T> ALhs, [CanBeNull] T** ARhs) => ALhs.Raw == ARhs;
        /// <summary>
        /// Check pointer inequality for a <see cref="Movable{T}"/> struct and a pointer.
        /// </summary>
        /// <param name="ALhs">The left hand side.</param>
        /// <param name="ARhs">The right hand side.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="ALhs"/> and <paramref name="ARhs"/> point
        /// to a different target pointer; otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator !=(Movable<T> ALhs, [CanBeNull] T** ARhs) => ALhs.Raw != ARhs;

        /// <summary>
        /// Implicitly convert a <see cref="Movable{T}"/> to it's !<see cref="IsAbsent"/>.
        /// </summary>
        /// <param name="AFixed">The <see cref="Movable{T}"/>.</param>
        public static implicit operator bool(Movable<T> AFixed) => !AFixed.IsAbsent;

        /// <summary>
        /// Implicitly convert a pointer to a <see cref="Movable{T}"/>.
        /// </summary>
        /// <param name="APtr">The pointer.</param>
        public static implicit operator Movable<T>([CanBeNull] T** APtr) => new Movable<T>(APtr);
        /// <summary>
        /// Implicitly convert a <see cref="Movable{T}"/> to a pointer.
        /// </summary>
        /// <param name="AMovable">The <see cref="Movable{T}"/>.</param>
        [CanBeNull]
        public static implicit operator T** (Movable<T> AMovable) => AMovable.Raw;
        #endregion
    }
    // ReSharper restore errors
}
