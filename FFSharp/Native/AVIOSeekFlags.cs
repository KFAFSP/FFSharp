using System;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Native
{
    /// <summary>
    /// Flags for <see cref="Unsafe.ffmpeg.avio_seek(Unsafe.AVIOContext*, long, int)"/> operations.
    /// </summary>
    [Flags]
    internal enum AVIOSeekFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// Force seeking if possible (reopen, linear reading, ...).
        /// </summary>
        Force = Unsafe.ffmpeg.AVSEEK_FORCE,
        /// <summary>
        /// Instead of seeking, return the file size.
        /// </summary>
        Size = Unsafe.ffmpeg.AVSEEK_SIZE
    }
    // ReSharper restore errors
}
