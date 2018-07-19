using System.Diagnostics;

using JetBrains.Annotations;

using Whetstone.Contracts;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Native
{
    /// <summary>
    /// Provides methods that operate on <see cref="Unsafe.AVFrame"/>.
    /// </summary>
    // ReSharper disable errors
    internal static unsafe class AVFrame
    {
        /// <summary>
        /// Allocate a new <see cref="Unsafe.AVFrame"/>.
        /// </summary>
        /// <returns><see cref="Result{T}"/> with the new <see cref="Unsafe.AVFrame"/>.</returns>
        [MustUseReturnValue]
        public static Result<Fixed<Unsafe.AVFrame>> Alloc()
        {
            Fixed<Unsafe.AVFrame> frame = Unsafe.ffmpeg.av_frame_alloc();
            if (frame.IsNull) return new BadAllocationException(typeof(Unsafe.AVFrame));

            return frame;
        }

        /// <summary>
        /// Clone a <see cref="Unsafe.AVFrame"/>.
        /// </summary>
        /// <param name="AFrame">The source <see cref="Unsafe.AVFrame"/>.</param>
        /// <returns><see cref="Result{T}"/> with the new <see cref="Unsafe.AVFrame"/>.</returns>
        [MustUseReturnValue]
        public static Result<Fixed<Unsafe.AVFrame>> Clone(Fixed<Unsafe.AVFrame> AFrame)
        {
            Debug.Assert(
                !AFrame.IsNull,
                "Frame is null.",
                "This indicates a contract violation."
            );

            Fixed<Unsafe.AVFrame> frame = Unsafe.ffmpeg.av_frame_clone(AFrame);
            if (frame.IsNull) return new FFmpegException("Error cloning frame.");

            return frame;
        }

        /// <summary>
        /// Free a <see cref="Unsafe.AVFrame"/>.
        /// </summary>
        /// <param name="AFrame">The <see cref="Unsafe.AVFrame"/>.</param>
        /// <remarks>
        /// Sets <paramref name="AFrame"/>'s target to <see langword="null"/>.
        /// </remarks>
        public static void Free(Movable<Unsafe.AVFrame> AFrame)
        {
            // No null-check necessary.

            Unsafe.ffmpeg.av_frame_free(AFrame);
        }

        /// <summary>
        /// Overwrite the destination frame with the source by moving and reset source.
        /// </summary>
        /// <param name="ADst">The destination <see cref="Unsafe.AVFrame"/>.</param>
        /// <param name="ASrc">The source <see cref="Unsafe.AVFrame"/>.</param>
        public static void MoveRef(Fixed<Unsafe.AVFrame> ADst, Fixed<Unsafe.AVFrame> ASrc)
        {
            Debug.Assert(
                !ADst.IsNull,
                "Dst is null.",
                "This indicates a contract violation."
            );

            Debug.Assert(
                !ASrc.IsNull,
                "Src is null.",
                "This indicates a contract violation."
            );

            Unsafe.ffmpeg.av_frame_move_ref(ADst, ASrc);
        }

        /// <summary>
        /// Setup a new reference to the data of the source frame in the destination frame.
        /// </summary>
        /// <param name="ADst">The destination <see cref="Unsafe.AVFrame"/>.</param>
        /// <param name="ASrc">The source <see cref="Unsafe.AVFrame"/>.</param>
        /// <returns><see cref="Result"/> of the operation.</returns>
        [MustUseReturnValue]
        public static Result Ref(Fixed<Unsafe.AVFrame> ADst, Fixed<Unsafe.AVFrame> ASrc)
        {
            Debug.Assert(
                !ADst.IsNull,
                "Dst is null.",
                "This indicates a contract violation."
            );

            Debug.Assert(
                !ASrc.IsNull,
                "Src is null.",
                "This indicates a contract violation."
            );

            return Unsafe.ffmpeg.av_frame_ref(ADst, ASrc).ToResult();
        }

        /// <summary>
        /// Unreference the data buffers of the specified <see cref="Unsafe.AVFrame"/>.
        /// </summary>
        /// <param name="AFrame">The <see cref="Unsafe.AVFrame"/>.</param>
        public static void Unref(Fixed<Unsafe.AVFrame> AFrame)
        {
            // No null-check necessary.

            Unsafe.ffmpeg.av_frame_unref(AFrame);
        }
    }
    // ReSharper restore errors
}
