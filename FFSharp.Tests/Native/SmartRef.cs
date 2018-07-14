using System;

using NUnit.Framework;

using Whetstone.Contracts;

namespace FFSharp.Native
{
    // ReSharper disable errors
    [TestFixture]
    [Description("Testing the SmartRef type itself.")]
    [Category("Native")]
    [TestOf(typeof(SmartRef<>))]
    internal sealed unsafe class SmartRefTests
    {
        class Exemplar : Disposable
        {
            public static int Disposed;

            protected override void Dispose(bool ADisposing)
            {
                ++Disposed;
                base.Dispose(ADisposing);
            }
        }

        Exemplar FExemplar;

        [SetUp]
        public void Setup()
        {
            FExemplar = new Exemplar();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Exemplar.Disposed = 0;
        }

        [Test]
        [Description("Owning constructor makes an owning SmartRef.")]
        public void Constructor_Owning_MakesOwning()
        {
            var test = new SmartRef<int>();

            Assert.That(test.IsOwning, Is.True);
        }

        [Test]
        [Description("Shared on non-null constructor makes a shared SmartRef.")]
        public void Constructor_SharedNonNull_MakesShared()
        {
            int* i;
            var test = new SmartRef<int>(&i);

            Assert.That(test.IsOwning, Is.False);
        }

        [Test]
        [Description("Shared on null constructor throws ArgumentException.")]
        public void Constructor_SharedNull_ThrowsArgumentException()
        {
            Assert.That(() =>
            {
                var _ = new SmartRef<int>(Movable<int>.Null);
            }, Throws.ArgumentException);
        }

        [Test]
        [Description("Link on disposed throws ObjectDisposedException.")]
        public void Link_OnDisposed_ThrowsObjectDisposedException()
        {
            int* i;
            var test = new SmartRef<int>(&i);
            test.Dispose();

            Assert.That(() =>
            {
                test.Link(FExemplar);
            }, Throws.InstanceOf<ObjectDisposedException>());
        }

        [Test]
        [Description("Link does not prevent GC on holder.")]
        public void Link_DoesNotPreventGC()
        {
            int* i;
            var test = new SmartRef<int>(&i);
            test.Link(FExemplar);

            FExemplar = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.That(Exemplar.Disposed, Is.EqualTo(1));
        }

        [Test]
        [Description("Link ignores double link.")]
        public void Link_Linked_DoesNothing()
        {
            int* i;
            var test = new SmartRef<int>(&i);
            test.Link(FExemplar);
            test.Link(FExemplar);

            test.Unlink(FExemplar);

            Assert.That(test.IsLinked, Is.False);
        }

        [Test]
        [Description("Link handles dead holder references.")]
        public void Link_HandlesDeadHolder()
        {
            int* i;
            var test = new SmartRef<int>(&i);
            test.Link(FExemplar);

            FExemplar = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var anchor = new Exemplar();
            test.Link(anchor);

            Assert.That(test.IsLinked, Is.True);
        }

        [Test]
        [Description("Unlink on disposed does nothing.")]
        public void Unlink_OnDisposed_DoesNothing()
        {
            int* i;
            var test = new SmartRef<int>(&i);
            test.Dispose();
            test.Unlink(FExemplar);
        }

        [Test]
        [Description("Unlink with non-last linked holder unlinks holder.")]
        public void Unlink_WithNonLastLinked_Unlinks()
        {
            var anchor = new Exemplar();

            int* i;
            var test = new SmartRef<int>(&i);
            test.Link(anchor);
            test.Link(FExemplar);
            test.Unlink(FExemplar);

            test.Dispose();

            Assert.That(Exemplar.Disposed, Is.EqualTo(1));
        }

        [Test]
        [Description("Unlink with last linked holder disposes.")]
        public void Unlink_WithLastLinked_Disposes()
        {
            int* i;
            var test = new SmartRef<int>(&i);
            test.Link(FExemplar);
            test.Unlink(FExemplar);

            Assert.That(test.IsDisposed, Is.True);
        }

        [Test]
        [Description("Unlink handles dead holder references.")]
        public void Unlink_HandlesDeadHolder()
        {
            var anchor = new Exemplar();

            int* i;
            var test = new SmartRef<int>(&i);
            test.Link(FExemplar);
            test.Link(anchor);
            FExemplar = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            test.Unlink(anchor);

            Assert.That(test.IsLinked, Is.False);
        }

        [Test]
        [Description("Getting IsLinked handles dead holder references.")]
        public void GetIsLinked_HandlesDeadHolder()
        {
            int* i;
            var test = new SmartRef<int>(&i);
            test.Link(FExemplar);
            FExemplar = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.That(test.IsLinked, Is.False);
        }

        [Test]
        [Description("Dispose on owning executes the cleanup action.")]
        public void Dispose_Owning_ExecutesCleanup()
        {
            bool executed = false;
            var test = new SmartRef<int>(X => { executed = true; });
            test.Dispose();

            Assert.That(executed, Is.True);
        }

        [Test]
        [Description("Dispose on shared does not modify the underlying Movable.")]
        public void Dispose_Shared_DoesNothing()
        {
            int i = 10;
            int* j = &i;
            var test = new SmartRef<int>(&j);
            test.Dispose();

            Assert.That(j == &i);
            Assert.That(i, Is.EqualTo(10));
        }

        [Test]
        [Description("Dispose with linked holder propagates dispose.")]
        public void Dispose_WithLinked_PropagatesDispose()
        {
            int* i;
            var test = new SmartRef<int>(&i);
            test.Link(FExemplar);
            test.Dispose();

            Assert.That(FExemplar.IsDisposed, Is.True);
        }

        [Test]
        [Description("Geting Movable on disposed throws ObjectDisposedException.")]
        public void GetMovable_OnDisposed_ThrowsObjectDisposedException()
        {
            int* i;
            var test = new SmartRef<int>(&i);
            test.Dispose();

            Assert.That(() =>
            {
                var _ = test.Movable;
            }, Throws.InstanceOf<ObjectDisposedException>());
        }

        [Test]
        [Description("Getting Movable on not disposed returns the Movable.")]
        public void GetMovable_OnNotDisposed_ReturnsMovable()
        {
            int* i;
            var test = new SmartRef<int>(&i);

            Assert.That(test.Movable.Raw == &i);
        }

        [Test]
        [Description("Implicit cast to bool on not disposed returns true.")]
        public void ImplicitCast_ToBoolOnNotDisposed_ReturnsTrue()
        {
            int* i;
            bool test = new SmartRef<int>(&i);

            Assert.That(test, Is.True);
        }

        [Test]
        [Description("Implicit cast to bool on disposed returns false.")]
        public void ImplicitCast_ToBoolOnDisposed_ReturnsFalse()
        {
            int* i;
            var smartRef = new SmartRef<int>(&i);
            smartRef.Dispose();
            bool test = smartRef;

            Assert.That(test, Is.False);
        }

        [Test]
        [Description("Implicit cast to movable on disposed throws ObjectDisposedException.")]
        public void ImplicitCast_ToMovableOnDisposed_ThrowsObjectDisposedException()
        {
            int* i;
            var smartRef = new SmartRef<int>(&i);
            smartRef.Dispose();

            Assert.That(() =>
            {
                Movable<int> _ = smartRef;
            }, Throws.InstanceOf<ObjectDisposedException>());
        }

        [Test]
        [Description("Implicit cast to movable on not disposed returns Movable.")]
        public void ImplicitCast_ToMovableOnNotDisposed_ReturnsMovable()
        {
            int* i;
            var smartRef = new SmartRef<int>(&i);
            Movable<int> test = smartRef;

            Assert.That(test.Raw == &i);
        }

        [Test]
        [Description("Implicit cast to fixed on disposed throws ObjectDispoedException.")]
        public void ImplicitCast_ToFixedOnDisposed_ThrowsObjectDisposedException()
        {
            int* i;
            var smartRef = new SmartRef<int>(&i);
            smartRef.Dispose();

            Assert.That(() =>
            {
                Fixed<int> _ = smartRef;
            }, Throws.InstanceOf<ObjectDisposedException>());
        }

        [Test]
        [Description("Implicit cast to fixed on not disposed returns the Fixed target.")]
        public void ImplicitCast_ToFixedOnNotDisposed_ReturnsTarget()
        {
            int i;
            int* j = &i;
            var smartRef = new SmartRef<int>(&j);
            Fixed<int> test = smartRef;

            Assert.That(test.Raw == j);
        }
    }
    // ReSharper restore errors
}
