using System;
using System.Collections;
using System.Collections.Generic;

using JetBrains.Annotations;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Native
{
    /// <summary>
    /// Wrapper class for the libavformat library.
    /// </summary>
    // ReSharper disable errors
    [PublicAPI]
    public static class AVFormat
    {
        /// <summary>
        /// <see cref="IEnumerator{T}"/> over all supported muxers.
        /// </summary>
        internal sealed unsafe class MuxerEnumerator : Disposable,
            IEnumerator<Ref<Unsafe.AVOutputFormat>>
        {
            bool FStarted;
            void* FState;

            /// <summary>
            /// Create a new <see cref="MuxerEnumerator"/> instance.
            /// </summary>
            public MuxerEnumerator()
            {
                Reset();
            }

            #region IEnumerator<Ref<Unsafe.AVOutputFormat>>
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
                    Current = Unsafe.ffmpeg.av_muxer_iterate(state);
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
            public Ref<Unsafe.AVOutputFormat> Current { get; private set; }
            #endregion

            #region IEnumerator
            object IEnumerator.Current => Current;
            #endregion
        }

        /// <summary>
        /// Get an <see cref="IEnumerable{T}"/> of all supported muxers.
        /// </summary>
        [NotNull]
        internal static IEnumerable<Ref<Unsafe.AVOutputFormat>> Muxers { get; } =
            new FactoryEnumerable<Ref<Unsafe.AVOutputFormat>>(() => new MuxerEnumerator());

        /// <summary>
        /// <see cref="IEnumerator{T}"/> over all supported demuxers.
        /// </summary>
        internal sealed unsafe class DemuxerEnumerator : Disposable,
            IEnumerator<Ref<Unsafe.AVInputFormat>>
        {
            bool FStarted;
            void* FState;

            /// <summary>
            /// Create a new <see cref="MuxerEnumerator"/> instance.
            /// </summary>
            public DemuxerEnumerator()
            {
                Reset();
            }

            #region IEnumerator<Ref<Unsafe.AVInputFormat>>
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
                    Current = Unsafe.ffmpeg.av_demuxer_iterate(state);
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
            public Ref<Unsafe.AVInputFormat> Current { get; private set; }
            #endregion

            #region IEnumerator
            object IEnumerator.Current => Current;
            #endregion
        }

        /// <summary>
        /// Get an <see cref="IEnumerable{T}"/> of all supported demuxers.
        /// </summary>
        [NotNull]
        internal static IEnumerable<Ref<Unsafe.AVInputFormat>> Demuxers { get; } =
            new FactoryEnumerable<Ref<Unsafe.AVInputFormat>>(() => new DemuxerEnumerator());

        static AVFormat()
        {
            _FVersion = new Lazy<uint>(Unsafe.ffmpeg.avformat_version);
            _FConfiguration = new Lazy<string>(() =>
            {
                return Unsafe.ffmpeg.avformat_configuration()
                       ?? throw new FFmpegException("Error fetching configuration string.");
            });
            _FLicense = new Lazy<string>(() =>
            {
                return Unsafe.ffmpeg.avformat_license()
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
