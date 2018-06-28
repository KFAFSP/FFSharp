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
                this.ToList().CopyTo(AArray, AArrayIndex);
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

        #region IDictionary<string, string>
        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// <paramref name="AKey"/> is <see langword="null"/>.
        /// </exception>
        [ContractAnnotation("AKey: null => halt")]
        public unsafe bool TryGetValue(string AKey, [CanBeNull] out string AValue)
        {
            if (AKey == null)
            {
                throw new ArgumentNullException(nameof(AKey));
            }

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
        [ContractAnnotation("AKey: null => halt")]
        public bool ContainsKey(string AKey) => TryGetValue(AKey, out _);

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// <paramref name="AKey"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="AKey"/> already exists in the <see cref="Dictionary"/>.
        /// </exception>
        /// <exception cref="FFmpegError">Error setting value.</exception>
        [ContractAnnotation("AKey: null => halt")]
        public void Add(string AKey, string AValue)
        {
            if (AKey == null)
            {
                throw new ArgumentNullException(nameof(AKey));
            }

            // On exists, we must throw. However, DoNotOverwrite will not indicate that.
            if (ContainsKey(AKey))
            {
                throw new ArgumentException("Key already exists.");
            }

            AVDictionary.Set(Movable, AKey, AValue, Flags)
                .ThrowIfError();
        }
        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// <paramref name="AKey"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="FFmpegError">Error setting value.</exception>
        [ContractAnnotation("AKey: null => halt")]
        public bool Remove(string AKey)
        {
            // On missing, we must return false. However, Set will not indicate that.
            if (ContainsKey(AKey))
            {
                AVDictionary.Set(Movable, AKey, null, Flags)
                    .ThrowIfError();

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
        public string this[string AKey]
        {
            [ContractAnnotation("AKey: null => halt")]
            get
            {
                if (!TryGetValue(AKey, out var value))
                {
                    throw new KeyNotFoundException();
                }

                return value;
            }
            [ContractAnnotation("AKey: null => halt")]
            set => AVDictionary.Set(Movable, AKey, value, Flags).ThrowIfError();
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
        public void Clear()
        {
            AVDictionary.Free(Movable);
        }

        /// <inheritdoc />
        [ContractAnnotation("AArray: null => halt")]
        public void CopyTo(KeyValuePair<string, string>[] AArray, int AArrayIndex)
        {
            this.ToList().CopyTo(AArray, AArrayIndex);
        }

        /// <inheritdoc />
        public int Count => AVDictionary.Count(Movable.Fixed);
        /// <inheritdoc />
        public bool IsReadOnly => false;
        #endregion

        #region IEnumerable<KeyValuePair<string, string>>
        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return AVDictionary.GetEnumerator(
                Movable.Fixed,
                "",
                AVDictionaryFlags.IgnoreSuffix | Flags
            ).Map(X => AVDictionary.ToPair(X));
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

        /// <summary>
        /// Get a value indicating whether this <see cref="Dictionary"/> is the owner of it's data.
        /// </summary>
        /// <remarks>
        /// If <see langword="false"/>, this means the <see cref="Dictionary"/> wraps the data of
        /// some other parent object and may become disposed at any point in time.
        /// </remarks>
        public bool IsOwning => Ref?.IsOwning ?? false;
        /// <summary>
        /// Get a value indicating whether the keys are matches case-insensitively.
        /// </summary>
        public bool IgnoreCase => (Flags & AVDictionaryFlags.MatchCase) == AVDictionaryFlags.None;
    }
    // ReSharper restore errors
}
