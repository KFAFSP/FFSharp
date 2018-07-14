using System;
using System.Collections;

using JetBrains.Annotations;

using NUnit.Framework;

namespace FFSharp.Native
{
    // ReSharper disable errors
    [TestFixture]
    [Description("Testing the Fixed type itself.")]
    [Category("Native")]
    [TestOf(typeof(Fixed<>))]
    internal sealed unsafe class FixedTTests
    {
        static readonly Fixed<int> _FIntPtrOne = new Fixed<int>((int*)1);
        static readonly Fixed<int> _FIntPtrTwo = new Fixed<int>((int*)2);
        static readonly Fixed<bool> _FBoolPtrOne = new Fixed<bool>((bool*)1);

        [Test]
        [Description("Default constructor initializes a null Fixed.")]
        public void Constructor_Default_IsNull()
        {
            var test = new Fixed<int>();

            Assert.That(test.IsNull, Is.True);
        }

        [Test]
        [Description("Init constructor initializes a Fixed wrapping the specified pointer.")]
        public void Constructor_Init_WrapsPointer()
        {
            int i;
            var test = new Fixed<int>(&i);

            Assert.That(test.Raw == &i);
        }

        [Test]
        [Description("Or on null Fixed returns the default.")]
        public void Or_OnNull_ReturnsDefault()
        {
            var test = new Fixed<int>();

            Assert.That(test.Or(_FIntPtrOne), Is.EqualTo(_FIntPtrOne));
        }

        [Test]
        [Description("Or on non-null Fixed returns this.")]
        public void Or_OnNonNull_ReturnsThis()
        {
            var test = _FIntPtrOne;

            Assert.That(test.Or(_FIntPtrTwo), Is.EqualTo(_FIntPtrOne));
        }

        [Test]
        [Description("Cast does not change the address.")]
        public void Cast_DoesNotChangeAddress()
        {
            var test = _FIntPtrOne.Cast<bool>();

            Assert.That(test.Address, Is.EqualTo(_FIntPtrOne.Address));
        }

        [Test]
        [Description("Equals on Fixed and pointer is correct.")]
        public void Equals_FixedPtr_Correct()
        {
            var test = _FIntPtrOne;

            Assert.That(test.Equals(_FIntPtrTwo.Raw), Is.False);
            Assert.That(test != _FIntPtrTwo.Raw && !(test == _FIntPtrTwo.Raw));

            Assert.That(test.Equals(test.Raw), Is.True);
            Assert.That(test == test.Raw && !(test != test.Raw));
        }

        [TestCaseSource(nameof(FixedAnyAlikeTestCases))]
        [TestCaseSource(nameof(FixedAnyNotAlikeTestCases))]
        [Description("Equals on Fixed and any is correct.")]
        public bool Equals_FixedAny_Correct(object AFixed, object AOther)
        {
            return AFixed.Equals(AOther);
        }

        [Test]
        [Description("GetHashCode returns the hash of Address.")]
        public void GetHashCode_ReturnsAddressHash()
        {
            Assert.That(_FIntPtrOne.GetHashCode(), Is.EqualTo(_FIntPtrOne.Address.GetHashCode()));
        }

        [Test]
        [Description("ToString returns an address literal.")]
        public void ToString_ReturnsAddressLiteral()
        {
            Assert.That(_FIntPtrOne.ToString(), Is.EqualTo("Fixed<Int32>(0x0000000000000001)"));
        }

        [Test]
        [Description("Getting Address returns the address of the pointer.")]
        public void GetAddress_ReturnsAddress()
        {
            Assert.That(_FIntPtrOne.Address, Is.EqualTo((IntPtr) 1));
        }

        [Test]
        [Description("Getting IsNull on null returns true.")]
        public void GetIsNull_OnNull_ReturnsTrue()
        {
            var test = new Fixed<int>();

            Assert.That(test.IsNull, Is.True);
        }

        [Test]
        [Description("Getting IsNull on non-null returns false.")]
        public void GetIsNull_OnNonNull_ReturnsFalse()
        {
            Assert.That(_FIntPtrOne.IsNull, Is.False);
        }

        [Test]
        [Description("Getting AsRef on non-null returns a modifiable reference to the struct.")]
        public void GetAsRef_OnNonNull_ReturnsModifiableReference()
        {
            int i = 1;
            var test = new Fixed<int>(&i);
            ref int r = ref test.AsRef;

            r = 2;
            Assert.That(r, Is.EqualTo(2));
        }

        [Test]
        [Description("Implicit cast from pointer wraps the pointer as a Fixed.")]
        public void ImplicitCast_FromPointer_WrapsPointer()
        {
            int i;
            Fixed<int> test = &i;

            Assert.That(test.Raw == &i);
        }

        [Test]
        [Description("Implicit cast to pointer returns Raw.")]
        public void ImplicitCast_ToPointer_ReturnsRaw()
        {
            int i;
            int* j = new Fixed<int>(&i);

            Assert.That(j == &i);
        }

        [Test]
        [Description("Implicit cast to bool on null Fixed returns false.")]
        public void ImplicitCast_ToBoolOnNull_ReturnsFalse()
        {
            bool test = new Fixed<int>();

            Assert.That(test, Is.False);
        }

        [Test]
        [Description("Implicit cast to bool on non-null Fixed returns true.")]
        public void ImplicitCast_ToBoolOnNonNull_ReturnsTrue()
        {
            bool test = _FIntPtrOne;

            Assert.That(test, Is.True);
        }

        [Test]
        [Description("Implicit cast to IntPtr returns Address.")]
        public void ImplicitCast_ToIntPtr_ReturnsAddress()
        {
            IntPtr test = _FIntPtrOne;

            Assert.That(test, Is.EqualTo(_FIntPtrOne.Address));
        }

        [Test]
        [Description("Implicit cast from IntPtr makes and wraps the pointer.")]
        public void ImplicitCast_FromIntPtr_MakesAndWrapsPointer()
        {
            Fixed<int> test = (IntPtr) 1;

            Assert.That(test, Is.EqualTo(_FIntPtrOne));
        }

        [TestCaseSource(nameof(FixedAnyAlikeTestCases))]
        [Description("Binary equals operator on Fixeds is correct.")]
        public bool EqOp_FixedFixed_Correct(Fixed<int> ALeft, Fixed<int> ARight)
        {
            return ALeft == ARight && !(ALeft != ARight);
        }

        [UsedImplicitly]
        public static IEnumerable FixedAnyAlikeTestCases
        {
            get
            {
                yield return new TestCaseData(new Fixed<int>(), new Fixed<int>())
                    .Returns(true)
                    .SetDescription("Null and null is true.");
                yield return new TestCaseData(_FIntPtrOne, _FIntPtrOne)
                    .Returns(true)
                    .SetDescription("Pointer and same pointer is true.");
                yield return new TestCaseData(_FIntPtrOne, _FIntPtrTwo)
                    .Returns(false)
                    .SetDescription("Pointer and different pointer is false.");
            }
        }

        [UsedImplicitly]
        public static IEnumerable FixedAnyNotAlikeTestCases
        {
            get
            {
                yield return new TestCaseData(new Fixed<int>(), new Fixed<bool>())
                    .Returns(false)
                    .SetDescription("Null and different-typed null is false.");
                yield return new TestCaseData(_FIntPtrOne, _FBoolPtrOne)
                    .Returns(false)
                    .SetDescription("Pointer and different-typed pointer is false.");
            }
        }
    }
    // ReSharper restore errors
}
