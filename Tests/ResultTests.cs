using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace FFSharp
{
    [TestFixture]
    [Description("Tests for the value-less Result struct.")]
    [Category("Utilities")]
    [TestOf(typeof(Result))]
    public class ResultTests
    {
        [Test]
        public void Ok_IsSuccess()
        {
            Assert.That(Result.Ok().IsSuccess);
        }

        [Test]
        public void Fail_Null_Throws_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Result.Fail(null));
        }

        [Test]
        public void Fail_NotNull_IsError()
        {
            Assert.That(!Result.Fail(Result.UninitializedError).IsSuccess);
        }

        [Test]
        public void Default_IsError()
        {
            Result result = default;
            Assert.That(!result.IsSuccess);
        }

        [Test]
        public void Equality()
        {
            Result def = default;
            Result ok = Result.Ok();
            Result err = Result.Fail(Result.UninitializedError);

            Assert.That(def, Is.Not.EqualTo(def));
            Assert.That(def != def);
            Assert.That(err, Is.EqualTo(err));
            Assert.That(err == err);
            Assert.That(err, Is.Not.EqualTo(def));
            Assert.That(err != def);

            Assert.That(ok, Is.EqualTo(ok));
            Assert.That(ok == ok);
            Assert.That(ok, Is.Not.EqualTo(err));
            Assert.That(ok != err);
        }

        [Test]
        public void ThrowIfError_Ok_NoOperation()
        {
            Assert.DoesNotThrow(() => Result.Ok().ThrowIfError());
        }

        [Test]
        public void ThrowIfError_Error_ThrowsContained()
        {
            var error = new Exception();
            var thrown = Assert.Throws<Exception>(() => Result.Fail(error).ThrowIfError());

            Assert.That(thrown, Is.SameAs(error));
        }

        [Test]
        public void OnSuccess_Ok_IsExecuted()
        {
            var executed = false;
            Result.Ok().OnSuccess(() => executed = true);

            Assert.That(executed);
        }

        [Test]
        public void OnSuccess_Error_NoOperation()
        {
            var executed = false;
            Result.Fail(Result.UninitializedError).OnSuccess(() => executed = true);

            Assert.That(!executed);
        }

        [Test]
        public void OnSuccess_Propagates()
        {
            var ok = Result.Ok();
            var err = Result.Fail(new Exception());

            Assert.That(ok.OnSuccess(() => { }), Is.EqualTo(ok));
            Assert.That(err.OnSuccess(() => { }), Is.EqualTo(err));
        }

        [Test]
        public void OnError_Ok_NoOperation()
        {
            var executed = false;
            Result.Ok().OnError(X => executed = true);

            Assert.That(!executed);
        }

        [Test]
        public void OnError_Error_IsExecuted()
        {
            var error = new Exception();
            var executed = false;
            Result.Fail(error).OnError(X =>
            {
                Assert.That(X, Is.SameAs(X));
                executed = true;
            });

            Assert.That(executed);
        }

        [Test]
        public void OnError_Propagates()
        {
            var ok = Result.Ok();
            var err = Result.Fail(new Exception());

            Assert.That(ok.OnError(X => { }), Is.EqualTo(ok));
            Assert.That(err.OnError(X => { }), Is.EqualTo(err));
        }

        [Test]
        public void AndThen_Error_Propagates()
        {
            var err = Result.Fail(new Exception());

            Assert.That(err.AndThen(() => 1).Error, Is.SameAs(err.Error));
        }

        [Test]
        public void AndThen_Success_Wraps()
        {
            var ok = Result.Ok();

            Assert.That(ok.AndThen(() => 1).Value, Is.EqualTo(1));
        }

        [Test]
        public void Error_Ok_IsNull()
        {
            Assert.That(Result.Ok().Error, Is.Null);
        }

        [Test]
        public void Error_Error_IsError()
        {
            var error = new Exception();
            Assert.That(Result.Fail(error).Error, Is.SameAs(error));
        }

        [Test]
        public void Error_Default_IsNotNull()
        {
            Result result = default;
            Assert.That(result.Error, Is.Not.Null);
        }

        [Test]
        public void ImplicitToBool_Error_IsFalse()
        {
            Assert.That(!Result.Fail(Result.UninitializedError));
        }

        [Test]
        public void ImplicitToBool_Ok_IsTrue()
        {
            Assert.That(Result.Ok());
        }

        [Test]
        public void ImplicitFromBool_True_IsOk()
        {
            Result result = true;
            Assert.That(result.IsSuccess);
        }

        [Test]
        public void ImplicitFromBool_False_IsError()
        {
            Result result = false;
            Assert.That(!result.IsSuccess);
        }

        [Test]
        public void ImplicitFromException_NotNull_IsError()
        {
            Result result = Result.UninitializedError;
            Assert.That(!result.IsSuccess);
        }

        [Test]
        public void ImplicitFromException_Null_IsOk()
        {
            Result result = (Exception)null;
            Assert.That(result.IsSuccess);
        }
    }
}
