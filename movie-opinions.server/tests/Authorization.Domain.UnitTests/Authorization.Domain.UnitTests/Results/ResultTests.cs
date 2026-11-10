using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Results;

namespace Authorization.Domain.UnitTests.Results
{
    public class ResultTests
    {
        #region Helpers
        // Конкретний підтип відкриває захищений конструктор Result для перевірок.
        // A concrete subtype exposes the protected Result constructor for testing.
        private sealed class TestResult : Result
        {
            public TestResult(
                bool isSuccess,
                IEnumerable<Error> errors)
                : base(isSuccess, errors)
            {
            }
        }

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

        #region Constructor
        [Fact]
        public void Constructor_SuccessWithEmptyErrors_CreatesSuccessfulResult()
        {
            var result = new TestResult(
                true,
                Array.Empty<Error>()
            );

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Errors);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Constructor_FailureWithErrors_CreatesFailedResult()
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

            var result = new TestResult(
                false,
                new[] { firstError, secondError }
            );

            Assert.False(result.IsSuccess);
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Constructor_NullErrors_ThrowsDomainDataInconsistencyException(bool isSuccess)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                new TestResult(isSuccess, null!)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.EMPTY_VALUE",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.DataInconsistency,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "Result",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Create",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "errors",
                exception.Context["Field"]
            );
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Constructor_NullErrorEntries_ThrowsDomainInvariantViolationException(bool isSuccess)
        {
            var firstError = CreateError(
                "ERR.FIRST",
                "First error",
                ErrorType.Validation
            );

            var scenarios = new IEnumerable<Error>[]
            {
                new Error?[] { null! }!,
                new Error?[] { null!, firstError }!,
                new Error?[] { firstError, null! }!,
                new Error?[] { firstError, null!, firstError }!,
                new Error?[] { null!, null! }!
            };

            foreach (var errors in scenarios)
            {
                var errorList = errors.ToList();
                var nullCount = errorList.Count(e => e is null);

                var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                    new TestResult(isSuccess, errors)
                );

                Assert.Equal(
                    "DOMAIN.INVARIANT_VIOLATION.INVALID_STATE",
                    exception.ExceptionCode
                );

                Assert.Equal(
                    ExceptionType.InvariantViolation,
                    exception.ExceptionType
                );

                Assert.Equal(
                    "Domain",
                    exception.Context["Layer"]
                );

                Assert.Equal(
                    "Result",
                    exception.Context["Type"]
                );

                Assert.Equal(
                    "Create",
                    exception.Context["Operation"]
                );

                Assert.Equal(
                    "A result's error collection must not contain null entries.",
                    exception.Context["Rule"]
                );

                Assert.Equal(
                    isSuccess,
                    exception.Context["State_IsSuccess"]
                );

                Assert.Equal(
                    errorList.Count,
                    exception.Context["State_ErrorCount"]
                );

                Assert.Equal(
                    nullCount,
                    exception.Context["State_NullErrorCount"]
                );

                Assert.Equal(
                    "Every error entry must be a non-null Error instance.",
                    exception.Context["State_Expected"]
                );

                Assert.Equal(
                    "The error collection contains null entries.",
                    exception.Context["State_Violation"]
                );
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void Constructor_SuccessWithErrors_ThrowsDomainInvariantViolationException(int errorCount)
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

            var errors = new[] { firstError, secondError }
                .Take(errorCount)
                .ToArray();

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                new TestResult(true, errors)
            );

            Assert.Equal(
                "DOMAIN.INVARIANT_VIOLATION.INVALID_STATE",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.InvariantViolation,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "Result",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Create",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "A successful result cannot contain errors.",
                exception.Context["Rule"]
            );

            Assert.Equal(
                true,
                exception.Context["State_IsSuccess"]
            );

            Assert.Equal(
                "Success => Errors.Count == 0",
                exception.Context["State_Expected"]
            );

            Assert.Equal(
                $"isSuccess=True, errors.Count={errorCount}",
                exception.Context["State_Actual"]
            );

            Assert.Equal(
                "A successful result contains unexpected errors.",
                exception.Context["State_Violation"]
            );

            var actualErrors = Assert
                .IsAssignableFrom<IEnumerable<object>>(
                    exception.Context["State_Errors"]
                )
                .ToArray();

            Assert.Equal(
                errorCount,
                actualErrors.Length
            );

            for (var index = 0; index < errors.Length; index++)
            {
                Assert.Equivalent(
                    new
                    {
                        Code = errors[index].Code,
                        Message = errors[index].Message,
                        Type = errors[index].ErrorType.ToString()
                    },
                    actualErrors[index],
                    strict: true
                );
            }
        }

        [Fact]
        public void Constructor_FailureWithEmptyErrors_ThrowsDomainInvariantViolationException()
        {
            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                new TestResult(
                    false,
                    Array.Empty<Error>()
                )
            );

            Assert.Equal(
                "DOMAIN.INVARIANT_VIOLATION.INVALID_STATE",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.InvariantViolation,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "Result",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Create",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "A failed result must contain at least one error.",
                exception.Context["Rule"]
            );

            Assert.Equal(
                false,
                exception.Context["State_IsSuccess"]
            );

            Assert.Equal(
                "Failure => Errors.Count >= 1",
                exception.Context["State_Expected"]
            );

            Assert.Equal(
                "isSuccess=False, errors.Count=0",
                exception.Context["State_Actual"]
            );

            Assert.Equal(
                "A failed result was created without errors",
                exception.Context["State_Violation"]
            );
        }

        [Fact]
        public void Constructor_SuccessSourceChanges_DoesNotChangeResult()
        {
            var source = new List<Error>();

            var result = new TestResult(true, source);

            source.Add(
                CreateError(
                    "ERR.ADDED",
                    "Added later",
                    ErrorType.Validation
                )
            );

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Errors);
        }
        #endregion

        #region Success
        [Fact]
        public void Success_ReturnsSuccessfulResultWithoutErrors()
        {
            var result = Result.Success();

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Errors);
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

            var result = Result.Failure(error);

            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);

            var actual = Assert.Single(result.Errors);

            Assert.Same(
                error,
                actual
            );
        }

        [Fact]
        public void Failure_NullError_ThrowsDomainInvariantViolationException()
        {
            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                Result.Failure((Error)null!)
            );

            Assert.Equal(
                "DOMAIN.INVARIANT_VIOLATION.INVALID_STATE",
                exception.ExceptionCode
            );

            Assert.Equal(
                "A result's error collection must not contain null entries.",
                exception.Context["Rule"]
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

            var single = Result.Failure(new[] { firstError });
            var multiple = Result.Failure(new[] { firstError, secondError });

            Assert.True(single.IsFailure);

            Assert.Same(
                firstError,
                Assert.Single(single.Errors)
            );

            Assert.True(multiple.IsFailure);

            Assert.Equal(
                2,
                multiple.Errors.Count
            );

            Assert.Same(
                firstError,
                multiple.Errors.ElementAt(0)
            );

            Assert.Same(
                secondError,
                multiple.Errors.ElementAt(1)
            );
        }

        [Fact]
        public void Failure_EmptyErrorCollection_ThrowsDomainInvariantViolationException()
        {
            var fromArray = Assert.Throws<DomainInvariantViolationException>(() =>
                Result.Failure(Array.Empty<Error>())
            );
            var fromList = Assert.Throws<DomainInvariantViolationException>(() =>
                Result.Failure(new List<Error>())
            );

            Assert.Equal(
                "A failed result must contain at least one error.",
                fromArray.Context["Rule"]
            );

            Assert.Equal(
                "A failed result must contain at least one error.",
                fromList.Context["Rule"]
            );
        }

        [Fact]
        public void Failure_NullErrorCollection_ThrowsDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                Result.Failure((IEnumerable<Error>)null!)
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
        public void Failure_ErrorCollectionContainsNull_ThrowsDomainInvariantViolationException()
        {
            var firstError = CreateError(
                "ERR.FIRST",
                "First error",
                ErrorType.Validation
            );

            var onlyNull = Assert.Throws<DomainInvariantViolationException>(() =>
                Result.Failure(new Error?[] { null! }!)
            );
            var mixed = Assert.Throws<DomainInvariantViolationException>(() =>
                Result.Failure(new Error?[] { firstError, null! }!)
            );

            Assert.Equal(
                1,
                onlyNull.Context["State_ErrorCount"]
            );

            Assert.Equal(
                1,
                onlyNull.Context["State_NullErrorCount"]
            );

            Assert.Equal(
                2,
                mixed.Context["State_ErrorCount"]
            );

            Assert.Equal(
                1,
                mixed.Context["State_NullErrorCount"]
            );
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Failure_DuplicateErrors_PreservesDuplicates(bool useGeneric)
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
            var errors = new[] { firstError, secondError, firstError };

            Result result = useGeneric
                ? Result<int>.Failure(errors)
                : Result.Failure(errors);

            Assert.Equal(
                3,
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

            Assert.Same(
                firstError,
                result.Errors.ElementAt(2)
            );
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Failure_SourceCollectionChanges_DoesNotChangeErrors(bool useGeneric)
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
            var thirdError = CreateError(
                "ERR.THIRD",
                "Third error",
                ErrorType.Validation
            );
            var source = new List<Error> { firstError, secondError };

            Result result = useGeneric
                ? Result<int>.Failure(source)
                : Result.Failure(source);

            source[0] = thirdError;
            source.Add(thirdError);
            source.Clear();

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

        #region Errors
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Errors_CollectionIsReadOnly_RejectsMutation(bool useGeneric)
        {
            var error = CreateError(
                "ERR.ONE",
                "One error",
                ErrorType.Validation
            );

            Result success = useGeneric
                ? Result<int>.Success(1)
                : Result.Success();

            Result failure = useGeneric
                ? Result<int>.Failure(error)
                : Result.Failure(error);

            Assert.Throws<NotSupportedException>(() =>
                ((ICollection<Error>)success.Errors).Add(error)
            );

            Assert.Throws<NotSupportedException>(() =>
                ((ICollection<Error>)failure.Errors).Add(error)
            );

            Assert.True(success.IsSuccess);
            Assert.Empty(success.Errors);
            Assert.True(failure.IsFailure);

            Assert.Same(
                error,
                Assert.Single(failure.Errors)
            );
        }
        #endregion

        #region Failure: enumeration
        // Лічильник виявляє повторне перелічення під час створення чи читання Errors.
        // The counter detects repeated enumeration during construction or Errors access.
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Failure_LazyErrorSequence_IsEnumeratedOnce(bool useGeneric)
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
            var enumerationStarts = 0;

            IEnumerable<Error> Sequence()
            {
                enumerationStarts++;
                yield return firstError;
                yield return secondError;
            }

            Result result = useGeneric
                ? Result<int>.Failure(Sequence())
                : Result.Failure(Sequence());

            Assert.Equal(
                1,
                enumerationStarts
            );

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

            Assert.Equal(
                1,
                enumerationStarts
            );
        }

        // Виняток виникає під час перелічення, уже після першого елемента.
        // The exception occurs during enumeration, after the first item has been yielded.
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Failure_ErrorSequenceThrows_PropagatesSameException(bool useGeneric)
        {
            var firstError = CreateError(
                "ERR.FIRST",
                "First error",
                ErrorType.Validation
            );
            var expected = new InvalidOperationException(
                "Sequence failure."
            );

            IEnumerable<Error> Sequence()
            {
                yield return firstError;
                throw expected;
            }

            var thrown = Assert.Throws<InvalidOperationException>(() =>
            {
                if (useGeneric)
                    Result<int>.Failure(Sequence());
                else
                    Result.Failure(Sequence());
            });

            Assert.Same(
                expected,
                thrown
            );
        }
        #endregion
    }
}
