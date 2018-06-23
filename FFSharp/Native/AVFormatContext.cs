using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

using JetBrains.Annotations;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Native
{
    /// <summary>
    /// Provides methods that operate on <see cref="Unsafe.AVFormatContext"/>.
    /// </summary>
    // ReSharper disable errors
    internal static unsafe class AVFormatContext
    {
        /// <summary>
        /// Allocate a new <see cref="Unsafe.AVFormatContext"/>.
        /// </summary>
        /// <returns>A new <see cref="Unsafe.AVFormatContext"/>.</returns>
        [MustUseReturnValue]
        public static Ref<Unsafe.AVFormatContext> Alloc()
        {
            return FFmpegHelper.CheckAlloc<Unsafe.AVFormatContext>(
                Unsafe.ffmpeg.avformat_alloc_context()
            );
        }

        /// <summary>
        /// Free a <see cref="Unsafe.AVFormatContext"/>.
        /// </summary>
        /// <param name="ARef">The <see cref="Unsafe.AVFormatContext"/>.</param>
        public static void Free(ref Ref<Unsafe.AVFormatContext> ARef)
        {
            // No null-check necessary.

            Unsafe.ffmpeg.avformat_free_context(ARef);
            ARef = null;
        }
    }
    // ReSharper restore errors
}
