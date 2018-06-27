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
    public unsafe class MovableTests
    {
        public unsafe struct Unmanaged
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
        #endregion

        #region Conversion operators

        #endregion
    }
}
