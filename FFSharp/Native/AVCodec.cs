using System;
using System.Collections;
using System.Collections.Generic;

using JetBrains.Annotations;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Native
{
    /// <summary>
    /// Wrapper class for the libavcodec library.
    /// </summary>
    // ReSharper disable errors
    [PublicAPI]
    public static class AVCodec
    {
        /// <summary>
        /// <see cref="IEnumerator{T}"/> over all supported codecs.
        /// </summary>
        internal sealed unsafe class Enumerator : Disposable,
            IEnumerator<Ref<Unsafe.AVCodec>>
        {
            bool FStarted;
            void* FState;

            /// <summary>
            /// Create a new <see cref="Enumerator"/> instance.
            /// </summary>
            public Enumerator()
            {
                Reset();
            }

            #region IEnumerator<Ref<Unsafe.AVCodec>>
            /// <inheritdoc />
            public bool MoveNext()
            {
                if (FStarted && Current.IsNull)
                {
                    return false;
                }

                FStarted = true;
                fixed (void** state = &FState)
                {
                    Current = Unsafe.ffmpeg.av_codec_iterate(state);
                }

                return !Current.IsNull;
            }
            /// <inheritdoc />
            public void Reset()
            {
                FStarted = false;
                Current = null;
            }

            /// <inheritdoc />
            public Ref<Unsafe.AVCodec> Current { get; private set; }
            #endregion

            #region IEnumerator
            object IEnumerator.Current => Current;
            #endregion
        }

        /// <summary>
        /// Get an <see cref="IEnumerable{T}"/> of all supported codecs.
        /// </summary>
        [NotNull]
        internal static IEnumerable<Ref<Unsafe.AVCodec>> Codecs { get; } =
            new FactoryEnumerable<Ref<Unsafe.AVCodec>>(() => new Enumerator());

        static AVCodec()
        {
            _FVersion = new Lazy<uint>(Unsafe.ffmpeg.avcodec_version);
            _FConfiguration = new Lazy<string>(() =>
            {
                return Unsafe.ffmpeg.avcodec_configuration()
                       ?? throw new FFmpegException("Error fetching configuration string.");
            });
            _FLicense = new Lazy<string>(() =>
            {
                return Unsafe.ffmpeg.avcodec_license()
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
        [NotNull]
        public static string Configuration => _FConfiguration.Value;
        /// <summary>
        /// Get the library license string.
        /// </summary>
        [NotNull]
        public static string License => _FLicense.Value;
        #endregion
    }
    // ReSharper restore errors
}
