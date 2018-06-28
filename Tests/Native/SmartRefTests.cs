using System;
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace FFSharp.Native
{
    [TestFixture]
    [Description("Tests for the SmartRef container class.")]
    [Category("Native")]
    [Category("Utilities")]
    [TestOf(typeof(SmartRef<>))]
    // ReSharper disable errors
    public unsafe class SmartRefTests
    {
        public struct Unmanaged
        {
            public int a;
            public int b;
        }

        public class Subscriber : IDisposable
        {
            public static int Disposed;

            ~Subscriber()
            {
                Dispose();
            }

            public void Dispose()
            {
                ++Disposed;
                GC.SuppressFinalize(this);
            }
        }

        Movable<Unmanaged> FSharedPtr;
        Fixed<Unmanaged> FSharedInst;

        SmartRef<Unmanaged> FOwning;
        SmartRef<Unmanaged> FShared;

        Subscriber FSubscriber;
        Subscriber FDead;

        bool FCleanupExecuted;

        [OneTimeSetUp]
        public void Alloc()
        {
            FSharedPtr = Marshal.AllocHGlobal(sizeof(void*));
            FSharedInst = Marshal.AllocHGlobal(Marshal.SizeOf<Unmanaged>());
        }

        [OneTimeTearDown]
        public void Free()
        {
            Marshal.FreeHGlobal(FSharedInst);
            Marshal.FreeHGlobal(FSharedPtr);
        }

        [SetUp]
        public void Setup()
        {
            FSharedInst.Raw->a = 1;
            FSharedInst.Raw->b = 1;

            FSharedPtr.SetTarget(FSharedInst);

            FShared = new SmartRef<Unmanaged>(FSharedPtr);
            FOwning = new SmartRef<Unmanaged>(X => FCleanupExecuted = true);

            FSubscriber = new Subscriber();
            FDead = new Subscriber();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Subscriber.Disposed = 0;

            FCleanupExecuted = false;
        }

        #region Constructors
        [Test]
        [Description("New shared instances indicate shared.")]
        public void New_Shared_IsShared()
        {
            Assert.That(!FShared.IsOwning);
        }

        [Test]
        [Description("New shared instances wrap the shared pointer.")]
        public void New_Shared_WrapsPointer()
        {
            Assert.That(FShared.Movable == FSharedPtr);
        }

        [Test]
        [Description("New owning instances indicate owning.")]
        public void New_Owning_IsOwning()
        {
            Assert.That(FOwning.IsOwning);
        }

        [Test]
        [Description("New owning instances allocate a new Movable with null taget.")]
        public void New_Owning_AllocatesNewZeroed()
        {
            Assert.That(FOwning.Movable);
            Assert.That(FOwning.Movable.Target == null);
        }
        #endregion

        [Test]
        [Description("Acquire on not disposed returns true.")]
        public void Acquire_NotDisposed_ReturnsTrue()
        {
            Assert.DoesNotThrow(() => FShared.Acquire(FSubscriber));
        }

        [Test]
        [Description("Acquire on disposed throws ObjectDisposedException.")]
        public void Acquire_Disposed_ThrowsObjectDisposedException()
        {
            FShared.Dispose();

            Assert.Throws<ObjectDisposedException>(() => FShared.Acquire(FSubscriber));
        }

        [Test]
        [Description("Acquiring does not prevent the GC from collection the subscriber.")]
        public void Acquire_DoesNotPreventGCOnSubscriber()
        {
            Assume.That(Subscriber.Disposed == 0);

            var weakRef = new WeakReference<Subscriber>(FSubscriber);
            FShared.Acquire(FSubscriber);
            FSubscriber = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assume.That(!weakRef.TryGetTarget(out _));

            Assert.That(Subscriber.Disposed == 1);
        }

        [Test]
        [Description("Releasing and propagating dispose can deal with dead subscribers.")]
        public void ReleaseAndPropagate_CanHandleDeadRefs()
        {
            Assume.That(Subscriber.Disposed == 0);

            var weakRef = new WeakReference<Subscriber>(FDead);
            FShared.Acquire(FDead);
            FShared.Acquire(FSubscriber);

            FDead = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assume.That(!weakRef.TryGetTarget(out _));

            FShared.Release(FSubscriber);
            FShared.Dispose();

            Assert.That(Subscriber.Disposed == 1);
        }

        [Test]
        [Description("Release on unsubscribed does nothing.")]
        public void Release_Missing_DoesNothing()
        {
            Assume.That(Subscriber.Disposed == 0);

            FShared.Acquire(FSubscriber);

            FShared.Release(FShared);
            FShared.Dispose();

            Assert.That(Subscriber.Disposed == 1);
        }

        #region Dispose
        [Test]
        [Description("Disposing propagates to all subscribers.")]
        public void Dispose_PropagatesToSubscribers()
        {
            Assume.That(Subscriber.Disposed == 0);

            FShared.Acquire(FSubscriber);
            FShared.Dispose();

            Assert.That(Subscriber.Disposed == 1);
        }

        [Test]
        [Description("Disposing does not propagate to ex subscribers.")]
        public void Dispose_DoesNotPropagateToUnsubscribed()
        {
            Assume.That(Subscriber.Disposed == 0);

            FShared.Acquire(FSubscriber);
            FShared.Release(FSubscriber);
            FShared.Dispose();

            Assert.That(Subscriber.Disposed == 0);
        }

        [Test]
        [Description("Disposing shared detaches the Movable without modifying it.")]
        public void Dispose_Shared_DetachesWithoutModify()
        {
            Assume.That(FShared.Movable == FSharedPtr);
            Assume.That(FSharedPtr.Target == FSharedInst);

            FShared.Dispose();

            Assert.That(!FShared.Movable);
            Assert.That(FSharedPtr.Target == FSharedInst);
        }

        [Test]
        [Description("Disposing owning executes the cleanup and detaches the Movable.")]
        public void Dispose_Owning_ExecutesCleanupAndDetaches()
        {
            Assume.That(!FCleanupExecuted);

            FOwning.Dispose();

            Assert.That(FCleanupExecuted);
            Assert.That(!FOwning.Movable);
        }
        #endregion
    }
    // ReSharper restore errors
}
