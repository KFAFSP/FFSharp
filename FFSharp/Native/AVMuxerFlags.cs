using System;

namespace FFSharp.Native
{
    /// <summary>
    /// Enumeration of flags for <see cref="FFmpeg.AutoGen.AVOutputFormat"/>.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [Flags]
    internal enum AVMuxerFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        NONE = 0,

        AVFMT_NOFILE = FFmpeg.AutoGen.ffmpeg.AVFMT_NOFILE,
        AVFMT_NEEDNUMBER = FFmpeg.AutoGen.ffmpeg.AVFMT_NEEDNUMBER,
        AVFMT_GLOBALHEADER = FFmpeg.AutoGen.ffmpeg.AVFMT_GLOBALHEADER,
        AVFMT_NOTIMESTAMPS = FFmpeg.AutoGen.ffmpeg.AVFMT_NOTIMESTAMPS,
        AVFMT_VARIABLE_FPS = FFmpeg.AutoGen.ffmpeg.AVFMT_VARIABLE_FPS,
        AVFMT_NODIMENSIONS = FFmpeg.AutoGen.ffmpeg.AVFMT_NODIMENSIONS,
        AVFMT_NOSTREAMS = FFmpeg.AutoGen.ffmpeg.AVFMT_NOSTREAMS,
        AVFMT_ALLOW_FLUSH = FFmpeg.AutoGen.ffmpeg.AVFMT_ALLOW_FLUSH,
        AVFMT_TS_NONSTRICT = FFmpeg.AutoGen.ffmpeg.AVFMT_TS_NONSTRICT,
        AVFMT_TS_NEGATIVE = FFmpeg.AutoGen.ffmpeg.AVFMT_TS_NEGATIVE
    }
}