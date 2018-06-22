using JetBrains.Annotations;

namespace FFSharp
{
    /// <summary>
    /// Helpers for working with FFmpeg library functions.
    /// </summary>
    internal static class FFmpegHelper
    {
        /// <summary>
        /// Check an FFmpeg error code and throw a <see cref="FFmpegError"/> if appropriate.
        /// </summary>
        /// <param name="ACode">The error code.</param>
        /// <exception cref="FFmpegError">Result code indicated an error.</exception>
        public static void CheckFFmpeg(this int ACode)
        {
            if (ACode < 0)
            {
                throw new FFmpegError(ACode);
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
        [NotNull]
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