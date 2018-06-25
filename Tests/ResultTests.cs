using System;

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
        [Description("Ok returns a successful result.")]
        public void Ok_IsSuccess()
        {
            Assert.That(Result.Ok().IsSuccess);
        }

        [Test]
        [Description("Fail with null throws an ArgumentNullException.")]
        public void Fail_Null_Throws_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Result.Fail(null));
        }

        [Test]
        [Description("Fail with Exception wraps that Exception.")]
        public void Fail_NotNull_IsError()
        {
            Assert.That(!Result.Fail(Result.UninitializedError).IsSuccess);
        }

        [Test]
        [Description("Default initialized is an error.")]
        public void Default_IsError()
        {
            Result result = default;
            Assert.That(!result.IsSuccess);
        }

        [Test]
        [Description("Equality is upheld.")]
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
        [Description("ThrowIfError on OK does nothing.")]
        public void ThrowIfError_Ok_NoOperation()
        {
            Assert.DoesNotThrow(() => Result.Ok().ThrowIfError());
        }

        [Test]
        [Description("ThrowIfError on error throws that error.")]
        public void ThrowIfError_Error_ThrowsContained()
        {
            var error = new Exception();
            var thrown = Assert.Throws<Exception>(() => Result.Fail(error).ThrowIfError());

            Assert.That(thrown, Is.SameAs(error));
        }

        [Test]
        [Description("OnSuccess on OK is executed.")]
        public void OnSuccess_Ok_IsExecuted()
        {
            var executed = false;
            Result.Ok().OnSuccess(() => executed = true);

            Assert.That(executed);
        }

        [Test]
        [Description("OnSuccess on error is not executed.")]
        public void OnSuccess_Error_NoOperation()
        {
            var executed = false;
            Result.Fail(Result.UninitializedError).OnSuccess(() => executed = true);

            Assert.That(!executed);
        }

        [Test]
        [Description("OnSuccess propagates no matter what.")]
        public void OnSuccess_Propagates()
        {
            var ok = Result.Ok();
            var err = Result.Fail(new Exception());

            Assert.That(ok.OnSuccess(() => { }), Is.EqualTo(ok));
            Assert.That(err.OnSuccess(() => { }), Is.EqualTo(err));
        }

        [Test]
        [Description("OnError on OK does nothing.")]
        public void OnError_Ok_NoOperation()
        {
            var executed = false;
            Result.Ok().OnError(X => executed = true);

            Assert.That(!executed);
        }

        [Test]
        [Description("OnError on error is executed on that error.")]
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
        [Description("OnError propagates no matter what.")]
        public void OnError_Propagates()
        {
            var ok = Result.Ok();
            var err = Result.Fail(new Exception());

            Assert.That(ok.OnError(X => { }), Is.EqualTo(ok));
            Assert.That(err.OnError(X => { }), Is.EqualTo(err));
        }

        [Test]
        [Description("AndThen on error propagates that error.")]
        public void AndThen_Error_Propagates()
        {
            var err = Result.Fail(new Exception());

            Assert.That(err.AndThen(() => 1).Error, Is.SameAs(err.Error));
        }

        [Test]
        [Description("AndThen on success wraps the result.")]
        public void AndThen_Success_Wraps()
        {
            var ok = Result.Ok();

            Assert.That(ok.AndThen(() => 1).Value, Is.EqualTo(1));
        }

        [Test]
        [Description("Error of OK is null.")]
        public void Error_Ok_IsNull()
        {
            Assert.That(Result.Ok().Error, Is.Null);
        }

        [Test]
        [Description("Error of error is that error.")]
        public void Error_Error_IsError()
        {
            var error = new Exception();
            Assert.That(Result.Fail(error).Error, Is.SameAs(error));
        }

        [Test]
        [Description("Error of default initialized is not null.")]
        public void Error_Default_IsNotNull()
        {
            Result result = default;
            Assert.That(result.Error, Is.Not.Null);
        }

        [Test]
        [Description("Implicit conversion to bool of error is false.")]
        public void ImplicitToBool_Error_IsFalse()
        {
            Assert.That(!Result.Fail(Result.UninitializedError));
        }

        [Test]
        [Description("Implicit conversion to bool of OK is true.")]
        public void ImplicitToBool_Ok_IsTrue()
        {
            Assert.That(Result.Ok());
        }

        [Test]
        [Description("Implicit conversion from bool of true is success.")]
        public void ImplicitFromBool_True_IsOk()
        {
            Result result = true;
            Assert.That(result.IsSuccess);
        }

        [Test]
        [Description("Implicit conversion from bool of false is default initialized.")]
        public void ImplicitFromBool_False_IsError()
        {
            Result result = false;
            Assert.That(!result.IsSuccess);
        }

        [Test]
        [Description("Implicit conversion from exception that is not null is that error.")]
        public void ImplicitFromException_NotNull_IsError()
        {
            Result result = Result.UninitializedError;
            Assert.That(!result.IsSuccess);
        }

        [Test]
        [Description("Implicit conversion from exception that is null is OK.")]
        public void ImplicitFromException_Null_IsOk()
        {
            Result result = (Exception)null;
            Assert.That(result.IsSuccess);
        }

        [Test]
        [Description("Implicit conversion to exception is Error.")]
        public void ImplicitToException_IsError()
        {
            var error = new Exception();

            Exception ok = Result.Ok();
            Exception err = Result.Fail(error);

            Assert.That(ok, Is.Null);
            Assert.That(err, Is.SameAs(error));
        }
    }
}
