using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using FFSharp.Native;

using JetBrains.Annotations;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp
{
    /// <summary>
    /// Represents a key-value dictionary used by FFmpeg.
    /// </summary>
    // ReSharper disable errors
    [PublicAPI]
    public sealed class Dictionary :
        IDictionary<string, string>
    {
        sealed class Enumerator :
            MapEnumerator<Ref<Unsafe.AVDictionaryEntry>, KeyValuePair<string, string>>
        {
            /// <summary>
            /// Create a new <see cref="Enumerator"/> instance.
            /// </summary>
            /// <param name="ADict">The parent <see cref="Dictionary"/>.</param>
            public Enumerator([NotNull] Dictionary ADict)
                : base(new AVDictionary.Enumerator(ADict.Ref, ADict.Flags))
            {
            }

            #region MapEnumerator<...> overrides
            /// <inheritdoc />
            protected override unsafe KeyValuePair<string, string> Map(Ref<Unsafe.AVDictionaryEntry> AIn)
            {
                return new KeyValuePair<string, string>(
                    Marshal.PtrToStringAnsi((IntPtr) AIn.Ptr->key),
                    Marshal.PtrToStringAnsi((IntPtr) AIn.Ptr->value)
                );
            }
            #endregion
        }

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
            public void Add(string AItem) => throw new NotSupportedException();
            /// <inheritdoc />
            public bool Remove(string AIitem) => throw new NotSupportedException();
            /// <inheritdoc />
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
        /// Parse a <see cref="Dictionary"/> from a string.
        /// </summary>
        /// <param name="AString">The string to parse.</param>
        /// <param name="AKeyValSep">The key-value separator list.</param>
        /// <param name="APairSep">The pairs separator list.</param>
        /// <param name="AIgnoreCase">If <c>true</c>, case is ignored when comparing keys.</param>
        /// <returns>The parsed <see cref="Dictionary"/>.</returns>
        [NotNull]
        public static Dictionary Parse(
            [CanBeNull] string AString,
            string AKeyValSep,
            string APairSep,
            bool AIgnoreCase = true
        )
        {
            Dictionary result = new Dictionary(AIgnoreCase);
            try
            {
                result.Deserialize(AString, AKeyValSep, APairSep);
                return result;
            }
            catch
            {
                result.Clear();
                throw;
            }
        }

        /// <summary>
        /// Internal reference to the <see cref="Unsafe.AVDictionary"/>.
        /// </summary>
        internal Ref<Unsafe.AVDictionary> Ref;

        /// <summary>
        /// Create a new <see cref="Dictionary"/> instance.
        /// </summary>
        /// <param name="ARef">The <see cref="Unsafe.AVDictionary"/>.</param>
        /// <param name="AFlags">The <see cref="AVDictFlags"/>.</param>
        internal Dictionary(Ref<Unsafe.AVDictionary> ARef, AVDictFlags AFlags)
        {
            Ref = ARef;
            Flags = AFlags;

            Keys = new KeyCollection(this);
            Values = new ValueCollection(this);
        }
        /// <summary>
        /// Create a new <see cref="Dictionary"/> instance.
        /// </summary>
        /// <param name="AIgnoreCase">If <c>true</c>, ignore case when comparing keys.</param>
        public Dictionary(bool AIgnoreCase = true)
        : this(
            AVDictionary.Alloc(),
            AIgnoreCase
                ? AVDictFlags.NONE
                : AVDictFlags.AV_DICT_MATCH_CASE
        )
        { }

        /// <summary>
        /// Finalize this instance.
        /// </summary>
        /// <remarks>
        /// Instead of a dispose pattern, call the simpler <see cref="Clear()"/>.
        /// </remarks>
        ~Dictionary()
        {
            Clear();
        }

        /// <summary>
        /// Copy the contents of this <see cref="Dictionary"/> to another.
        /// </summary>
        /// <param name="ADictionary"></param>
        [ContractAnnotation("ADictionary: null => halt")]
        public void CopyTo(Dictionary ADictionary)
        {
            if (ADictionary == null)
            {
                throw new ArgumentNullException(nameof(ADictionary));
            }

            AVDictionary.Copy(ref ADictionary.Ref, Ref, Flags);
        }
        /// <summary>
        /// Clone this <see cref="Dictionary"/>.
        /// </summary>
        /// <returns>The cloned <see cref="Dictionary"/>.</returns>
        [NotNull]
        public Dictionary Clone()
        {
            var clone = new Dictionary(Flags);

            try
            {
                AVDictionary.Copy(ref clone.Ref, Ref, Flags);
            }
            catch
            {
                clone.Clear();
                throw;
            }
            
            return clone;
        }

        /// <summary>
        /// Serialize this <see cref="Dictionary"/> into a string.
        /// </summary>
        /// <param name="AKeyValSep">The key-value separator.</param>
        /// <param name="APairsSep">The pairs separator.</param>
        /// <returns>The serialized string.</returns>
        [NotNull]
        public string Serialize(char AKeyValSep, char APairsSep)
        {
            const byte C_Null = (byte)'\0';
            const byte C_Slash = (byte)'\\';

            var seps = Encoding.ASCII.GetBytes(new[] {AKeyValSep, APairsSep});

            if (seps.Length != 2)
            {
                throw new ArgumentException("Separators are non-ASCII.");
            }
            if (seps.Any(X => X == C_Null || X == C_Slash))
            {
                throw new ArgumentException("Separators contain invalid characters.");
            }
            if (seps[0] == seps[1])
            {
                throw new ArgumentException("Separators are the same.");
            }

            return AVDictionary.GetString(Ref, seps[0], seps[1]);
        }
        /// <summary>
        /// Deserialize a string into this <see cref="Dictionary"/>.
        /// </summary>
        /// <param name="AString">The string to deserialize.</param>
        /// <param name="AKeyValSep">The key-value separator list.</param>
        /// <param name="APairsSep">The pair separator list.</param>
        public void Deserialize([CanBeNull] string AString, string AKeyValSep, string APairsSep)
        {
            const char C_Null = '\0';
            const char C_Slash = '\\';

            if (AString == null)
            {
                return;
            }
            if (AKeyValSep == null)
            {
                throw new ArgumentNullException(nameof(AKeyValSep));
            }
            if (APairsSep == null)
            {
                throw new ArgumentNullException(nameof(APairsSep));
            }

            if (AKeyValSep.Any(X => X == C_Null || X == C_Slash))
            {
                throw new ArgumentException("Invalid character.", nameof(AKeyValSep));
            }
            if (APairsSep.Any(X => X == C_Null || X == C_Slash))
            {
                throw new ArgumentException("Invalid character.", nameof(APairsSep));
            }
            if (AKeyValSep.Union(APairsSep).Any())
            {
                throw new ArgumentException("Separators overlap.");
            }

            AVDictionary.ParseString(ref Ref, AString, AKeyValSep, APairsSep, Flags);
        }

        #region IDictionary<string, string>
        /// <inheritdoc />
        [ContractAnnotation("AKey: null => halt")]
        public unsafe bool TryGetValue(string AKey, out string AValue)
        {
            if (AKey == null)
            {
                throw new ArgumentNullException(nameof(AKey));
            }

            var get = AVDictionary.Get(Ref, AKey, default, Flags);
            if (get.IsNull)
            {
                AValue = null;
                return false;
            }

            AValue = Marshal.PtrToStringAnsi((IntPtr) get.Ptr->value);
            return true;
        }
        /// <inheritdoc />
        [ContractAnnotation("AKey: null => halt")]
        public bool ContainsKey(string AKey) => TryGetValue(AKey, out _);

        /// <inheritdoc />
        [ContractAnnotation("AKey: null => halt")]
        public void Add(string AKey, string AValue)
        {
            if (AKey == null)
            {
                throw new ArgumentNullException(nameof(AKey));
            }

            AVDictionary.Set(ref Ref, AKey, AValue, Flags);
        }
        /// <inheritdoc />
        [ContractAnnotation("AKey: null => halt")]
        public bool Remove(string AKey)
        {
            if (ContainsKey(AKey))
            {
                AVDictionary.Set(ref Ref, AKey, null, Flags);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public ICollection<string> Keys { get; }
        /// <inheritdoc />
        public ICollection<string> Values { get; }

        /// <inheritdoc />
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
            set => Add(AKey, value);
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
                AVDictionary.Set(ref Ref, AItem.Key, null, Flags);
                return true;
            }

            return false;
        }
        /// <inheritdoc />
        public void Clear()
        {
            AVDictionary.Free(ref Ref);
        }

        /// <inheritdoc />
        [ContractAnnotation("AArray: null => halt")]
        public void CopyTo(KeyValuePair<string, string>[] AArray, int AArrayIndex)
        {
            this.ToList().CopyTo(AArray, AArrayIndex);
        }

        /// <inheritdoc />
        public int Count => AVDictionary.Count(Ref);
        /// <inheritdoc />
        public bool IsReadOnly => false;
        #endregion

        #region IEnumerable<KeyValuePair<string, string>>
        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

        /// <summary>
        /// Get a value indicating whether keys are compare case-insensitively.
        /// </summary>
        public bool IgnoreCase => !Flags.HasFlag(AVDictFlags.AV_DICT_MATCH_CASE);

        /// <summary>
        /// The <see cref="AVDictFlags"/> for this dict.
        /// </summary>
        internal AVDictFlags Flags { get; }
    }
    // ReSharper restore errors
}
