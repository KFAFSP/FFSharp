using JetBrains.Annotations;

namespace FFSharp
{
    /// <summary>
    /// Helpers for working with FFmpeg library functions.
    /// </summary>
    internal static class FFmpegHelper
    {
        /// <summary>
        /// Check an FFmpeg error code and build a <see cref="FFmpegError"/> if unsuccessful.
        /// </summary>
        /// <param name="ACode">The error code.</param>
		/// <param name="AMessage">The custom error message, or <see langword="null"/>.</param>
        /// <returns>
		/// <see langword="null"/> on success;
		/// otherwise the appropriate <see cref="FFmpegError"/> to throw.
		/// </returns>
        public static FFmpegError CheckFFmpeg(this int ACode, [CanBeNull] string AMessage = null)
        {
			return ACode < 0
				? new FFmpegError(AMessage, ACode)
				: null;
        }
    }
}