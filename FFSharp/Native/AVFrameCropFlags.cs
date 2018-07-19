using System;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Native
{
    /// <summary>
    /// Flags for the
    /// <see cref="Unsafe.ffmpeg.av_frame_apply_cropping(Unsafe.AVFrame*, int)"/> function.
    /// </summary>
    [Flags]
    internal enum AVFrameCropFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// Allow producing unaligned output pointers.
        /// </summary>
        Unaligned = 1 << 0 // AV_FRAME_CROP_UNALIGNED
    }
}
