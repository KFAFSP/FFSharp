using System;

namespace FFSharp.Native
{
    /// <summary>
    /// Flags for <see cref="FFmpeg.AutoGen.AVDictionary"/>-related function family.
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
        MatchCase = FFmpeg.AutoGen.ffmpeg.AV_DICT_MATCH_CASE,
        /// <summary>
        /// Match key until end of needle instead of haystack.
        /// </summary>
        IgnoreSuffix = FFmpeg.AutoGen.ffmpeg.AV_DICT_IGNORE_SUFFIX,

        /// <summary>
        /// Do not copy the key string, but still free it on error.
        /// </summary>
        DoNotStrdupKeys = FFmpeg.AutoGen.ffmpeg.AV_DICT_DONT_STRDUP_KEY,
        /// <summary>
        /// Do not copy the value string, but still free it on error.
        /// </summary>
        DoNotStrdupValues = FFmpeg.AutoGen.ffmpeg.AV_DICT_DONT_STRDUP_VAL,

        /// <summary>
        /// Do not overwrite values that already exist.
        /// </summary>
        DoNotOverwrite = FFmpeg.AutoGen.ffmpeg.AV_DICT_DONT_OVERWRITE,
        /// <summary>
        /// Append to existing values instead of overwriting.
        /// </summary>
        Append = FFmpeg.AutoGen.ffmpeg.AV_DICT_APPEND
    }
}