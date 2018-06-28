using System;

using System.Runtime.InteropServices;

using NUnit.Framework;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable EqualExpressionComparison
#pragma warning disable CS1718 // Reflexive equality comparions

namespace FFSharp.Native
{
    [TestFixture]
    [Description("Tests for the Fixed wrapper struct.")]
    [Category("Native")]
    [Category("Utilities")]
    [TestOf(typeof(Fixed<>))]
    // ReSharper disable errors
    public unsafe class FixedTests
    {
        public struct Unmanaged
        {
            public struct Nested
            {
                public bool val;
            }

            public int a;
            public Nested b;
            public Nested* c;
        }

        Unmanaged* FStruct;

        Fixed<Unmanaged> FNull;
        Fixed<Unmanaged> FFixed;
        Fixed<Unmanaged> FDefault;

        [OneTimeSetUp]
        public void Alloc()
        {
            FStruct = (Unmanaged*)Marshal.AllocHGlobal(Marshal.SizeOf<Unmanaged>());
            FStruct->c = (Unmanaged.Nested*)Marshal.AllocHGlobal(Marshal.SizeOf<Unmanaged.Nested>());
        }

        [OneTimeTearDown]
        public void Free()
        {
            Marshal.FreeHGlobal((IntPtr)FStruct->c);
            Marshal.FreeHGlobal((IntPtr)FStruct);
        }

        [SetUp]
        public void Setup()
        {
            FStruct->a = 1;
            FStruct->b.val = true;
            FStruct->c->val = true;

            FNull = Fixed<Unmanaged>.Null;
            FFixed = new Fixed<Unmanaged>(FStruct);
            FDefault = default;
        }

        #region Constructors
        [Test]
        [Description("Null returns a null wrapping Fixed.")]
        public void Null_IsNull()
        {
            Assert.That(Fixed.Null<Unmanaged>().Raw == null);
        }

        [Test]
        [Description("Of returns a pointer-wrapping Fixed.")]
        public void Of_IsPointer()
        {
            Assert.That(Fixed.Of<Unmanaged>(FStruct).Raw == FStruct);
        }

        [Test]
        [Description("Default initialized is wrapping null.")]
        public void Default_IsNull()
        {
            Assert.That(FDefault.Raw == null);
        }
        #endregion

        #region Equals
        [Test]
        [Description("Equals is reflexive.")]
        public void Equals_Reflexive()
        {
            Assert.That(FNull.Equals(FNull));
            Assert.That(FNull == FNull);

            Assert.That(FFixed.Equals(FFixed));
            Assert.That(FFixed == FFixed);

            Assert.That(FDefault.Equals(FDefault));
            Assert.That(FDefault == FDefault);
        }

        [Test]
        [Description("Fixed are only equal to each other if their pointers match.")]
        public void Equals_Fixed_MustEqualsRawPointer()
        {
            Assume.That(FNull.Raw == null);
            Assume.That(FFixed.Raw == FStruct);

            Assert.That(!FFixed.Equals(FNull));
            Assert.That(FFixed != FNull);
        }

        [Test]
        [Description("Fixed is equal to it's wrapped pointer.")]
        public void Equals_Pointer_EqualsRawPointer()
        {
            Unmanaged* nullptr = null;

            Assume.That(FNull.Raw == nullptr);
            Assume.That(FFixed.Raw == FStruct);

            Assert.That(FNull.Equals(nullptr));
            Assert.That(FNull == nullptr);

            Assert.That(FFixed.Equals(FStruct));
            Assert.That(FFixed == FStruct);

            Assert.That(!FFixed.Equals(nullptr));
            Assert.That(FFixed != nullptr);
        }
        #endregion

        [Test]
        [Description("Or on non null returns this.")]
        public void Or_NonNull_This()
        {
            Assume.That(FFixed.Raw != null);

            Assert.That(FFixed.Or(null) == FFixed);
        }

        [Test]
        [Description("Or on null returns provided default.")]
        public void Or_Null_ReturnsDefault()
        {
            Assume.That(FNull.Raw == null);
            Assume.That(FFixed.Raw != null);

            Assert.That(FNull.Or(FFixed) == FFixed);
        }

        [Test]
        [Description("Casting does not change the address.")]
        public void Cast_RetainsAddress()
        {
            Assert.That(FNull.Cast<bool>().Address == FNull.Address);
            Assert.That(FFixed.Cast<int>().Address == FFixed.Address);
        }

        #region Properties
        [Test]
        [Description("Address returns the address of the wrapped pointer.")]
        public void Address_IsAddress()
        {
            Assert.That(FNull.Address == IntPtr.Zero);
            Assert.That(FFixed.Address == (IntPtr)FStruct);
        }

        [Test]
        [Description("IsNull is true if the wrapped pointer is null.")]
        public void IsNull_Null_True()
        {
            Assume.That(FNull.Raw == null);

            Assert.That(FNull.IsNull);
        }

        [Test]
        [Description("IsNull is false if the wrapped pointer is not null.")]
        public void IsNull_NotNull_False()
        {
            Assume.That(FFixed.Raw != null);

            Assert.That(!FFixed.IsNull);
        }

        [Test]
        [Description("AsRef returns a modifiable C# reference to the underlying struct.")]
        public void AsRef_NotNull_ModifiableRef()
        {
            Assume.That(FStruct->a, Is.EqualTo(1));
            Assume.That(FFixed.Raw == FStruct);

            ref var asRef = ref FFixed.AsRef;

            ++asRef.a;
            Assert.That(FStruct->a, Is.EqualTo(2));
        }
        #endregion

        #region Conversion operators
        [Test]
        [Description("Implicit conversion to bool on null-wrapping returns false.")]
        public void ImplicitToBool_Null_False()
        {
            Assume.That(FNull.Raw == null);

            Assert.That(!FNull);
        }

        [Test]
        [Description("Implicit conversion to bool on not-null-wrapping returns true.")]
        public void ImplicitToBool_NotNull_True()
        {
            Assume.That(FFixed.Raw != null);

            Assert.That(FFixed);
        }

        [Test]
        [Description("Implict conversion to pointer returns the contained pointer.")]
        public void ImplicitToPointer_IsContainedPointer()
        {
            Unmanaged* ptr = FNull;
            Assert.That(ptr == FNull.Raw);

            ptr = FFixed;
            Assert.That(ptr == FFixed.Raw);
        }

        [Test]
        [Description("Implicit conversion from pointer wraps the pointer.")]
        public void ImplicitFromPointer_WrapsPointer()
        {
            Fixed<Unmanaged> test = FStruct;
            Assert.That(test.Raw == FStruct);
        }

        [Test]
        [Description("Implicit conversion to address returns the address.")]
        public void ImplicitToAddress_IsAddress()
        {
            IntPtr addr = FNull;
            Assert.That(addr == FNull.Address);

            addr = FFixed;
            Assert.That(addr == FFixed.Address);
        }

        [Test]
        [Description("Implicit conversion from address wraps the pointer to that address.")]
        public void ImplictFromAddress_WrapsPointer()
        {
            Fixed<Unmanaged> test = (IntPtr)FStruct;
            Assert.That(test.Raw == FStruct);
        }
        #endregion
    }
    // ReSharper restore errors
}

#pragma warning restore CS1718
