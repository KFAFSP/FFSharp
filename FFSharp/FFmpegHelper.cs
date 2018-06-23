using System.Diagnostics;

using JetBrains.Annotations;

namespace FFSharp
{
    /// <summary>
    /// Helpers for working with FFmpeg library functions.
    /// </summary>
    internal static class FFmpegHelper
    {
        /// <summary>
        /// Check an FFmpeg error code and create na <see cref="FFmpegError"/> if appropriate.
        /// </summary>
        /// <param name="ACode">The error code.</param>
        /// <param name="AMessage">The custom message.</param>
        /// <returns>
        /// <see langword="null"/> if no error occured; otherwise an <see cref="FFmpegError"/>
        /// instance to throw.
        /// </returns>
        [MustUseReturnValue]
        public static FFmpegError CheckFFmpeg(this int ACode, [CanBeNull] string AMessage = null)
        {
            return ACode < 0 ? new FFmpegError(AMessage, ACode) : null;
        }

        /// <summary>
        /// Throw an <see cref="FFmpegError"/> if it is not <see langword="null"/>.
        /// </summary>
        /// <param name="AError">The <see cref="FFmpegError"/>.</param>
        /// <exception cref="FFmpegError">An error was present.</exception>
        [DebuggerHidden]
        public static void ThrowIfPresent(this FFmpegError AError)
        {
            if (AError != null)
            {
                throw AError;
            }
        }

        /// <summary>
        /// Check an allocated pointer and throw a <see cref="BadAllocationException"/> if
        /// <see langword="null"/>.
        /// </summary>
        /// <typeparam name="T">The allocated type.</typeparam>
        /// <param name="APtr">The allocated pointer.</param>
        /// <returns>The checked non-<see langword="null"/> pointer.</returns>
        // ReSharper disable errors
        [DebuggerHidden]
        [NotNull]
        [MustUseReturnValue]
        public static unsafe T* CheckAlloc<T>(T* APtr)
            where T : unmanaged
        {
            if (APtr == null)
            {
                throw new BadAllocationException(typeof(T));
            }

            return APtr;
        }
        // ReSharper restore errors
    }
}