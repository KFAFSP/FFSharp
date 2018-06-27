using System;

using System.Runtime.InteropServices;

using NUnit.Framework;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable EqualExpressionComparison

namespace FFSharp.Native
{
    [TestFixture]
    [Description("Tests for the Fixed wrapper struct.")]
    [Category("Native")]
    [Category("Utilities")]
    [TestOf(typeof(Fixed<>))]
    public unsafe class FixedTests
    {
        public unsafe struct Unmanaged
        {
            public struct Nested
            {
                bool val;
            }

            public int a;
            public Nested b;
            public Nested* c;
        }

        Unmanaged* FStruct;

        Fixed<Unmanaged> FNull;
        Fixed<Unmanaged> FFixed;
        Fixed<Unmanaged> FDefault;

        [SetUp]
        public void Setup()
        {
            FStruct = (Unmanaged*)Marshal.AllocHGlobal(Marshal.SizeOf<Unmanaged>());

            FNull = Fixed<Unmanaged>.Null;
            FFixed = new Fixed<Unmanaged>(FStruct);
            FDefault = default;
        }

        [TearDown]
        public void Teardown()
        {
            Marshal.FreeHGlobal((IntPtr)FStruct);
        }
    }
}
