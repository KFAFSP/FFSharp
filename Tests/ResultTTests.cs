using System;

using NUnit.Framework;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable EqualExpressionComparison
#pragma warning disable CS1718 // Reflexive equality comparions

namespace FFSharp
{
    [TestFixture]
    [Description("Tests for the Result<T> struct.")]
    [Category("Utilities")]
    [TestOf(typeof(Result<>))]
    public class ResultTTests
    {
        Exception FException;

        Result<int> FOne;
        Result<int> FTwo;
        Result<int> FError;
        Result<int> FDefault;

        [SetUp]
        public void Setup()
        {
            FException = new Exception();

            FOne = Result.Ok(1);
            FTwo = Result.Ok(2);
            FError = Result.Fail<int>(FException);
            FDefault = default;
        }

        #region Constructors
        [Test]
        [Description("Ok returns a successful result.")]
        public void Ok_IsSuccess()
        {
            Assert.That(FOne.IsSuccess);
            Assert.That(FTwo.IsSuccess);
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
        [Description("Erroneous results are never equal to values.")]
        public void Equals_ErrorOnValue_False()
        {
            Assume.That(!FError.IsSuccess);

            Assert.That(!FError.Equals(0));
            Assert.That(FError != 0);
        }

        [Test]
        [Description("Successful results are equal to their contained values.")]
        public void Equals_SuccessOnValue_ValueEquals()
        {
            Assume.That(FOne.IsSuccess);

            Assert.That(FOne.Equals(1));
            Assert.That(FOne == 1);

            Assert.That(!FOne.Equals(2));
            Assert.That(FOne != 2);
        }

        [Test]
        [Description("Successes are compared using the values contained.")]
        public void Equals_Success_ValueEquals()
        {
            Assert.That(FOne.Equals(FOne));
            Assert.That(FOne == FOne);

            Assert.That(!FOne.Equals(FTwo));
            Assert.That(FOne != FTwo);
        }

        [Test]
        [Description("Uninitialized results are never equal.")]
        public void Equals_NotReflexiveOnUninitialized()
        {
            Assert.That(!FDefault.Equals(FDefault));
            Assert.That(FDefault != FDefault);
        }

        [Test]
        [Description("Erroneous results wrapping the same error instance are equal.")]
        public void Equals_SameError_True()
        {
            var error = Result.Fail<int>(FException);

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
            Assume.That(FOne.IsSuccess);
            Assume.That(!FError.IsSuccess);

            Assert.That(!FOne.Equals(FError));
            Assert.That(FOne != FError);
        }
        #endregion

        [Test]
        [Description("ThrowIfError on successful does nothing.")]
        public void ThrowIfError_Ok_NoOperation()
        {
            Assume.That(FOne.IsSuccess);

            Assert.DoesNotThrow(() => FOne.ThrowIfError());
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
        [Description("OrDefault with a successful returns the contained value.")]
        public void OrDefault_Success_ReturnsValue()
        {
            Assume.That(FOne.IsSuccess);

            Assert.That(FOne.OrDefault(2), Is.EqualTo(1));
        }

        [Test]
        [Description("OrDefault with an erroneous returns the default value.")]
        public void OrDefault_Error_ReturnsDefault()
        {
            Assume.That(!FError.IsSuccess);

            Assert.That(FError.OrDefault(1), Is.EqualTo(1));
        }

        [Test]
        [Description("Or with a succesful returns this.")]
        public void Or_Success_ReturnsThis()
        {
            Assume.That(FOne.IsSuccess);

            Assert.That(FOne.Or(FTwo), Is.EqualTo(FOne));
        }

        [Test]
        [Description("Or with an erroneous returns the default result.")]
        public void Or_Error_ReturnsDefault()
        {
            Assume.That(!FError.IsSuccess);

            Assert.That(FError.Or(FOne), Is.EqualTo(FOne));
        }

        [Test]
        [Description("OnSuccess on successful is executed on the value.")]
        public void OnSuccess_Ok_IsExecuted()
        {
            Assume.That(FOne.IsSuccess);

            var pass = false;
            FOne.OnSuccess(X =>
            {
                Assert.That(X, Is.EqualTo(1));
                pass = true;
            });

            Assert.That(pass);
        }

        [Test]
        [Description("OnSuccess on erroneous is not executed.")]
        public void OnSuccess_Error_NoOperation()
        {
            Assume.That(!FError.IsSuccess);

            var pass = true;
            FError.OnSuccess(X => pass = false);

            Assert.That(pass);
        }

        [Test]
        [Description("OnSuccess propagates no matter what.")]
        public void OnSuccess_Propagates()
        {
            Assume.That(FOne.IsSuccess);
            Assume.That(!FError.IsSuccess);

            Assert.That(FOne.OnSuccess(X => { }), Is.EqualTo(FOne));
            Assert.That(FError.OnSuccess(X => { }), Is.EqualTo(FError));
        }

        [Test]
        [Description("OnError on successful does nothing.")]
        public void OnError_Ok_NoOperation()
        {
            Assume.That(FOne.IsSuccess);

            var pass = true;
            FOne.OnError(X => pass = false);

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
            Assume.That(FOne.IsSuccess);
            Assume.That(!FError.IsSuccess);

            Assert.That(FOne.OnError(X => { }), Is.EqualTo(FOne));
            Assert.That(FError.OnError(X => { }), Is.EqualTo(FError));
        }

        [Test]
        [Description("AndThen on erroneous propagates that error.")]
        public void AndThen_Error_Propagates()
        {
            Assume.That(!FError.IsSuccess);

            Assert.That(FError.AndThen(X => X*2).Error, Is.SameAs(FError.Error));
        }

        [Test]
        [Description("AndThen on successful wraps the map result.")]
        public void AndThen_Success_Wraps()
        {
            Assume.That(FTwo.IsSuccess);

            Assert.That(FTwo.AndThen(X => X*2).Value, Is.EqualTo(4));
        }

        #region Properties
        [Test]
        [Description("Error of successful is null.")]
        public void Error_Ok_IsNull()
        {
            Assume.That(FOne.IsSuccess);

            Assert.That(FOne.Error, Is.Null);
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

        [Test]
        [Description("Value of successful returns the contained value.")]
        public void Value_Ok_IsValue()
        {
            Assume.That(FOne.IsSuccess);

            Assert.That(FOne.Value, Is.EqualTo(1));
        }

        [Test]
        [Description("Value of erroneous throws the contained error.")]
        public void Value_Error_ThrowsContained()
        {
            Assume.That(!FError.IsSuccess);

            var thrown = Assert.Throws<Exception>(() =>
            {
                var value = FError.Value;
                Assert.That(value == 0); // Dummy to use var.
            });
            Assert.That(thrown, Is.SameAs(FError.Error));
        }
        #endregion

        #region Enumerable
        [Test]
        [Description("Enumerable of successful contains just the value.")]
        public void Enumerable_Ok_IsValue()
        {
            Assume.That(FOne.IsSuccess);

            Assert.That(FOne, Is.EquivalentTo(new[] { 1 }));
        }

        [Test]
        [Description("Enumerable of erroneous is empty.")]
        public void Enumerable_Error_IsEmpty()
        {
            Assume.That(!FError.IsSuccess);

            Assert.That(FError, Is.Empty);
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
            Assume.That(FOne.IsSuccess);

            Assert.That(FOne);
        }

        [Test]
        [Description("Implicit conversion from value is equal to successful of value.")]
        public void ImplicitFromBool_True_IsOk()
        {
            Result<int> result = 1;
            Assert.That(result.Equals(FOne));
        }

        [Test]
        [Description("Implicit conversion from exception that is not null is that error.")]
        public void ImplicitFromException_NotNull_IsError()
        {
            Assume.That(!FError.IsSuccess);
            Assume.That(FError.Error, Is.SameAs(FException));

            Result<int> result = FException;
            Assert.That(result.Equals(FError));
        }

        [Test]
        [Description("Explicit conversion to value on erroneous throws contained error.")]
        public void ImplicitToValue_Error_ThrowsContained()
        {
            Assume.That(!FError.IsSuccess);

            var thrown = Assert.Throws<Exception>(() =>
            {
                var value = (int)FError;
                Assert.That(value == 0); // Dummy to use var.
            });
            Assert.That(thrown, Is.SameAs(FError.Error));
        }

        [Test]
        [Description("Explicit conversion to value on successful returns value.")]
        public void ExplicitToValue_Success_ReturnsValue()
        {
            Assume.That(FOne.IsSuccess);

            var value = (int)FOne;
            Assert.That(value == 1);
        }

        [Test]
        [Description("Implicit conversion from exception that is null throws ArgumentNullException.")]
        public void ImplicitFromException_Null_IsOk()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                Result<int> res = null;
                Assert.That(!res); // Dummy to use var.
            });
        }

        [Test]
        [Description("Implicit conversion to exception is Error property.")]
        public void ImplicitToException_IsError()
        {
            Assume.That(FOne.IsSuccess);
            Assume.That(!FError.IsSuccess);
            Assume.That(FError.Error, Is.SameAs(FException));

            Exception ok = FOne;
            Exception err = FError;

            Assert.That(ok, Is.Null);
            Assert.That(err, Is.SameAs(FException));
        }
        #endregion
    }
}

#pragma warning restore CS1718
