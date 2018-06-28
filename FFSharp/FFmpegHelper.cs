using FFSharp.Native;

using JetBrains.Annotations;

namespace FFSharp
{
    /// <summary>
    /// Helpers for working with FFmpeg library functions.
    /// </summary>
    internal static class FFmpegHelper
    {
        /// <summary>
        /// Turn a C-style integer to a <see cref="bool"/>.
        /// </summary>
        /// <param name="ACBool">The C-style boolean value.</param>
        /// <returns>
        /// <see langword="false"/> if <paramref name="ACBool"/> is 0; otherwise
        /// <see langword="true"/>.
        /// </returns>
        public static bool ToBool(this int ACBool) => ACBool != 0;

        /// <summary>
        /// Check an FFmpeg error code-
        /// </summary>
        /// <param name="ACode">The error code.</param>
        /// <param name="AMessage">The custom error message, or <see langword="null"/>.</param>
        /// <returns>A <see cref="Result{T}"/> wrapping the result.</returns>
        /// <remarks>
        /// Indicates an appropriate error if <paramref name="ACode"/> is less than 0.
        /// </remarks>
        [MustUseReturnValue]
        public static Result CheckFFmpeg(this int ACode, [CanBeNull] string AMessage = null)
        {
			return ACode < 0
				? new Result(new FFmpegError(AMessage, ACode))
				: Result.Ok();
        }

        /// <summary>
        /// Check the result of an allocation.
        /// </summary>
        /// <typeparam name="T">The allocated struct type.</typeparam>
        /// <param name="ARef">The allocated <see cref="Fixed{T}"/>.</param>
        /// <returns>A <see cref="Result{T}"/> wrapping the result.</returns>
        /// <remarks>
        /// Indicates an error if <paramref name="ARef"/> is <see langword="null"/>.
        /// </remarks>
        // ReSharper disable errors
        [MustUseReturnValue]
        public static Result<Fixed<T>> CheckAlloc<T>(this Fixed<T> ARef)
            where T : unmanaged
        {
            return ARef.IsNull
                ? new Result<Fixed<T>>(new BadAllocationException(typeof(T)))
                : new Result<Fixed<T>>(ARef);
        }
        // ReSharper restore errors
        /// <summary>
        /// Check the result of an allocation.
        /// </summary>
        /// <typeparam name="T">The allocated struct type.</typeparam>
        /// <param name="ARef">The allocated <see cref="Movable{T}"/>.</param>
        /// <returns>A <see cref="Result{T}"/> wrapping the result.</returns>
        /// <remarks>
        /// Only indicates an error if the <see cref="Movable{T}"/> is present and it's
        /// <see cref="Movable{T}.Target"/> is <see langword="null"/>.
        /// </remarks>
        // ReSharper disable errors
        [MustUseReturnValue]
        public static Result<Movable<T>> CheckAlloc<T>(this Movable<T> ARef)
            where T : unmanaged
        {
            return !ARef.IsNull && ARef.Fixed.IsNull
                ? new Result<Movable<T>>(new BadAllocationException(typeof(T)))
                : new Result<Movable<T>>(ARef);
        }
        // ReSharper restore errors
    }
}