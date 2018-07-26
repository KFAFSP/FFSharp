using System;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Native
{
    /// <summary>
    /// Flags for the
    /// <see cref="Unsafe.ffmpeg.avformat_seek_file(Unsafe.AVFormatContext*, int, long, long, long, int)"/>
    /// operation.
    /// </summary>
    [Flags]
    internal enum AVSeekFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// Allow seeking to non-keyframes.
        /// </summary>
        Any = Unsafe.ffmpeg.AVSEEK_FLAG_ANY,

        /// <summary>
        /// Seek to frame numbers instead of PTS.
        /// </summary>
        Frame = Unsafe.ffmpeg.AVSEEK_FLAG_FRAME,
        /// <summary>
        /// Seek to byte offsets instead of PTS.
        /// </summary>
        Byte = Unsafe.ffmpeg.AVSEEK_FLAG_BYTE
    }
}