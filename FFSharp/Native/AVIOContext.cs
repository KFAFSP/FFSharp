using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using JetBrains.Annotations;

using Whetstone.Contracts;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Native
{
    /// <summary>
    /// Provides methods that operate on <see cref="Unsafe.AVIOContext"/>.
    /// </summary>
    // ReSharper disable errors
    internal static unsafe class AVIOContext
    {
        /// <summary>
        /// Allocate a new <see cref="Unsafe.AVIOContext"/>.
        /// </summary>
        /// <param name="ABuffer">The underlying buffer.</param>
        /// <param name="ABufferSize">The size of the buffer.</param>
        /// <param name="AWritable">Whether the context supports writing.</param>
        /// <param name="AOpaque">An opaque private pointer.</param>
        /// <param name="AReadPacket">The read callback, can be null.</param>
        /// <param name="AWritePacket">The write callback, can be null.</param>
        /// <param name="ASeek">The seek callback, can be null.</param>
        /// <returns>
        /// <see cref="Result{T}"/> wrapping the new <see cref="Unsafe.AVIOContext"/>.
        /// </returns>
        [MustUseReturnValue]
        public static Result<Fixed<Unsafe.AVIOContext>> Alloc(
            Fixed<byte> ABuffer,
            int ABufferSize,
            bool AWritable,
            Fixed AOpaque,
            [CanBeNull] Func<Fixed, Fixed<byte>, int, int> AReadPacket,
            [CanBeNull] Func<Fixed, Fixed<byte>, int, int> AWritePacket,
            [CanBeNull] Func<Fixed, long, AVIOSeekOrigin, AVIOSeekFlags, long> ASeek
        )
        {
            Debug.Assert(
                !ABuffer.IsNull,
                "Buffer is null.",
                "This indicates a contract violation."
            );

            Debug.Assert(
                ABufferSize >= 0,
                "BufferSize is negative.",
                "This indicates a contract violation."
            );

            Unsafe.avio_alloc_context_read_packet make_read_delegate()
            {
                if (AReadPacket == null) return null;

                return (opaque, buf, sz) => AReadPacket(opaque, buf, sz);
            }

            Unsafe.avio_alloc_context_write_packet make_write_delegate()
            {
                if (AWritePacket == null) return null;

                return (opaque, buf, sz) => AWritePacket(opaque, buf, sz);
            }

            Unsafe.avio_alloc_context_seek make_seek_delegate()
            {
                if (ASeek == null) return null;

                return (opaque, offset, whence) =>
                {
                    var flags = AVIOSeekFlags.None;
                    if ((whence & (int)AVIOSeekFlags.Force) > 0)
                    {
                        whence ^= (int)AVIOSeekFlags.Force;
                        flags |= AVIOSeekFlags.Force;
                    }
                    if ((whence & (int)AVIOSeekFlags.Size) > 0)
                    {
                        whence ^= (int)AVIOSeekFlags.Size;
                        flags |= AVIOSeekFlags.Size;
                    }

                    if (whence < (int)AVIOSeekOrigin.Begin || whence > (int)AVIOSeekOrigin.End)
                        return Unsafe.ffmpeg.AVERROR(Unsafe.ffmpeg.EINVAL);

                    return ASeek(opaque, offset, (AVIOSeekOrigin)whence, flags);
                };
            }

            Fixed<Unsafe.AVIOContext> context = Unsafe.ffmpeg.avio_alloc_context(
                ABuffer,
                ABufferSize,
                AWritable ? 1 : 0,
                AOpaque,
                make_read_delegate(),
                make_write_delegate(),
                make_seek_delegate()
            );
            if (context.IsNull) return new BadAllocationException(typeof(Unsafe.AVIOContext));

            return context;
        }

        /// <summary>
        /// Close an <see cref="Unsafe.AVIOContext"/>.
        /// </summary>
        /// <returns><see cref="Result"/> of the operation.</returns>
        [MustUseReturnValue]
        public static Result Close(Fixed<Unsafe.AVIOContext> AContext)
        {
            // No null-check necessary.

            return Unsafe.ffmpeg.avio_close(AContext).ToResult();
        }

        /// <summary>
        /// Get a value indicating whether a <see cref="Unsafe.AVIOContext"/> has reached EOF.
        /// </summary>
        /// <param name="AContext">The <see cref="Unsafe.AVIOContext"/>.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="AContext"/> is at EOF; otherwise
        /// <see langword="false"/>.
        /// </returns>
        [Pure]
        public static bool Feof(Fixed<Unsafe.AVIOContext> AContext)
        {
            // No null-check necessary.

            return Unsafe.ffmpeg.avio_feof(AContext).ToBool();
        }

        /// <summary>
        /// Flush the <see cref="Unsafe.AVIOContext"/>.
        /// </summary>
        /// <param name="AContext">The <see cref="Unsafe.AVIOContext"/>.</param>
        public static void Flush(Fixed<Unsafe.AVIOContext> AContext)
        {
            Debug.Assert(
                !AContext.IsNull,
                "Context is null.",
                "This indicates a contract violation."
            );

            Unsafe.ffmpeg.avio_flush(AContext);
        }

        /// <summary>
        /// Free an <see cref="Unsafe.AVIOContext"/>.
        /// </summary>
        /// <param name="AContext">The <see cref="Unsafe.AVIOContext"/>.</param>
        public static void Free(Movable<Unsafe.AVIOContext> AContext)
        {
            Debug.Assert(
                AContext.IsPresent,
                "Context is absent.",
                "This indicates a contract violation."
            );

            Unsafe.ffmpeg.avio_context_free(AContext);
        }

        /// <summary>
        /// Create and open a <see cref="Unsafe.AVIOContext"/>.
        /// </summary>
        /// <param name="AContext">The <see cref="Unsafe.AVIOContext"/>.</param>
        /// <param name="AUrl">The URL.</param>
        /// <param name="AOpenMode">The <see cref="AVIOOpenMode"/>.</param>
        /// <param name="AFlags">The <see cref="AVIOFlags"/>.</param>
        /// <returns><see cref="Result"/> of the operation.</returns>
        [MustUseReturnValue]
        public static Result Open(
            Movable<Unsafe.AVIOContext> AContext,
            [NotNull] string AUrl,
            AVIOOpenMode AOpenMode,
            AVIOFlags AFlags = AVIOFlags.None
        )
        {
            Debug.Assert(
                !AContext.IsNull,
                "Context is null.",
                "This indicates a contract violation."
            );

            Debug.Assert(
                AUrl != null,
                "URL is null.",
                "This indicates a contract violation."
            );

            // No present-check necessary.

            return Unsafe.ffmpeg.avio_open(AContext, AUrl, (int)AOpenMode | (int)AFlags).ToResult();
        }

        /// <summary>
        /// Create and open a <see cref="Unsafe.AVIOContext"/>.
        /// </summary>
        /// <param name="AContext">The <see cref="Unsafe.AVIOContext"/>.</param>
        /// <param name="AUrl">The URL.</param>
        /// <param name="AOpenMode">The <see cref="AVIOOpenMode"/>.</param>
        /// <param name="AFlags">The <see cref="AVIOFlags"/>.</param>
        /// <param name="AInterruptCallback">The <see cref="Unsafe.AVIOInterruptCB"/>.</param>
        /// <param name="AOptions">The <see cref="Unsafe.AVDictionary"/> of options.</param>
        /// <returns><see cref="Result"/> of the operation.</returns>
        [MustUseReturnValue]
        public static Result Open2(
            Movable<Unsafe.AVIOContext> AContext,
            [NotNull] string AUrl,
            AVIOOpenMode AOpenMode,
            AVIOFlags AFlags = AVIOFlags.None,
            [CanBeNull] Fixed<Unsafe.AVIOInterruptCB> AInterruptCallback = default,
            Movable<Unsafe.AVDictionary> AOptions = default
        )
        {
            Debug.Assert(
                !AContext.IsNull,
                "Context is null.",
                "This indicates a contract violation."
            );

            Debug.Assert(
                AUrl != null,
                "URL is null.",
                "This indicates a contract violation."
            );

            // No present-check necessary.

            return Unsafe.ffmpeg.avio_open2(
                AContext,
                AUrl,
                (int)AOpenMode | (int)AFlags,
                AInterruptCallback,
                AOptions
            ).ToResult();
        }

        /// <summary>
        /// Seek to a specified offset in the file.
        /// </summary>
        /// <param name="AContext">The <see cref="Unsafe.AVIOContext"/>.</param>
        /// <param name="AOffset">The offset in bytes.</param>
        /// <param name="AOrigin">The <see cref="AVIOSeekOrigin"/>.</param>
        /// <param name="AFlags">The <see cref="AVIOSeekFlags"/>.</param>
        /// <returns><see cref="Result{T}"/> wrapping the arrived-at position.</returns>
        [MustUseReturnValue]
        public static Result<long> Seek(
            Fixed<Unsafe.AVIOContext> AContext,
            long AOffset,
            AVIOSeekOrigin AOrigin = AVIOSeekOrigin.Begin,
            AVIOSeekFlags AFlags = AVIOSeekFlags.None
        )
        {
            // No null-check necessary.

            var result = Unsafe.ffmpeg.avio_seek(AContext, AOffset, (int)AOffset | (int)AFlags);
            if (result < 0) return new FFmpegError((int)result);

            return result;
        }

        /// <summary>
        /// Get the size of the file in bytes.
        /// </summary>
        /// <param name="AContext">The <see cref="Unsafe.AVIOContext"/>.</param>
        /// <returns><see cref="Result{T}"/> wrapping the file size in bytes.</returns>
        [MustUseReturnValue]
        public static Result<long> Size(Fixed<Unsafe.AVIOContext> AContext)
        {
            // No null-check necessary.

            var result = Unsafe.ffmpeg.avio_size(AContext);
            if (result < 0) return new FFmpegError((int)result);

            return result;
        }

        /// <summary>
        /// Skip the next few bytes in the file.
        /// </summary>
        /// <param name="AContext">The <see cref="Unsafe.AVIOContext"/>.</param>
        /// <param name="ACount">The number of bytes to skip.</param>
        /// <returns><see cref="Result{T}"/> wrapping the new file position.</returns>
        [MustUseReturnValue]
        public static Result<long> Skip(Fixed<Unsafe.AVIOContext> AContext, long ACount)
        {
            // No null-check necessary.

            var result = Unsafe.ffmpeg.avio_skip(AContext, ACount);
            if (result < 0) return new FFmpegError((int)result);

            return result;
        }

        /// <summary>
        /// Get the current position in the file.
        /// </summary>
        /// <param name="AContext">The <see cref="Unsafe.AVIOContext"/>.</param>
        /// <returns><see cref="Result{T}"/> wrapping the file position.</returns>
        [MustUseReturnValue]
        public static Result<long> Tell(Fixed<Unsafe.AVIOContext> AContext)
        {
            // No null-check necessary.

            var result = Unsafe.ffmpeg.avio_seek(AContext, 0, (int)AVIOSeekOrigin.Current);
            if (result < 0) return new FFmpegError((int)result);

            return result;
        }
    }
    // ReSharper restore errors
}
