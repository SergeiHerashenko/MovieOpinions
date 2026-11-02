using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Common.Validation.Interfaces;

namespace Authorization.Domain.UnitTests.Common.Validation
{
    public class DomainValidationExecutorTests
    {
        private sealed class TestValidationData : IValidationData { }

        private sealed class TestOperationData : IHasOperationType
        {
            public OperationType OperationType { get; }

            public TestOperationData(OperationType operationType)
            {
                OperationType = operationType;
            }
        }

        private sealed class TestRule<TData, TFailure>
            : IValidationRule<TData, TFailure>
            where TData : class, IValidationData
            where TFailure : class, IValidationFailure
        {
            private readonly Func<TData, TFailure?> _validate;

            public ValidationPriority Priority
                => ValidationPriority.Presence;

            public int CallCount { get; private set; }

            public TData? LastData { get; private set; }

            public TestRule(Func<TData, TFailure?> validate)
            {
                _validate = validate;
            }

            public TFailure? Validate(TData data)
            {
                CallCount++;
                LastData = data;

                return _validate(data);
            }
        }

        [Fact]
        public void ValidateForError_AllRulesPass_ReturnsNull()
        {
            var data = new TestValidationData();

            var rule = new TestRule<TestValidationData, ErrorValidationFailure>(
                _ => null
            );

            var validator = new ValidationOrchestrator<TestValidationData, ErrorValidationFailure>(
                [rule]
            );

            var result = DomainValidationExecutor.ValidateForError(
                validator,
                data
            );

            Assert.Null(result);

            Assert.Equal(
                1,
                rule.CallCount
            );
        }

        [Fact]
        public void ValidateForError_RuleFails_ReturnsSameError()
        {
            var data = new TestValidationData();

            var expectedError = new Error(
                "TEST.VALIDATION",
                "Test validation failure",
                ErrorType.Validation
            );

            var failure = new ErrorValidationFailure
            {
                Error = expectedError
            };

            var rule = new TestRule<TestValidationData, ErrorValidationFailure>(
                _ => failure
            );

            var validator = new ValidationOrchestrator<TestValidationData, ErrorValidationFailure>(
                [rule]
            );

            var result = DomainValidationExecutor.ValidateForError(
                validator,
                data
            );

            var actualFailure = Assert.IsType<ValidationRulesFailure<Error>>(result);

            Assert.Same(
                expectedError,
                actualFailure.Value
            );

            Assert.Equal(
                1,
                rule.CallCount
            );
        }

        [Fact]
        public void ValidateForError_PassesOriginalDataToRuleOnce()
        {
            var data = new TestValidationData();

            var rule = new TestRule<TestValidationData, ErrorValidationFailure>(
                _ => null
            );

            var validator =
                new ValidationOrchestrator<TestValidationData, ErrorValidationFailure>(
                    [rule]
                );

            var result = DomainValidationExecutor.ValidateForError(
                validator,
                data
            );

            Assert.Null(result);

            Assert.Same(
                data,
                rule.LastData
            );

            Assert.Equal(
                1,
                rule.CallCount
            );
        }

        [Fact]
        public void ValidateForError_RuleThrows_PropagatesSameException()
        {
            var data = new TestValidationData();

            var expectedException = new InvalidOperationException("Rule execution failed.");

            var rule = new TestRule<TestValidationData, ErrorValidationFailure>(
                _ => throw expectedException
            );

            var validator = new ValidationOrchestrator<TestValidationData, ErrorValidationFailure>(
                [rule]
            );

            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                DomainValidationExecutor.ValidateForError(
                    validator,
                    data
                );
            });

            Assert.Same(
                expectedException,
                exception
            );

            Assert.Equal(
                1,
                rule.CallCount
            );
        }

        [Fact]
        public void ValidateForError_FailureSupportsException_DoesNotBuildException()
        {
            var data = new TestValidationData();

            var expectedError = new Error(
                "TEST.VALIDATION",
                "Test validation failure",
                ErrorType.Validation
            );

            var buildCalls = 0;

            var failure = new ValidationFailure
            {
                Error = expectedError,

                BuildException = _ =>
                {
                    buildCalls++;

                    return new InvalidOperationException(
                        "Exception factory was called."
                    );
                }
            };

            var rule = new TestRule<TestValidationData, ValidationFailure>(
                _ => failure
            );

            var validator = new ValidationOrchestrator<TestValidationData, ValidationFailure>(
                [rule]
            );

            var result = DomainValidationExecutor.ValidateForError(
                validator,
                data
            );

            Assert.NotNull(result);

            Assert.Same(
                expectedError,
                result.Value
            );

            Assert.Equal(
                0,
                buildCalls
            );

            Assert.Equal(
                1,
                rule.CallCount
            );
        }

        [Fact]
        public void ValidateForException_AllRulesPass_ReturnsNull()
        {
            var data = new TestOperationData(OperationType.Restore);

            var rule = new TestRule<TestOperationData, ValidationFailure>(
                _ => null
            );

            var validator = new ValidationOrchestrator<TestOperationData, ValidationFailure>(
                [rule]
            );

            var result = DomainValidationExecutor.ValidateForException(
                validator,
                data
            );

            Assert.Null(result);

            Assert.Equal(
                1,
                rule.CallCount
            );
        }

        [Fact]
        public void ValidateForException_RuleFails_ReturnsSameBuiltException()
        {
            var data = new TestOperationData(OperationType.Restore);

            var error = new Error(
                "TEST.VALIDATION",
                "Test validation failure",
                ErrorType.Validation
            );

            var expectedException = new InvalidOperationException("Built validation exception.");

            var buildCalls = 0;

            var failure = new ValidationFailure
            {
                Error = error,

                BuildException = _ =>
                {
                    buildCalls++;
                    return expectedException;
                }
            };

            var rule = new TestRule<TestOperationData, ValidationFailure>(
                _ => failure
            );

            var validator = new ValidationOrchestrator<TestOperationData, ValidationFailure>(
                [rule]
            );

            var result = DomainValidationExecutor.ValidateForException(
                validator,
                data
            );

            Assert.NotNull(result);

            Assert.Same(
                expectedException,
                result.Value
            );

            Assert.Equal(
                1,
                buildCalls
            );

            Assert.Equal(
                1,
                rule.CallCount
            );
        }

        [Theory]
        [InlineData(OperationType.Restore)]
        [InlineData(OperationType.Create)]
        [InlineData(OperationType.Update)]
        [InlineData(OperationType.Delete)]
        [InlineData(OperationType.Read)]
        [InlineData(OperationType.Compare)]
        public void ValidateForException_ExceptionFactory_ReceivesOperationType(
    OperationType operationType)
        {
            var data = new TestOperationData(operationType);

            var error = new Error(
                "TEST.VALIDATION",
                "Test validation failure",
                ErrorType.Validation
            );

            var expectedException = new InvalidOperationException("Built validation exception.");

            var receivedOperations = new List<OperationType>();

            var failure = new ValidationFailure
            {
                Error = error,

                BuildException = operation =>
                {
                    receivedOperations.Add(operation);
                    return expectedException;
                }
            };

            var rule = new TestRule<TestOperationData, ValidationFailure>(
                _ => failure
            );

            var validator = new ValidationOrchestrator<TestOperationData, ValidationFailure>(
                [rule]
            );

            var result = DomainValidationExecutor.ValidateForException(
                validator,
                data
            );

            Assert.NotNull(result);

            Assert.Same(
                expectedException,
                result.Value
            );

            Assert.Single(receivedOperations);

            Assert.Equal(
                operationType,
                receivedOperations[0]
            );

            Assert.Equal(
                1,
                rule.CallCount
            );
        }

        [Fact]
        public void ValidateForException_PassesOriginalDataToRuleOnce()
        {
            var data = new TestOperationData(OperationType.Restore);

            var rule = new TestRule<TestOperationData, ValidationFailure>(
                _ => null
            );

            var validator = new ValidationOrchestrator<TestOperationData, ValidationFailure>(
                [rule]
            );

            var result = DomainValidationExecutor.ValidateForException(
                validator,
                data
            );

            Assert.Null(result);

            Assert.Same(
                data,
                rule.LastData
            );

            Assert.Equal(
                1,
                rule.CallCount
            );
        }

        [Fact]
        public void ValidateForException_RuleThrows_PropagatesSameException()
        {
            var data = new TestOperationData(OperationType.Restore);

            var expectedException = new InvalidOperationException("Rule execution failed.");

            var rule = new TestRule<TestOperationData, ValidationFailure>(
                _ => throw expectedException
            );

            var validator = new ValidationOrchestrator<TestOperationData, ValidationFailure>(
                [rule]
            );

            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                DomainValidationExecutor.ValidateForException(
                    validator,
                    data
                );
            });

            Assert.Same(
                expectedException,
                exception
            );

            Assert.Equal(
                1,
                rule.CallCount
            );
        }

        [Fact]
        public void ValidateForException_ExceptionFactoryThrows_PropagatesSameException()
        {
            var data = new TestOperationData(OperationType.Restore);

            var error = new Error(
                "TEST.VALIDATION",
                "Test validation failure",
                ErrorType.Validation
            );

            var expectedException = new InvalidOperationException("Exception factory failed.");

            var buildCalls = 0;

            var failure = new ValidationFailure
            {
                Error = error,

                BuildException = _ =>
                {
                    buildCalls++;
                    throw expectedException;
                }
            };

            var rule = new TestRule<TestOperationData, ValidationFailure>(
                _ => failure
            );

            var validator = new ValidationOrchestrator<TestOperationData, ValidationFailure>(
                [rule]
            );

            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                DomainValidationExecutor.ValidateForException(
                    validator,
                    data
                );
            });

            Assert.Same(
                expectedException,
                exception
            );

            Assert.Equal(
                1,
                buildCalls
            );

            Assert.Equal(
                1,
                rule.CallCount
            );
        }

        [Fact]
        public void ValidateForException_RepeatedCalls_UseCurrentOperationType()
        {
            var error = new Error(
                "TEST.VALIDATION",
                "Test validation failure",
                ErrorType.Validation
            );

            var receivedOperations = new List<OperationType>();

            var failure = new ValidationFailure
            {
                Error = error,

                BuildException = operation =>
                {
                    receivedOperations.Add(operation);

                    return new InvalidOperationException(
                        operation.ToString()
                    );
                }
            };

            var rule = new TestRule<TestOperationData, ValidationFailure>(
                _ => failure
            );

            var validator = new ValidationOrchestrator<TestOperationData, ValidationFailure>(
                [rule]
            );

            var firstData = new TestOperationData(OperationType.Create);
            var secondData = new TestOperationData(OperationType.Restore);

            var firstResult = DomainValidationExecutor.ValidateForException(
                validator,
                firstData
            );

            var secondResult = DomainValidationExecutor.ValidateForException(
                validator,
                secondData
            );

            Assert.NotNull(firstResult);
            Assert.NotNull(secondResult);

            Assert.Equal(
                "Create",
                firstResult.Value.Message
            );

            Assert.Equal(
                "Restore",
                secondResult.Value.Message
            );

            Assert.Equal(
                new[]
                {
                    OperationType.Create,
                    OperationType.Restore
                },
                receivedOperations
            );

            Assert.Equal(
                2,
                rule.CallCount
            );

            Assert.Same(
                secondData,
                rule.LastData
            );
        }
    }
}
