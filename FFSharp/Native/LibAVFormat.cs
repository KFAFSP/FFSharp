using System;
using System.Collections;
using System.Collections.Generic;

using JetBrains.Annotations;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Native
{
    /// <summary>
    /// Provides top-level functions of the avformat library.
    /// </summary>
    // ReSharper disable errors
    internal static unsafe class LibAVFormat
    {
        [NotNull]
        static readonly Lazy<uint> _FVersion;
        [NotNull]
        static readonly Lazy<string> _FConfiguration;
        [NotNull]
        static readonly Lazy<string> _FLicense;

        static LibAVFormat()
        {
            _FVersion = new Lazy<uint>(Unsafe.ffmpeg.avformat_version);
            _FConfiguration = new Lazy<string>(
                () => Unsafe.ffmpeg.avformat_configuration() ?? "N/A"
            );
            _FLicense = new Lazy<string>(
                () => Unsafe.ffmpeg.avformat_license() ?? "N/A"
            );
        }

        class ProtocolEnumerator : IEnumerator<string>
        {
            readonly bool FOutput;

            void* FState;
            bool FStarted;

            public ProtocolEnumerator(bool AOutput)
            {
                FOutput = AOutput;

                Reset();
            }

            #region IDisposable
            public void Dispose() { }
            #endregion

            #region IEnumerator
            public void Reset()
            {
                FStarted = false;
                FState = null;
                Current = null;
            }

            public bool MoveNext()
            {
                if (FStarted && Current == null)
                    return false;

                FStarted = true;
                fixed (void** state = &FState)
                    Current = Unsafe.ffmpeg.avio_enum_protocols(state, FOutput ? 1 : 0);

                return Current != null;
            }

            object IEnumerator.Current => Current;
            #endregion

            #region IEnumerator<string>
            public string Current { get; private set; }
            #endregion
        }

        /// <summary>
        /// Enumerate all available protocols.
        /// </summary>
        /// <param name="AOutput">
        /// On <see langword="true"/> enumerate output protocols; otherwise input protocols.
        /// </param>
        /// <returns>An <see cref="IEnumerator{T}"/> of all available protocols.</returns>
        public static IEnumerator<string> EnumProtocols(bool AOutput)
            => new ProtocolEnumerator(AOutput);

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
