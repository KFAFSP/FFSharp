using System.Diagnostics;

using FFSharp.Native;

using JetBrains.Annotations;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp
{
    /// <summary>
    /// Represents a stream of data inside a <see cref="Container"/>.
    /// </summary>
    // ReSharper disable errors
    [PublicAPI]
    public abstract class Stream : Disposable
    {
        /// <summary>
        /// Internal reference to the underlying <see cref="Unsafe.AVStream"/>.
        /// </summary>
        internal Ref<Unsafe.AVStream> Ref { get; }

        /// <summary>
        /// Create a new <see cref="Stream"/> instance.
        /// </summary>
        /// <param name="ARef">The <see cref="Unsafe.AVStream"/>.</param>
        internal Stream(Ref<Unsafe.AVStream> ARef)
        {
            Debug.Assert(
                !ARef.IsNull,
                "Ref is null.",
                "This indicates a severe logic error in the code."
            );

            Ref = ARef;
        }

        /// <summary>
        /// Get the index in the container.
        /// </summary>
        public unsafe int Index => Ref.Ptr->index;
        /// <summary>
        /// Get the format specific stream ID.
        /// </summary>
        public unsafe int Id => Ref.Ptr->id;
        /// <summary>
        /// Get the stream time base <see cref="Rational"/>.
        /// </summary>
        public unsafe Rational TimeBase => Ref.Ptr->time_base;
    }
    // ReSharper restore errors
}
