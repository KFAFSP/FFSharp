using System;

namespace FFSharp.Native
{
    /// <summary>
    /// Flags for the <see cref="FFmpeg.AutoGen.ffmpeg.av_seek_frame"/> operation.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [Flags]
    internal enum AVSeekFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        NONE = 0,

        /// <summary>
        /// Round downwards instead of forwards.
        /// </summary>
        AVSEEK_FLAG_BACKWARD = FFmpeg.AutoGen.ffmpeg.AVSEEK_FLAG_BACKWARD,

        /// <summary>
        /// Seek to a byte position instead of a timestamp.
        /// </summary>
        AVSEEK_FLAG_BYTE = FFmpeg.AutoGen.ffmpeg.AVSEEK_FLAG_BYTE,

        /// <summary>
        /// Seek to any frame and not only keyframes.
        /// </summary>
        AVSEEK_FLAG_ANY = FFmpeg.AutoGen.ffmpeg.AVSEEK_FLAG_ANY,

        /// <summary>
        /// Seek to a frame index instead of a timestamp.
        /// </summary>
        AVSEEK_FLAG_FRAME = FFmpeg.AutoGen.ffmpeg.AVSEEK_FLAG_FRAME
    }
}