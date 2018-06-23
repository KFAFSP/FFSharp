using System;

namespace FFSharp.Native
{
    /// <summary>
    /// Enumeration of flags for <see cref="FFmpeg.AutoGen.AVDictionary"/> functions.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [Flags]
    internal enum AVDictFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        NONE = 0,

        /// <summary>
        /// Match key case-sesnsitive.
        /// </summary>
        AV_DICT_MATCH_CASE = 1,

        /// <summary>
        /// Match until end of match instead of value.
        /// </summary>
        AV_DICT_IGNORE_SUFFIX = 2,

        /// <summary>
        /// Do not copy the key string, but still free it on error.
        /// </summary>
        AV_DICT_DONT_STRDUP_KEY = 4,

        /// <summary>
        /// Do not copy the value string, but still free it on error.
        /// </summary>
        AV_DICT_DONT_STRDUP_VAL = 8,

        /// <summary>
        /// Do not overwrite values that already exist.
        /// </summary>
        AV_DICT_DONT_OVERWRITE = 16,

        /// <summary>
        /// Append to existing values instead of overwriting.
        /// </summary>
        AV_DICT_APPEND = 32
    }
}