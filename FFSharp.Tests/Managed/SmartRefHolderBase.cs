using System;

using FFSharp.Native;

using JetBrains.Annotations;

using NUnit.Framework;

// ReSharper disable AssignNullToNotNullAttribute

namespace FFSharp.Managed
{
    [TestFixture]
    [Description("Testing the SmartRefHolderBase type itself.")]
    [Category("Managed")]
    [TestOf(typeof(SmartRefHolderBase<>))]
    // ReSharper disable errors
    public sealed unsafe class SmartRefHolderTests
    {
        internal sealed class Exemplar : SmartRefHolderBase<int>
        {
            public Exemplar([NotNull] SmartRef<int> ASmartRef)
                : base(ASmartRef)
            { }
        }

        [Test]
        [Description("Constructor with null throws ArgumentNullException.")]
        public void Constructor_Null_ThrowsArgumentNullException()
        {
            Assert.That(() =>
            {
                var _ = new Exemplar(null);
            }, Throws.ArgumentNullException);
        }

        [Test]
        [Description("Constructor with not null wraps the SmartRef.")]
        public void Constructor_NotNull_WrapsSmartRef()
        {
            int* i;
            var smartRef = new SmartRef<int>(&i);
            var test = new Exemplar(smartRef);

            Assert.That(test.Movable.Raw == &i);
        }

        [Test]
        [Description("Constructor links with SmartRef.")]
        public void Constructor_Links()
        {
            int* i;
            var smartRef = new SmartRef<int>(&i);
            var test = new Exemplar(smartRef);

            Assert.That(smartRef.IsLinked, Is.True);
        }

        [Test]
        [Description("Dispose unlinks with SmartRef.")]
        public void Dispose_Unlinks()
        {
            int* i;
            var smartRef = new SmartRef<int>(&i);
            var test = new Exemplar(smartRef);
            test.Dispose();

            Assert.That(smartRef.IsDisposed, Is.True);
        }

        [Test]
        [Description("Getting Movable on disposed throws ObjectDisposedException.")]
        public void GetMovable_Disposed_ThrowsObjectDisposedException()
        {
            int* i;
            var smartRef = new SmartRef<int>(&i);
            var test = new Exemplar(smartRef);
            test.Dispose();

            Assert.That(() =>
            {
                var _ = test.Movable;
            }, Throws.InstanceOf<ObjectDisposedException>());
        }

        [Test]
        [Description("Getting Movable on not disposed returns the Movable.")]
        public void GetMovable_NotDisposed_ReturnsMovable()
        {
            int* i;
            var smartRef = new SmartRef<int>(&i);
            var test = new Exemplar(smartRef);

            Assert.That(test.Movable.Raw == &i);
        }

        [Test]
        [Description("Getting Fixed on disposed throws ObjectDisposedException.")]
        public void GetFixed_Disposed_ThrowsObjectDisposedException()
        {
            int* i;
            var smartRef = new SmartRef<int>(&i);
            var test = new Exemplar(smartRef);
            test.Dispose();

            Assert.That(() =>
            {
                var _ = test.Fixed;
            }, Throws.InstanceOf<ObjectDisposedException>());
        }

        [Test]
        [Description("Getting Fixed on not disposed returns the target.")]
        public void GetFixed_NotDisposed_ReturnsTarget()
        {
            int i;
            int* j = &i;
            var smartRef = new SmartRef<int>(&j);
            var test = new Exemplar(smartRef);

            Assert.That(test.Fixed.Raw == j);
        }

        [Test]
        [Description("Setting Fixed on disposed throws ObjectDisposedException.")]
        public void SetFixed_Disposed_ThrowsObjectDisposedException()
        {
            int* i;
            var smartRef = new SmartRef<int>(&i);
            var test = new Exemplar(smartRef);
            test.Dispose();

            Assert.That(() =>
            {
                test.Fixed = null;
            }, Throws.InstanceOf<ObjectDisposedException>());
        }

        [Test]
        [Description("Setting Fixed on not disposed changes the target.")]
        public void SetFixed_NotDisposed_ChangesTarget()
        {
            int i;
            int* j = null;
            var smartRef = new SmartRef<int>(&j);
            var test = new Exemplar(smartRef);

            test.Fixed = &i;
            Assert.That(j == &i);
        }
    }
    // ReSharper restore errors
}
