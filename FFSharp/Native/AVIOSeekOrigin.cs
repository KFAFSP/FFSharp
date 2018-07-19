using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Native
{
    /// <summary>
    /// Origin for <see cref="Unsafe.ffmpeg.avio_seek(Unsafe.AVIOContext*, long, int)"/> operations.
    /// </summary>
    internal enum AVIOSeekOrigin
    {
        /// <summary>
        /// Seek from the start of the file.
        /// </summary>
        Begin = 0,
        /// <summary>
        /// Seek from the current position in the file.
        /// </summary>
        Current = 1,
        /// <summary>
        /// Seek from the end of the file.
        /// </summary>
        End = 2
    }
    // ReSharper restore errors
}
