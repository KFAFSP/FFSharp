using System;
using System.Collections.Generic;
using System.Linq;

using FFSharp.Native;

using NUnit.Framework;

using Unsafe = FFmpeg.AutoGen;

// ReSharper disable AssignNullToNotNullAttribute

namespace FFSharp.Managed
{
    [TestFixture]
    [Description("Tests for the Dictionary wrapper class.")]
    [Category("Managed")]
    [TestOf(typeof(Dictionary))]
    public sealed class DictionaryTests
    {
        #region Set
        [Test]
        [Description("Set on disposed throws ObjectDisposedException.")]
        public void Set_OnDisposed_ThrowsObjectDisposedException()
        {
            var dict = new Dictionary();
            dict.Dispose();

            Assert.That(() =>
            {
                dict.Set("Key", "Value");
            }, Throws.InstanceOf<ObjectDisposedException>());
        }

        [Test]
        [Description("Set with null key throws ArgumentNullException.")]
        public void Set_KeyIsNull_ThrowsArgumentNullException()
        {
            using (var dict = new Dictionary())
            {
                // ReSharper disable once AccessToDisposedClosure
                Assert.That(() => dict.Set(null, ""), Throws.ArgumentNullException);
            }
        }

        [TestCase(true, Description = "Set non existing with overwrite adds a new pair.")]
        [TestCase(false, Description = "Set non existing without overwrite adds a new pair.")]
        public void Set_NonExisting_AddsValue(bool AOverwrite)
        {
            using (var dict = new Dictionary())
            {
                dict.Set("Key", "Value", AOverwrite);

                Assert.That(dict["Key"], Is.EqualTo("Value"));
            }
        }

        [Test]
        [Description("Set existing with overwrite changes the value.")]
        public void Set_ExistingWithOverwrite_ChangesValue()
        {
            using (var dict = new Dictionary())
            {
                dict["Key"] = "Value";

                dict.Set("Key", "success");

                Assert.That(dict["Key"], Is.EqualTo("success"));
            }
        }

        [Test]
        [Description("Set existing without overwrite does not change the value.")]
        public void Set_ExistingWithoutOverwrite_DoesNotChangeValue()
        {
            using (var dict = new Dictionary())
            {
                dict["Key"] = "Value";

                dict.Set("Key", "fail", false);

                Assert.That(dict["Key"], Is.EqualTo("Value"));
            }
        }

        [Test]
        [Description("Set null value with overwrite removes the pair.")]
        public void Set_ValueIsNullWithOverwrite_DeletesValue()
        {
            using (var dict = new Dictionary())
            {
                dict["Key"] = "Value";

                dict.Set("Key", null);

                Assert.That(dict.ContainsKey("Key"), Is.False);
            }
        }

        [Test]
        [Description("Set null value without overwrite does nothing.")]
        public void Set_ValueIsNullWithoutOverwrite_DoesNothing()
        {
            using (var dict = new Dictionary())
            {
                dict["Key"] = "Value";

                dict.Set("Key", null, false);

                Assert.That(dict.ContainsKey("Key"), Is.True);
            }
        }
        #endregion

        #region Append
        [Test]
        [Description("Append on disposed throws ObjectDisposedException.")]
        public void Append_OnDisposed_ThrowsObjectDisposedException()
        {
            var dict = new Dictionary();
            dict.Dispose();

            Assert.That(() =>
            {
                dict.Append("Key", "Value");
            }, Throws.InstanceOf<ObjectDisposedException>());
        }

        [Test]
        [Description("Append to null key throws ArgumentNullException.")]
        public void Append_KeyIsNull_ThrowsArgumentNullException()
        {
            using (var dict = new Dictionary())
            {
                // ReSharper disable once AccessToDisposedClosure
                Assert.That(() => dict.Append(null, ""), Throws.ArgumentNullException);
            }
        }

        [Test]
        [Description("Append null value throws ArgumentNullException.")]
        public void Append_ValueIsNull_ThrowsArgumentNullException()
        {
            using (var dict = new Dictionary())
            {
                // ReSharper disable once AccessToDisposedClosure
                Assert.That(() => dict.Append("", null), Throws.ArgumentNullException);
            }
        }

        [Test]
        [Description("Append on existing appends to the existing value.")]
        public void Append_Existing_AppendsToValue()
        {
            using (var dict = new Dictionary())
            {
                dict["Key"] = "Value";

                dict.Append("Key", "Value");

                Assert.That(dict["Key"], Is.EqualTo("ValueValue"));
            }
        }

        [Test]
        [Description("Append on not existing adds the value.")]
        public void Append_NotExisting_AddsValue()
        {
            using (var dict = new Dictionary())
            {
                dict.Append("Key", "Value");

                Assert.That(dict["Key"], Is.EqualTo("Value"));
            }
        }
        #endregion

        #region Get
        [Test]
        [Description("Get on disposed throws ObjectDisposedException.")]
        public void Get_OnDisposed_ThrowsObjectDisposedException()
        {
            var dict = new Dictionary();
            dict.Dispose();

            Assert.That(() =>
            {
                var _ = dict.Get("");
            }, Throws.InstanceOf<ObjectDisposedException>());
        }

        [Test]
        [Description("Get using null match throws ArgumentNullException.")]
        public void Get_MatchIsNull_ThrowsArgumentNullException()
        {
            using (var dict = new Dictionary())
            {
                // ReSharper disable once AccessToDisposedClosure
                Assert.That(() => dict.Get(null), Throws.ArgumentNullException);
            }
        }

        [Test]
        [Description("Get using an exact match yields the pair.")]
        public void Get_MatchOne_YieldsPair()
        {
            using (var dict = new Dictionary())
            {
                dict["Key"] = "Value";
                dict["Key2"] = "Value2";

                using (var enumerator = dict.Get("Key"))
                {
                    Assert.That(enumerator.MoveNext(), Is.True);
                    Assert.That(enumerator.Current.Key, Is.EqualTo("Key"));
                    Assert.That(enumerator.Current.Value, Is.EqualTo("Value"));

                    Assert.That(enumerator.MoveNext(), Is.False);
                }
            }
        }

        [Test]
        [Description("Get using a prefix match yields all pairs.")]
        public void Get_MatchPrefix_YieldsAllPairs()
        {
            using (var dict = new Dictionary())
            {
                dict["K"] = "V";
                dict["Key"] = "Value";
                dict["Key2"] = "Value2";

                var found = new List<KeyValuePair<string, string>>();
                using (var enumerator = dict.Get("Key", true))
                {
                    Assert.That(enumerator.MoveNext(), Is.True);
                    found.Add(enumerator.Current);

                    Assert.That(enumerator.MoveNext(), Is.True);
                    found.Add(enumerator.Current);

                    Assert.That(enumerator.MoveNext(), Is.False);
                }

                Assert.That(
                    found,
                    Contains.Item(new KeyValuePair<string, string>("Key", "Value"))
                );
                Assert.That(
                    found,
                    Contains.Item(new KeyValuePair<string, string>("Key2", "Value2"))
                );
            }
        }
        #endregion

        #region TryGetValue
        [Test]
        [Description("TryGetValue on disposed throws ObjectDisposedException.")]
        public void TryGetValue_OnDisposed_ThrowsObjectDisposedException()
        {
            var dict = new Dictionary();
            dict.Dispose();

            Assert.That(() =>
            {
                dict.TryGetValue("Key", out _);
            }, Throws.InstanceOf<ObjectDisposedException>());
        }

        [Test]
        [Description("TryGetValue with null key throws ArgumentNullException.")]
        public void TryGetValue_KeyIsNull_ThrowsArgumentNullException()
        {
            using (var dict = new Dictionary())
            {
                // ReSharper disable once AccessToDisposedClosure
                Assert.That(() => dict.TryGetValue(null, out _), Throws.ArgumentNullException);
            }
        }

        [Test]
        [Description("TryGetValue on existing returns true and the value.")]
        public void TryGetValue_Existing_ReturnsTrueAndValue()
        {
            using (var dict = new Dictionary())
            {
                dict["Key"] = "Value";

                Assert.That(dict.TryGetValue("Key", out var val), Is.True);
                Assert.That(val, Is.EqualTo("Value"));
            }
        }

        [Test]
        [Description("TryGetValue on not existing returns false.")]
        public void TryGetValue_NotExisting_ReturnsFalse()
        {
            using (var dict = new Dictionary())
            {
                Assert.That(dict.TryGetValue("Key", out _), Is.False);
            }
        }
        #endregion

        #region ContainsKey
        [Test]
        [Description("ContainsKey on disposed throws ObjectDisposedException.")]
        public void ContainsKey_OnDisposed_ThrowsObjectDisposedException()
        {
            var dict = new Dictionary();
            dict.Dispose();

            Assert.That(() =>
            {
                var _ = dict.ContainsKey("Key");
            }, Throws.InstanceOf<ObjectDisposedException>());
        }

        [Test]
        [Description("ContainsKey with null key throws ArgumentNullException.")]
        public void ContainsKey_KeyIsNull_ThrowsArgumentNullException()
        {
            using (var dict = new Dictionary())
            {
                // ReSharper disable once AccessToDisposedClosure
                Assert.That(() => dict.ContainsKey(null), Throws.ArgumentNullException);
            }
        }

        [Test]
        [Description("ContainsKey on existing returns true.")]
        public void ContainsKey_Existing_ReturnsTrue()
        {
            using (var dict = new Dictionary())
            {
                dict["Key"] = "Value";

                Assert.That(dict.ContainsKey("Key"), Is.True);
            }
        }

        [Test]
        [Description("ContainsKey on not existing returns false.")]
        public void ContainsKey_NotExisting_ReturnsFalse()
        {
            using (var dict = new Dictionary())
            {
                Assert.That(dict.ContainsKey("Key"), Is.False);
            }
        }
        #endregion

        #region Add
        [Test]
        [Description("Add on disposed throws ObjectDisposedException.")]
        public void Add_OnDisposed_ThrowsObjectDisposedException()
        {
            var dict = new Dictionary();
            dict.Dispose();

            Assert.That(() =>
            {
                dict.Add("Key", "Value");
            }, Throws.InstanceOf<ObjectDisposedException>());
        }

        [Test]
        [Description("Add with null key throws ArgumentNullException.")]
        public void Add_KeyIsNull_ThrowsArgumentNullException()
        {
            using (var dict = new Dictionary())
            {
                // ReSharper disable once AccessToDisposedClosure
                Assert.That(() => dict.Add(null, ""), Throws.ArgumentNullException);
            }
        }

        [Test]
        [Description("Add with null value throws ArgumentNullException.")]
        public void Add_ValueIsNull_ThrowsArgumentNullException()
        {
            using (var dict = new Dictionary())
            {
                // ReSharper disable once AccessToDisposedClosure
                Assert.That(() => dict.Add("", null), Throws.ArgumentNullException);
            }
        }

        [Test]
        [Description("Add with existing key throws ArgumentException.")]
        public void Add_KeyExists_ThrowsArgumentException()
        {
            using (var dict = new Dictionary())
            {
                dict["Key"] = "Value";

                // ReSharper disable once AccessToDisposedClosure
                Assert.That(() => dict.Add("Key", ""), Throws.ArgumentException);
            }
        }

        [Test]
        [Description("Add with new key adds the pair.")]
        public void Add_NotExisting_AddsPair()
        {
            using (var dict = new Dictionary())
            {
                dict.Add("Key", "Value");

                Assert.That(dict["Key"], Is.EqualTo("Value"));
            }
        }
        #endregion

        #region Remove
        [Test]
        [Description("Remove on disposed throws ObjectDisposedException.")]
        public void Remove_OnDisposed_ThrowsObjectDisposedException()
        {
            var dict = new Dictionary();
            dict.Dispose();

            Assert.That(() =>
            {
                dict.Remove("");
            }, Throws.InstanceOf<ObjectDisposedException>());
        }

        [Test]
        [Description("Remove with null key throws ArgumentNullException.")]
        public void Remove_KeyIsNull_ThrowsArgumentNullException()
        {
            using (var dict = new Dictionary())
            {
                // ReSharper disable once AccessToDisposedClosure
                Assert.That(() => dict.Remove(null), Throws.ArgumentNullException);
            }
        }

        [Test]
        [Description("Remove existing key returns true and removes the key.")]
        public void Remove_Existing_ReturnsTrueAndRemoves()
        {
            using (var dict = new Dictionary())
            {
                dict.Add("Key", "Value");

                Assert.That(dict.Remove("Key"), Is.True);
                Assert.That(dict.ContainsKey("Key"), Is.False);
            }
        }

        [Test]
        [Description("Remove not existing key returns false.")]
        public void Remove_NotExisting_ReturnsFalse()
        {
            using (var dict = new Dictionary())
            {
                Assert.That(dict.Remove("Key"), Is.False);
            }
        }
        #endregion

        #region Keys
        [Test]
        [Description("Getting keys returns not null.")]
        public void GetKey_ReturnsNotNull()
        {
            using(var dict = new Dictionary())
            {
                Assert.That(dict.Keys, Is.Not.Null);
            }
        }
        #endregion

        #region Values
        [Test]
        [Description("Getting Values returns not null.")]
        public void GetValues_ReturnsNotNull()
        {
            using (var dict = new Dictionary())
            {
                Assert.That(dict.Values, Is.Not.Null);
            }
        }
        #endregion

        #region this
        [Test]
        [Description("Getting this on disposed throws ObjectDisposedException.")]
        public void GetThis_OnDisposed_ThrowsObjectDisposedException()
        {
            var dict = new Dictionary();
            dict.Dispose();

            Assert.That(() =>
            {
                var _ = dict[""];
            }, Throws.InstanceOf<ObjectDisposedException>());
        }

        [Test]
        [Description("Getting this with null key throws ArgumentNullException.")]
        public void GetThis_KeyIsNull_ThrowsArgumentNullException()
        {
            using (var dict = new Dictionary())
            {
                // ReSharper disable once AccessToDisposedClosure
                Assert.That(() => dict[null], Throws.ArgumentNullException);
            }
        }

        [Test]
        [Description("Getting this for non existing key throws KeyNotFoundException.")]
        public void GetThis_NotExisting_ThrowsKeyNotFoundException()
        {
            using (var dict = new Dictionary())
            {
                // ReSharper disable once AccessToDisposedClosure
                Assert.That(() => dict[""], Throws.InstanceOf<KeyNotFoundException>());
            }
        }

        [Test]
        [Description("Getting this for existing key returns the value.")]
        public void GetThis_Existing_ReturnsValue()
        {
            using (var dict = new Dictionary())
            {
                dict["Key"] = "Value";

                Assert.That(dict["Key"], Is.EqualTo("Value"));
            }
        }

        [Test]
        [Description("Setting this on disposed throws ObjectDisposedException.")]
        public void SetThis_OnDisposed_ThrowsObjectDisposedException()
        {
            var dict = new Dictionary();
            dict.Dispose();

            Assert.That(() =>
            {
                dict[""] = "";
            }, Throws.InstanceOf<ObjectDisposedException>());
        }

        [Test]
        [Description("Setting this with null key throws ArgumentNullException.")]
        public void SetThis_KeyIsNull_ThrowsArgumentNullException()
        {
            using (var dict = new Dictionary())
            {
                // ReSharper disable once AccessToDisposedClosure
                Assert.That(() => dict[null] = "", Throws.ArgumentNullException);
            }
        }

        [Test]
        [Description("Setting this for non existing key adds the pair.")]
        public void SetThis_NotExisting_AddsPair()
        {
            using (var dict = new Dictionary())
            {
                dict["Key"] = "Value";

                Assert.That(dict["Key"], Is.EqualTo("Value"));
            }
        }

        [Test]
        [Description("Setting this for existing key overwrites the value.")]
        public void SetThis_Existing_OverwritesValue()
        {
            using (var dict = new Dictionary())
            {
                dict["Key"] = "Value";
                dict["Key"] = "success";

                Assert.That(dict["Key"], Is.EqualTo("success"));
            }
        }
        #endregion

        #region Clear
        [Test]
        [Description("Clear on disposed throws ObjectDisposedException.")]
        public void Clear_OnDisposed_ThrowsObjectDisposedException()
        {
            var dict = new Dictionary();
            dict.Dispose();

            Assert.That(() => dict.Clear(), Throws.InstanceOf<ObjectDisposedException>());
        }

        [Test]
        [Description("Clear removes all pairs.")]
        public void Clear_RemovesAllPairs()
        {
            using (var dict = new Dictionary())
            {
                dict["Key"] = "Value";
                dict["Key2"] = "Value2";

                dict.Clear();

                Assert.That(dict.Count, Is.EqualTo(0));
            }
        }
        #endregion

        #region CopyTo
        [Test]
        [Description("CopyTo on disposed throws ObjectDisposedException.")]
        public void CopyTo_OnDisposed_ThrowsObjectDisposedException()
        {
            var dict = new Dictionary();
            var array = new KeyValuePair<string, string>[0];
            dict.Dispose();

            Assert.That(() => dict.CopyTo(array, 0), Throws.InstanceOf<ObjectDisposedException>());
        }

        [Test]
        [Description("CopyTo with null array throws ArgumentNullException.")]
        public void CopyTo_ArrayIsNull_ThrowsArgumentNullException()
        {
            using (var dict = new Dictionary())
            {
                // ReSharper disable once AccessToDisposedClosure
                Assert.That(() => dict.CopyTo(null, 0), Throws.ArgumentNullException);
            }
        }

        [Test]
        [Description("CopyTo with array index out of range throws ArgumentOutOfRangeException.")]
        public void CopyTo_ArrayIndexOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            var array = new KeyValuePair<string, string>[2];

            using (var dict = new Dictionary())
            {
                // ReSharper disable once AccessToDisposedClosure
                Assert.That(() =>
                {
                    dict.CopyTo(array, -1);
                }, Throws.InstanceOf<ArgumentOutOfRangeException>());

                // ReSharper disable once AccessToDisposedClosure
                Assert.That(() =>
                {
                    dict.CopyTo(array, 2);
                }, Throws.InstanceOf<ArgumentOutOfRangeException>());
            }
        }

        [Test]
        [Description("CopyTo with array too small throws ArgumentException.")]
        public void CopyTo_ArrayTooSmall_ThrowsArgumentException()
        {
            var array = new KeyValuePair<string, string>[2];

            using (var dict = new Dictionary())
            {
                dict["Key"] = "Value";
                dict["Key2"] = "Value2";

                // ReSharper disable once AccessToDisposedClosure
                Assert.That(() => dict.CopyTo(array, 1), Throws.ArgumentException);
            }
        }

        [Test]
        [Description("CopyTo copies all pairs.")]
        public void CopyTo_CopiesAllPairs()
        {
            using (var dict = new Dictionary())
            {
                dict["Key"] = "Value";
                dict["Key2"] = "Value2";

                var array = new KeyValuePair<string, string>[2];
                dict.CopyTo(array, 0);

                Assert.That(
                    array,
                    Contains.Item(new KeyValuePair<string, string>("Key", "Value"))
                );
                Assert.That(
                    array,
                    Contains.Item(new KeyValuePair<string, string>("Key2", "Value2"))
                );
            }
        }
        #endregion

        #region Count
        [Test]
        [Description("Getting Count on disposed returns 0.")]
        public void GetCount_OnDisposed_ReturnsZero()
        {
            var dict = new Dictionary();
            dict.Dispose();

            Assert.That(dict.Count, Is.Zero);
        }

        [Test]
        [Description("Getting Count returns the number of pairs.")]
        public void GetCount_ReturnsCount()
        {
            using (var dict = new Dictionary())
            {
                dict["Key"] = "Value";
                dict["Key2"] = "Value2";

                Assert.That(dict.Count, Is.EqualTo(2));
            }
        }
        #endregion

        #region IsReadOnly
        [Test]
        [Description("Getting IsReadOnly returns false.")]
        public void GetIsReadOnly_ReturnsFalse()
        {
            using (var dict = new Dictionary())
            {
                Assert.That(dict.IsReadOnly, Is.False);
            }
        }
        #endregion

        #region GetEnumerator
        [Test]
        [Description("GetEnumerator on disposed throws ObjectDisposedException.")]
        public void GetEnumerator_OnDisposed_ThrowsObjectDisposedException()
        {
            var dict = new Dictionary();
            dict.Dispose();

            Assert.That(() => dict.GetEnumerator(), Throws.InstanceOf<ObjectDisposedException>());
        }

        [Test]
        [Description("GetEnumerator returns enumerator yielding all pairs.")]
        public void GetEnumerator_YieldsAllPairs()
        {
            using (var dict = new Dictionary())
            {
                dict["Key"] = "Value";
                dict["Key2"] = "Value2";

                var found = dict.GetEnumerator().ToList();

                Assert.That(
                    found,
                    Contains.Item(new KeyValuePair<string, string>("Key", "Value"))
                );
                Assert.That(
                    found,
                    Contains.Item(new KeyValuePair<string, string>("Key2", "Value2"))
                );
            }
        }
        #endregion

        #region GetIgnoreCase
        [Description("Getting IgnoreCase returns the IgnoreCase flag state.")]
        [TestCase(true)]
        [TestCase(false)]
        public void GetIgnoreCase_ReturnsIgnoreCase(bool AIgnoreCase)
        {
            using (var dict = new Dictionary(AIgnoreCase))
            {
                Assert.That(dict.IgnoreCase, Is.EqualTo(AIgnoreCase));
            }
        }
        #endregion
    }
}
