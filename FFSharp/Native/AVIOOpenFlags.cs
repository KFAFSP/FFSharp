using System;

namespace FFSharp.Native
{
    /// <summary>
    /// Enumeration of flags for <see cref="FFmpeg.AutoGen.ffmpeg.avio_open"/> family functions.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [Flags]
    internal enum AVIOOpenFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        NONE = 0,

        /// <summary>
        /// Open for reading.
        /// </summary>
        AVIO_FLAG_READ = FFmpeg.AutoGen.ffmpeg.AVIO_FLAG_READ,

        /// <summary>
        /// Open for writing.
        /// </summary>
        AVIO_FLAG_WRITE = FFmpeg.AutoGen.ffmpeg.AVIO_FLAG_WRITE,

        /// <summary>
        /// Open for reading and writing, which FFmpeg treats as write-only.
        /// </summary>
        AVIO_FLAG_READWRITE = FFmpeg.AutoGen.ffmpeg.AVIO_FLAG_READ_WRITE,

        /// <summary>
        /// Bypass the buffer if possible.
        /// </summary>
        AVIO_FLAG_DIRECT = FFmpeg.AutoGen.ffmpeg.AVIO_FLAG_DIRECT,

        /// <summary>
        /// Execute nonblocking only, otherwise return EAGAIN.
        /// </summary>
        AVIO_FLAG_NONBLOCK = FFmpeg.AutoGen.ffmpeg.AVIO_FLAG_NONBLOCK,
    }
}