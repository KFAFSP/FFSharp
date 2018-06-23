using System;

namespace FFSharp.Native
{
    /// <summary>
    /// Enumeration of flags for <see cref="FFmpeg.AutoGen.AVInputFormat"/>.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [Flags]
    internal enum AVDemuxerFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        NONE = 0,

        AVFMT_NOFILE = FFmpeg.AutoGen.ffmpeg.AVFMT_NOFILE,
        AVFMT_NEEDNUMBER = FFmpeg.AutoGen.ffmpeg.AVFMT_NEEDNUMBER,
        AVFMT_SHOW_IDS = FFmpeg.AutoGen.ffmpeg.AVFMT_SHOW_IDS,
        AVFMT_GENERIC_INDEX = FFmpeg.AutoGen.ffmpeg.AVFMT_GENERIC_INDEX,
        AVFMT_TS_DISCONT = FFmpeg.AutoGen.ffmpeg.AVFMT_TS_DISCONT,
        AVFMT_NOBINSEARCH = FFmpeg.AutoGen.ffmpeg.AVFMT_NOBINSEARCH,
        AVFMT_NOGENSEARCH = FFmpeg.AutoGen.ffmpeg.AVFMT_NOGENSEARCH,
        AVFMT_NO_BYTE_SEEK = FFmpeg.AutoGen.ffmpeg.AVFMT_NO_BYTE_SEEK,
        AVFMT_SEEK_TO_PTS = FFmpeg.AutoGen.ffmpeg.AVFMT_SEEK_TO_PTS
    }
}