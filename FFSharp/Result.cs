using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using JetBrains.Annotations;

namespace FFSharp
{
    /// <summary>
    /// Safely wraps a successful result or an <see cref="Exception"/> instead.
    /// </summary>
    /// <remarks>
    /// Value-less variant of the <see cref="Result{T}"/> struct.
    /// </remarks>
    [PublicAPI]
    public readonly struct Result :
        IEquatable<Result>
    {
        const string C_Message = "An uninitialized result was returned.";
        /// <summary>
        /// Get the <see cref="Exception"/> that is contained in an uninitialized erroneous
        /// <see cref="Result"/>.
        /// </summary>
        public static Exception UninitializedError => new Exception(C_Message);

        #region Generic factories
        /// <summary>
        /// Initialize a successful <see cref="Result{T}"/>.
        /// </summary>
        /// <typeparam name="T">The successful value type.</typeparam>
        /// <param name="AValue">The success value.</param>
        /// <returns>
        /// A successful <see cref="Result{T}"/> wrapping <paramref name="AValue"/>.
        /// </returns>
        [Pure]
        public static Result<T> Ok<T>(T AValue)
        {
            return new Result<T>(AValue);
        }
        /// <summary>
        /// Initialize an erroneous <see cref="Result{T}"/>.
        /// </summary>
        /// <typeparam name="T">The successful value type.</typeparam>
        /// <param name="AError">The error.</param>
        /// <returns>
        /// An erroneous <see cref="Result{T}"/> wrapping <paramref name="AError"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="AError"/> is <see langword="null"/>.
        /// </exception>
        [Pure]
        public static Result<T> Fail<T>(Exception AError)
        {
            if (AError == null)
            {
                throw new ArgumentNullException(nameof(AError));
            }

            return new Result<T>(AError);
        }
        #endregion

        #region Value-less factories
        /// <summary>
        /// Initialize a successful <see cref="Result"/>.
        /// </summary>
        /// <returns>The successful <see cref="Result"/>.</returns>
        [Pure]
        public static Result Ok()
        {
            return new Result(true);
        }
        /// <summary>
        /// Initialize an erroneous <see cref="Result"/>.
        /// </summary>
        /// <param name="AError">The error.</param>
        /// <returns>
        /// An erroneous <see cref="Result"/> wrapping <paramref name="AError"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="AError"/> is <see langword="null"/>.
        /// </exception>
        [Pure]
        public static Result Fail(Exception AError)
        {
            if (AError == null)
            {
                throw new ArgumentNullException(nameof(AError));
            }

            return new Result(AError);
        }
        #endregion

        /// <summary>
        /// Get a value indicating whether this <see cref="Result{T}"/> is successful.
        /// </summary>
        public bool IsSuccess { get; }
        [CanBeNull]
        readonly Exception FError;

        /// <summary>
        /// Initialize a successful <see cref="Result"/>.
        /// </summary>
        /// <param name="AIgnored">Ignored.</param>
        internal Result(bool AIgnored)
        {
            IsSuccess = true;
            FError = default;
        }
        /// <summary>
        /// Initialize an erroneous <see cref="Result"/>.
        /// </summary>
        /// <param name="AError">The error.</param>
        internal Result([NotNull] Exception AError)
        {
            Debug.Assert(
                AError != null,
                "Error is null.",
                "This indicates a contract violation."
            );

            IsSuccess = false;
            FError = AError;
        }

        #region IEquatable<Result>
        /// <inheritdoc/>
        public bool Equals(Result AOther)
        {
            return IsSuccess && AOther.IsSuccess || Error == AOther.Error;
        }
        #endregion

        #region System.Object overrides
        /// <inheritdoc/>
        public override bool Equals(object AObject)
        {
            switch (AObject)
            {
            case Result result:
                return Equals(result);

            default:
                return false;
            }
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return IsSuccess ? 0 : 1;
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return IsSuccess
                ? $"<Error: {Error}>"
                : "<Success>";
        }
        #endregion

        /// <summary>
        /// Throw the contained <see cref="Exception"/> if this <see cref="Result"/> is
        /// erroneous.
        /// </summary>
        [DebuggerHidden]
        public void ThrowIfError()
        {
            if (!IsSuccess)
            {
                Debug.Assert(Error != null);
                throw Error;
            }
        }

        #region Convenience
        /// <summary>
        /// Execute an <see cref="Action"/> if the <see cref="Result"/> is successful.
        /// </summary>
        /// <param name="AAction">The <see cref="Action"/> to execute.</param>
        /// <returns>This <see cref="Result"/>.</returns>
        /// <remarks>
        /// Does not perform a <see langword="null"/>-check on <paramref name="AAction"/> and will
        /// fail if <paramref name="AAction"/> is <see langword="null"/> should it be executed.
        /// </remarks>
        public Result OnSuccess([InstantHandle] [NotNull] Action AAction)
        {
            if (IsSuccess)
            {
                AAction();
            }

            return this;
        }
        /// <summary>
        /// Execute an <see cref="Action"/> if the <see cref="Result"/> is erroneous.
        /// </summary>
        /// <param name="AAction">The <see cref="Action"/> to execute.</param>
        /// <returns>This <see cref="Result"/>.</returns>
        /// <remarks>
        /// Does not perform a <see langword="null"/>-check on <paramref name="AAction"/> and will
        /// fail if <paramref name="AAction"/> is <see langword="null"/> should it be executed.
        /// </remarks>
        public Result OnError([InstantHandle] [NotNull] Action<Exception> AAction)
        {
            if (!IsSuccess)
            {
                AAction(Error);
            }

            return this;
        }
        /// <summary>
        /// Compute a <see cref="Func{TResult}"/> if the <see cref="Result"/> is successful.
        /// </summary>
        /// <typeparam name="TResult">The result value type.</typeparam>
        /// <param name="AFunc">The <see cref="Func{TResult}"/> to apply.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> wrapping the result of executing <paramref name="AFunc"/>, or
        /// an erroneous <see cref="Result{T}"/> carrying the error of this <see cref="Result"/>.
        /// </returns>
        /// <remarks>
        /// Does not perform a <see langword="null"/>-check on <paramref name="AFunc"/> and will
        /// fail if <paramref name="AFunc"/> is <see langword="null"/> should it be executed.
        /// </remarks>
        [MustUseReturnValue]
        public Result<TResult> AndThen<TResult>([InstantHandle] [NotNull] Func<TResult> AFunc)
        {
            return IsSuccess
                ? new Result<TResult>(AFunc())
                : new Result<TResult>(FError ?? UninitializedError);
        }
        #endregion

        /// <summary>
        /// Get the <see cref="Exception"/> wrapped in this <see cref="Result"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="IsSuccess"/> is <see langword="true"/>, this will always be
        /// <see langword="null"/>, otherwise it is guaranteed to be non-<see langword="null"/>.
        /// </remarks>
        [CanBeNull]
        public Exception Error => IsSuccess ? null : FError ?? UninitializedError;

        #region Operator overloads
        /// <summary>
        /// Compare two <see cref="Result"/>s for equality.
        /// </summary>
        /// <param name="ALhs">The left hand side.</param>
        /// <param name="ARhs">The right hand side.</param>
        /// <returns>
        /// <see langword="true"/> if both are the same <see cref="IsSuccess"/> state; otherwise
        /// <see langword="false"/>.
        /// </returns>
        public static bool operator ==(Result ALhs, Result ARhs) => ALhs.Equals(ARhs);
        /// <summary>
        /// Compare two <see cref="Result"/>s for inequality.
        /// </summary>
        /// <param name="ALhs">The left hand side.</param>
        /// <param name="ARhs">The right hand side.</param>
        /// <returns>
        /// <see langword="true"/> if both have a different <see cref="IsSuccess"/> state; otherwise
        /// <see langword="false"/>.
        /// </returns>
        public static bool operator !=(Result ALhs, Result ARhs) => !ALhs.Equals(ARhs);

        /// <summary>
        /// Implicitly convert a <see cref="Result"/> to it's <see cref="IsSuccess"/>.
        /// </summary>
        /// <param name="AResult">The <see cref="Result"/>.</param>
        public static implicit operator bool(Result AResult) => AResult.IsSuccess;

        /// <summary>
        /// Implicitly initialize a successful <see cref="Result"/>.
        /// </summary>
        /// <param name="ASuccess">The success value.</param>
        public static implicit operator Result(bool ASuccess) => ASuccess ? Ok() : default;
        /// <summary>
        /// Implicitly initalize an erroneous <see cref="Result"/>.
        /// </summary>
        /// <param name="AError">The error.</param>
        public static implicit operator Result(Exception AError)
            => AError == null ? Ok() : new Result(AError);
        /// <summary>
        /// Implicitly convert a <see cref="Result"/> to it's <see cref="Error"/>.
        /// </summary>
        /// <param name="AResult">The <see cref="Result"/>.</param>
        public static implicit operator Exception(Result AResult) => AResult.Error;
        #endregion
    }

    /// <summary>
    /// Safely wraps a successful result value or an <see cref="Exception"/> instead.
    /// </summary>
    /// <typeparam name="T">The successful value type.</typeparam>
    /// <remarks>
    /// For ease of use, the type may not specify the error subclass, and may not use any
    /// <typeparamref name="T"/> which is an <see cref="Exception"/>!
    /// </remarks>
    [PublicAPI]
    public readonly struct Result<T> :
        IEnumerable<T>,
        IEquatable<T>
    {
        /// <summary>
        /// Get a value indicating whether this <see cref="Result{T}"/> is successful.
        /// </summary>
        public bool IsSuccess { get; }
        [CanBeNull]
        readonly T[] FBox;
        T Unbox
        {
            get
            {
                Debug.Assert(FBox != null);
                return FBox[0];
            }
        }
        [CanBeNull]
        readonly Exception FError;

        /// <summary>
        /// Initialize a successful <see cref="Result{T}"/>.
        /// </summary>
        /// <param name="ASuccess">The success value.</param>
        internal Result(T ASuccess)
        {
            IsSuccess = true;
            FBox = new[] {ASuccess};
            FError = default;
        }
        /// <summary>
        /// Initialize an erroneous <see cref="Result{T}"/>.
        /// </summary>
        /// <param name="AError">The error.</param>
        internal Result([NotNull] Exception AError)
        {
            Debug.Assert(
                AError != null,
                "Error is null.",
                "This indicates a contract violation."
            );

            IsSuccess = false;
            FBox = default;
            FError = AError;
        }

        #region IEnumerable<T>
        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            if (IsSuccess)
            {
                yield return Unbox;
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

        #region IEquatable<T>
        /// <inheritdoc/>
        public bool Equals(T AOther)
        {
            return IsSuccess && EqualityComparer<T>.Default.Equals(Unbox, AOther);
        }
        #endregion

        #region System.Object overrides
        /// <inheritdoc/>
        public override bool Equals(object AObject)
        {
            switch (AObject)
            {
                case T value:
                    return Equals(value);

                default:
                    return false;
            }
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return IsSuccess ? Unbox?.GetHashCode() ?? 0 : 0;
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return IsSuccess
                ? $"<Error: {Error}>"
                : $"<Success: {Unbox}>";
        }
        #endregion

        /// <summary>
        /// Throw the contained <see cref="Exception"/> if this <see cref="Result{T}"/> is
        /// erroneous.
        /// </summary>
        [DebuggerHidden]
        public void ThrowIfError()
        {
            if (!IsSuccess)
            {
                Debug.Assert(Error != null);
                throw Error;
            }
        }

        #region Convenience
        /// <summary>
        /// Get the contained value if successful or return a default.
        /// </summary>
        /// <param name="ADefault">The default value.</param>
        /// <returns>
        /// <see cref="Value"/> if successful; otherwise <paramref name="ADefault"/>.
        /// </returns>
        [Pure]
        public T Or(T ADefault)
        {
            return IsSuccess ? Unbox : ADefault;
        }
        /// <summary>
        /// Get this if successful or return a default.
        /// </summary>
        /// <param name="ADefault">The default <see cref="Result{T}"/>.</param>
        /// <returns>This if successful; otherwise <paramref name="ADefault"/>.</returns>
        [Pure]
        public Result<T> Or(Result<T> ADefault)
        {
            return IsSuccess ? this : ADefault;
        }

        /// <summary>
        /// Execute an <see cref="Action{T}"/> if the <see cref="Result{T}"/> is successful.
        /// </summary>
        /// <param name="AAction">The <see cref="Action{T}"/> to execute.</param>
        /// <returns>This <see cref="Result{T}"/>.</returns>
        /// <remarks>
        /// Does not perform a <see langword="null"/>-check on <paramref name="AAction"/> and will
        /// fail if <paramref name="AAction"/> is <see langword="null"/> should it be executed.
        /// </remarks>
        public Result<T> OnSuccess([InstantHandle] [NotNull] Action<T> AAction)
        {
            if (IsSuccess)
            {
                AAction(Unbox);
            }

            return this;
        }
        /// <summary>
        /// Execute an <see cref="Action"/> if the <see cref="Result{T}"/> is erroneous.
        /// </summary>
        /// <param name="AAction">The <see cref="Action"/> to execute.</param>
        /// <returns>This <see cref="Result{T}"/>.</returns>
        /// <remarks>
        /// Does not perform a <see langword="null"/>-check on <paramref name="AAction"/> and will
        /// fail if <paramref name="AAction"/> is <see langword="null"/> should it be executed.
        /// </remarks>
        public Result<T> OnError([InstantHandle] [NotNull] Action<Exception> AAction)
        {
            if (IsSuccess)
            {
                AAction(Error);
            }

            return this;
        }
        /// <summary>
        /// Apply a <see cref="Func{T, TResult}"/> if the <see cref="Result{T}"/> is successful.
        /// </summary>
        /// <typeparam name="TResult">The result value type.</typeparam>
        /// <param name="AFunc">The <see cref="Func{T, TResult}"/> to apply.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> wrapping the result of applying <paramref name="AFunc"/> to
        /// the successful value, or an erroneous <see cref="Result{T}"/> carrying the error of this
        /// <see cref="Result{T}"/>.
        /// </returns>
        /// <remarks>
        /// Does not perform a <see langword="null"/>-check on <paramref name="AFunc"/> and will
        /// fail if <paramref name="AFunc"/> is <see langword="null"/> should it be executed.
        /// </remarks>
        [MustUseReturnValue]
        public Result<TResult> AndThen<TResult>([InstantHandle] [NotNull] Func<T, TResult> AFunc)
        {
            return IsSuccess
                ? new Result<TResult>(AFunc(Unbox))
                : new Result<TResult>(FError ?? Result.UninitializedError);
        }
        #endregion

        /// <summary>
        /// Unpack the successful value.
        /// </summary>
        /// <remarks>
        /// Throws the contained <see cref="Error"/> if the <see cref="Result{T}"/> is erroneous.
        /// </remarks>
        public T Value
        {
            get
            {
                ThrowIfError();
                return Unbox;
            }
        }
        /// <summary>
        /// Get the <see cref="Exception"/> wrapped in this <see cref="Result{T}"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="IsSuccess"/> is <see langword="true"/>, this will always be
        /// <see langword="null"/>, otherwise it is guaranteed to be non-<see langword="null"/>.
        /// </remarks>
        [CanBeNull]
        public Exception Error => IsSuccess ? null : FError ?? Result.UninitializedError;

        #region Operator overloads
        /// <summary>
        /// Compare a <see cref="Result{T}"/> and a value for equality.
        /// </summary>
        /// <param name="ALhs">The left hand side.</param>
        /// <param name="ARhs">The right hand side.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="ALhs"/> is successful and it's
        /// <see cref="Value"/> is equal to <paramref name="ARhs"/>; otherwise
        /// <see langword="false"/>.
        /// </returns>
        public static bool operator ==(Result<T> ALhs, T ARhs) => ALhs.Equals(ARhs);
        /// <summary>
        /// Compare a <see cref="Result{T}"/> and a value for inequality.
        /// </summary>
        /// <param name="ALhs">The left hand side.</param>
        /// <param name="ARhs">The right hand side.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="ALhs"/> is erroneous or it's
        /// <see cref="Value"/> is inequal to <paramref name="ARhs"/>; otherwise
        /// <see langword="false"/>.
        /// </returns>
        public static bool operator !=(Result<T> ALhs, T ARhs) => !ALhs.Equals(ARhs);

        /// <summary>
        /// Implicitly convert a <see cref="Result{T}"/> to it's <see cref="IsSuccess"/>.
        /// </summary>
        /// <param name="AResult">The <see cref="Result{T}"/>.</param>
        public static implicit operator bool(Result<T> AResult) => AResult.IsSuccess;

        /// <summary>
        /// Implicitly initialize a successful <see cref="Result{T}"/>.
        /// </summary>
        /// <param name="ASuccess">The success value.</param>
        public static implicit operator Result<T>(T ASuccess) => Result.Ok(ASuccess);
        /// <summary>
        /// Implicitly initalize an erroneous <see cref="Result{T}"/>.
        /// </summary>
        /// <param name="AError">The error.</param>
        public static implicit operator Result<T>(Exception AError) => new Result<T>(AError);
        /// <summary>
        /// Explicitly unpack the <see cref="Value"/> of a <see cref="Result{T}"/>.
        /// </summary>
        /// <param name="AResult">The <see cref="Result{T}"/>.</param>
        public static explicit operator T(Result<T> AResult) => AResult.Value;
        /// <summary>
        /// Implicitly convert a <see cref="Result{T}"/> to it's <see cref="Error"/>.
        /// </summary>
        /// <param name="AResult">The <see cref="Result{T}"/>.</param>
        public static implicit operator Exception(Result<T> AResult) => AResult.Error;
        #endregion
    }
}
