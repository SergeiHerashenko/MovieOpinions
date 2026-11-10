using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Results;

namespace Authorization.Domain.UnitTests.Results
{
    public class ResultOfTTests
    {
        #region Helpers
        private static Error CreateError(
            string code,
            string message,
            ErrorType errorType)
        {
            return new Error(
                code,
                message,
                errorType
            );
        }
        #endregion

        #region Success
        [Fact]
        public void Success_ReferenceValue_PreservesSameInstance()
        {
            var value = new object();

            var result = Result<object>.Success(value);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Errors);
            Assert.Empty(result.Errors);

            Assert.Same(
                value,
                result.Value
            );
        }

        [Theory]
        [InlineData("text")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        public void Success_StringValue_PreservesExactValue(string value)
        {
            var result = Result<string>.Success(value);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Empty(result.Errors);

            Assert.Equal(
                value,
                result.Value
            );
        }

        [Fact]
        public void Success_NullReferenceValue_PreservesNull()
        {
            var result = Result<string?>.Success(null);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Empty(result.Errors);
            Assert.Null(result.Value);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(42)]
        public void Success_ValueType_PreservesValue(int value)
        {
            var result = Result<int>.Success(value);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Empty(result.Errors);

            Assert.Equal(
                value,
                result.Value
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        [InlineData(42)]
        public void Success_NullableValueType_PreservesValue(int? value)
        {
            var result = Result<int?>.Success(value);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Empty(result.Errors);

            Assert.Equal(
                value,
                result.Value
            );
        }
        #endregion

        #region Value: successful result
        [Fact]
        public void Value_SuccessfulResult_RepeatedReadsPreserveValueAndState()
        {
            var value = new object();
            var result = Result<object>.Success(value);

            Assert.Same(
                value,
                result.Value
            );

            Assert.Same(
                value,
                result.Value
            );

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Empty(result.Errors);
        }
        #endregion

        #region Failure
        [Fact]
        public void Failure_SingleError_ReturnsFailureWithSameError()
        {
            var error = CreateError(
                "ERR.SINGLE",
                "Single error",
                ErrorType.Validation
            );

            var result = Result<int>.Failure(error);

            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);

            Assert.Same(
                error,
                Assert.Single(result.Errors)
            );
        }

        [Fact]
        public void Failure_ErrorCollection_PreservesErrorsAndOrder()
        {
            var firstError = CreateError(
                "ERR.FIRST",
                "First error",
                ErrorType.Validation
            );
            var secondError = CreateError(
                "ERR.SECOND",
                "Second error",
                ErrorType.BusinessRule
            );

            var result = Result<int>.Failure(
                new[] { firstError, secondError }
            );

            Assert.True(result.IsFailure);

            Assert.Equal(
                2,
                result.Errors.Count
            );

            Assert.Same(
                firstError,
                result.Errors.ElementAt(0)
            );

            Assert.Same(
                secondError,
                result.Errors.ElementAt(1)
            );
        }

        [Fact]
        public void Failure_NullError_ThrowsDomainInvariantViolationException()
        {
            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                Result<int>.Failure((Error)null!)
            );

            Assert.Equal(
                "DOMAIN.INVARIANT_VIOLATION.INVALID_STATE",
                exception.ExceptionCode
            );

            Assert.Equal(
                "Result",
                exception.Context["Type"]
            );

            Assert.Equal(
                1,
                exception.Context["State_ErrorCount"]
            );

            Assert.Equal(
                1,
                exception.Context["State_NullErrorCount"]
            );
        }

        [Fact]
        public void Failure_NullErrorCollection_ThrowsDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                Result<int>.Failure((IEnumerable<Error>)null!)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.EMPTY_VALUE",
                exception.ExceptionCode
            );

            Assert.Equal(
                "errors",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Result",
                exception.Context["Type"]
            );
        }

        [Fact]
        public void Failure_EmptyErrorCollection_ThrowsDomainInvariantViolationException()
        {
            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                Result<int>.Failure(Array.Empty<Error>())
            );

            Assert.Equal(
                "A failed result must contain at least one error.",
                exception.Context["Rule"]
            );

            Assert.Equal(
                "Result",
                exception.Context["Type"]
            );
        }

        [Fact]
        public void Failure_ErrorCollectionContainsNull_ThrowsDomainInvariantViolationException()
        {
            var firstError = CreateError(
                "ERR.FIRST",
                "First error",
                ErrorType.Validation
            );

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                Result<int>.Failure(new Error?[] { firstError, null! }!)
            );

            Assert.Equal(
                2,
                exception.Context["State_ErrorCount"]
            );

            Assert.Equal(
                1,
                exception.Context["State_NullErrorCount"]
            );

            Assert.Equal(
                "Result",
                exception.Context["Type"]
            );
        }
        #endregion

        #region Value: failed result
        [Fact]
        public void Value_FailedResultWithValueType_ThrowsDomainInvalidOperationException()
        {
            var error = CreateError(
                "ERR.ONE",
                "One error",
                ErrorType.Validation
            );

            var fromSingle = Result<int>.Failure(error);
            var fromCollection = Result<int>.Failure(new[] { error });

            AssertValueAccessOnFailure(fromSingle);
            AssertValueAccessOnFailure(fromCollection);
        }

        [Fact]
        public void Value_FailedResultWithReferenceType_ThrowsDomainInvalidOperationException()
        {
            var error = CreateError(
                "ERR.ONE",
                "One error",
                ErrorType.Validation
            );

            var fromSingle = Result<string>.Failure(error);
            var fromCollection = Result<string>.Failure(new[] { error });

            AssertValueAccessOnFailure(fromSingle);
            AssertValueAccessOnFailure(fromCollection);
        }

        [Fact]
        public void Value_FailedResultWithNullableValueType_ThrowsDomainInvalidOperationException()
        {
            var error = CreateError(
                "ERR.ONE",
                "One error",
                ErrorType.Validation
            );

            var fromSingle = Result<int?>.Failure(error);
            var fromCollection = Result<int?>.Failure(new[] { error });

            AssertValueAccessOnFailure(fromSingle);
            AssertValueAccessOnFailure(fromCollection);
        }

        [Fact]
        public void Value_FailedResult_RepeatedAccessDoesNotChangeState()
        {
            var firstError = CreateError(
                "ERR.FIRST",
                "First error",
                ErrorType.Validation
            );
            var secondError = CreateError(
                "ERR.SECOND",
                "Second error",
                ErrorType.BusinessRule
            );
            var result = Result<int>.Failure(
                new[] { firstError, secondError }
            );

            AssertValueAccessOnFailure(result);
            AssertValueAccessOnFailure(result);

            Assert.True(result.IsFailure);

            Assert.Equal(
                2,
                result.Errors.Count
            );

            Assert.Same(
                firstError,
                result.Errors.ElementAt(0)
            );

            Assert.Same(
                secondError,
                result.Errors.ElementAt(1)
            );
        }
        #endregion

        #region Helpers: value access diagnostics
        // Спільна перевірка контракту помилки читання Value для різних типів T.
        // Shared verification of the Value access error contract across different T types.
        private static void AssertValueAccessOnFailure<T>(Result<T> result)
        {
            var exception = Assert.Throws<DomainInvalidOperationException>(() =>
                _ = result.Value
            );

            Assert.Equal(
                "DOMAIN.INVALID_OPERATION.VALUE_ACCESS_ON_FAILURE",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.InvalidOperation,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "Result`1",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Read",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "Value",
                exception.Context["ValueName"]
            );
        }
        #endregion
    }
}
