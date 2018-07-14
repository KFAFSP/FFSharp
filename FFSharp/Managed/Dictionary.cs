using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using FFSharp.Native;

using JetBrains.Annotations;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Managed
{
    /// <summary>
    /// Represents a <see cref="IDictionary{TKey, TValue}"/> of <see cref="string"/> key-value-pairs
    /// used by FFmpeg to represent options and metadata.
    /// </summary>
    // ReSharper disable errors
    [PublicAPI]
    public sealed class Dictionary : SmartRefHolderBase<Unsafe.AVDictionary>,
        IDictionary<string, string>
    {
        /// <summary>
        /// Helper for implementing the <see cref="Keys"/> and <see cref="Values"/> collections.
        /// </summary>
        abstract class DummyCollection :
            ICollection<string>
        {
            [NotNull]
            protected Dictionary Parent { get; }

            protected DummyCollection([NotNull] Dictionary AParent)
            {
                Parent = AParent;
            }

            #region ICollection<string>
            /// <inheritdoc />
            [ContractAnnotation("=> halt")]
            public void Add(string AItem) => throw new NotSupportedException();
            /// <inheritdoc />
            [ContractAnnotation("=> halt")]
            public bool Remove(string AIitem) => throw new NotSupportedException();
            /// <inheritdoc />
            [ContractAnnotation("=> halt")]
            public void Clear() => throw new NotSupportedException();

            /// <inheritdoc />
            public abstract bool Contains(string AItem);

            /// <inheritdoc />
            [ContractAnnotation("AArray: null => halt")]
            public void CopyTo(string[] AArray, int AArrayIndex)
            {
                if (AArray == null)
                    throw new ArgumentNullException(nameof(AArray));

                GetEnumerator().CopyTo(AArray, AArrayIndex);
            }

            /// <inheritdoc />
            public int Count => Parent.Count;
            /// <inheritdoc />
            public bool IsReadOnly => true;
            #endregion

            #region IEnumerable<string>
            /// <inheritdoc />
            public abstract IEnumerator<string> GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            #endregion
        }

        /// <summary>
        /// Composed 1:1 class that implements <see cref="Keys"/> like
        /// <see cref="Dictionary{TKey, TValue}"/> does.
        /// </summary>
        sealed class KeyCollection :
            DummyCollection
        {
            public KeyCollection([NotNull] Dictionary AParent)
                : base(AParent)
            { }

            #region DummyCollection overrides
            /// <inheritdoc />
            [ContractAnnotation("AItem: null => halt")]
            public override bool Contains(string AItem)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                return Parent.ContainsKey(AItem);
            }
            /// <inheritdoc />
            public override IEnumerator<string> GetEnumerator()
            {
                return Parent.Select(X => X.Key).GetEnumerator();
            }
            #endregion
        }

        /// <summary>
        /// Composed 1:1 class that implements <see cref="Values"/> like
        /// <see cref="Dictionary{TKey, TValue}"/> does.
        /// </summary>
        sealed class ValueCollection :
            DummyCollection
        {
            public ValueCollection([NotNull] Dictionary AParent)
                : base(AParent)
            { }

            #region DummyCollection overrides
            /// <inheritdoc />
            public override bool Contains(string AItem)
            {
                return this.Any(X => EqualityComparer<string>.Default.Equals(X, AItem));
            }
            /// <inheritdoc />
            public override IEnumerator<string> GetEnumerator()
            {
                return Parent.Select(X => X.Value).GetEnumerator();
            }
            #endregion
        }

        /// <summary>
        /// Get the <see cref="AVDictionaryFlags"/> used by this accessor.
        /// </summary>
        internal AVDictionaryFlags Flags { get; }

        /// <summary>
        /// Create a new <see cref="Dictionary"/> accessor on a shared
        /// <see cref="Unsafe.AVDictionary"/>.
        /// </summary>
        /// <param name="AShared">The shared <see cref="Unsafe.AVDictionary"/>.</param>
        /// <param name="AFlags">The <see cref="AVDictionaryFlags"/> for this accessor.</param>
        internal Dictionary(
            [NotNull] SmartRef<Unsafe.AVDictionary> AShared,
            AVDictionaryFlags AFlags
        ) : base(AShared)
        {
            Flags = AFlags;

            Keys = new KeyCollection(this);
            Values = new ValueCollection(this);
        }
        /// <summary>
        /// Create a new <see cref="Dictionary"/>.
        /// </summary>
        /// <remarks>
        /// This <see cref="Dictionary"/> will have ownership of it's data.
        /// </remarks>
        public Dictionary(bool AIgnoreCase = true)
            : this(
                new SmartRef<Unsafe.AVDictionary>(AVDictionary.Free),
                AIgnoreCase
                    ? AVDictionaryFlags.None
                    : AVDictionaryFlags.MatchCase
            )
        { }

        /// <summary>
        /// Set a value in the <see cref="Dictionary"/>.
        /// </summary>
        /// <param name="AKey">The key.</param>
        /// <param name="AValue">The value to set.</param>
        /// <param name="AOverwrite">
        /// On <see langword="false"/>, any existing value will not be replaced.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="AKey"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="FFmpegError">Error setting value.</exception>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        [ContractAnnotation("AKey: null => halt")]
        public void Set(string AKey, [CanBeNull] string AValue, bool AOverwrite = true)
        {
            if (AKey == null)
                throw new ArgumentNullException(nameof(AKey));

            var flags = Flags;
            if (!AOverwrite)
            {
                if (AValue == null) return;

                flags |= AVDictionaryFlags.DoNotOverwrite;
            }

            AVDictionary.Set(Movable, AKey, AValue, flags).ThrowIfError();
        }

        /// <summary>
        /// Append to a value in the <see cref="Dictionary"/>.
        /// </summary>
        /// <param name="AKey">The key.</param>
        /// <param name="AValue">The value to append.</param>
        /// <remarks>
        /// If the key didn't exist, the value is created; otherwise the value string is appended to
        /// the existing value using ordinary string concatenation.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="AKey"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="AValue"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="FFmpegError">Error setting value.</exception>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        [ContractAnnotation("AKey: null => halt; AValue: null => halt")]
        public void Append(string AKey, string AValue)
        {
            if (AKey == null)
                throw new ArgumentNullException(nameof(AKey));
            if (AValue == null)
                throw new ArgumentNullException(nameof(AKey));

            var flags = Flags | AVDictionaryFlags.Append;

            AVDictionary.Set(Movable, AKey, AValue, flags).ThrowIfError();
        }

        /// <summary>
        /// Get all matching key-value pairs in the <see cref="Dictionary"/>.
        /// </summary>
        /// <param name="AMatch">The key match string.</param>
        /// <param name="AIgnoreSuffix">
        /// If <see langword="true"/>, the keys will only be matched to the end of the match string.
        /// </param>
        /// <returns>An <see cref="IEnumerator{T}"/> of all matching pairs.</returns>
        /// <remarks>
        /// Matching ordinally compares the match string with all key strings, optionally ignoring
        /// case if <see cref="IgnoreCase"/> is <see langword="true"/> for this
        /// <see cref="Dictionary"/>. To match keys with a common prefix, supply
        /// <see langword="true"/> for <paramref name="AIgnoreSuffix"/>.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        [ContractAnnotation("AMatch: null => halt")]
        public IEnumerator<KeyValuePair<string, string>> Get(
            string AMatch,
            bool AIgnoreSuffix = false
        )
        {
            if (AMatch == null)
                throw new ArgumentNullException(nameof(AMatch));

            var flags = Flags;

            if (AIgnoreSuffix)
                flags |= AVDictionaryFlags.IgnoreSuffix;

            return AVDictionary.GetEnumerator(Fixed, AMatch, flags)
                .Map(X => AVDictionary.ToPair(X));
        }

        #region IDictionary<string, string>
        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// <paramref name="AKey"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        [ContractAnnotation("AKey: null => halt")]
        public unsafe bool TryGetValue(string AKey, [CanBeNull] out string AValue)
        {
            if (AKey == null)
                throw new ArgumentNullException(nameof(AKey));

            var get = AVDictionary.Get(Fixed, AKey, default, Flags);
            if (get.IsNull)
            {
                AValue = null;
                return false;
            }

            AValue = Marshal.PtrToStringAnsi((IntPtr)get.Raw->value);
            return true;
        }
        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// <paramref name="AKey"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        [ContractAnnotation("AKey: null => halt")]
        public bool ContainsKey(string AKey) => TryGetValue(AKey, out _);

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// <paramref name="AKey"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="AValue"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="AKey"/> already exists in the <see cref="Dictionary"/>.
        /// </exception>
        /// <exception cref="FFmpegError">Error setting value.</exception>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        [ContractAnnotation("AKey: null => halt; AValue: null => halt")]
        public void Add(string AKey, string AValue)
        {
            if (AKey == null)
                throw new ArgumentNullException(nameof(AKey));

            // Otherwise the pair will not exist after, so fail.
            if (AValue == null)
                throw new ArgumentNullException(nameof(AValue));

            // On exists, we must throw. However, DoNotOverwrite will not indicate that.
            if (ContainsKey(AKey))
                throw new ArgumentException("Key already exists.");

            AVDictionary.Set(Movable, AKey, AValue, Flags).ThrowIfError();
        }
        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// <paramref name="AKey"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="FFmpegError">Error setting value.</exception>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        [ContractAnnotation("AKey: null => halt")]
        public bool Remove(string AKey)
        {
            // On missing, we must return false. However, Set will not indicate that.
            if (ContainsKey(AKey))
            {
                AVDictionary.Set(Movable, AKey, null, Flags).ThrowIfError();

                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public ICollection<string> Keys { get; }
        /// <inheritdoc />
        public ICollection<string> Values { get; }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// <paramref name="AKey"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="FFmpegError">Error setting value.</exception>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        public string this[string AKey]
        {
            [ContractAnnotation("AKey: null => halt")]
            get
            {
                if (!TryGetValue(AKey, out var value))
                    throw new KeyNotFoundException();

                return value;
            }
            [ContractAnnotation("AKey: null => halt")]
            set
            {
                if (AKey == null)
                    throw new ArgumentNullException(nameof(AKey));

                AVDictionary.Set(Movable, AKey, value, Flags).ThrowIfError();
            }
        }
        #endregion

        #region ICollection<KeyValuePair<string, string>>
        bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> AItem)
        {
            if (!TryGetValue(AItem.Key, out var value))
            {
                return false;
            }

            return EqualityComparer<string>.Default.Equals(value, AItem.Value);
        }

        void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> AItem)
        {
            Add(AItem.Key, AItem.Value);
        }
        bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> AItem)
        {
            if (TryGetValue(AItem.Key, out var value)
                && EqualityComparer<string>.Default.Equals(value, AItem.Value))
            {
                AVDictionary.Set(Movable, AItem.Key, null, Flags)
                    .ThrowIfError();

                return true;
            }

            return false;
        }
        /// <inheritdoc />
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        public void Clear()
        {
            AVDictionary.Free(Movable);
        }

        /// <inheritdoc />
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        [ContractAnnotation("AArray: null => halt")]
        public void CopyTo(KeyValuePair<string, string>[] AArray, int AArrayIndex)
        {
            if (AArray == null)
                throw new ArgumentNullException(nameof(AArray));

            Get("", true).CopyTo(AArray, AArrayIndex);
        }

        /// <inheritdoc />
        public int Count => IsDisposed ? 0 : AVDictionary.Count(Fixed);
        /// <inheritdoc />
        public bool IsReadOnly => false;
        #endregion

        #region IEnumerable<KeyValuePair<string, string>>
        /// <inheritdoc />
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return AVDictionary.GetEnumerator(
                Fixed,
                "",
                AVDictionaryFlags.IgnoreSuffix | Flags
            ).Map(X => AVDictionary.ToPair(X));
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

        /// <summary>
        /// Get a value indicating whether the keys are matches case-insensitively.
        /// </summary>
        public bool IgnoreCase => (Flags & AVDictionaryFlags.MatchCase) == AVDictionaryFlags.None;
    }
    // ReSharper restore errors
}
