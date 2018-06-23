using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using JetBrains.Annotations;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Native
{
    /// <summary>
    /// Wrapper class for the libavformat IO facilities.
    /// </summary>
    // ReSharper disable errors
    internal static unsafe class AVIO
    {
        /// <summary>
        /// Close a <see cref="Unsafe.AVIOContext"/>.
        /// </summary>
        /// <param name="ARef">The <see cref="Unsafe.AVIOContext"/>.</param>
        public static void Close(ref Ref<Unsafe.AVIOContext> ARef)
        {
            // No null-check necessary.

            var error = Unsafe.ffmpeg.avio_close(ARef)
                .CheckFFmpeg("Error closing context.");
            ARef = null;

            Debug.Assert(
                error == null,
                "Error on close.",
                "Though this is technically possible (raised from protocol close), this is very " +
                "bad behaviour. Normal execution MUST swallow this error anyways."
            );
        }

        /// <summary>
        /// Open a <see cref="Unsafe.AVIOContext"/>.
        /// </summary>
        /// <param name="ARef">The <see cref="Unsafe.AVIOContext"/>.</param>
        /// <param name="AUrl">The URL to open.</param>
        /// <param name="AFlags">The <see cref="AVIOOpenFlags"/>.</param>
        /// <param name="AIntCb">The <see cref="Unsafe.AVIOInterruptCB"/>.</param>
        /// <param name="AOptions">The options <see cref="Unsafe.AVDictionary"/>.</param>
        /// <exception cref="FFmpegError">Error opening context.</exception>
        public static void Open(
            ref Ref<Unsafe.AVIOContext> ARef,
            [NotNull] string AUrl,
            AVIOOpenFlags AFlags,
            Ref<Unsafe.AVIOInterruptCB> AIntCb,
            ref Ref<Unsafe.AVDictionary> AOptions
        )
        {
            const AVIOOpenFlags C_Useless = AVIOOpenFlags.AVIO_FLAG_READWRITE;

            // No null-check necessary.

            Debug.Assert(
                AUrl != null,
                "Url is null.",
                "The URL must be supplied. This indicates a contract violation."
            );
            Debug.Assert(
                (AFlags & C_Useless) != C_Useless,
                "Useless flag combination.",
                "The read-write open mode is de-facto not supported and results in write-only " +
                "streams. This indicates a logic error in code."
            );

            var ptr = ARef.Ptr;
            var opt = AOptions.Ptr;
            var error = Unsafe.ffmpeg.avio_open2(
                &ptr,
                AUrl,
                (int)AFlags,
                AIntCb,
                &opt
            ).CheckFFmpeg("Error opening context.");

            ARef = ptr;
            AOptions = opt;

            // Postponed throw to make reference modification visible to caller at all costs.
            error.ThrowIfPresent();
        }
    }
    // ReSharper restore errors
}
