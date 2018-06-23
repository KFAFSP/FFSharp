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
    /// Represents a container used in FFmpeg.
    /// </summary>
    // ReSharper disable errors
    [PublicAPI]
    public sealed class Container : Disposable
    {
        /// <summary>
        /// Get the internal reference to the <see cref="Unsafe.AVFormatContext"/>.
        /// </summary>
        internal Ref<Unsafe.AVFormatContext> Ref;

        /// <summary>
        /// Create a new <see cref="Container"/> instance.
        /// </summary>
        /// <param name="ARef">The <see cref="Unsafe.AVFormatContext"/>.</param>
        /// <param name="AMode">The <see cref="StreamOpenMode"/>.</param>
        internal Container(Ref<Unsafe.AVFormatContext> ARef, StreamOpenMode AMode)
        {
            Ref = ARef;
            Mode = AMode;
        }
        /// <summary>
        /// Create a new <see cref="Container"/> instance.
        /// </summary>
        /// <exception cref="BadAllocationException">Error allocating context.</exception>
        public Container()
            : this(AVFormatContext.Alloc(), StreamOpenMode.Closed)
        { }

        #region Disposable overrides
        /// <inheritdoc />
        protected override unsafe void Dispose(bool ADisposing)
        {
            switch (Mode)
            {
                case StreamOpenMode.Input:
                    AVFormatContext.CloseInput(ref Ref);
                    break;

                case StreamOpenMode.Output:
                    // Close output AVIOContext first.
                    Ref<Unsafe.AVIOContext> pb = Ref.Ptr->pb;
                    Ref.Ptr->pb = null;
                    AVIO.Close(ref pb);

                    AVFormatContext.Free(ref Ref);
                    break;

                case StreamOpenMode.Closed:
                    AVFormatContext.Free(ref Ref);
                    break;
            }

            Mode = StreamOpenMode.Closed;
            base.Dispose(ADisposing);
        }
        #endregion

        /// <summary>
        /// Close this <see cref="Container"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        public void Close()
        {
            ThrowIfDisposed();

            Dispose();
        }

        /// <summary>
        /// Open an URL for input.
        /// </summary>
        /// <param name="AUrl">The input URL.</param>
        /// <param name="ADemuxer">
        /// The <see cref="Demuxer"/> to use, or <see langword="null"/> to use default.
        /// </param>
        /// <param name="AOptions">
        /// The options <see cref="Dictionary"/>, or <see langword="null"/> to ignore.
        /// </param>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        /// <exception cref="InvalidOperationException">Stream is already open.</exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="AUrl"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="FFmpegError">Error opening container.</exception>
        [ContractAnnotation("AUrl: null => halt")]
        public void OpenInput(
            string AUrl,
            [CanBeNull] Demuxer ADemuxer = null,
            [CanBeNull] Dictionary AOptions = null
        )
        {
            ThrowIfDisposed();

            if (Mode != StreamOpenMode.Closed)
            {
                throw new InvalidOperationException("Stream is already open.");
            }

            if (AUrl == null)
            {
                throw new ArgumentNullException(nameof(AUrl));
            }

            Ref<Unsafe.AVDictionary> opt = null;

            // If options were provided, input them.
            if (AOptions != null)
            {
                opt = AOptions.Ref;
            }

            try
            {
                AVFormatContext.OpenInput(
                    ref Ref,
                    AUrl,
                    ADemuxer != null ? ADemuxer.Ref : null,
                    ref opt
                );

                Mode = StreamOpenMode.Input;
            }
            finally
            {
                if (AOptions != null)
                {
                    // Options were provided, update them.
                    AOptions.Ref = opt;
                }
                else
                {
                    // Options are unwanted, free them.
                    AVDictionary.Free(ref opt);
                }
            }
        }

        /// <summary>
        /// Get the <see cref="StreamOpenMode"/>.
        /// </summary>
        public StreamOpenMode Mode { get; private set; }
    }
    // ReSharper restore errors
}
