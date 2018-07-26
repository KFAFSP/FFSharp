using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

using Whetstone.Contracts;

namespace FFSharp
{
    /// <summary>
    /// Helpers for working with FFmpeg library functions.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class FFmpegHelper
    {
        /// <summary>
        /// Turn a C-style boolean to a <see cref="bool"/>.
        /// </summary>
        /// <param name="ACBool">The C-style boolean value.</param>
        /// <returns>
        /// <see langword="false"/> if <paramref name="ACBool"/> is 0; otherwise
        /// <see langword="true"/>.
        /// </returns>
        [Pure]
        public static bool ToBool(this int ACBool) => ACBool != 0;

        /// <summary>
        /// Turn a <see cref="bool"/> into a C-style boolean.
        /// </summary>
        /// <param name="ABool"></param>
        /// <returns>1 if <paramref name="ABool"/> is <see langword="true"/>; otherwise 0.</returns>
        [Pure]
        public static int ToCBool(this bool ABool) => ABool ? 1 : 0;

        /// <summary>
        /// Check an FFmpeg result code.
        /// </summary>
        /// <param name="ACode">The result code.</param>
        /// <param name="AMessage">An optional custom error message.</param>
        /// <returns>A <see cref="Result{T}"/> wrapping the result.</returns>
        /// <remarks>
        /// Wraps an appropriate error if <paramref name="ACode"/> is less than 0.
        /// </remarks>
        [MustUseReturnValue]
        public static Result<uint> ToResultUInt(this int ACode, [CanBeNull] string AMessage = null)
        {
			return ACode < 0
				? new Result<uint>(new FFmpegError(AMessage, ACode))
				: Result.Ok((uint)ACode);
        }

        /// <summary>
        /// Check an FFmpeg result code discarding success values.
        /// </summary>
        /// <param name="ACode">The result code.</param>
        /// <param name="AMessage">An optional custom error message.</param>
        /// <returns>A <see cref="Result"/> of the operation.</returns>
        /// <remarks>
        /// Wraps an appropriate error if <paramref name="ACode"/> is less than 0.
        /// </remarks
        [MustUseReturnValue]
        public static Result ToResult(this int ACode, [CanBeNull] string AMessage = null)
        {
            var result = ACode.ToResultUInt();
            return result.IsSuccess
                ? Result.Ok()
                : new Result(result.Error);
        }
    }
}