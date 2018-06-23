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
    /// Represents a demuxer used by FFmpeg.
    /// </summary>
    // ReSharper disable errors
    [PublicAPI]
    public sealed class Demuxer
    {
        /// <summary>
        /// Get an internalized <see cref="Demuxer"/>.
        /// </summary>
        /// <param name="ARef">The <see cref="Unsafe.AVInputFormat"/>.</param>
        /// <returns>The internalized <see cref="Demuxer"/> instance.</returns>
        internal static Demuxer Of(Ref<Unsafe.AVInputFormat> ARef)
        {
            return Internalized<Unsafe.AVInputFormat, Demuxer>.Of(ARef, X => new Demuxer(X));
        }

        /// <summary>
        /// Get an <see cref="IEnumerable{T}"/> of all registered <see cref="Demuxer"/>s.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public static IEnumerable<Demuxer> Registered { get; } =
            new FactoryEnumerable<Demuxer>(() => AVFormat.Demuxers.Select(Of).GetEnumerator());

        /// <summary>
        /// Get the internal reference to the <see cref="Unsafe.AVInputFormat"/>.
        /// </summary>
        internal Ref<Unsafe.AVInputFormat> Ref { get; }

        unsafe Demuxer(Ref<Unsafe.AVInputFormat> ARef)
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
                var list = Marshal.PtrToStringAnsi((IntPtr)Ref.Ptr->extensions) ?? "";
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
        /// Get the <see cref="AVDemuxerFlags"/>.
        /// </summary>
        internal unsafe AVDemuxerFlags Flags => (AVDemuxerFlags)Ref.Ptr->flags;
    }
    // ReSharper restore errors
}
