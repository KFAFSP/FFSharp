using System;

using NUnit.Framework;

namespace FFSharp
{
    [TestFixture]
    [Description("Tests for the Result<T> struct.")]
    [Category("Utilities")]
    [TestOf(typeof(Result<>))]
    public class ResultTTests
    {
        [Test]
        [Description("Ok returns a successful result.")]
        public void Ok_IsSuccess()
        {
            Assert.That(Result.Ok(0).IsSuccess);
        }

        [Test]
        [Description("Fail with null throws an ArgumentNullException.")]
        public void Fail_Null_Throws_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Result.Fail<int>(null));
        }

        [Test]
        [Description("Fail with Exception wraps that Exception.")]
        public void Fail_NotNull_IsError()
        {
            Assert.That(!Result.Fail<int>(Result.UninitializedError).IsSuccess);
        }

        [Test]
        [Description("Default initialized is an error.")]
        public void Default_IsError()
        {
            Result<int> result = default;
            Assert.That(!result.IsSuccess);
        }

        [Test]
        [Description("Equality is upheld.")]
        public void Equality()
        {
            Result<int> def = default;
            Result<int> ok0 = Result.Ok(0);
            Result<int> ok1 = Result.Ok(1);
            Result<int> err = Result.Fail<int>(Result.UninitializedError);

            Assert.That(def, Is.Not.EqualTo(def));
            Assert.That(def != def);
            Assert.That(err, Is.EqualTo(err));
            Assert.That(err == err);
            Assert.That(err, Is.Not.EqualTo(def));
            Assert.That(err != def);

            Assert.That(ok0, Is.EqualTo(ok0));
            Assert.That(ok0 == ok0);
            Assert.That(ok0, Is.Not.EqualTo(err));
            Assert.That(ok0 != err);

            Assert.That(ok0, Is.Not.EqualTo(ok1));
            Assert.That(ok0 != ok1);
            Assert.That(ok1, Is.EqualTo(1));
            Assert.That(ok1 == 1);
        }

        [Test]
        [Description("ThrowIfError on OK does nothing.")]
        public void ThrowIfError_Ok_NoOperation()
        {
            Assert.DoesNotThrow(() => Result.Ok(0).ThrowIfError());
        }

        [Test]
        [Description("ThrowIfError on error throws that error.")]
        public void ThrowIfError_Error_ThrowsContained()
        {
            var error = new Exception();
            var thrown = Assert.Throws<Exception>(() => Result.Fail<int>(error).ThrowIfError());

            Assert.That(thrown, Is.SameAs(error));
        }

        [Test]
        [Description("OnSuccess on OK is executed.")]
        public void OnSuccess_Ok_IsExecuted()
        {
            var executed = false;
            Result.Ok(1).OnSuccess(X =>
            {
                Assert.That(X, Is.EqualTo(1));
                executed = true;
            });

            Assert.That(executed);
        }

        [Test]
        [Description("OnSuccess on error is not executed.")]
        public void OnSuccess_Error_NoOperation()
        {
            var executed = false;
            Result.Fail<int>(Result.UninitializedError).OnSuccess(X => executed = true);

            Assert.That(!executed);
        }

        [Test]
        [Description("OnSuccess propagates no matter what.")]
        public void OnSuccess_Propagates()
        {
            var ok = Result.Ok(0);
            var err = Result.Fail<int>(new Exception());

            Assert.That(ok.OnSuccess(X => { }), Is.EqualTo(ok));
            Assert.That(err.OnSuccess(X => { }), Is.EqualTo(err));
        }

        [Test]
        [Description("OnError on OK does nothing.")]
        public void OnError_Ok_NoOperation()
        {
            var executed = false;
            Result.Ok(0).OnError(X => executed = true);

            Assert.That(!executed);
        }

        [Test]
        [Description("OnError on error is executed on that error.")]
        public void OnError_Error_IsExecuted()
        {
            var error = new Exception();
            var executed = false;
            Result.Fail<int>(error).OnError(X =>
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
            var ok = Result.Ok(0);
            var err = Result.Fail<int>(new Exception());

            Assert.That(ok.OnError(X => { }), Is.EqualTo(ok));
            Assert.That(err.OnError(X => { }), Is.EqualTo(err));
        }

        [Test]
        [Description("AndThen on error propagates that error.")]
        public void AndThen_Error_Propagates()
        {
            var err = Result.Fail<int>(new Exception());

            Assert.That(err.AndThen(X =>
            {
                Assert.Fail();
                return X * 2;
            }).Error, Is.SameAs(err.Error));
        }

        [Test]
        [Description("AndThen on success wraps the result.")]
        public void AndThen_Success_Wraps()
        {
            var ok = Result.Ok(2);

            Assert.That(ok.AndThen(X => X*2).Value, Is.EqualTo(4));
        }

        [Test]
        [Description("Error of OK is null.")]
        public void Error_Ok_IsNull()
        {
            Assert.That(Result.Ok(0).Error, Is.Null);
        }

        [Test]
        [Description("Error of error is that error.")]
        public void Error_Error_IsError()
        {
            var error = new Exception();
            Assert.That(Result.Fail<int>(error).Error, Is.SameAs(error));
        }

        [Test]
        [Description("Error of default initialized is not null.")]
        public void Error_Default_IsNotNull()
        {
            Result<int> result = default;
            Assert.That(result.Error, Is.Not.Null);
        }

        [Test]
        [Description("Implicit conversion to bool of error is false.")]
        public void ImplicitToBool_Error_IsFalse()
        {
            Assert.That(!Result.Fail<int>(Result.UninitializedError));
        }

        [Test]
        [Description("Implicit conversion to bool of OK is true.")]
        public void ImplicitToBool_Ok_IsTrue()
        {
            Assert.That(Result.Ok(0));
        }

        [Test]
        [Description("Implicit conversion from exception that is not null is that error.")]
        public void ImplicitFromException_NotNull_IsError()
        {
            Result<int> result = Result.UninitializedError;
            Assert.That(!result.IsSuccess);
        }

        [Test]
        [Description("Implicit conversion from exception that is null is OK.")]
        public void ImplicitFromException_Null_ThrowsArgumentNullException()
        {
            Result<int> result;
            Assert.Throws<ArgumentNullException>(() => result = (Exception) null);
        }

        [Test]
        [Description("Implicit conversion to exception is Error.")]
        public void ImplicitToException_IsError()
        {
            var error = new Exception();

            Exception ok = Result.Ok(0);
            Exception err = Result.Fail<int>(error);

            Assert.That(ok, Is.Null);
            Assert.That(err, Is.SameAs(error));
        }
    }
}
