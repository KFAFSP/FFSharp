using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

using FFSharp.Native;

using JetBrains.Annotations;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp
{
    /// <summary>
    /// Represents a codec used by FFmpeg.
    /// </summary>
    // ReSharper disable errors
    [PublicAPI]
    public sealed class Codec
    {
        /// <summary>
        /// Get an internalized <see cref="Codec"/>.
        /// </summary>
        /// <param name="ARef">The <see cref="Unsafe.AVCodec"/>.</param>
        /// <returns>The internalized <see cref="Codec"/> instance.</returns>
        internal static Codec Of(Ref<Unsafe.AVCodec> ARef)
        {
            return Internalized<Unsafe.AVCodec, Codec>.Of(ARef, X => new Codec(X));
        }

        /// <summary>
        /// Get the internal reference to the <see cref="Unsafe.AVCodec"/>.
        /// </summary>
        internal Ref<Unsafe.AVCodec> Ref { get; }

        /// <summary>
        /// Get an <see cref="IEnumerable{T}"/> of all registered <see cref="Codec"/>s.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public static IEnumerable<Codec> Registered { get; } =
            new FactoryEnumerable<Codec>(() => AVCodec.Codecs.Select(Of).GetEnumerator());

        unsafe Codec(Ref<Unsafe.AVCodec> ARef)
        {
            Debug.Assert(
                !ARef.IsNull,
                "Ref is null.",
                "This indicates a severe logic error in the code."
            );

            Ref = ARef;

            IsEncoder = Unsafe.ffmpeg.av_codec_is_encoder(Ref) != 0;
            IsDecoder = Unsafe.ffmpeg.av_codec_is_decoder(Ref) != 0;

            FName = new Lazy<string>(() =>
            {
                return Marshal.PtrToStringAnsi((IntPtr)Ref.Ptr->name)
                       ?? throw new FFmpegException("Error getting name.");
            });
            FDescription = new Lazy<string>(() =>
            {
                return Marshal.PtrToStringAnsi((IntPtr)Ref.Ptr->long_name);
            });
        }

        #region Lazy properties
        [NotNull]
        readonly Lazy<string> FName;
        [NotNull]
        readonly Lazy<string> FDescription;

        /// <summary>
        /// Get a value indicating whether this is an encoder.
        /// </summary>
        public bool IsEncoder { get; }
        /// <summary>
        /// Get a value indicating whether this is a decoder.
        /// </summary>
        public bool IsDecoder { get; }

        /// <summary>
        /// Get the name.
        /// </summary>
        [NotNull]
        public string Name => FName.Value;
        /// <summary>
        /// Get the human readable description.
        /// </summary>
        [CanBeNull]
        public string Description => FDescription.Value;
        #endregion
    }
    // ReSharper restore errors
}
