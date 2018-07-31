using System;
using System.Diagnostics;

using JetBrains.Annotations;

using Whetstone.Contracts;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Native
{
    /// <summary>
    /// Provides methods that operate on <see cref="Unsafe.AVBufferRef"/>.
    /// </summary>
    // ReSharper disable errors
    internal static unsafe class AVBuffer
    {
        /// <summary>
        /// Allocate a new buffer and get a <see cref="Unsafe.AVBufferRef"/> to it.
        /// </summary>
        /// <param name="ASize">The size of the buffer in bytes.</param>
        /// <returns>
        /// <see cref="Result{T}"/> wrapping the new <see cref="Unsafe.AVBufferRef"/>.
        /// </returns>
        [MustUseReturnValue]
        public static Result<Fixed<Unsafe.AVBufferRef>> Alloc(int ASize)
        {
            Debug.Assert(
                ASize >= 0,
                "Size is negative.",
                "This indicates a contract violation."
            );

            Fixed<Unsafe.AVBufferRef> buffer = Unsafe.ffmpeg.av_buffer_alloc(ASize);
            if (buffer.IsNull) return new BadAllocationException(typeof(Unsafe.AVBufferRef));

            return buffer;
        }

        /// <summary>
        /// Allocate a new zeroed buffer and get a <see cref="Unsafe.AVBufferRef"/> to it.
        /// </summary>
        /// <param name="ASize">The size of the buffer in bytes.</param>
        /// <returns>
        /// <see cref="Result{T}"/> wrapping the new <see cref="Unsafe.AVBufferRef"/>.
        /// </returns>
        [MustUseReturnValue]
        public static Result<Fixed<Unsafe.AVBufferRef>> AllocZ(int ASize)
        {
            Debug.Assert(
                ASize >= 0,
                "Size is negative.",
                "This indicates a contract violation."
            );

            Fixed<Unsafe.AVBufferRef> buffer = Unsafe.ffmpeg.av_buffer_allocz(ASize);
            if (buffer.IsNull) return new BadAllocationException(typeof(Unsafe.AVBufferRef));

            return buffer;
        }

        /// <summary>
        /// Create a new buffer from taking ownership of existing data and return a
        /// <see cref="Unsafe.AVBufferRef"/> to it.
        /// </summary>
        /// <param name="AData">The underlying data array.</param>
        /// <param name="ASize">The size of <paramref name="AData"/> in bytes.</param>
        /// <param name="AFree">
        /// The free callback, defaults to a wrapped <see cref="Unsafe.ffmpeg.av_free"/> in
        /// the form of <see cref="Unsafe.ffmpeg.av_buffer_default_free"/>.
        /// </param>
        /// <param name="AOpaque">The opaque private tag pointer.</param>
        /// <param name="AFlags">The <see cref="AVBufferFlags"/>.</param>
        /// <returns>
        /// <see cref="Result{T}"/> wrapping the new <see cref="Unsafe.AVBufferRef"/>.
        /// </returns>
        [MustUseReturnValue]
        public static Result<Fixed<Unsafe.AVBufferRef>> Create(
            Fixed<byte> AData,
            int ASize,
            [CanBeNull] Action<Fixed, Fixed<byte>> AFree = null,
            Fixed AOpaque = default,
            AVBufferFlags AFlags = AVBufferFlags.None
        )
        {
            Debug.Assert(
                !AData.IsNull,
                "Data is null.",
                "This indicates a contract violation."
            );

            Debug.Assert(
                ASize >= 0,
                "Size is negative.",
                "This indicates a contract violation."
            );

            Unsafe.av_buffer_create_free MakeFreeDelegate()
            {
                if (AFree == null) return null;

                return (BOpaque, BBuffer) => AFree(BOpaque, BBuffer);
            }

            Fixed<Unsafe.AVBufferRef> buffer = Unsafe.ffmpeg.av_buffer_create(
                AData,
                ASize,
                MakeFreeDelegate(),
                AOpaque,
                (int)AFlags
            );
            if (buffer.IsNull) return new BadAllocationException(typeof(Unsafe.AVBufferRef));

            return buffer;
        }

        /// <summary>
        /// Get the opaque pointer from the underlying <see cref="Unsafe.AVBuffer"/>.
        /// </summary>
        /// <param name="ABuf">The <see cref="Unsafe.AVBufferRef"/>.</param>
        /// <returns>The opaque <see cref="Fixed"/> pointer.</returns>
        [Pure]
        public static Fixed GetOpaque(Fixed<Unsafe.AVBufferRef> ABuf)
        {
            Debug.Assert(
                !ABuf.IsNull,
                "Buf is null.",
                "This indicates a contract violation."
            );

            return Unsafe.ffmpeg.av_buffer_get_opaque(ABuf);
        }

        /// <summary>
        /// Get the number of references to a buffer.
        /// </summary>
        /// <param name="ABuf">The <see cref="Unsafe.AVBufferRef"/>.</param>
        /// <returns>The number of references to the underlying buffer.</returns>
        [Pure]
        public static int GetRefCount(Fixed<Unsafe.AVBufferRef> ABuf)
        {
            Debug.Assert(
                !ABuf.IsNull,
                "Buf is null.",
                "This indicates a contract violation."
            );

            return Unsafe.ffmpeg.av_buffer_get_ref_count(ABuf);
        }

        /// <summary>
        /// Get a value indicating whether a <see cref="Unsafe.AVBufferRef"/> is writable.
        /// </summary>
        /// <param name="ABuf">The <see cref="Unsafe.AVBufferRef"/>.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="ABuf"/> is the only reference to the
        /// underlying buffer and not read-only; otherwise <see langword="false"/>.
        /// </returns>
        [Pure]
        public static bool IsWritable(Fixed<Unsafe.AVBufferRef> ABuf)
        {
            Debug.Assert(
                !ABuf.IsNull,
                "Buf is null.",
                "This indicates a contract violation."
            );

            return Unsafe.ffmpeg.av_buffer_is_writable(ABuf) == 1;
        }

        /// <summary>
        /// Ensure that the specified <see cref="Unsafe.AVBufferRef"/> is writable.
        /// </summary>
        /// <param name="ABuf">The <see cref="Unsafe.AVBufferRef"/>.</param>
        /// <returns><see cref="Result"/> of the operation.</returns>
        /// <remarks>
        /// If <paramref name="ABuf"/> is not writable, creates a copy that is writable.
        /// </remarks>
        [MustUseReturnValue]
        public static Result MakeWritable(Movable<Unsafe.AVBufferRef> ABuf)
        {
            Debug.Assert(
                ABuf.IsPresent,
                "Buf is absent.",
                "This indicates a contract violation."
            );

            return Unsafe.ffmpeg.av_buffer_make_writable(ABuf).ToResult();
        }

        /// <summary>
        /// Reallocate an existing or allocate a new buffer.
        /// </summary>
        /// <param name="ABuf">The <see cref="Unsafe.AVBufferRef"/>.</param>
        /// <param name="ASize">The new size in bytes.</param>
        /// <returns><see cref="Result"/> of the operation.</returns>
        [MustUseReturnValue]
        public static Result Realloc(
            Movable<Unsafe.AVBufferRef> ABuf,
            int ASize
        )
        {
            Debug.Assert(
                !ABuf.IsNull,
                "Buf is null.",
                "This indicates a contract violation."
            );

            Debug.Assert(
                ASize >= 0,
                "Size is negative.",
                "This indicates a contract violation."
            );

            // No present-check necessary.

            return Unsafe.ffmpeg.av_buffer_realloc(ABuf, ASize).ToResult();
        }

        /// <summary>
        /// Reference an existing buffer.
        /// </summary>
        /// <param name="ABuf">The <see cref="Unsafe.AVBufferRef"/>.</param>
        /// <returns>
        /// <see cref="Result{T}"/> wrapping the new <see cref="Unsafe.AVBufferRef"/>.
        /// </returns>
        [MustUseReturnValue]
        public static Result<Fixed<Unsafe.AVBufferRef>> Ref(Fixed<Unsafe.AVBufferRef> ABuf)
        {
            Debug.Assert(
                !ABuf.IsNull,
                "Buf is null.",
                "This indicates a contract violation."
            );

            Fixed<Unsafe.AVBufferRef> buffer = Unsafe.ffmpeg.av_buffer_ref(ABuf);
            if (buffer.IsNull) return new FFmpegException("Error referencing buffer.");

            return buffer;
        }

        /// <summary>
        /// Unreference a <see cref="Unsafe.AVBufferRef"/>.
        /// </summary>
        /// <param name="ABuf">The <see cref="Unsafe.AVBufferRef"/>.</param>
        public static void Unref(Movable<Unsafe.AVBufferRef> ABuf)
        {
            // No null-check necessary.

            Unsafe.ffmpeg.av_buffer_unref(ABuf);
        }
    }
    // ReSharper restore errors
}
