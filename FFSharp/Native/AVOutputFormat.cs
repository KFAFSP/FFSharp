using System.Diagnostics;

using JetBrains.Annotations;

using Whetstone.Contracts;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Native
{
    /// <summary>
    /// Provides methods that operate on <see cref="Unsafe.AVOutputFormat"/>.
    /// </summary>
    // ReSharper disable errors
    internal static unsafe class AVOutputFormat
    {
        /// <summary>
        /// Check whether the format can write uncoded frames.
        /// </summary>
        /// <param name="AContext">The <see cref="Unsafe.AVFormatContext"/>.</param>
        /// <param name="AStream">The stream index.</param>
        /// <returns>
        /// <see langword="true"/> if writing uncoded frames is supported; otherwise
        /// <see langword="false"/>.
        /// </returns>
        [Pure]
        public static bool CanWriteUncoded(
            Fixed<Unsafe.AVFormatContext> AContext,
            int AStream
        )
        {
            Debug.Assert(
                !AContext.IsNull,
                "Context is null.",
                "This indicates a contract violation."
            );

            Debug.Assert(
                AStream >= 0 && AStream < AContext.Raw->nb_streams,
                "Stream out of range.",
                "This indicates a contract violation."
            );

            return Unsafe.ffmpeg.av_write_uncoded_frame_query(AContext, AStream).ToBool();
        }

        /// <summary>
        /// Guess the <see cref="Unsafe.AVOutputFormat"/>.
        /// </summary>
        /// <param name="AShortName">Optional name of the format.</param>
        /// <param name="AFileName">Optional file name of the output.</param>
        /// <param name="AMimeType">Optional MIME type of the output.</param>
        /// <returns>
        /// The guessed <see cref="Unsafe.AVOutputFormat"/> or <see langword="null"/>.
        /// </returns>
        [Pure]
        public static Fixed<Unsafe.AVOutputFormat> Guess(
            [CanBeNull] string AShortName,
            [CanBeNull] string AFileName,
            [CanBeNull] string AMimeType
        )
        {
            return Unsafe.ffmpeg.av_guess_format(AShortName, AFileName, AMimeType);
        }

        /// <summary>
        /// Initialize the muxer but do not write the header.
        /// </summary>
        /// <param name="AContext">The <see cref="Unsafe.AVFormatContext"/>.</param>
        /// <param name="AOptions">The <see cref="Unsafe.AVDictionary"/> of options.</param>
        /// <returns>
        /// <see cref="Result{T}"/> wrapping whether <see cref="WriteHeader"/> still needs to be
        /// called afterwards before starting output.
        /// </returns>
        [MustUseReturnValue]
        public static Result<bool> Init(
            Fixed<Unsafe.AVFormatContext> AContext,
            Movable<Unsafe.AVDictionary> AOptions = default
        )
        {
            Debug.Assert(
                !AContext.IsNull,
                "Context is null.",
                "This indicates a contract violation."
            );

            var result = Unsafe.ffmpeg.avformat_init_output(AContext, AOptions);
            if (result < 0)
                throw new FFmpegError(result);

            return result == Unsafe.ffmpeg.AVSTREAM_INIT_IN_WRITE_HEADER;
        }

        /// <summary>
        /// Write a packet to the container.
        /// </summary>
        /// <param name="AContext">The <see cref="Unsafe.AVFormatContext"/>.</param>
        /// <param name="APacket">The <see cref="Unsafe.AVPacket"/>.</param>
        /// <returns><see cref="Result"/> of the operation.</returns>
        [MustUseReturnValue]
        public static Result Write(
            Fixed<Unsafe.AVFormatContext> AContext,
            Fixed<Unsafe.AVPacket> APacket
        )
        {
            Debug.Assert(
                !AContext.IsNull,
                "Context is null.",
                "This indicates a contract violation."
            );

            Debug.Assert(
                !APacket.IsNull || (AContext.Raw->flags & (int)AVOutputFormatFlags.AllowFlush) > 0,
                "Frame is null.",
                "This indicates a logic error: the output format does not allow flushing."
            );

            return Unsafe.ffmpeg.av_write_frame(AContext, APacket).ToResult();
        }

        /// <summary>
        /// Initialize the muxer and write the header.
        /// </summary>
        /// <param name="AContext">The <see cref="Unsafe.AVFormatContext"/>.</param>
        /// <param name="AOptions">The <see cref="Unsafe.AVDictionary"/> of options.</param>
        /// <returns><see cref="Result"/> of the operation.</returns>
        [MustUseReturnValue]
        public static Result WriteHeader(
            Fixed<Unsafe.AVFormatContext> AContext,
            Movable<Unsafe.AVDictionary> AOptions = default
        )
        {
            Debug.Assert(
                !AContext.IsNull,
                "Context is null.",
                "This indicates a contract violation."
            );

            return Unsafe.ffmpeg.avformat_write_header(AContext, AOptions).ToResult();
        }

        /// <summary>
        /// Write a packet to the container, letting libav handle the interleaving.
        /// </summary>
        /// <param name="AContext">The <see cref="Unsafe.AVFormatContext"/>.</param>
        /// <param name="APacket">The <see cref="Unsafe.AVPacket"/>.</param>
        /// <returns><see cref="Result"/> of the operation.</returns>
        [MustUseReturnValue]
        public static Result WriteInterleaved(
            Fixed<Unsafe.AVFormatContext> AContext,
            Fixed<Unsafe.AVPacket> APacket
        )
        {
            Debug.Assert(
                !AContext.IsNull,
                "Context is null.",
                "This indicates a contract violation."
            );

            // No null-check necessary.

            return Unsafe.ffmpeg.av_interleaved_write_frame(AContext, APacket).ToResult();
        }

        /// <summary>
        /// Write the trailer to the container and finalize output.
        /// </summary>
        /// <param name="AContext">The <see cref="Unsafe.AVFormatContext"/>.</param>
        /// <returns><see cref="Result"/> of the operation.</returns>
        [MustUseReturnValue]
        public static Result WriteTrailer(Fixed<Unsafe.AVFormatContext> AContext)
        {
            Debug.Assert(
                !AContext.IsNull,
                "Context is null.",
                "This indicates a contract violation."
            );

            return Unsafe.ffmpeg.av_write_trailer(AContext).ToResult();
        }

        /// <summary>
        /// Write an uncoded <see cref="Unsafe.AVFrame"/> to the container.
        /// </summary>
        /// <param name="AContext">The <see cref="Unsafe.AVFormatContext"/>.</param>
        /// <param name="AStream">The stream index.</param>
        /// <param name="AFrame">The <see cref="Unsafe.AVFrame"/>.</param>
        /// <returns><see cref="Result"/> of the operation.</returns>
        [MustUseReturnValue]
        public static Result WriteUncoded(
            Fixed<Unsafe.AVFormatContext> AContext,
            int AStream,
            Fixed<Unsafe.AVFrame> AFrame
        )
        {
            Debug.Assert(
                !AContext.IsNull,
                "Context is null.",
                "This indicates a contract violation."
            );

            Debug.Assert(
               AStream >= 0 && AStream < AContext.Raw->nb_streams,
               "Stream out of range.",
               "This indicates a contract violation."
            );

            Debug.Assert(
                !AFrame.IsNull || (AContext.Raw->flags & (int)AVOutputFormatFlags.AllowFlush) > 0,
                "Frame is null.",
                "This indicates a logic error: the output format does not allow flushing."
            );

            return Unsafe.ffmpeg.av_write_uncoded_frame(AContext, AStream, AFrame).ToResult();
        }

        /// <summary>
        /// Write an uncoded <see cref="Unsafe.AVFrame"/> to the container, letting libav handle the
        /// interleaving.
        /// </summary>
        /// <param name="AContext">The <see cref="Unsafe.AVFormatContext"/>.</param>
        /// <param name="AStream">The stream index.</param>
        /// <param name="AFrame">The <see cref="Unsafe.AVFrame"/>.</param>
        /// <returns><see cref="Result"/> of the operation.</returns>
        [MustUseReturnValue]
        public static Result WriteUncodedInterleaved(
            Fixed<Unsafe.AVFormatContext> AContext,
            int AStream,
            Fixed<Unsafe.AVFrame> AFrame
        )
        {
            Debug.Assert(
                !AContext.IsNull,
                "Context is null.",
                "This indicates a contract violation."
            );

            Debug.Assert(
               AStream >= 0 && AStream < AContext.Raw->nb_streams,
               "Stream out of range.",
               "This indicates a contract violation."
            );

            return Unsafe.ffmpeg.av_interleaved_write_uncoded_frame(AContext, AStream, AFrame)
                .ToResult();
        }
    }
    // ReSharper restore errors
}
