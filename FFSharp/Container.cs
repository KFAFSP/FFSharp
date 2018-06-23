using System.Diagnostics;

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
    public abstract class Container : Disposable
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
            Debug.Assert(
                !ARef.IsNull,
                "Ref is null.",
                "This indicates a severe logic error in the code."
            );

            Ref = ARef;
            Mode = AMode;
        }
        /// <summary>
        /// Create a new <see cref="Container"/> instance.
        /// </summary>
        /// <exception cref="BadAllocationException">Error allocating context.</exception>
        internal Container()
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
        /// Get the <see cref="StreamOpenMode"/>.
        /// </summary>
        public StreamOpenMode Mode { get; protected set; }
    }
    // ReSharper restore errors
}
