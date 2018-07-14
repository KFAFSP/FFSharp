using System;
using System.Collections;

using JetBrains.Annotations;

using NUnit.Framework;

namespace FFSharp.Native
{
    // ReSharper disable errors
    [TestFixture]
    [Description("Testing the Movable type itself.")]
    [Category("Native")]
    [TestOf(typeof(Movable<>))]
    internal sealed unsafe class MovableTTests
    {
        static readonly Movable<int> _FIntPtrOne = new Movable<int>((int**)1);
        static readonly Movable<int> _FIntPtrTwo = new Movable<int>((int**)2);
        static readonly Movable<bool> _FBoolPtrOne = new Movable<bool>((bool**)1);

        [Test]
        [Description("Default constructor initializes a null Movable.")]
        public void Constructor_Default_IsNull()
        {
            var test = new Movable<int>();

            Assert.That(test.IsNull, Is.True);
        }

        [Test]
        [Description("Init constructor initializes a Movable wrapping the specified pointer.")]
        public void Constructor_Init_WrapsPointer()
        {
            int* i;
            var test = new Movable<int>(&i);

            Assert.That(test.Raw == &i);
        }

        [Test]
        [Description("Or on null Movable returns the default.")]
        public void Or_OnNull_ReturnsDefault()
        {
            var test = new Movable<int>();

            Assert.That(test.Or(_FIntPtrOne), Is.EqualTo(_FIntPtrOne));
        }

        [Test]
        [Description("Or on non-null Movable returns this.")]
        public void Or_OnNonNull_ReturnsThis()
        {
            var test = _FIntPtrOne;

            Assert.That(test.Or(_FIntPtrTwo), Is.EqualTo(_FIntPtrOne));
        }

        [Test]
        [Description("TargetOr on null Movable returns the default.")]
        public void TargetOr_OnNull_ReturnsDefault()
        {
            var test = new Movable<int>();
            var def = new Fixed<int>((int*) 1);

            Assert.That(test.TargetOr(def), Is.EqualTo(def));
        }

        [Test]
        [Description("TargetOr on absent Movable returns the default.")]
        public void TargetOr_OnAbsent_ReturnsDefault()
        {
            int* i = null;
            var test = new Movable<int>(&i);
            var def = new Fixed<int>((int*)1);

            Assert.That(test.TargetOr(def), Is.EqualTo(def));
        }

        [Test]
        [Description("TargetOr on present Movable returns this.")]
        public void TargetOr_OnPresent_ReturnsTarget()
        {
            int i;
            int* j = &i;
            var test = new Movable<int>(&j);
            var def = new Fixed<int>((int*)1);

            Assert.That(test.TargetOr(def).Raw == j);
        }

        [Test]
        [Description("Cast does not change the address.")]
        public void Cast_DoesNotChangeAddress()
        {
            var test = _FIntPtrOne.Cast<bool>();

            Assert.That(test.Address, Is.EqualTo(_FIntPtrOne.Address));
        }

        [Test]
        [Description("Equals on Movable and pointer is correct.")]
        public void Equals_MovablePtr_Correct()
        {
            var test = _FIntPtrOne;

            Assert.That(test.Equals(_FIntPtrTwo.Raw), Is.False);
            Assert.That(test != _FIntPtrTwo.Raw && !(test == _FIntPtrTwo.Raw));

            Assert.That(test.Equals(test.Raw), Is.True);
            Assert.That(test == test.Raw && !(test != test.Raw));
        }

        [TestCaseSource(nameof(MovableAnyAlikeTestCases))]
        [TestCaseSource(nameof(MovableAnyNotAlikeTestCases))]
        [Description("Equals on Movable and any is correct.")]
        public bool Equals_MovableAny_Correct(object AMovable, object AOther)
        {
            return AMovable.Equals(AOther);
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
            int* i = null;
            var test = new Movable<int>(&i);
            var addr = test.Address.ToUInt64().ToString("X16");

            Assert.That(test.ToString(), Is.EqualTo($"Movable<Int32>(0x{addr})"));
        }

        [Test]
        [Description("Getting Address returns the address of the pointer.")]
        public void GetAddress_ReturnsAddress()
        {
            Assert.That(_FIntPtrOne.Address, Is.EqualTo((IntPtr)1));
        }

        [Test]
        [Description("SetTarget on non-null Movable changes the target pointer.")]
        public void SetTarget_OnNonNull_ChangesTarget()
        {
            int i;
            int* j = null;
            var test = new Movable<int>(&j);

            test.SetTarget(&i);
            Assert.That(test.Target.Raw == &i);
        }

        [Test]
        [Description("Getting IsNull on null returns true.")]
        public void GetIsNull_OnNull_ReturnsTrue()
        {
            var test = new Movable<int>();

            Assert.That(test.IsNull, Is.True);
        }

        [Test]
        [Description("Getting IsNull on non-null returns false.")]
        public void GetIsNull_OnNonNull_ReturnsFalse()
        {
            Assert.That(_FIntPtrOne.IsNull, Is.False);
        }

        [Test]
        [Description("Implicit cast from pointer wraps the pointer as a Fixed.")]
        public void ImplicitCast_FromPointer_WrapsPointer()
        {
            int* i;
            Movable<int> test = &i;

            Assert.That(test.Raw == &i);
        }

        [Test]
        [Description("Implicit cast to pointer returns Raw.")]
        public void ImplicitCast_ToPointer_ReturnsRaw()
        {
            int* i;
            int** j = new Movable<int>(&i);

            Assert.That(j == &i);
        }

        [Test]
        [Description("Implicit cast to bool on null Movable returns false.")]
        public void ImplicitCast_ToBoolOnNull_ReturnsFalse()
        {
            bool test = new Movable<int>();

            Assert.That(test, Is.False);
        }

        [Test]
        [Description("Implicit cast to bool on non-null Movable returns true.")]
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
            Movable<int> test = (IntPtr) 1;

            Assert.That(test, Is.EqualTo(_FIntPtrOne));
        }

        [TestCaseSource(nameof(MovableAnyAlikeTestCases))]
        [Description("Binary equals operator on Movables is correct.")]
        public bool EqOp_MovableMocable_Correct(Movable<int> ALeft, Movable<int> ARight)
        {
            return ALeft == ARight && !(ALeft != ARight);
        }

        [UsedImplicitly]
        public static IEnumerable MovableAnyAlikeTestCases
        {
            get
            {
                yield return new TestCaseData(new Movable<int>(), new Movable<int>())
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
        public static IEnumerable MovableAnyNotAlikeTestCases
        {
            get
            {
                yield return new TestCaseData(new Movable<int>(), new Movable<bool>())
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
