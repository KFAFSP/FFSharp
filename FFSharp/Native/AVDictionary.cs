using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

using JetBrains.Annotations;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Native
{
    /// <summary>
    /// Provides methods that operate on <see cref="Unsafe.AVDictionary"/>.
    /// </summary>
    // ReSharper disable errors
    internal static unsafe class AVDictionary
    {
        /// <summary>
        /// Allocate a new <see cref="Unsafe.AVDictionary"/>.
        /// </summary>
        /// <returns>A new <see cref="Unsafe.AVDictionary"/>.</returns>
        /// <remarks>
        /// Never fails as this amounts to returning <see langword="null"/>.
        /// </remarks>
        [MustUseReturnValue]
        public static Fixed<Unsafe.AVDictionary> Alloc() => null;

        /// <summary>
        /// Copy entries from one <see cref="Unsafe.AVDictionary"/> into another.
        /// </summary>
        /// <param name="ADst">The destination <see cref="Unsafe.AVDictionary"/>.</param>
        /// <param name="ASrc">The source <see cref="Unsafe.AVDictionary"/>.</param>
        /// <param name="AFlags">The <see cref="AVDictionaryFlags"/>.</param>
        /// <returns><see cref="Result"/> of the copy operation.</returns>
        /// <remarks>
        /// If the copy operation fails, the <paramref name="ADst"/> might still be changed and
        /// may also have been allocated. The caller must cleanup after an error if that is not
        /// intended.
        /// </remarks>
        [MustUseReturnValue]
        public static Result Copy(
            Movable<Unsafe.AVDictionary> ADst,
            Fixed<Unsafe.AVDictionary> ASrc,
            AVDictionaryFlags AFlags = AVDictionaryFlags.None
        )
        {
            // No null-check necessary.

            return Unsafe.ffmpeg.av_dict_copy(ADst, ASrc, (int) AFlags)
                .CheckFFmpeg("Error copying values.");
        }

        /// <summary>
        /// Get the number of entries in the <see cref="Unsafe.AVDictionary"/>.
        /// </summary>
        /// <param name="ADict">The <see cref="Unsafe.AVDictionary"/>.</param>
        /// <returns>The number of entries.</returns>
        /// <remarks>This is a constant time operation and prefferable to iterating.</remarks>
        [Pure]
        public static int Count(Fixed<Unsafe.AVDictionary> ADict)
        {
            // No null-check necessary.

            return Unsafe.ffmpeg.av_dict_count(ADict);
        }

        /// <summary>
        /// Free a <see cref="Unsafe.AVDictionary"/>, clearing all entries.
        /// </summary>
        /// <param name="ADict">The <see cref="Unsafe.AVDictionary"/>.</param>
        public static void Free(Movable<Unsafe.AVDictionary> ADict)
        {
            // No null-check necessary.

            Unsafe.ffmpeg.av_dict_free(ADict);
        }

        /// <summary>
        /// Find an <see cref="Unsafe.AVDictionaryEntry"/> in a <see cref="Unsafe.AVDictionary"/>.
        /// </summary>
        /// <param name="ADict">The <see cref="Unsafe.AVDictionary"/>.</param>
        /// <param name="AKey">The key match string.</param>
        /// <param name="APrev">
        /// The previously returned <see cref="Unsafe.AVDictionaryEntry"/>.
        /// </param>
        /// <param name="AFlags">The <see cref="AVDictionaryFlags"/>.</param>
        /// <returns>The matched <see cref="Unsafe.AVDictionaryEntry"/> if any.</returns>
        /// <remarks>
        /// Supplying the <see cref="AVDictionaryFlags.DoNotStrdupKeys"/> flag is invalid behaviour.
        /// </remarks>
        [Pure]
        public static Fixed<Unsafe.AVDictionaryEntry> Get(
            Fixed<Unsafe.AVDictionary> ADict,
            [NotNull] string AKey,
            Fixed<Unsafe.AVDictionaryEntry> APrev = default,
            AVDictionaryFlags AFlags = AVDictionaryFlags.None
        )
        {
            const AVDictionaryFlags C_ProhibitedFlags = AVDictionaryFlags.DoNotStrdupKeys;

            // No null-check necessary.

            Debug.Assert(
                AKey != null,
                "Key is null.",
                "This indicates a contract violation."
            );
            Debug.Assert(
                (AFlags & C_ProhibitedFlags) != AVDictionaryFlags.None,
                "Flag is prohibited.",
                "Some provided flag (DoNotStrdupKeys) is invalid on marshalled strings and " +
                "therefore prohibited. This indicates a contract violation."
            );

            // Strip the prohibited flags at all cost.
            AFlags &= ~C_ProhibitedFlags;

            return Unsafe.ffmpeg.av_dict_get(ADict, AKey, APrev, (int)AFlags);
        }

        /// <summary>
        /// Enumerate all <see cref="Unsafe.AVDictionaryEntry"/>s in a
        /// <see cref="Unsafe.AVDictionary"/>.
        /// </summary>
        /// <param name="ADict">The <see cref="Unsafe.AVDictionary"/>.</param>
        /// <param name="AKey">The match key.</param>
        /// <param name="AFlags">The <see cref="AVDictionaryFlags"/>.</param>
        /// <returns>
        /// An <see cref="IEnumerator{T}"/> over all matching
        /// <see cref="Unsafe.AVDictionaryEntry"/>s.
        /// </returns>
        [NotNull]
        [Pure]
        public static IEnumerator<Fixed<Unsafe.AVDictionaryEntry>> GetEnumerator(
            Fixed<Unsafe.AVDictionary> ADict,
            [NotNull] string AKey = "",
            AVDictionaryFlags AFlags = AVDictionaryFlags.IgnoreSuffix
        )
        {
            // No null-check necessary.

            Debug.Assert(
                AKey != null,
                "Key is null.",
                "This indicates a contract violation."
            );

            Fixed<Unsafe.AVDictionaryEntry> last = null;

            while (true)
            {
                last = Get(ADict, AKey, last, AFlags);
                if (!last) yield break;
                yield return last;
            }
        }

        /// <summary>
        /// Get a string representation of the <see cref="Unsafe.AVDictionary"/>.
        /// </summary>
        /// <param name="ADict">The <see cref="Unsafe.AVDictionary"/>.</param>
        /// <param name="AKeyValSep">The key-value separator char.</param>
        /// <param name="APairsSep">The pairs separator char.</param>
        /// <returns>A <see cref="Result{T}"/> of the string representation.</returns>
        [Pure]
        public static Result<string> GetString(
            Fixed<Unsafe.AVDictionary> ADict,
            byte AKeyValSep,
            byte APairsSep
        )
        {
            const byte C_Null = (byte) '\0';
            const byte C_Slash = (byte) '\\';

            // No null-check necessary.

            Debug.Assert(
                AKeyValSep != C_Null && AKeyValSep != C_Slash,
                "Key-value separator is invalid.",
                "The slash and zero terminator character are reserved and may not be used " +
                "for any separator. This indicates a contract violation."
            );
            Debug.Assert(
                APairsSep != C_Null && APairsSep != C_Slash,
                "Pairs separator is invalid.",
                "The slash and zero terminator character are reserved and may not be used " +
                "for any separator. This indicates a contract violation."
            );
            Debug.Assert(
                AKeyValSep != APairsSep,
                "Separators are equal.",
                "The two separators may not be identical. This indicates a contract violation."
            );

            byte* buffer;

            var getResult = Unsafe.ffmpeg.av_dict_get_string(
                ADict,
                &buffer,
                AKeyValSep,
                APairsSep
            ).CheckFFmpeg("Error building string.");

            if (!getResult)
            {
                return getResult.Error;
            }

            // Marshalling copies the string and the buffer must still be freed.
            var result = Marshal.PtrToStringAnsi((IntPtr)buffer);
            Unsafe.ffmpeg.av_free(buffer);

            Debug.Assert(
                result != null,
                "Result is null.",
                "Resulting string is null. This indicates a contract violation."
            );

            return result;
        }

        /// <summary>
        /// Parse a string into a <see cref="Unsafe.AVDictionary"/>.
        /// </summary>
        /// <param name="ADict">The <see cref="Unsafe.AVDictionary"/>.</param>
        /// <param name="ABuffer">The string to parse.</param>
        /// <param name="AKeyValSep">The key-value separator list.</param>
        /// <param name="APairsSep">The pairs separator list.</param>
        /// <param name="AFlags">The <see cref="AVDictionaryFlags"/>.</param>
        /// <returns>The <see cref="Result"/> of the parse operation.</returns>
        /// <remarks>
        /// If the parse operation fails, the <paramref name="ADict"/> might still be changed and
        /// may also have been allocated. The caller must cleanup after an error if that is not
        /// intended.
        /// </remarks>
        [MustUseReturnValue]
        public static Result ParseString(
            Movable<Unsafe.AVDictionary> ADict,
            [CanBeNull] string ABuffer,
            [NotNull] string AKeyValSep,
            [NotNull] string APairsSep,
            AVDictionaryFlags AFlags = AVDictionaryFlags.None
        )
        {
            const char C_Null = '\0';
            const char C_Slash = '\\';
            const char C_Max = '\x7F';

            // No null-check necessary.

            Debug.Assert(
                AKeyValSep != null,
                "Key-value separator list is null.",
                "The separator list may not be null. This indicates a severe logic error in the " +
                "code."
            );
            Debug.Assert(
                !AKeyValSep.Any(X => X == C_Null || X == C_Slash || X > C_Max),
                "Key-value separator contains invalid chars.",
                "The slash and zero terminator character are reserved and may not be used " +
                "for any separator. All chars must be ANSI. This indicates a contract violation."
            );
            Debug.Assert(
                APairsSep != null,
                "Pairs separator list is null.",
                "The separator list may not be null. This indicates a severe logic error in the " +
                "code."
            );
            Debug.Assert(
                !APairsSep.Any(X => X == C_Null || X == C_Slash || X > C_Max),
                "Pairs separator contains invalid chars.",
                "The slash and zero terminator character are reserved and may not be used " +
                "for any separator. All chars must be ANSI. This indicates a contract violation."
            );

            return Unsafe.ffmpeg.av_dict_parse_string(
                ADict,
                ABuffer,
                AKeyValSep,
                APairsSep,
                (int)AFlags
            ).CheckFFmpeg("Error parsing string.");
        }

        /// <summary>
        /// Set a value in the <see cref="Unsafe.AVDictionary"/>.
        /// </summary>
        /// <param name="ADict">The <see cref="Unsafe.AVDictionary"/>.</param>
        /// <param name="AKey">The key string.</param>
        /// <param name="AValue">The value string.</param>
        /// <param name="AFlags">The <see cref="AVDictionaryFlags"/>.</param>
        /// <returns>The <see cref="Result"/> of the set operation.</returns>
        /// <remarks>
        /// Supplying the <see cref="AVDictionaryFlags.DoNotStrdupKeys"/> or the
        /// <see cref="AVDictionaryFlags.DoNotStrdupValues"/> flag is invalid behaviour.
        /// </remarks>
        public static Result Set(
            Movable<Unsafe.AVDictionary> ADict,
            [NotNull] string AKey,
            [CanBeNull] string AValue,
            AVDictionaryFlags AFlags = AVDictionaryFlags.None
        )
        {
            const AVDictionaryFlags C_ProhibitedFlags =
                AVDictionaryFlags.DoNotStrdupKeys
                | AVDictionaryFlags.DoNotStrdupValues;

            // No null-check necessary.

            Debug.Assert(
                AKey != null,
                "Key is null.",
                "The key for a set operation may not be null. This indicates a severe logic " +
                "error in the code."
            );
            Debug.Assert(
                (AFlags & C_ProhibitedFlags) != AVDictionaryFlags.None,
                "Flag is prohibited.",
                "Some provided flags (DoNotStrdup family) are invalid on marshalled strings and " +
                "therefore prohibited. This indicates a logic error in the code."
            );

            // Strip the prohibited flags at all cost.
            AFlags &= ~C_ProhibitedFlags;

            return Unsafe.ffmpeg.av_dict_set(ADict, AKey, AValue, (int)AFlags)
                .CheckFFmpeg("Error setting value.");
        }

        /// <summary>
        /// Convert a <see cref="Unsafe.AVDictionaryEntry"/> to a
        /// <see cref="KeyValuePair{TKey, TValue}"/> of strings.
        /// </summary>
        /// <param name="AEntry">The <see cref="Unsafe.AVDictionaryEntry"/>.</param>
        /// <returns>The <see cref="KeyValuePair{TKey, TValue}"/>.</returns>
        /// <remarks>
        /// Creates copies of the strings!
        /// </remarks>
        public static KeyValuePair<string, string> ToPair(
            Fixed<Unsafe.AVDictionaryEntry> AEntry
        )
        {
            Debug.Assert(
                !AEntry.IsNull,
                "AEntry is null.",
                "This indicates a severe logic error in the code."
            );

            return new KeyValuePair<string, string>(
                Marshal.PtrToStringAnsi((IntPtr)AEntry.Raw->key),
                Marshal.PtrToStringAnsi((IntPtr)AEntry.Raw->value)
            );
        }
    }
    // ReSharper restore errors
}
