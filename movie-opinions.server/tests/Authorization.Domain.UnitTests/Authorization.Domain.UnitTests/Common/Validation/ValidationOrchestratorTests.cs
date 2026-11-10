using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Common.Validation.Interfaces;

namespace Authorization.Domain.UnitTests.Common.Validation
{
    public class ValidationOrchestratorTests
    {
        #region Helpers
        private sealed class TestValidationData : IValidationData { }

        // Кероване правило для спостереження за викликами валідатора.
        // Controllable rule for observing validator calls.
        private sealed class TestRule
            : IValidationRule<TestValidationData, ErrorValidationFailure>
        {
            private readonly Func<
                TestValidationData,
                ErrorValidationFailure?> _validate;

            public ValidationPriority Priority { get; }

            public int CallCount { get; private set; }

            public TestValidationData? LastData { get; private set; }

            public TestRule(
                ValidationPriority priority,
                Func<TestValidationData, ErrorValidationFailure?> validate)
            {
                Priority = priority;
                _validate = validate;
            }

            public ErrorValidationFailure? Validate(
                TestValidationData data)
            {
                CallCount++;
                LastData = data;

                return _validate(data);
            }
        }
        #endregion

        #region Constructor
        [Fact]
        public void Constructor_NullRules_ThrowsDomainInvalidOperationException()
        {
            var exception = Assert.Throws<DomainInvalidOperationException>(() =>
                new ValidationOrchestrator<TestValidationData, ErrorValidationFailure>(
                    null!
                )
            );

            Assert.Equal(
                "DOMAIN.INVALID_OPERATION.PRECONDITION_FAILED",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.InvalidOperation,
                exception.ExceptionType
            );

            Assert.Equal(
                "Create",
                exception.Context["Operation"]
            );

            Assert.Equal(
                true,
                exception.Context["Custom_RulesIsNull"]
            );

            Assert.Equal(
                0,
                exception.Context["Custom_RuleCount"]
            );
        }

        [Fact]
        public void Constructor_EmptyRules_ThrowsDomainInvalidOperationException()
        {
            var exception = Assert.Throws<DomainInvalidOperationException>(() =>
                new ValidationOrchestrator<TestValidationData, ErrorValidationFailure>(
                    Array.Empty<IValidationRule<TestValidationData, ErrorValidationFailure>>()
                )
            );

            Assert.Equal(
                "DOMAIN.INVALID_OPERATION.PRECONDITION_FAILED",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.InvalidOperation,
                exception.ExceptionType
            );

            Assert.Equal(
                "Create",
                exception.Context["Operation"]
            );

            Assert.Equal(
                false,
                exception.Context["Custom_RulesIsNull"]
            );

            Assert.Equal(
                0,
                exception.Context["Custom_RuleCount"]
            );
        }

        [Fact]
        public void Constructor_ValidRules_DoesNotExecuteRules()
        {
            var first = new TestRule(
                ValidationPriority.Presence,
                _ => null
            );
            var second = new TestRule(
                ValidationPriority.Format,
                _ => null
            );

            _ = new ValidationOrchestrator<TestValidationData, ErrorValidationFailure>(
                [first, second]
            );

            Assert.Equal(
                0,
                first.CallCount
            );

            Assert.Equal(
                0,
                second.CallCount
            );
        }

        // Зміна початкового списку не повинна змінювати збережений склад.
        // Changing the input list must not alter the stored collection contents.
        [Fact]
        public void Constructor_InputCollectionChanges_DoNotChangeConfiguredRules()
        {
            var first = new TestRule(
                ValidationPriority.Presence,
                _ => null
            );
            var second = new TestRule(
                ValidationPriority.Format,
                _ => null
            );
            var third = new TestRule(
                ValidationPriority.Length,
                _ => null
            );

            var inputRules = new List<IValidationRule<TestValidationData, ErrorValidationFailure>>
            {
                first,
                second
            };

            var orchestrator = new ValidationOrchestrator<TestValidationData, ErrorValidationFailure>(
                inputRules
            );

            inputRules.Clear();
            inputRules.Add(third);

            var result = orchestrator.Validate(new TestValidationData());

            Assert.Null(result);

            Assert.Equal(
                1,
                first.CallCount
            );

            Assert.Equal(
                1,
                second.CallCount
            );

            Assert.Equal(
                0,
                third.CallCount
            );
        }
        #endregion

        #region Validate
        [Fact]
        public void Validate_AllRulesPass_ReturnsNullAndExecutesEachRuleOnce()
        {
            var first = new TestRule(
                ValidationPriority.Presence,
                _ => null
            );
            var second = new TestRule(
                ValidationPriority.Format,
                _ => null
            );
            var third = new TestRule(
                ValidationPriority.Length,
                _ => null
            );

            var orchestrator = new ValidationOrchestrator<TestValidationData, ErrorValidationFailure>(
                [first, second, third]
            );

            var result = orchestrator.Validate(new TestValidationData());

            Assert.Null(result);

            Assert.Equal(
                1,
                first.CallCount
            );

            Assert.Equal(
                1,
                second.CallCount
            );

            Assert.Equal(
                1,
                third.CallCount
            );
        }

        [Fact]
        public void Validate_ExecutesRulesInAscendingPriority()
        {
            var executionOrder = new List<ValidationPriority>();

            var lengthRule = new TestRule(
                ValidationPriority.Length,
                _ =>
                {
                    executionOrder.Add(ValidationPriority.Length);
                    return null;
                }
            );
            var formatRule = new TestRule(
                ValidationPriority.Format,
                _ =>
                {
                    executionOrder.Add(ValidationPriority.Format);
                    return null;
                }
            );
            var presenceRule = new TestRule(
                ValidationPriority.Presence,
                _ =>
                {
                    executionOrder.Add(ValidationPriority.Presence);
                    return null;
                }
            );
            var businessRule = new TestRule(
                ValidationPriority.BusinessRule,
                _ =>
                {
                    executionOrder.Add(ValidationPriority.BusinessRule);
                    return null;
                }
            );

            var orchestrator = new ValidationOrchestrator<TestValidationData, ErrorValidationFailure>(
                [lengthRule, formatRule, presenceRule, businessRule]
            );

            var result = orchestrator.Validate(new TestValidationData());

            Assert.Null(result);

            Assert.Equal(
                new[]
                {
                    ValidationPriority.Presence,
                    ValidationPriority.BusinessRule,
                    ValidationPriority.Format,
                    ValidationPriority.Length
                },
                executionOrder
            );
        }

        // За однакового пріоритету важливий початковий порядок правил.
        // The original rule order matters when priorities are equal.
        [Fact]
        public void Validate_EqualPriorities_PreserveInputOrder()
        {
            var executionOrder = new List<string>();

            var first = new TestRule(
                ValidationPriority.Format,
                _ =>
                {
                    executionOrder.Add("first");
                    return null;
                }
            );
            var second = new TestRule(
                ValidationPriority.Format,
                _ =>
                {
                    executionOrder.Add("second");
                    return null;
                }
            );
            var third = new TestRule(
                ValidationPriority.Format,
                _ =>
                {
                    executionOrder.Add("third");
                    return null;
                }
            );

            var orchestrator = new ValidationOrchestrator<TestValidationData, ErrorValidationFailure>(
                [second, first, third]
            );

            var result = orchestrator.Validate(new TestValidationData());

            Assert.Null(result);

            Assert.Equal(
                new[] { "second", "first", "third" },
                executionOrder
            );
        }

        [Fact]
        public void Validate_FirstRuleFails_ReturnsSameFailureAndSkipsRemainingRules()
        {
            var expectedFailure = new ErrorValidationFailure
            {
                Error = new Error(
                    "TEST.VALIDATION",
                    "Test validation failure",
                    ErrorType.Validation
                )
            };

            var first = new TestRule(
                ValidationPriority.Presence,
                _ => expectedFailure
            );
            var second = new TestRule(
                ValidationPriority.Format,
                _ => null
            );
            var third = new TestRule(
                ValidationPriority.Length,
                _ => null
            );

            var orchestrator = new ValidationOrchestrator<TestValidationData, ErrorValidationFailure>(
                [first, second, third]
            );

            var result = orchestrator.Validate(new TestValidationData());

            Assert.Same(
                expectedFailure,
                result
            );

            Assert.Equal(
                1,
                first.CallCount
            );

            Assert.Equal(
                0,
                second.CallCount
            );

            Assert.Equal(
                0,
                third.CallCount
            );
        }

        [Fact]
        public void Validate_LaterRuleFails_ReturnsSameFailureAndSkipsFollowingRules()
        {
            var expectedFailure = new ErrorValidationFailure
            {
                Error = new Error(
                    "TEST.VALIDATION",
                    "Test validation failure",
                    ErrorType.Validation
                )
            };

            var first = new TestRule(
                ValidationPriority.Presence,
                _ => null
            );
            var second = new TestRule(
                ValidationPriority.Format,
                _ => expectedFailure
            );
            var third = new TestRule(
                ValidationPriority.Length,
                _ => null
            );

            var orchestrator = new ValidationOrchestrator<TestValidationData, ErrorValidationFailure>(
                [first, second, third]
            );

            var result = orchestrator.Validate(new TestValidationData());

            Assert.Same(
                expectedFailure,
                result
            );

            Assert.Equal(
                1,
                first.CallCount
            );

            Assert.Equal(
                1,
                second.CallCount
            );

            Assert.Equal(
                0,
                third.CallCount
            );
        }

        [Fact]
        public void Validate_LastRuleFails_ReturnsSameFailure()
        {
            var expectedFailure = new ErrorValidationFailure
            {
                Error = new Error(
                    "TEST.VALIDATION",
                    "Test validation failure",
                    ErrorType.Validation
                )
            };

            var first = new TestRule(
                ValidationPriority.Presence,
                _ => null
            );
            var second = new TestRule(
                ValidationPriority.Format,
                _ => null
            );
            var third = new TestRule(
                ValidationPriority.Length,
                _ => expectedFailure
            );

            var orchestrator = new ValidationOrchestrator<TestValidationData, ErrorValidationFailure>(
                [first, second, third]
            );

            var result = orchestrator.Validate(new TestValidationData());

            Assert.Same(
                expectedFailure,
                result
            );

            Assert.Equal(
                1,
                first.CallCount
            );

            Assert.Equal(
                1,
                second.CallCount
            );

            Assert.Equal(
                1,
                third.CallCount
            );
        }

        // Першу помилку визначає пріоритет виконання правил.
        // Rule execution priority determines the first failure.
        [Fact]
        public void Validate_MultipleFailingRules_ReturnsFirstByPriority()
        {
            var presenceFailure = new ErrorValidationFailure
            {
                Error = new Error(
                    "TEST.PRESENCE",
                    "Presence failure",
                    ErrorType.Validation
                )
            };
            var formatFailure = new ErrorValidationFailure
            {
                Error = new Error(
                    "TEST.FORMAT",
                    "Format failure",
                    ErrorType.Validation
                )
            };

            var formatRule = new TestRule(
                ValidationPriority.Format,
                _ => formatFailure
            );
            var presenceRule = new TestRule(
                ValidationPriority.Presence,
                _ => presenceFailure
            );

            var orchestrator = new ValidationOrchestrator<TestValidationData, ErrorValidationFailure>(
                [formatRule, presenceRule]
            );

            var result = orchestrator.Validate(new TestValidationData());

            Assert.Same(
                presenceFailure,
                result
            );

            Assert.Equal(
                1,
                presenceRule.CallCount
            );

            Assert.Equal(
                0,
                formatRule.CallCount
            );
        }

        [Fact]
        public void Validate_PassesOriginalDataToEachExecutedRule()
        {
            var data = new TestValidationData();

            var first = new TestRule(
                ValidationPriority.Presence,
                _ => null
            );
            var second = new TestRule(
                ValidationPriority.Format,
                _ => null
            );
            var third = new TestRule(
                ValidationPriority.Length,
                _ => null
            );

            var orchestrator = new ValidationOrchestrator<TestValidationData, ErrorValidationFailure>(
                [first, second, third]
            );

            var result = orchestrator.Validate(data);

            Assert.Null(result);

            Assert.Same(
                data,
                first.LastData
            );

            Assert.Same(
                data,
                second.LastData
            );

            Assert.Same(
                data,
                third.LastData
            );
        }

        [Fact]
        public void Validate_RuleThrows_PropagatesSameExceptionAndSkipsFollowingRules()
        {
            var expectedException = new InvalidOperationException(
                "Rule execution failed."
            );

            var first = new TestRule(
                ValidationPriority.Presence,
                _ => null
            );
            var second = new TestRule(
                ValidationPriority.Format,
                _ => throw expectedException
            );
            var third = new TestRule(
                ValidationPriority.Length,
                _ => null
            );

            var orchestrator = new ValidationOrchestrator<TestValidationData, ErrorValidationFailure>(
                [first, second, third]
            );

            var exception = Assert.Throws<InvalidOperationException>(() =>
                orchestrator.Validate(new TestValidationData())
            );

            Assert.Same(
                expectedException,
                exception
            );

            Assert.Equal(
                1,
                first.CallCount
            );

            Assert.Equal(
                1,
                second.CallCount
            );

            Assert.Equal(
                0,
                third.CallCount
            );
        }

        [Fact]
        public void Validate_RepeatedCalls_StartFromFirstRuleAgain()
        {
            var firstData = new TestValidationData();
            var secondData = new TestValidationData();

            var first = new TestRule(
                ValidationPriority.Presence,
                _ => null
            );
            var second = new TestRule(
                ValidationPriority.Format,
                _ => null
            );

            var orchestrator = new ValidationOrchestrator<TestValidationData, ErrorValidationFailure>(
                [first, second]
            );

            var firstResult = orchestrator.Validate(firstData);
            var secondResult = orchestrator.Validate(secondData);

            Assert.Null(firstResult);
            Assert.Null(secondResult);

            Assert.Equal(
                2,
                first.CallCount
            );

            Assert.Equal(
                2,
                second.CallCount
            );

            Assert.Same(
                secondData,
                first.LastData
            );

            Assert.Same(
                secondData,
                second.LastData
            );
        }

        [Fact]
        public void Validate_PreviousFailure_DoesNotPreventNextValidation()
        {
            var expectedFailure = new ErrorValidationFailure
            {
                Error = new Error(
                    "TEST.VALIDATION",
                    "Test validation failure",
                    ErrorType.Validation
                )
            };

            var shouldFail = true;

            var firstRule = new TestRule(
                ValidationPriority.Presence,
                _ => shouldFail ? expectedFailure : null
            );
            var secondRule = new TestRule(
                ValidationPriority.Format,
                _ => null
            );

            var orchestrator = new ValidationOrchestrator<TestValidationData, ErrorValidationFailure>(
                [firstRule, secondRule]
            );

            var firstResult = orchestrator.Validate(new TestValidationData());

            Assert.Same(
                expectedFailure,
                firstResult
            );

            Assert.Equal(
                1,
                firstRule.CallCount
            );

            Assert.Equal(
                0,
                secondRule.CallCount
            );

            shouldFail = false;

            var secondResult = orchestrator.Validate(new TestValidationData());

            Assert.Null(secondResult);

            Assert.Equal(
                2,
                firstRule.CallCount
            );

            Assert.Equal(
                1,
                secondRule.CallCount
            );
        }
        #endregion
    }
}
