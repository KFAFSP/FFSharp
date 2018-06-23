using System;

using FFSharp.Native;

using JetBrains.Annotations;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp
{
    /// <summary>
    /// Specializes a <see cref="Container"/> for writing streams.
    /// </summary>
    // ReSharper disable errors
    [PublicAPI]
    public sealed class OutputContainer : Container
    {
        /// <summary>
        /// Enumeration of possible output stream states.
        /// </summary>
        public enum OutputState
        {
            /// <summary>
            /// The stream is closed.
            /// </summary>
            Closed,
            /// <summary>
            /// The stream is opened, but not initialized.
            /// </summary>
            Opened,
            /// <summary>
            /// The stream is ready for data.
            /// </summary>
            Ready,
            /// <summary>
            /// The stream was finalized.
            /// </summary>
            Finalized
        }

        #region Disposable overrides
        /// <inheritdoc />
        protected override void Dispose(bool ADisposing)
        {
            if (ADisposing && State == OutputState.Ready)
            {
                try
                {
                    EndData();
                }
                catch
                {
                    // Ignore this error here.
                }
            }

            base.Dispose(ADisposing);
            State = OutputState.Closed;
        }
        #endregion

        /// <summary>
        /// Open the output context.
        /// </summary>
        /// <param name="AUrl">The output URL.</param>
        /// <param name="AMuxer">The <see cref="Muxer"/>.</param>
        /// <param name="AOptions">The options <see cref="Dictionary"/>.</param>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        /// <exception cref="InvalidOperationException">Stream is already open.</exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="AUrl"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="AMuxer"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="FFmpegError">Error opening context.</exception>
        public unsafe void Open(
            string AUrl,
            Muxer AMuxer,
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
            if (AMuxer == null)
            {
                throw new ArgumentNullException(nameof(AMuxer));
            }

            Ref<Unsafe.AVIOContext> pb = Ref.Ptr->pb;

            Ref<Unsafe.AVDictionary> opt = null;
            if (AOptions != null)
            {
                opt = AOptions.Ref;
            }

            try
            {
                AVIO.Open(ref pb, AUrl, AVIOOpenFlags.AVIO_FLAG_WRITE, null, ref opt);
                Ref.Ptr->pb = pb;
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

            Ref.Ptr->oformat = AMuxer.Ref;
            Mode = StreamOpenMode.Output;
            State = OutputState.Opened;
        }

        /// <summary>
        /// Add a <see cref="Stream"/> to the container.
        /// </summary>
        /// <param name="ACodec"></param>
        public Stream AddStream(Codec ACodec)
        {
            ThrowIfDisposed();

            if (State != OutputState.Opened)
            {
                throw new InvalidOperationException("Stream is not in the Opened state.");
            }

            if (ACodec == null)
            {
                throw new ArgumentNullException(nameof(ACodec));
            }

            if (!ACodec.IsEncoder)
            {
                throw new ArgumentException("Not an encoder.", nameof(ACodec));
            }

            var stream = AVFormatContext.NewStream(Ref, ACodec.Ref);
            // TODO : Wrap AVStream.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Begin writing stream data.
        /// </summary>
        /// <param name="AOptions"></param>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        /// <exception cref="InvalidOperationException">
        /// Stream is not in the Opened state.
        /// </exception>
        /// <exception cref="FFmpegError">Error writing header.</exception>
        public void BeginData([CanBeNull] Dictionary AOptions = null)
        {
            ThrowIfDisposed();

            if (State != OutputState.Opened)
            {
                throw new InvalidOperationException("Stream is not in the Opened state.");
            }

            // TODO : Implement stream check.

            Ref<Unsafe.AVDictionary> opt = null;

            // If options were provided, input them.
            if (AOptions != null)
            {
                opt = AOptions.Ref;
            }

            try
            {
                AVFormatContext.WriteHeader(Ref, ref opt);
                State = OutputState.Ready;
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
        /// Finish writing stream data and finalize output.
        /// </summary>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        /// <exception cref="InvalidOperationException">
        /// Stream is not in the Ready state.
        /// </exception>
        /// <exception cref="FFmpegError">Error writing trailer.</exception>
        public void EndData()
        {
            ThrowIfDisposed();

            if (State != OutputState.Ready)
            {
                throw new InvalidOperationException("Stream is not in the Ready state.");
            }

            AVFormatContext.WriteTrailer(Ref);
            State = OutputState.Finalized;
        }

        /// <summary>
        /// Get the <see cref="OutputState"/>.
        /// </summary>
        public OutputState State { get; private set; } = OutputState.Closed;
    }
    // ReSharper restore errors
}