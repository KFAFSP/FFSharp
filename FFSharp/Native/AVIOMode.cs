using System;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Native
{
    /// <summary>
    /// Open modes for the <see cref="Unsafe.AVIOContext"/>-related function family.
    /// </summary>
    [Flags]
    internal enum AVIOOpenMode
    {
        /// <summary>
        /// Open for reading.
        /// </summary>
        Read = Unsafe.ffmpeg.AVIO_FLAG_READ,
        /// <summary>
        /// Open for writing.
        /// </summary>
        Write = Unsafe.ffmpeg.AVIO_FLAG_WRITE,

        /// <summary>
        /// Open for reading and writing.
        /// </summary>
        ReadWrite = Unsafe.ffmpeg.AVIO_FLAG_READ_WRITE
    }
}
