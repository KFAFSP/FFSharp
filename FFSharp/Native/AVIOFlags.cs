using System;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Native
{
    /// <summary>
    /// Flags for the <see cref="Unsafe.AVIOContext"/>-related function family.
    /// </summary>
    [Flags]
    internal enum AVIOFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// Use non-blocking mode (fail fast instead of wait).
        /// </summary>
        NonBlocking = Unsafe.ffmpeg.AVIO_FLAG_NONBLOCK,
        /// <summary>
        /// Use direct mode (avoid buffering).
        /// </summary>
        Direct = Unsafe.ffmpeg.AVIO_FLAG_DIRECT
    }
}
