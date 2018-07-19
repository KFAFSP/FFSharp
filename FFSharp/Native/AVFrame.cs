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
        /// Apply the cropping stored in the <see cref="Unsafe.AVFrame"/> to it's data pointers and
        /// size.
        /// </summary>
        /// <param name="AFrame">The <see cref="Unsafe.AVFrame"/>.</param>
        /// <param name="AFlags">The <see cref="AVFrameCropFlags"/>.</param>
        /// <returns><see cref="Result"/> of the operation.</returns>
        [MustUseReturnValue]
        public static Result ApplyCropping(
            Fixed<Unsafe.AVFrame> AFrame,
            AVFrameCropFlags AFlags = AVFrameCropFlags.None
        )
        {
            Debug.Assert(
                !AFrame.IsNull,
                "Frame is null.",
                "This indicates a contract violation."
            );

            return Unsafe.ffmpeg.av_frame_apply_cropping(AFrame, (int)AFlags).ToResult();
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
        /// Copy data from the source to the destination <see cref="Unsafe.AVFrame"/>.
        /// </summary>
        /// <param name="ADst">The destination <see cref="Unsafe.AVFrame"/>.</param>
        /// <param name="ASrc">The source <see cref="Unsafe.AVFrame"/>.</param>
        /// <returns><see cref="Result"/> of the operation.</returns>
        [MustUseReturnValue]
        public static Result Copy(Fixed<Unsafe.AVFrame> ADst, Fixed<Unsafe.AVFrame> ASrc)
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

            return Unsafe.ffmpeg.av_frame_copy(ADst, ASrc).ToResult();
        }

        /// <summary>
        /// Copy metadatadata from the source to the destination <see cref="Unsafe.AVFrame"/>.
        /// </summary>
        /// <param name="ADst">The destination <see cref="Unsafe.AVFrame"/>.</param>
        /// <param name="ASrc">The source <see cref="Unsafe.AVFrame"/>.</param>
        /// <returns><see cref="Result"/> of the operation.</returns>
        [MustUseReturnValue]
        public static Result CopyProps(Fixed<Unsafe.AVFrame> ADst, Fixed<Unsafe.AVFrame> ASrc)
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

            return Unsafe.ffmpeg.av_frame_copy_props(ADst, ASrc).ToResult();
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
        /// Allocate buffers for the <see cref="Unsafe.AVFrame"/>.
        /// </summary>
        /// <param name="AFrame">The <see cref="Unsafe.AVFrame"/>.</param>
        /// <param name="AAlign">The alignment, 0 for automatic.</param>
        /// <returns><see cref="Result"/> of the operation.</returns>
        [MustUseReturnValue]
        public static Result GetBuffer(Fixed<Unsafe.AVFrame> AFrame, int AAlign = 0)
        {
            Debug.Assert(
                !AFrame.IsNull,
                "Frame is null.",
                "This indicates a contract violation."
            );

            return Unsafe.ffmpeg.av_frame_get_buffer(AFrame, AAlign).ToResult();
        }

        /// <summary>
        /// Get <see cref="Unsafe.AVFrameSideData"/> of a <see cref="Unsafe.AVFrame"/>.
        /// </summary>
        /// <param name="AFrame">The <see cref="Unsafe.AVFrame"/>.</param>
        /// <param name="AType">The <see cref="Unsafe.AVFrameSideDataType"/>.</param>
        /// <returns>
        /// <see cref="Result"/> wrapping the <see cref="Unsafe.AVFrameSideData"/>.
        /// </returns>
        [Pure]
        public static Fixed<Unsafe.AVFrameSideData> GetSideData(
            Fixed<Unsafe.AVFrame> AFrame,
            Unsafe.AVFrameSideDataType AType
        )
        {
            Debug.Assert(
                !AFrame.IsNull,
                "Frame is null.",
                "This indicates a contract violation."
            );

            return Unsafe.ffmpeg.av_frame_get_side_data(AFrame, AType);
        }

        /// <summary>
        /// Get the <see cref="Unsafe.AVBufferRef"/> for the specified plane of a
        /// <see cref="Unsafe.AVFrame"/>.
        /// </summary>
        /// <param name="AFrame">The <see cref="Unsafe.AVFrame"/>.</param>
        /// <param name="APlane">The plane index.</param>
        /// <returns>
        /// <see cref="Result{T}"/> wrapping the plane's <see cref="Unsafe.AVBufferRef"/>.
        /// </returns>
        [Pure]
        public static Result<Fixed<Unsafe.AVBufferRef>> GetPlaneBuffer(
            Fixed<Unsafe.AVFrame> AFrame,
            int APlane
        )
        {
            Debug.Assert(
                !AFrame.IsNull,
                "Frame is null.",
                "This indicates a contract violation."
            );

            Fixed<Unsafe.AVBufferRef> buffer = Unsafe.ffmpeg.av_frame_get_plane_buffer(
                AFrame,
                APlane
            );
            if (buffer.IsNull) return new FFmpegException("Error getting plane buffer.");

            return buffer;
        }

        /// <summary>
        /// Get a value indicating whether the <see cref="Unsafe.AVFrame"/> is writable.
        /// </summary>
        /// <param name="AFrame">The <see cref="Unsafe.AVFrame"/>.</param>
        /// <returns>
        /// <see langword="true"/> if all underlying buffers are writable; otherwise
        /// <see langword="false"/>.
        /// </returns>
        [Pure]
        public static bool IsWritable(Fixed<Unsafe.AVFrame> AFrame)
        {
            Debug.Assert(
                !AFrame.IsNull,
                "Frame is null.",
                "This indicates a contract violation."
            );

            return Unsafe.ffmpeg.av_frame_is_writable(AFrame) > 0;
        }

        /// <summary>
        /// Ensure that the <see cref="Unsafe.AVFrame"/> is writable.
        /// </summary>
        /// <param name="AFrame">The <see cref="Unsafe.AVFrame"/>.</param>
        /// <returns><see cref="Result"/> of the operation.</returns>
        /// <remarks>
        /// If <paramref name="AFrame"/> is not writable, creates a copy that is writable.
        /// </remarks>
        [MustUseReturnValue]
        public static Result MakeWriteable(Fixed<Unsafe.AVFrame> AFrame)
        {
            Debug.Assert(
                !AFrame.IsNull,
                "Frame is null.",
                "This indicates a contract violation."
            );

            return Unsafe.ffmpeg.av_frame_make_writable(AFrame).ToResult();
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
        /// Allocate and add new side data to a <see cref="Unsafe.AVFrame"/>.
        /// </summary>
        /// <param name="AFrame">The <see cref="Unsafe.AVFrame"/>.</param>
        /// <param name="AType">The <see cref="Unsafe.AVPacketSideDataType"/>.</param>
        /// <param name="ASize">The size of the data in bytes.</param>
        /// <returns>
        /// <see cref="Result{T}"/> wrapping the newly added <see cref="Unsafe.AVFrameSideData"/>.
        /// </returns>
        [MustUseReturnValue]
        public static Result<Fixed<Unsafe.AVFrameSideData>> NewSideData(
            Fixed<Unsafe.AVFrame> AFrame,
            Unsafe.AVFrameSideDataType AType,
            int ASize
        )
        {
            Debug.Assert(
                !AFrame.IsNull,
                "Frame is null.",
                "This indicates a contract violation."
            );

            Debug.Assert(
                ASize >= 0,
                "Size is negative.",
                "This indicates a contract violation."
            );

            Fixed<Unsafe.AVFrameSideData> sideData = Unsafe.ffmpeg.av_frame_new_side_data(
                AFrame,
                AType,
                ASize
            );
            if (sideData.IsNull) return new FFmpegException("Error adding frame side data.");

            return sideData;
        }

        /// <summary>
        /// Add new side data to a <see cref="Unsafe.AVFrame"/> using an existing buffer.
        /// </summary>
        /// <param name="AFrame">The <see cref="Unsafe.AVFrame"/>.</param>
        /// <param name="AType">The <see cref="Unsafe.AVFrameSideDataType"/>.</param>
        /// <param name="ABuf">The <see cref="Unsafe.AVBufferRef"/>.</param>
        /// <returns>
        /// <see cref="Result{T}"/> wrapping the added <see cref="Unsafe.AVFrameSideData"/>.
        /// </returns>
        [MustUseReturnValue]
        public static Result<Fixed<Unsafe.AVFrameSideData>> NewSideDataFromBuf(
            Fixed<Unsafe.AVFrame> AFrame,
            Unsafe.AVFrameSideDataType AType,
            Fixed<Unsafe.AVBufferRef> ABuf
        )
        {
            Debug.Assert(
                !AFrame.IsNull,
                "Frame is null.",
                "This indicates a contract violation."
            );

            Debug.Assert(
                !ABuf.IsNull,
                "Buf is null.",
                "This indicates a contract violation."
            );

            Fixed<Unsafe.AVFrameSideData> sideData = Unsafe.ffmpeg.av_frame_new_side_data_from_buf(
                AFrame,
                AType,
                ABuf
            );
            if (sideData.IsNull) return new FFmpegException("Error adding frame side data.");

            return sideData;
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
        /// Remove side data from a <see cref="Unsafe.AVFrame"/>.
        /// </summary>
        /// <param name="AFrame">The <see cref="Unsafe.AVFrame"/></param>
        /// <param name="AType">The <see cref="Unsafe.AVFrameSideDataType"/>.</param>
        public static void RemoveSideData(
            Fixed<Unsafe.AVFrame> AFrame,
            Unsafe.AVFrameSideDataType AType
        )
        {
            Debug.Assert(
                !AFrame.IsNull,
                "Frame is null.",
                "This indicates a contract violation."
            );

            Unsafe.ffmpeg.av_frame_remove_side_data(AFrame, AType);
        }

        /// <summary>
        /// Get the description of the specified <see cref="Unsafe.AVFrameSideDataType"/>.
        /// </summary>
        /// <param name="AType">The <see cref="Unsafe.AVFrameSideDataType"/>.</param>
        /// <returns>
        /// The description for <paramref name="AType"/>, or <see langword="null"/> if unknown.
        /// </returns>
        [CanBeNull]
        public static string SideDataName(Unsafe.AVFrameSideDataType AType)
        {
            return Unsafe.ffmpeg.av_frame_side_data_name(AType);
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
