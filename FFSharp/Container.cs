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
    public sealed class Container
    {
        /// <summary>
        /// Get the internal reference to the <see cref="Unsafe.AVFormatContext"/>.
        /// </summary>
        internal Ref<Unsafe.AVFormatContext> Ref;

        /// <summary>
        /// Create a new <see cref="Container"/> instance.
        /// </summary>
        /// <param name="ARef">The <see cref="Unsafe.AVFormatContext"/>.</param>
        internal Container(Ref<Unsafe.AVFormatContext> ARef)
        {
            Ref = ARef;
        }
        /// <summary>
        /// Create a new <see cref="Container"/> instance.
        /// </summary>
        public Container()
            : this(AVFormatContext.Alloc())
        { }
    }
    // ReSharper restore errors
}
