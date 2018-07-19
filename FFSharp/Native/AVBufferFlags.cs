using System;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Native
{
    /// <summary>
    /// Flags for <see cref="Unsafe.AVBufferRef"/>-related function family.
    /// </summary>
    [Flags]
    internal enum AVBufferFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// Buffer is read-only.
        /// </summary>
        ReadOnly = Unsafe.ffmpeg.AV_BUFFER_FLAG_READONLY
    }
    // ReSharper restore errors
}
