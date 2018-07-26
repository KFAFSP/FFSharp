using System;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Native
{
    /// <summary>
    /// Flags for the <see cref="Unsafe.AVInputFormat"/> struct.
    /// </summary>
    [Flags]
    internal enum AVOutputFormatFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that the muxer will open the file, an IO context shall not be provided.
        /// </summary>
        NoFile = Unsafe.ffmpeg.AVFMT_NOFILE,

        /// <summary>
        /// Indicates that the filename needs to contain a %d frame number replacement sequence.
        /// </summary>
        NeedNumber = Unsafe.ffmpeg.AVFMT_NEEDNUMBER,

        /// <summary>
        /// Indicates that the format uses a global header.
        /// </summary>
        GlobalHeader = Unsafe.ffmpeg.AVFMT_GLOBALHEADER,

        /// <summary>
        /// Indicates that the FPS may vary.
        /// </summary>
        VariableFPS = Unsafe.ffmpeg.AVFMT_VARIABLE_FPS,

        /// <summary>
        /// Indicates that width and height do not have to be set.
        /// </summary>
        NoDimensions = Unsafe.ffmpeg.AVFMT_NODIMENSIONS,
        /// <summary>
        /// Indicates that streams do not have to be created.
        /// </summary>
        NoStreams = Unsafe.ffmpeg.AVFMT_NOSTREAMS,

        /// <summary>
        /// Indicates that the muxer can handle flush (NULL) packets.
        /// </summary>
        AllowFlush = Unsafe.ffmpeg.AVFMT_ALLOW_FLUSH,

        /// <summary>
        /// Indicates that no timestamps are required.
        /// </summary>
        NoTimestamps = Unsafe.ffmpeg.AVFMT_NOTIMESTAMPS,
        /// <summary>
        /// Indicates that timestamps do not have to be strictly monotonous (but still monotnous).
        /// </summary>
        TimestampNonStrict = Unsafe.ffmpeg.AVFMT_TS_NONSTRICT,
        /// <summary>
        /// Indicates that timestamps may be negative.
        /// </summary>
        TimestampNegative = Unsafe.ffmpeg.AVFMT_TS_NEGATIVE
    }
}