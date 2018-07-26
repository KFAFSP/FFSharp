using System;

using JetBrains.Annotations;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Native
{
    /// <summary>
    /// Provides top-level functions of the avutil library.
    /// </summary>
    // ReSharper disable errors
    internal static unsafe class LibAVUtil
    {
        [NotNull]
        static readonly Lazy<uint> _FVersion;
        [NotNull]
        static readonly Lazy<string> _FConfiguration;
        [NotNull]
        static readonly Lazy<string> _FLicense;

        static LibAVUtil()
        {
            _FVersion = new Lazy<uint>(Unsafe.ffmpeg.avutil_version);
            _FConfiguration = new Lazy<string>(
                () => Unsafe.ffmpeg.avutil_configuration() ?? "N/A"
            );
            _FLicense = new Lazy<string>(
                () => Unsafe.ffmpeg.avutil_license() ?? "N/A"
            );
        }

        /// <summary>
        /// Get the library version number.
        /// </summary>
        public static uint Version => _FVersion.Value;
        /// <summary>
        /// Get the library build configuration.
        /// </summary>
        [NotNull]
        public static string Configuration => _FConfiguration.Value;
        /// <summary>
        /// Get the library license identifier.
        /// </summary>
        [NotNull]
        public static string License => _FLicense.Value;
    }
    // ReSharper restore errors
}
