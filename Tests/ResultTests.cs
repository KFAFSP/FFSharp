using System;

using NUnit.Framework;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable EqualExpressionComparison
#pragma warning disable CS1718 // Reflexive equality comparions

namespace FFSharp
{
    [TestFixture]
    [Description("Tests for the value-less Result struct.")]
    [Category("Utilities")]
    [TestOf(typeof(Result))]
    public class ResultTests
    {
        Exception FException;

        Result FOk;
        Result FError;
        Result FDefault;

        [SetUp]
        public void Setup()
        {
            FException = new Exception();

            FOk = Result.Ok();
            FError = Result.Fail(FException);
            FDefault = default;
        }

        #region Constructors
        [Test]
        [Description("Ok returns a successful result.")]
        public void Ok_IsSuccess()
        {
            Assert.That(FOk.IsSuccess);
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
            Assert.That(!FError.IsSuccess);
        }

        [Test]
        [Description("Default initialized is an error.")]
        public void Default_IsError()
        {
            Assert.That(!FDefault.IsSuccess);
        }
        #endregion

        #region Equals
        [Test]
        [Description("Initialized results are always equal to themselves.")]
        public void Equals_ReflexiveOnInitialized()
        {
            Assert.That(FOk.Equals(FOk));
            Assert.That(FOk == FOk);

            Assert.That(FError.Equals(FError));
            Assert.That(FError == FError);
        }

        [Test]
        [Description("Uninitialized results are never equal.")]
        public void Equals_NotReflexiveOnUninitialized()
        {
            Assert.That(!FDefault.Equals(FDefault));
            Assert.That(FDefault != FDefault);
        }

        [Test]
        [Description("Successful results are always equal.")]
        public void Equals_Successes_True()
        {
            var ok = Result.Ok();

            Assume.That(FOk.IsSuccess);
            Assume.That(ok.IsSuccess);

            Assert.That(FOk.Equals(ok));
            Assert.That(FOk == ok);
        }

        [Test]
        [Description("Erroneous results wrapping the same error instance are equal.")]
        public void Equals_SameError_True()
        {
            var error = Result.Fail(FException);

            Assume.That(FError.Error, Is.SameAs(FException));
            Assume.That(error.Error, Is.SameAs(FException));

            Assert.That(FError.Equals(error));
            Assert.That(FError == error);
        }

        [Test]
        [Description("Erroneous results wrapping different errors are not equal.")]
        public void Equals_DifferentError_False()
        {
            Assume.That(FError.Error, Is.Not.SameAs(FDefault.Error));

            Assert.That(!FError.Equals(FDefault));
            Assert.That(FError != FDefault);
        }

        [Test]
        [Description("Successful results are never equal to erroneous results.")]
        public void Equals_SuccessAndError_False()
        {
            Assume.That(FOk.IsSuccess);
            Assume.That(!FError.IsSuccess);

            Assert.That(!FOk.Equals(FError));
            Assert.That(FOk != FError);
        }
        #endregion

        [Test]
        [Description("ThrowIfError on successful does nothing.")]
        public void ThrowIfError_Ok_NoOperation()
        {
            Assume.That(FOk.IsSuccess);

            Assert.DoesNotThrow(() => FOk.ThrowIfError());
        }

        [Test]
        [Description("ThrowIfError on erroneous throws the wrapped error.")]
        public void ThrowIfError_Error_ThrowsContained()
        {
            Assume.That(!FError.IsSuccess);
            Assume.That(FError.Error, Is.SameAs(FException));

            var thrown = Assert.Throws<Exception>(() => FError.ThrowIfError());
            Assert.That(thrown, Is.SameAs(FException));
        }

        [Test]
        [Description("OnSuccess on successful is executed.")]
        public void OnSuccess_Ok_IsExecuted()
        {
            Assume.That(FOk.IsSuccess);

            var pass = false;
            FOk.OnSuccess(() => pass = true);

            Assert.That(pass);
        }

        [Test]
        [Description("OnSuccess on erroneous is not executed.")]
        public void OnSuccess_Error_NoOperation()
        {
            Assume.That(!FError.IsSuccess);

            var pass = true;
            FError.OnSuccess(() => pass = false);

            Assert.That(pass);
        }

        [Test]
        [Description("OnSuccess propagates no matter what.")]
        public void OnSuccess_Propagates()
        {
            Assume.That(FOk.IsSuccess);
            Assume.That(!FError.IsSuccess);

            Assert.That(FOk.OnSuccess(() => { }), Is.EqualTo(FOk));
            Assert.That(FError.OnSuccess(() => { }), Is.EqualTo(FError));
        }

        [Test]
        [Description("OnError on successful does nothing.")]
        public void OnError_Ok_NoOperation()
        {
            Assume.That(FOk.IsSuccess);

            var pass = true;
            FOk.OnError(X => pass = false);

            Assert.That(pass);
        }

        [Test]
        [Description("OnError on erroneous is executed on that error.")]
        public void OnError_Error_IsExecuted()
        {
            Assume.That(!FError.IsSuccess);

            var pass = false;
            FError.OnError(X =>
            {
                Assert.That(X, Is.SameAs(FError.Error));
                pass = true;
            });

            Assert.That(pass);
        }

        [Test]
        [Description("OnError propagates no matter what.")]
        public void OnError_Propagates()
        {
            Assume.That(FOk.IsSuccess);
            Assume.That(!FError.IsSuccess);

            Assert.That(FOk.OnError(X => { }), Is.EqualTo(FOk));
            Assert.That(FError.OnError(X => { }), Is.EqualTo(FError));
        }

        [Test]
        [Description("AndThen on erroneous propagates that error.")]
        public void AndThen_Error_Propagates()
        {
            Assume.That(!FError.IsSuccess);

            Assert.That(FError.AndThen(() => 1).Error, Is.SameAs(FError.Error));
        }

        [Test]
        [Description("AndThen on successful wraps the call result.")]
        public void AndThen_Success_Wraps()
        {
            Assume.That(FOk.IsSuccess);

            Assert.That(FOk.AndThen(() => 1).Value, Is.EqualTo(1));
        }

        #region Properties
        [Test]
        [Description("Error of successful is null.")]
        public void Error_Ok_IsNull()
        {
            Assume.That(FOk.IsSuccess);

            Assert.That(FOk.Error, Is.Null);
        }

        [Test]
        [Description("Error of erroneous is that error.")]
        public void Error_Error_IsError()
        {
            Assume.That(!FError.IsSuccess);

            Assert.That(FError.Error, Is.SameAs(FException));
        }

        [Test]
        [Description("Error of default initialized is not null.")]
        public void Error_Default_IsNotNull()
        {
            Assume.That(!FDefault.IsSuccess);

            Assert.That(FDefault.Error, Is.Not.Null);
        }
        #endregion

        #region Conversion operators
        [Test]
        [Description("Implicit conversion to bool of erroneous is false.")]
        public void ImplicitToBool_Error_IsFalse()
        {
            Assume.That(!FError.IsSuccess);

            Assert.That(!FError);
        }

        [Test]
        [Description("Implicit conversion to bool of successful is true.")]
        public void ImplicitToBool_Ok_IsTrue()
        {
            Assume.That(FOk.IsSuccess);

            Assert.That(FOk);
        }

        [Test]
        [Description("Implicit conversion from bool of true is successful.")]
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
            Assume.That(!FError.IsSuccess);
            Assume.That(FError.Error, Is.SameAs(FException));

            Result result = FException;
            Assert.That(result.Equals(FError));
        }

        [Test]
        [Description("Implicit conversion from exception that is null is successful.")]
        public void ImplicitFromException_Null_IsOk()
        {
            Result result = null;
            Assert.That(result.IsSuccess);
        }

        [Test]
        [Description("Implicit conversion to exception is Error property.")]
        public void ImplicitToException_IsError()
        {
            Assume.That(FOk.IsSuccess);
            Assume.That(!FError.IsSuccess);
            Assume.That(FError.Error, Is.SameAs(FException));

            Exception ok = FOk;
            Exception err = FError;

            Assert.That(ok, Is.Null);
            Assert.That(err, Is.SameAs(FException));
        }
        #endregion
    }
}

#pragma warning restore CS1718
