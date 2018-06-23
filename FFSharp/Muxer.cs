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
    /// Represents a muxer used by FFmpeg.
    /// </summary>
    // ReSharper disable errors
    [PublicAPI]
    public sealed class Muxer
    {
        /// <summary>
        /// Get an internalized <see cref="Muxer"/>.
        /// </summary>
        /// <param name="ARef">The <see cref="Unsafe.AVOutputFormat"/>.</param>
        /// <returns>The internalized <see cref="Muxer"/> instance.</returns>
        internal static Muxer Of(Ref<Unsafe.AVOutputFormat> ARef)
        {
            return Internalized<Unsafe.AVOutputFormat, Muxer>.Of(ARef, X => new Muxer(X));
        }

        /// <summary>
        /// Get the internal reference to the <see cref="Unsafe.AVOutputFormat"/>.
        /// </summary>
        internal Ref<Unsafe.AVOutputFormat> Ref { get; }

        /// <summary>
        /// Get an <see cref="IEnumerable{T}"/> of all registered <see cref="Muxer"/>s.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public static IEnumerable<Muxer> Registered { get; } =
            new FactoryEnumerable<Muxer>(() => AVFormat.Muxers.Select(Of).GetEnumerator());

        unsafe Muxer(Ref<Unsafe.AVOutputFormat> ARef)
        {
            Debug.Assert(
                !ARef.IsNull,
                "Ref is null.",
                "This indicates a severe logic error in the code."
            );

            Ref = ARef;

            FName = new Lazy<string>(() =>
            {
                return Marshal.PtrToStringAnsi((IntPtr)Ref.Ptr->name)
                       ?? throw new FFmpegException("Error getting name.");
            });
            FDescription = new Lazy<string>(() =>
            {
                return Marshal.PtrToStringAnsi((IntPtr)Ref.Ptr->long_name);
            });
            FMimeType = new Lazy<string>(() =>
            {
                return Marshal.PtrToStringAnsi((IntPtr)Ref.Ptr->mime_type);
            });
            FExtensions = new Lazy<string[]>(() =>
            {
                var list = Marshal.PtrToStringAnsi((IntPtr) Ref.Ptr->extensions) ?? "";
                return list.Split(',');
            });
        }

        #region Lazy properties
        [NotNull]
        readonly Lazy<string> FName;
        [NotNull]
        readonly Lazy<string> FDescription;
        [NotNull]
        readonly Lazy<string> FMimeType;
        [NotNull]
        readonly Lazy<string[]> FExtensions;

        /// <summary>
        /// Get the name.
        /// </summary>
        /// <exception cref="FFmpegException">Error getting name.</exception>
        [NotNull]
        public string Name => FName.Value;
        /// <summary>
        /// Get the human readable description.
        /// </summary>
        [CanBeNull]
        public string Description => FDescription.Value;
        /// <summary>
        /// Get the MIME type string.
        /// </summary>
        [CanBeNull]
        public string MimeType => FMimeType.Value;
        /// <summary>
        /// Get the list of known file extensions.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public IEnumerable<string> Extensions => FExtensions.Value;
        #endregion

        /// <summary>
        /// Get the <see cref="AVMuxerFlags"/>.
        /// </summary>
        internal unsafe AVMuxerFlags Flags => (AVMuxerFlags)Ref.Ptr->flags;

        /// <summary>
        /// Get the default audio <see cref="Unsafe.AVCodecID"/>.
        /// </summary>
        internal unsafe Unsafe.AVCodecID AudioCodecId => Ref.Ptr->audio_codec;
        /// <summary>
        /// Get the default video <see cref="Unsafe.AVCodecID"/>.
        /// </summary>
        internal unsafe Unsafe.AVCodecID VideoCodecId => Ref.Ptr->video_codec;
        /// <summary>
        /// Get the default subtitle <see cref="Unsafe.AVCodecID"/>.
        /// </summary>
        internal unsafe Unsafe.AVCodecID SubtitleCodecId => Ref.Ptr->subtitle_codec;
        /// <summary>
        /// Get the default data <see cref="Unsafe.AVCodecID"/>.
        /// </summary>
        internal unsafe Unsafe.AVCodecID DataCodecId => Ref.Ptr->data_codec;
    }
    // ReSharper restore errors
}
