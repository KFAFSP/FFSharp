using System;
using System.Runtime.InteropServices;

using NUnit.Framework;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable EqualExpressionComparison

namespace FFSharp.Native
{
    [TestFixture]
    [Description("Tests for the Movable wrapper struct.")]
    [Category("Native")]
    [Category("Utilities")]
    [TestOf(typeof(Movable<>))]
    // ReSharper disable errors
    public unsafe class MovableTests
    {
        public struct Unmanaged
        {
            public int a;
            public int b;
        }

        Unmanaged** FNullPtrPtr;
        Unmanaged** FStructPtrPtr;
        Unmanaged* FStruct1Ptr;
        Unmanaged* FStruct2Ptr;

        Movable<Unmanaged> FNull;
        Movable<Unmanaged> FPresent;
        Movable<Unmanaged> FAbsent;
        Movable<Unmanaged> FDefault;

        [OneTimeSetUp]
        public void Alloc()
        {
            FNullPtrPtr = (Unmanaged**)Marshal.AllocHGlobal(sizeof(void*));
            FStructPtrPtr = (Unmanaged**)Marshal.AllocHGlobal(sizeof(void*));
            FStruct1Ptr = (Unmanaged*)Marshal.AllocHGlobal(Marshal.SizeOf<Unmanaged>());
            FStruct2Ptr = (Unmanaged*)Marshal.AllocHGlobal(Marshal.SizeOf<Unmanaged>());
        }

        [OneTimeTearDown]
        public void Free()
        {
            Marshal.FreeHGlobal((IntPtr)FNullPtrPtr);
            Marshal.FreeHGlobal((IntPtr)FStructPtrPtr);
            Marshal.FreeHGlobal((IntPtr)FStruct1Ptr);
            Marshal.FreeHGlobal((IntPtr)FStruct2Ptr);
        }

        [SetUp]
        public void Setup()
        {
            *FNullPtrPtr = null;
            *FStructPtrPtr = FStruct1Ptr;

            FStruct1Ptr->a = 1;
            FStruct1Ptr->b = 1;

            FStruct2Ptr->a = 2;
            FStruct2Ptr->b = 2;

            FNull = Movable<Unmanaged>.Null;
            FPresent = new Movable<Unmanaged>(FStructPtrPtr);
            FAbsent = new Movable<Unmanaged>(FNullPtrPtr);
            FDefault = default;
        }

        #region Constructors
        [Test]
        [Description("Null returns a null pointer Movable.")]
        public void Null_IsNull()
        {
            Assert.That(Movable.Null<Unmanaged>().Raw == null);
        }

        [Test]
        [Description("Of returns a pointer-wrapping Movable.")]
        public void Of_IsPointer()
        {
            Assert.That(Movable.Of<Unmanaged>(FStructPtrPtr).Raw == FStructPtrPtr);
        }

        [Test]
        [Description("Default initialized is null.")]
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

            Assert.That(FPresent.Equals(FPresent));
            Assert.That(FPresent == FPresent);

            Assert.That(FDefault.Equals(FDefault));
            Assert.That(FDefault == FDefault);
        }

        [Test]
        [Description("Movable are only equal to each other if their pointers match.")]
        public void Equals_Fixed_MustEqualsRawPointer()
        {
            Assume.That(FNull.Raw == null);
            Assume.That(FPresent.Raw == FStructPtrPtr);
            Assume.That(FAbsent.Raw == FNullPtrPtr);

            Assert.That(!FPresent.Equals(FNull));
            Assert.That(FPresent != FNull);

            Assert.That(!FPresent.Equals(FAbsent));
            Assert.That(FPresent != FAbsent);
        }

        [Test]
        [Description("Movable is equal to it's wrapped pointer.")]
        public void Equals_Pointer_EqualsRawPointer()
        {
            Unmanaged** nullptr = null;

            Assume.That(FNull.Raw == nullptr);
            Assume.That(FPresent.Raw == FStructPtrPtr);

            Assert.That(FNull.Equals(nullptr));
            Assert.That(FNull == nullptr);

            Assert.That(FPresent.Equals(FStructPtrPtr));
            Assert.That(FPresent == FStructPtrPtr);

            Assert.That(!FPresent.Equals(FNullPtrPtr));
            Assert.That(FPresent != FNullPtrPtr);
        }
        #endregion

        [Test]
        [Description("Or with null returns default.")]
        public void Or_Null_ReturnsDefault()
        {
            Assume.That(FNull.Raw == null);

            Assert.That(FNull.Or(FAbsent) == FAbsent);
        }

        [Test]
        [Description("Or with non null returns this.")]
        public void Or_NotNull_ReturnsThis()
        {
            Assume.That(FAbsent.Raw != null);

            Assert.That(FAbsent.Or(null) == FAbsent);
        }

        [Test]
        [Description("TargetOr with null returns default.")]
        public void TargetOr_Null_ReturnsDefault()
        {
            Assume.That(FNull.Raw == null);

            Assert.That(FNull.TargetOr(FStruct1Ptr) == FStruct1Ptr);
        }

        [Test]
        [Description("TargetOr with absent returns default.")]
        public void TargetOr_Absent_ReturnsDefault()
        {
            Assume.That(FAbsent.Raw != null);
            Assume.That(!FAbsent.IsPresent);

            Assert.That(FAbsent.TargetOr(FStruct2Ptr) == FStruct2Ptr);
        }

        [Test]
        [Description("TargetOr with present return this.")]
        public void TargetOr_Present_ReturnsThis()
        {
            Assume.That(FPresent.IsPresent);

            Assert.That(FPresent.TargetOr(FStruct2Ptr) == FPresent);
        }

        [Test]
        [Description("Casting does not change the address.")]
        public void Cast_RetainsAddress()
        {
            Assert.That(FNull.Cast<bool>().Address == FNull.Address);
            Assert.That(FPresent.Cast<int>().Address == FPresent.Address);
            Assert.That(FAbsent.Cast<int>().Address == FAbsent.Address);
        }

        #region Properties
        [Test]
        [Description("Address returns the address of the wrapped pointer.")]
        public void Address_IsAddress()
        {
            Assert.That(FNull.Address == IntPtr.Zero);
            Assert.That(FPresent.Address == (IntPtr)FStructPtrPtr);
        }

        [Test]
        [Description("Target of non null is target pointer.")]
        public void Target_NotNull_IsTargetPointer()
        {
            Assume.That(FNullPtrPtr != null);
            Assume.That(*FNullPtrPtr == null);
            Assume.That(FAbsent.Raw == FNullPtrPtr);

            Assert.That(FAbsent.Target == null);
        }

        [Test]
        [Description("Target of non null can be changed.")]
        public void Target_NotNull_CanBeChanged()
        {
            Assume.That(FPresent.Raw == FStructPtrPtr);
            Assume.That(*FStructPtrPtr == FStruct1Ptr);

            FPresent.Target = FStruct2Ptr;
            Assert.That(FPresent.Target == FStruct2Ptr);
        }

        [Test]
        [Description("IsNull is true if the wrapped pointer is null.")]
        public void IsNull_Null_True()
        {
            Assume.That(FNull.Raw == null);

            Assert.That(FNull.IsNull);
        }

        [Test]
        [Description("IsNull is false if the wrapped pointer is not false.")]
        public void IsNull_NotNull_False()
        {
            Assume.That(FPresent.Raw != null);

            Assert.That(!FAbsent.IsNull);
        }

        [Test]
        [Description("Null Movables are never present.")]
        public void IsPresent_Null_False()
        {
            Assume.That(FNull.Raw == null);

            Assert.That(!FNull.IsPresent);
        }

        [Test]
        [Description("Movables with a null target are not present.")]
        public void IsPresent_Absent_False()
        {
            Assume.That(FNullPtrPtr != null);
            Assume.That(*FNullPtrPtr == null);
            Assume.That(FAbsent.Raw == FNullPtrPtr);

            Assert.That(!FAbsent.IsPresent);
        }

        [Test]
        [Description("Movables with a non-null target are present.")]
        public void IsPresent_Present_True()
        {
            Assume.That(FStructPtrPtr != null);
            Assume.That(*FStructPtrPtr != null);
            Assume.That(FPresent.Raw == FStructPtrPtr);

            Assert.That(FPresent.IsPresent);
        }

        [Test]
        [Description("AsFixed wraps target pointer as Fixed.")]
        public void AsFixed_NotNull_WrapsTargetPointer()
        {
            Assume.That(FStructPtrPtr != null);
            Assume.That(*FStructPtrPtr == FStruct1Ptr);
            Assume.That(FPresent.Raw == FStructPtrPtr);

            Assert.That(FPresent.AsFixed == FStruct1Ptr);
        }

        [Test]
        [Description("AsFixed of non null can be changed.")]
        public void AsFixed_NotNull_CanBeChanged()
        {
            Assume.That(FPresent.Raw == FStructPtrPtr);
            Assume.That(*FStructPtrPtr == FStruct1Ptr);

            FPresent.AsFixed = Fixed.Of<Unmanaged>(FStruct2Ptr);
            Assert.That(FPresent.AsFixed == FStruct2Ptr);
        }
        #endregion

        #region Conversion operators
        [Test]
        [Description("Implicit conversion to bool on absent returns false.")]
        public void ImplicitToBool_Absent_False()
        {
            Assume.That(!FNull.IsPresent);
            Assume.That(!FAbsent.IsPresent);

            Assert.That(!FNull);
            Assert.That(!FAbsent);
        }

        [Test]
        [Description("Implicit conversion to bool on present returns true.")]
        public void ImplicitToBool_Present_True()
        {
            Assume.That(FPresent.IsPresent);

            Assert.That(FPresent);
        }

        [Test]
        [Description("Implict conversion to pointer returns the contained pointer.")]
        public void ImplicitToPointer_IsContainedPointer()
        {
            Unmanaged** ptr = FNull;
            Assert.That(ptr == FNull.Raw);

            ptr = FPresent;
            Assert.That(ptr == FPresent.Raw);
        }

        [Test]
        [Description("Implicit conversion from pointer wraps the pointer.")]
        public void ImplicitFromPointer_WrapsPointer()
        {
            Movable<Unmanaged> test = FStructPtrPtr;
            Assert.That(test.Raw == FStructPtrPtr);
        }

        [Test]
        [Description("Implicit conversion to address returns the address.")]
        public void ImplicitToAddress_IsAddress()
        {
            IntPtr addr = FNull;
            Assert.That(addr == FNull.Address);

            addr = FPresent;
            Assert.That(addr == FPresent.Address);
        }

        [Test]
        [Description("Implicit conversion from address wraps the pointer to that address.")]
        public void ImplictFromAddress_WrapsPointer()
        {
            Movable<Unmanaged> test = (IntPtr)FStructPtrPtr;
            Assert.That(test.Raw == FStructPtrPtr);
        }
        #endregion
    }
    // ReSharper restore errors
}
