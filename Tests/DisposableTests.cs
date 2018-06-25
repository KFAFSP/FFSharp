using System;

using NUnit.Framework;

namespace FFSharp
{
    [TestFixture]
    [Description("Tests for the Disposable base class.")]
    [Category("Utilities")]
    [TestOf(typeof(Disposable))]
    public class DisposableTests
    {
        public class Exemplar : Disposable
        {
            public static int Disposed;

            protected override void Dispose(bool ADisposing)
            {
                ++Disposed;
                base.Dispose(ADisposing);
            }
        }

        Exemplar FInstance;

        [SetUp]
        public void Setup()
        {
            Exemplar.Disposed = 0;
            FInstance = new Exemplar();
        }

        [Test]
        [Description("IsDisposed is initialized correctly in the constructor.")]
        public void IsDisposed_New_False()
        {
            Assert.That(!FInstance.IsDisposed);
        }

        [Test]
        [Description("Dispose shall execute the override if not disposed already.")]
        public void Dispose_NotDisposed_Disposed()
        {
            Assume.That(!FInstance.IsDisposed);

            FInstance.Dispose();

            Assert.That(FInstance.IsDisposed);
            Assert.That(Exemplar.Disposed, Is.EqualTo(1));
        }

        [Test]
        [Description("Dispose shall do nothing if already disposed.")]
        public void Dispose_Disposed_NoOperation()
        {
            FInstance.Dispose();

            Assume.That(FInstance.IsDisposed);
            Assume.That(Exemplar.Disposed, Is.EqualTo(1));

            FInstance.Dispose();

            Assert.That(Exemplar.Disposed, Is.EqualTo(1));
        }

        [Test]
        [Description("Finalizer shall execute the override if not disposed already.")]
        public void Finalize_NotDisposed_Disposed()
        {
            var weakRef = new WeakReference<Exemplar>(FInstance);
            FInstance = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assume.That(!weakRef.TryGetTarget(out _));

            Assert.That(Exemplar.Disposed, Is.EqualTo(1));
        }

        [Test]
        [Description("Finalizer shall do nothing (be not run at all) if already disposed.")]
        public void Finalize_Disposed_NoOperation()
        {
            var weakRef = new WeakReference<Exemplar>(FInstance);

            FInstance.Dispose();

            Assume.That(FInstance.IsDisposed);
            Assume.That(Exemplar.Disposed, Is.EqualTo(1));

            FInstance = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assume.That(!weakRef.TryGetTarget(out _));

            Assert.That(Exemplar.Disposed, Is.EqualTo(1));
        }

        [Test]
        [Description("ThrowIfDisposed shall do nothing if not disposed.")]
        public void ThrowIfDisposed_NotDisposed_NoOperation()
        {
            Assume.That(!FInstance.IsDisposed);

            Assert.DoesNotThrow(() => FInstance.ThrowIfDisposed());
        }

        [Test]
        [Description("ThrowIfDisposed shall throw a standard ObjectDisposedException if disposed.")]
        public void ThrowIfDisposed_Disposed_Throws_ObjectDisposedException()
        {
            FInstance.Dispose();

            Assume.That(FInstance.IsDisposed);

            var error = Assert.Throws<ObjectDisposedException>(() => FInstance.ThrowIfDisposed());
            Assert.That(error.ObjectName, Is.EqualTo(typeof(Exemplar).FullName));
        }
    }
}
