using System;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Native
{
    /// <summary>
    /// Flags for the <see cref="Unsafe.AVDictionary"/>-related function family.
    /// </summary>
    [Flags]
    internal enum AVDictionaryFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// Match key case-sesnsitive.
        /// </summary>
        MatchCase = Unsafe.ffmpeg.AV_DICT_MATCH_CASE,
        /// <summary>
        /// Match key until end of needle instead of haystack.
        /// </summary>
        IgnoreSuffix = Unsafe.ffmpeg.AV_DICT_IGNORE_SUFFIX,

        /// <summary>
        /// Assume ownership of the keys, only valid if they were allocated with the
        /// <see cref="Unsafe.ffmpeg.av_malloc(ulong)"/> family.
        /// </summary>
        DoNotStrdupKeys = Unsafe.ffmpeg.AV_DICT_DONT_STRDUP_KEY,
        /// <summary>
        /// Assume ownership of the values, only valid if they were allocated with the
        /// <see cref="Unsafe.ffmpeg.av_malloc(ulong)"/> family.
        /// </summary>
        DoNotStrdupValues = Unsafe.ffmpeg.AV_DICT_DONT_STRDUP_VAL,

        /// <summary>
        /// Do not overwrite values that already exist.
        /// </summary>
        DoNotOverwrite = Unsafe.ffmpeg.AV_DICT_DONT_OVERWRITE,
        /// <summary>
        /// Append to existing values instead of overwriting.
        /// </summary>
        Append = Unsafe.ffmpeg.AV_DICT_APPEND
    }
}