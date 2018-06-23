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
    /// Provides methods that operate on <see cref="Unsafe.AVDictionary"/>.
    /// </summary>
    // ReSharper disable errors
    internal static unsafe class AVDictionary
    {
        /// <summary>
        /// Enumerator for <see cref="Unsafe.AVDictionaryEntry"/>s inside
        /// <see cref="Unsafe.AVDictionary"/>.
        /// </summary>
        public sealed class Enumerator : Disposable,
            IEnumerator<Ref<Unsafe.AVDictionaryEntry>>
        {
            readonly Ref<Unsafe.AVDictionary> FRef;
            readonly AVDictFlags FFlags;

            bool FStarted;

            /// <summary>
            /// Create a new <see cref="Enumerator"/> instance.
            /// </summary>
            /// <param name="ARef">The <see cref="Unsafe.AVDictionary"/>.</param>
            /// <param name="AFlags">The <see cref="AVDictFlags"/>.</param>
            public Enumerator(Ref<Unsafe.AVDictionary> ARef, AVDictFlags AFlags = AVDictFlags.NONE)
            {
                FRef = ARef;
                FFlags = AFlags;

                Reset();
            }

            #region IEnumerator<Ref<Unsafe.AVDictionaryEntry>>
            /// <inheritdoc />
            public bool MoveNext()
            {
                if (FStarted && Current.IsNull)
                {
                    return false;
                }

                FStarted = true;
                Current = Get(FRef, "", Current, FFlags);

                return !Current.IsNull;
            }
            /// <inheritdoc />
            public void Reset()
            {
                FStarted = false;
                Current = null;
            }

            /// <inheritdoc />
            public Ref<Unsafe.AVDictionaryEntry> Current { get; private set; }
            #endregion

            #region IEnumerator
            object IEnumerator.Current => Current;
            #endregion
        }

        /// <summary>
        /// Allocate a new <see cref="Unsafe.AVDictionary"/>.
        /// </summary>
        /// <returns>A new <see cref="Unsafe.AVDictionary"/>.</returns>
        /// <remarks>
        /// As per FFmpeg implementation this does not actually do anything.
        /// </remarks>
        [MustUseReturnValue]
        public static Ref<Unsafe.AVDictionary> Alloc()
        {
            return null;
        }

        /// <summary>
        /// Copy pairs from one dictionary into another.
        /// </summary>
        /// <param name="ADst">The destination <see cref="Unsafe.AVDictionary"/>.</param>
        /// <param name="ASrc">The source <see cref="Unsafe.AVDictionary"/>.</param>
        /// <param name="AFlags">The <see cref="AVDictFlags"/>.</param>
        /// <exception cref="FFmpegError">Error copying values.</exception>
        /// <remarks>
        /// If the copy operation fails, the <paramref name="ADst"/> might still be changed and
        /// may also have been allocated. The caller must cleanup after an error if that is not
        /// intended.
        /// </remarks>
        public static void Copy(
            ref Ref<Unsafe.AVDictionary> ADst,
            Ref<Unsafe.AVDictionary> ASrc,
            AVDictFlags AFlags = AVDictFlags.NONE
        )
        {
            // No null-check necessary.

            var ptr = ADst.Ptr;
            var error = Unsafe.ffmpeg.av_dict_copy(&ptr, ASrc, (int) AFlags).CheckFFmpeg();
            ADst = ptr;

            // Postponed throw to make reference modification visible to caller at all costs.
            error.ThrowIfPresent();
        }

        /// <summary>
        /// Get the number of entries in the dictionary.
        /// </summary>
        /// <param name="ARef">The <see cref="Unsafe.AVDictionary"/>.</param>
        /// <returns>The number of entries.</returns>
        /// <remarks>
        /// This is a constant time operation and prefferable to iterating.
        /// </remarks>
        public static int Count(Ref<Unsafe.AVDictionary> ARef)
        {
            // No null-check necessary.

            return Unsafe.ffmpeg.av_dict_count(ARef);
        }

        /// <summary>
        /// Free a dictionary.
        /// </summary>
        /// <param name="ARef">The <see cref="Unsafe.AVDictionary"/>.</param>
        public static void Free(ref Ref<Unsafe.AVDictionary> ARef)
        {
            // No null-check necessary.

            var ptr = ARef.Ptr;
            Unsafe.ffmpeg.av_dict_free(&ptr);
            ARef = ptr;
        }

        /// <summary>
        /// Get a value from the dictionary.
        /// </summary>
        /// <param name="ARef">The <see cref="Unsafe.AVDictionary"/>.</param>
        /// <param name="AKey">The key match string.</param>
        /// <param name="APrev">
        /// The previously returned <see cref="Unsafe.AVDictionaryEntry"/>.
        /// </param>
        /// <param name="AFlags">The <see cref="AVDictFlags"/>.</param>
        /// <returns>The matched <see cref="Unsafe.AVDictionaryEntry"/> if any.</returns>
        [Pure]
        public static Ref<Unsafe.AVDictionaryEntry> Get(
            Ref<Unsafe.AVDictionary> ARef,
            [NotNull] string AKey,
            Ref<Unsafe.AVDictionaryEntry> APrev = default,
            AVDictFlags AFlags = AVDictFlags.NONE
        )
        {
            const AVDictFlags C_ProhibitedFlags =
                AVDictFlags.AV_DICT_DONT_STRDUP_KEY;

            // No null-check necessary.

            Debug.Assert(
                AKey != null,
                "Key is null.",
                "The key for a set operation may not be null. This indicates a severe logic " +
                "error in the code."
            );
            Debug.Assert(
                (AFlags & C_ProhibitedFlags) != AVDictFlags.NONE,
                "Flag is prohibited.",
                "Some provided flag (DONT_STRDUP_KEY) is invalid on marshalled strings and " +
                "therefore prohibited. This indicates a logic error in the code."
            );

            // Strip the prohibited flags at all cost.
            AFlags &= ~C_ProhibitedFlags;

            return Unsafe.ffmpeg.av_dict_get(ARef, AKey, APrev, (int) AFlags);
        }

        /// <summary>
        /// Get a string representation of the dictionary.
        /// </summary>
        /// <param name="ARef">The <see cref="Unsafe.AVDictionary"/>.</param>
        /// <param name="AKeyValSep">The key-value separator char.</param>
        /// <param name="APairsSep">The pairs separator char.</param>
        /// <returns>The string representation.</returns>
        /// <exception cref="FFmpegError">Error building string.</exception>
        [Pure]
        [NotNull]
        public static string GetString(
            Ref<Unsafe.AVDictionary> ARef,
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
            Unsafe.ffmpeg.av_dict_get_string(
                ARef,
                &buffer,
                AKeyValSep,
                APairsSep
            ).CheckFFmpeg().ThrowIfPresent();

            // Marshalling copies the string, but the buffer must still be freed.
            var result = Marshal.PtrToStringAnsi((IntPtr) buffer);
            Unsafe.ffmpeg.av_free(buffer);

            Debug.Assert(
                result != null,
                "Result is null.",
                "Resulting string is null. This indicates a contract violation."
            );

            return result;
        }

        /// <summary>
        /// Parse a string into a dictionary.
        /// </summary>
        /// <param name="ARef">The <see cref="Unsafe.AVDictionary"/>.</param>
        /// <param name="ABuffer">The string to parse.</param>
        /// <param name="AKeyValSep">The key-value separator list.</param>
        /// <param name="APairsSep">The pairs separator list.</param>
        /// <param name="AFlags">The <see cref="AVDictFlags"/>.</param>
        /// <exception cref="FFmpegError">Error parsing string.</exception>
        /// <remarks>
        /// If the parse operation fails, the <paramref name="ARef"/> might still be changed and
        /// may also have been allocated. The caller must cleanup after an error if that is not
        /// intended.
        /// </remarks>
        public static void ParseString(
            ref Ref<Unsafe.AVDictionary> ARef,
            [CanBeNull] string ABuffer,
            [NotNull] string AKeyValSep,
            [NotNull] string APairsSep,
            AVDictFlags AFlags = AVDictFlags.NONE
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

            var ptr = ARef.Ptr;
            var error = Unsafe.ffmpeg.av_dict_parse_string(
                &ptr,
                ABuffer,
                AKeyValSep,
                APairsSep,
                (int)AFlags
            ).CheckFFmpeg();
            ARef = ptr;

            // Postponed throw to make reference modification visible to caller at all costs.
            error.ThrowIfPresent();
        }

        /// <summary>
        /// Set a value in the dictionary.
        /// </summary>
        /// <param name="ARef">The <see cref="Unsafe.AVDictionary"/>.</param>
        /// <param name="AKey">The key string.</param>
        /// <param name="AValue">The value string.</param>
        /// <param name="AFlags">The <see cref="AVDictFlags"/>.</param>
        public static void Set(
            ref Ref<Unsafe.AVDictionary> ARef,
            [NotNull] string AKey,
            [CanBeNull] string AValue,
            AVDictFlags AFlags = AVDictFlags.NONE
        )
        {
            const AVDictFlags C_ProhibitedFlags =
                AVDictFlags.AV_DICT_DONT_STRDUP_KEY
                | AVDictFlags.AV_DICT_DONT_STRDUP_VAL;

            // No null-check necessary.

            Debug.Assert(
                AKey != null,
                "Key is null.",
                "The key for a set operation may not be null. This indicates a severe logic " +
                "error in the code."
            );
            Debug.Assert(
                (AFlags & C_ProhibitedFlags) != AVDictFlags.NONE,
                "Flag is prohibited.",
                "Some provided flags (DONT_STRDUP family) are invalid on marshalled strings and " +
                "therefore prohibited. This indicates a logic error in the code."
            );

            // Strip the prohibited flags at all cost.
            AFlags &= ~C_ProhibitedFlags;

            var ptr = ARef.Ptr;
            var error = Unsafe.ffmpeg.av_dict_set(&ptr, AKey, AValue, (int) AFlags).CheckFFmpeg();
            ARef = ptr;

            // Postponed throw to make reference modification visible to caller at all costs.
            error.ThrowIfPresent();
        }
    }
    // ReSharper restore errors
}
