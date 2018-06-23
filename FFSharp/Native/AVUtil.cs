using System;

using JetBrains.Annotations;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Native
{
    /// <summary>
    /// Wrapper class for the libavutil library.
    /// </summary>
    [PublicAPI]
    public static class AVUtil
    {
        static AVUtil()
        {
            _FVersion = new Lazy<uint>(Unsafe.ffmpeg.avutil_version);
            _FConfiguration = new Lazy<string>(() =>
            {
                return Unsafe.ffmpeg.avutil_configuration()
                       ?? throw new FFmpegException("Error fetching configuration string.");
            });
            _FLicense = new Lazy<string>(() =>
            {
                return Unsafe.ffmpeg.avutil_license()
                       ?? throw new FFmpegException("Error fetching license string.");
            });
        }

        #region Lazy properties
        [NotNull]
        static readonly Lazy<uint> _FVersion;
        [NotNull]
        static readonly Lazy<string> _FConfiguration;
        [NotNull]
        static readonly Lazy<string> _FLicense;

        /// <summary>
        /// Get the library version number.
        /// </summary>
        public static uint Version => _FVersion.Value;
        /// <summary>
        /// Get the library configuration string.
        /// </summary>
        /// <exception cref="FFmpegException">Error fetching configuration string.</exception>
        [NotNull]
        public static string Configuration => _FConfiguration.Value;
        /// <summary>
        /// Get the library license string.
        /// </summary>
        /// <exception cref="FFmpegException">Error fetching license string.</exception>
        [NotNull]
        public static string License => _FLicense.Value;
        #endregion
    }
}
