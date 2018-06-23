using System;

using FFSharp.Native;

using JetBrains.Annotations;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp
{
    /// <summary>
    /// Specializes a <see cref="Container"/> for reading streams.
    /// </summary>
    // ReSharper disable errors
    [PublicAPI]
    public sealed class InputContainer : Container
    {
        /// <summary>
        /// Open the input context.
        /// </summary>
        /// <param name="AUrl">The input URL.</param>
        /// <param name="ADemuxer">The <see cref="Demuxer"/>.</param>
        /// <param name="AOptions">The options <see cref="Dictionary"/>.</param>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        /// <exception cref="InvalidOperationException">Stream is already open.</exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="AUrl"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="FFmpegError">Error opening context.</exception>
        public void Open(
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
        /// Find streams from here.
        /// </summary>
        /// <param name="AOptions">The options <see cref="Dictionary"/>.</param>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        /// <exception cref="InvalidOperationException">Stream is closed.</exception>
        /// <exception cref="FFmpegError">Error finding stream info.</exception>
        public void FindStreamInfo([CanBeNull] Dictionary AOptions = null)
        {
            ThrowIfDisposed();

            if (Mode != StreamOpenMode.Input)
            {
                throw new InvalidOperationException("Stream is closed.");
            }

            Ref<Unsafe.AVDictionary> opt = null;
            if (AOptions != null)
            {
                opt = AOptions.Ref;
            }

            try
            {
                AVFormatContext.FindStreamInfo(Ref, ref opt);
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
    }
    // ReSharper restore errors
}