using System.Diagnostics;

using JetBrains.Annotations;

using Whetstone.Contracts;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Native
{
    /// <summary>
    /// Provides methods that operate on <see cref="Unsafe.AVInputFormat"/>.
    /// </summary>
    // ReSharper disable errors
    internal static unsafe class AVInputFormat
    {
        /// <summary>
        /// Close and free an input <see cref="Unsafe.AVFormatContext"/>.
        /// </summary>
        /// <param name="AContext">The <see cref="Unsafe.AVFormatContext"/>.</param>
        public static void Close(Movable<Unsafe.AVFormatContext> AContext)
        {
            // No null- or present- check necessary.

            Unsafe.ffmpeg.avformat_close_input(AContext);
        }

        /// <summary>
        /// Find a <see cref="Unsafe.AVInputFormat"/> by it's name.
        /// </summary>
        /// <param name="AShortName">The input format name.</param>
        /// <returns>
        /// The found <see cref="Unsafe.AVInputFormat"/> or <see langword="null"/> if not found.
        /// </returns>
        [Pure]
        public static Fixed<Unsafe.AVInputFormat> Find([CanBeNull] string AShortName)
        {
            // No null-check necessary.

            return Unsafe.ffmpeg.av_find_input_format(AShortName);
        }

        /// <summary>
        /// Try to find the stream matching the specified criteria best.
        /// </summary>
        /// <param name="AContext">The <see cref="Unsafe.AVFormatContext"/>.</param>
        /// <param name="AMediaType">The <see cref="Unsafe.AVMediaType"/>.</param>
        /// <param name="AWantedStream">Index of the stream requested, or -1 for automatic.</param>
        /// <param name="ARelatedStream">Index of a related stream, or -1 for none.</param>
        /// <param name="ADecoder">
        /// If non-<see langword="null"/>, will contain the <see cref="Unsafe.AVCodec"/> that
        /// decodes the stream.
        /// </param>
        /// <returns><see cref="Result{T}"/> wrapping the selected stream index.</returns>
        /// <remarks>
        /// If <paramref name="ADecoder"/> is non-<see langword="null"/>, it is implied that the
        /// selected stream must be decodable.
        /// </remarks>
        [MustUseReturnValue]
        public static Result<uint> FindBestStream(
            Fixed<Unsafe.AVFormatContext> AContext,
            Unsafe.AVMediaType AMediaType,
            int AWantedStream = -1,
            int ARelatedStream = -1,
            Movable<Unsafe.AVCodec> ADecoder = default
        )
        {
            Debug.Assert(
                !AContext.IsNull,
                "Context is null.",
                "This indicates a contract violation."
            );

            Debug.Assert(
                AWantedStream == -1 ||
                    (AWantedStream >= 0 && AWantedStream < AContext.Raw->nb_streams),
                "WantedStream is out of range.",
                "This indicates a contract violation."
            );

            Debug.Assert(
                ARelatedStream == -1 ||
                    (ARelatedStream >= 0 && ARelatedStream < AContext.Raw->nb_streams),
                "RelatedStream is out of range.",
                "This indicates a contract violation."
            );

            return Unsafe.ffmpeg.av_find_best_stream(
                AContext,
                AMediaType,
                AWantedStream,
                ARelatedStream,
                ADecoder,
                0
            ).ToResultUInt();
        }

        /// <summary>
        /// Find the stream information in an opened <see cref="Unsafe.AVFormatContext"/>.
        /// </summary>
        /// <param name="AContext">The <see cref="Unsafe.AVFormatContext"/>.</param>
        /// <param name="AOptions">
        /// An array of <see cref="Unsafe.AVDictionary"/> options for each stream.
        /// </param>
        /// <returns><see cref="Result"/> of the operation.</returns>
        /// <remarks>
        /// Options in <paramref name="AOptions"/> that were used during the operation are consumed
        /// from their respective dictionaries.
        /// </remarks>
        [MustUseReturnValue]
        public static Result FindStreamInfo(
            Fixed<Unsafe.AVFormatContext> AContext,
            [CanBeNull] Movable<Unsafe.AVDictionary>[] AOptions = null
        )
        {
            Debug.Assert(
                !AContext.IsNull,
                "Context is null.",
                "This indicates a contract violation."
            );

            if (AOptions == null)
            {
                return Unsafe.ffmpeg.avformat_find_stream_info(AContext, null).ToResult();
            }

            Debug.Assert(
                AOptions.Length == AContext.Raw->nb_streams,
                "Options array length mismatch.",
                "This indicates a contract violation."
            );

            var options = new Unsafe.AVDictionary*[AOptions.Length];
            for (int I = 0; I < options.Length; ++I)
            {
                options[I] = AOptions[I].TargetOr(null);
            }

            fixed (Unsafe.AVDictionary** optionsPtr = options)
            {
                var result = Unsafe.ffmpeg.avformat_find_stream_info(AContext, optionsPtr)
                    .ToResult();

                for (int I = 0; I < options.Length; ++I)
                {
                    AOptions[I].SetTarget(options[I]);
                }

                return result;
            }
        }

        /// <summary>
        /// Flush all buffers of the input <see cref="Unsafe.AVFormatContext"/>.
        /// </summary>
        /// <param name="AContext">The <see cref="Unsafe.AVFormatContext"/>.</param>
        /// <returns><see cref="Result"/> of the operation.</returns>
        [MustUseReturnValue]
        public static Result Flush(Fixed<Unsafe.AVFormatContext> AContext)
        {
            Debug.Assert(
                !AContext.IsNull,
                "Context is null.",
                "This indicates a contract violation."
            );

            return Unsafe.ffmpeg.avformat_flush(AContext).ToResult();
        }

        /// <summary>
        /// Allocate and/or open a <see cref="Unsafe.AVFormatContext"/> for input.
        /// </summary>
        /// <param name="AContext">The <see cref="Unsafe.AVFormatContext"/>.</param>
        /// <param name="AUrl">The input URL.</param>
        /// <param name="AFormat">
        /// The <see cref="Unsafe.AVInputFormat"/> or <see langword="null"/>.
        /// </param>
        /// <param name="AOptions">
        /// The <see cref="Unsafe.AVDictionary"/> of options.
        /// </param>
        /// <returns><see cref="Result"/> of the operation.</returns>
        /// <remarks>
        /// Options in <paramref name="AOptions"/> that were used during the operation are consumed
        /// from the dictionary.
        /// </remarks>
        [MustUseReturnValue]
        public static Result Open(
            Movable<Unsafe.AVFormatContext> AContext,
            [NotNull] string AUrl,
            Fixed<Unsafe.AVInputFormat> AFormat = default,
            Movable<Unsafe.AVDictionary> AOptions = default
        )
        {
            Debug.Assert(
                AUrl != null,
                "URL is null.",
                "This indicates a contract violation."
            );

            return Unsafe.ffmpeg.avformat_open_input(AContext, AUrl, AFormat, AOptions).ToResult();
        }

        /// <summary>
        /// Try to determine the format of a probed input stream.
        /// </summary>
        /// <param name="AProbeData">The <see cref="Unsafe.AVProbeData"/>.</param>
        /// <param name="AIsOpened">
        /// If <see langword="false"/>, <see cref="AVInputFormatFlags.NoFile"/> demuxers are
        /// also considered during search.
        /// </param>
        /// <param name="AScore">Score of the result.</param>
        /// <returns>The result <see cref="Unsafe.AVInputFormat"/> with the highest score.</returns>
        [Pure]
        public static Fixed<Unsafe.AVInputFormat> Probe(Fixed<Unsafe.AVProbeData> AProbeData, bool AIsOpened, out int AScore)
        {
            Debug.Assert(
                !AProbeData.IsNull,
                "ProbeData is null.",
                "This indicates a contract violation."
            );

            int score;
            var result = Unsafe.ffmpeg.av_probe_input_format3(
                AProbeData,
                AIsOpened.ToCBool(),
                &score
            );
            AScore = score;
            return result;
        }

        /// <summary>
        /// Read the next frame from the stream.
        /// </summary>
        /// <param name="AContext">The <see cref="Unsafe.AVFormatContext"/>.</param>
        /// <param name="APacket">The <see cref="Unsafe.AVPacket"/>.</param>
        /// <returns><see cref="Result{T}"/> wrapping the EOF state.</returns>
        /// <remarks>
        /// If <paramref name="APacket"/> contains a valid buffer ref it remains valid indefinitely,
        /// otherwise it is only valid until the next call to this function.
        /// </remarks>
        [MustUseReturnValue]
        public static Result<bool> Read(
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
                !APacket.IsNull,
                "Packet is null.",
                "This indicates a contract violation."
            );

            var code = Unsafe.ffmpeg.av_read_frame(AContext, APacket);
            if (code == Unsafe.ffmpeg.AVERROR_EOF)
            {
                return false;
            }
            if (code < 0)
            {
                return new FFmpegError(code);
            }

            return true;
        }

        /// <summary>
        /// Seek in the input file.
        /// </summary>
        /// <param name="AContext">The <see cref="Unsafe.AVFormatContext"/>.</param>
        /// <param name="AStream">Index of the reference stream, or -1.</param>
        /// <param name="AMin">Minimum seek target value.</param>
        /// <param name="ATimestamp">Seek target value.</param>
        /// <param name="AMax">Maximum seek target value.</param>
        /// <param name="AFlags">The <see cref="AVSeekFlags"/>.</param>
        /// <returns><see cref="Result"/> of the operation.</returns>
        /// <remarks>
        /// <paramref name="AMin"/>, <paramref name="ATimestamp"/> and <paramref name="AMax"/>
        /// are (in order of precedence):
        /// <list type="bullet">
        /// <item><description>
        /// In bytes if <see cref="AVSeekFlags.Byte"/> is set.
        /// </description></item>
        /// <item><description>
        /// In frames if <see cref="AVSeekFlags.Frame"/> is set.
        /// </description></item>
        /// <item><description>
        /// In stream time base if <paramref name="AStream"/> is not -1.
        /// </description></item>
        /// <item><description>
        /// In <see cref="Unsafe.ffmpeg.AV_TIME_BASE"/>.
        /// </description></item>
        /// </list>
        /// </remarks>
        [MustUseReturnValue]
        public static Result Seek(
            Fixed<Unsafe.AVFormatContext> AContext,
            int AStream,
            long AMin,
            long ATimestamp,
            long AMax,
            AVSeekFlags AFlags = AVSeekFlags.None
        )
        {
            Debug.Assert(
                !AContext.IsNull,
                "Context is null.",
                "This indicates a contract violation."
            );

            Debug.Assert(
                AStream == -1 ||
                    (AStream >= 0 && AStream < AContext.Raw->nb_streams),
                "Stream is out of range.",
                "This indicates a contract violation."
            );

            Debug.Assert(
                !((AFlags & AVSeekFlags.Byte) > 0 && (AFlags & AVSeekFlags.Frame) > 0),
                "Invalid flag combination.",
                "This indicates a contract violation."
            );

            return Unsafe.ffmpeg.avformat_seek_file(
                AContext,
                AStream,
                AMin,
                ATimestamp,
                AMax,
                (int)AFlags
            ). ToResult();
        }
    }
    // ReSharper enable errors
}
