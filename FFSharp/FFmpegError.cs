using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

using JetBrains.Annotations;

namespace FFSharp
{
    /// <summary>
    /// Wrapper <see cref="Exception"/> for FFmpeg library defined error codes.
    /// </summary>
    [PublicAPI]
    [Serializable]
    public sealed class FFmpegError : FFmpegException
    {
        const string C_Message = "Encountered an FFmpeg error.";
        const string C_NoDescription = "(No description available.)";

        [NotNull]
        static unsafe string GetDescription(int ACode)
        {
            // Stack-allocate a 1kb buffer for the error string.
            const int C_BufferSize = 1024;
            var abBuffer = stackalloc byte[C_BufferSize];

            if (FFmpeg.AutoGen.ffmpeg.av_strerror(ACode, abBuffer, C_BufferSize) < 0)
            {
                // Error fetching message.
                return C_NoDescription;
            }

            return Marshal.PtrToStringAnsi((IntPtr)abBuffer) ?? C_NoDescription;
        }

        /// <summary>
        /// Create a new <see cref="FFmpegError"/> instance with the specified error code and a
        /// generic error message.
        /// </summary>
        /// <param name="ACode">The FFmpeg error code.</param>
        internal FFmpegError(int ACode)
            : this(null, ACode, GetDescription(ACode))
        { }
        /// <summary>
        /// Create a new <see cref="FFmpegError"/> instance with the specified error code and
        /// custom error message.
        /// </summary>
        /// <param name="AMessage">The custom error message.</param>
        /// <param name="ACode">The FFmpeg error code.</param>
        internal FFmpegError([NotNull] string AMessage, int ACode)
            : this(AMessage, ACode, GetDescription(ACode))
        { }
        FFmpegError(
            [CanBeNull] string AMessage,
            int ACode,
            string ADescription
        )
            : base($"{AMessage ?? C_Message}: [{ACode}] {ADescription}")
        {
            Code = ACode;
            Description = ADescription;
        }

        /// <summary>
        /// Get the FFmpeg error code.
        /// </summary>
        public int Code { get; }
        /// <summary>
        /// Get the FFmpeg error description.
        /// </summary>
        [NotNull]
        public string Description { get; }

        #region Serializable implementation
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        FFmpegError([NotNull] SerializationInfo AInfo, StreamingContext AContext)
            : base(AInfo, AContext)
        {
            Code = AInfo.GetInt32("FFmpegCode");
            Description = AInfo.GetString("FFmpegDescription");
        }

        /// <inheritdoc />
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo AInfo, StreamingContext AContext)
        {
            AInfo.AddValue("FFmpegCode", Code);
            AInfo.AddValue("FFmpegDescription", Description);

            base.GetObjectData(AInfo, AContext);
        }
        #endregion
    }
}