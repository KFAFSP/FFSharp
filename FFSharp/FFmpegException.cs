using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security.Permissions;

using JetBrains.Annotations;

namespace FFSharp
{
    /// <summary>
    /// Base class for exceptions thrown by FFmpeg library components.
    /// </summary>
    [PublicAPI]
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class FFmpegException : Exception
    {
        const string C_Message = "Error in the FFmpeg submodule.";

        /// <summary>
        /// Create a new <see cref="FFmpegException"/> instance with the specified message.
        /// </summary>
        /// <param name="AMessage">The message string.</param>
        internal FFmpegException([CanBeNull] string AMessage = null)
            : this(AMessage, null)
        { }
        /// <summary>
        /// Create a new <see cref="FFmpegException"/> instance with the specified message and
        /// cause.
        /// </summary>
        /// <param name="AMessage">The message string.</param>
        /// <param name="ACause">The causing exception.</param>
        internal FFmpegException([CanBeNull] string AMessage, Exception ACause)
            : base(AMessage ?? C_Message, ACause)
        { }

        #region Serializable
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected FFmpegException([NotNull] SerializationInfo AInfo, StreamingContext AContext)
            : base(AInfo, AContext)
        { }
        #endregion
    }
}
