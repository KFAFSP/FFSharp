using System;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Native
{
    /// <summary>
    /// Flags for the <see cref="Unsafe.AVInputFormat"/> struct.
    /// </summary>
    [Flags]
    internal enum AVInputFormatFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that the demuxer will open the file, an IO context shall not be provided.
        /// </summary>
        NoFile = Unsafe.ffmpeg.AVFMT_NOFILE,

        /// <summary>
        /// Indicates that the filename needs to contain a %d frame number replacement sequence.
        /// </summary>
        NeedNumber = Unsafe.ffmpeg.AVFMT_NEEDNUMBER,

        /// <summary>
        /// Indicates that the stream ID number will be set by the demuxer.
        /// </summary>
        ShowIds = Unsafe.ffmpeg.AVFMT_SHOW_IDS,

        /// <summary>
        /// Indicates that generic code will build the index instead of the demuxer.
        /// </summary>
        GenericIndex = Unsafe.ffmpeg.AVFMT_GENERIC_INDEX,

        /// <summary>
        /// Indicates that timestamps returned might be discontinuous.
        /// </summary>
        TimestampDiscontinuous = Unsafe.ffmpeg.AVFMT_TS_DISCONT,

        /// <summary>
        /// Indicates that the format does not fallback to binary search.
        /// </summary>
        NoBinarySearch = Unsafe.ffmpeg.AVFMT_NOBINSEARCH,
        /// <summary>
        /// Indicates that the format does not fallback to generic search.
        /// </summary>
        NoGenericSearch = Unsafe.ffmpeg.AVFMT_NOGENSEARCH,

        /// <summary>
        /// Indicates that the format does not support seeking in byte offsets.
        /// </summary>
        NoByteSeek = Unsafe.ffmpeg.AVFMT_NO_BYTE_SEEK,
        /// <summary>
        /// Indicates that seeking is based on PTS.
        /// </summary>
        SeekToPts = Unsafe.ffmpeg.AVFMT_SEEK_TO_PTS
    }
}