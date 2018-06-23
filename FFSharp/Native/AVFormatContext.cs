using System.Diagnostics;

using JetBrains.Annotations;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Native
{
    /// <summary>
    /// Provides methods that operate on <see cref="Unsafe.AVFormatContext"/>.
    /// </summary>
    // ReSharper disable errors
    internal static unsafe class AVFormatContext
    {
        /// <summary>
        /// Allocate a new <see cref="Unsafe.AVFormatContext"/>.
        /// </summary>
        /// <returns>A new <see cref="Unsafe.AVFormatContext"/>.</returns>
        [MustUseReturnValue]
        public static Ref<Unsafe.AVFormatContext> Alloc()
        {
            return FFmpegHelper.CheckAlloc<Unsafe.AVFormatContext>(
                Unsafe.ffmpeg.avformat_alloc_context()
            );
        }

        /// <summary>
        /// Close a <see cref="Unsafe.AVFormatContext"/> opened for input.
        /// </summary>
        /// <param name="ARef">The <see cref="Unsafe.AVFormatContext"/>.</param>
        public static void CloseInput(ref Ref<Unsafe.AVFormatContext> ARef)
        {
            // No null-check necessary.

            var ptr = ARef.Ptr;
            Unsafe.ffmpeg.avformat_close_input(&ptr);
            ARef = ptr;
        }

        /// <summary>
        /// Read some packets and detect streams in the input <see cref="Unsafe.AVFormatContext"/>.
        /// </summary>
        /// <param name="ARef">The <see cref="Unsafe.AVFormatContext"/>.</param>
        /// <param name="AOptions">The options <see cref="Unsafe.AVDictionary"/>.</param>
        /// <exception cref="FFmpegError">Error finding stream info.</exception>
        public static void FindStreamInfo(
            Ref<Unsafe.AVFormatContext> ARef,
            ref Ref<Unsafe.AVDictionary> AOptions
        )
        {
            Debug.Assert(
                !ARef.IsNull,
                "Ref is null.",
                "The format context reference may not be null. This indicates a severe logic " +
                "error in the code."
            );

            var opt = AOptions.Ptr;
            var error = Unsafe.ffmpeg.avformat_find_stream_info(ARef, &opt).CheckFFmpeg();
            AOptions = opt;

            // Postponed throw to make reference modification visible to caller at all costs.
            error.ThrowIfPresent();
        }

        /// <summary>
        /// Free a <see cref="Unsafe.AVFormatContext"/>.
        /// </summary>
        /// <param name="ARef">The <see cref="Unsafe.AVFormatContext"/>.</param>
        public static void Free(ref Ref<Unsafe.AVFormatContext> ARef)
        {
            // No null-check necessary.

            Unsafe.ffmpeg.avformat_free_context(ARef);
            ARef = null;
        }

        /// <summary>
        /// Open a <see cref="Unsafe.AVFormatContext"/> for input.
        /// </summary>
        /// <param name="ARef">The <see cref="Unsafe.AVFormatContext"/>.</param>
        /// <param name="AUrl">The input URL.</param>
        /// <param name="AFormat">The forced <see cref="Unsafe.AVInputFormat"/>.</param>
        /// <param name="AOptions">The options <see cref="Unsafe.AVDictionary"/>.</param>
        /// <exception cref="FFmpegError">Error opening input.</exception>
        public static void OpenInput(
            ref Ref<Unsafe.AVFormatContext> ARef,
            [NotNull] string AUrl,
            Ref<Unsafe.AVInputFormat> AFormat,
            ref Ref<Unsafe.AVDictionary> AOptions
        )
        {
            // No null-check necessary.

            Debug.Assert(
                AUrl != null,
                "Url is null.",
                "The URL must be supplied. This indicates a contract violation."
            );

            var ptr = ARef.Ptr;
            var opt = AOptions.Ptr;
            var error = Unsafe.ffmpeg.avformat_open_input(&ptr, AUrl, AFormat, &opt).CheckFFmpeg();

            ARef = ptr;
            AOptions = opt;

            // Postponed throw to make reference modification visible to caller at all costs.
            if (error != null)
            {
                // Even user-allocated contexts are freed.
                ARef = null;
                throw error;
            }
        }

        /// <summary>
        /// Read the next frame from the opened input <see cref="Unsafe.AVFormatContext"/>.
        /// </summary>
        /// <param name="ARef">The <see cref="Unsafe.AVFormatContext"/>.</param>
        /// <param name="APacket">The <see cref="Unsafe.AVPacket"/>.</param>
        /// <returns><c>true</c> if a packet was read; <c>false</c> is EOF was reached.</returns>
        /// <exception cref="FFmpegError">Error reading frame.</exception>
        public static bool ReadFrame(
            Ref<Unsafe.AVFormatContext> ARef,
            Ref<Unsafe.AVPacket> APacket
        )
        {
            Debug.Assert(
                !ARef.IsNull,
                "Ref is null.",
                "The format context reference may not be null. This indicates a severe logic " +
                "error in the code."
            );
            Debug.Assert(
                !APacket.IsNull,
                "Packet is null.",
                "The output packet reference may not be null. This indicates a severe logic " +
                "error in the code."
            );

            var code = Unsafe.ffmpeg.av_read_frame(ARef, APacket);
            if (code == Unsafe.ffmpeg.AVERROR_EOF)
            {
                return false;
            }

            code.CheckFFmpeg().ThrowIfPresent();
            return true;
        }

        /// <summary>
        /// Seek inside the opened input <see cref="Unsafe.AVFormatContext"/>.
        /// </summary>
        /// <param name="ARef">The <see cref="Unsafe.AVFormatContext"/>.</param>
        /// <param name="AStreamIndex">The stream index.</param>
        /// <param name="ATimestamp">The target PTS / frame / byte offset.</param>
        /// <param name="AFlags">The <see cref="AVSeekFlags"/>.</param>
        /// <exception cref="FFmpegError">Error seeking.</exception>
        public static void SeekFrame(
            Ref<Unsafe.AVFormatContext> ARef,
            int AStreamIndex,
            long ATimestamp,
            AVSeekFlags AFlags = AVSeekFlags.NONE
        )
        {
            const AVSeekFlags C_Invalid =
                AVSeekFlags.AVSEEK_FLAG_BYTE
                | AVSeekFlags.AVSEEK_FLAG_FRAME;

            Debug.Assert(
                !ARef.IsNull,
                "Ref is null.",
                "The format context reference may not be null. This indicates a severe logic " +
                "error in the code."
            );
            Debug.Assert(
                AStreamIndex > 0 && AStreamIndex < ARef.Ptr->nb_streams,
                "Stream index out of range.",
                "The stream index must be in range of the available streams. This indicates a " +
                "contract violation. "
            );
            Debug.Assert(
                (AFlags & C_Invalid) != C_Invalid,
                "Invalid flags.",
                "Seeking cannot be performed both on byte offsets and frame indices at the same " +
                "time. This indicates a logic error in code. "
            );

            Unsafe.ffmpeg.av_seek_frame(ARef, AStreamIndex, ATimestamp, (int)AFlags)
                .CheckFFmpeg()
                .ThrowIfPresent();
        }
    }
    // ReSharper restore errors
}
