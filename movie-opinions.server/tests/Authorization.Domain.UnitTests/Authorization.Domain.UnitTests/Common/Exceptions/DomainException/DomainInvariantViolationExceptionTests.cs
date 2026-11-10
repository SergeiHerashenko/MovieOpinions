using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.EmailUser;

namespace Authorization.Domain.UnitTests.Common.Exceptions.DomainException
{
    public class DomainInvariantViolationExceptionTests
    {
        #region BrokenState
        [Fact]
        public void BrokenState_DefaultArguments_SetsExceptionMetadata()
        {
            const string ruleDescription = "Errors must not contain null entries";

            var stateContext = new Dictionary<string, object?>();

            var exception = DomainInvariantViolationException.BrokenState<Error>(
                ruleDescription: ruleDescription,
                stateContext: stateContext
            );

            Assert.Equal(
                "DOMAIN.INVARIANT_VIOLATION.INVALID_STATE",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.InvariantViolation,
                exception.ExceptionType
            );

            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void BrokenState_DefaultArguments_BuildsContext()
        {
            const string ruleDescription = "Errors must not contain null entries";

            var stateContext = new Dictionary<string, object?>();

            var exception = DomainInvariantViolationException.BrokenState<Error>(
                ruleDescription: ruleDescription,
                stateContext: stateContext
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "Error",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                ruleDescription,
                exception.Context["Rule"]
            );
        }

        [Fact]
        public void BrokenState_DefaultArguments_GeneratesDefaultMessage()
        {
            const string ruleDescription = "Errors must not contain null entries";
            const string expected =
                "Critical invariant violation in type 'Error'. " +
                "Broken rule: Errors must not contain null entries.";

            var stateContext = new Dictionary<string, object?>();

            var exception = DomainInvariantViolationException.BrokenState<Error>(
                ruleDescription: ruleDescription,
                stateContext: stateContext
            );

            Assert.Equal(
                expected,
                exception.Message
            );
        }

        [Theory]
        [InlineData(OperationType.Restore)]
        [InlineData(OperationType.Create)]
        [InlineData(OperationType.Update)]
        [InlineData(OperationType.Delete)]
        [InlineData(OperationType.Read)]
        [InlineData(OperationType.Compare)]
        public void BrokenState_ExplicitOperation_SetsOperationInContext(OperationType operationType)
        {
            const string ruleDescription = "Errors must not contain null entries";
            const string expectedMessage =
                "Critical invariant violation in type 'Error'. " +
                "Broken rule: Errors must not contain null entries.";

            var stateContext = new Dictionary<string, object?>();

            var exception = DomainInvariantViolationException.BrokenState<Error>(
                ruleDescription: ruleDescription,
                stateContext: stateContext,
                operationType: operationType
            );

            Assert.Equal(
                operationType.ToString(),
                exception.Context["Operation"]
            );

            Assert.Equal(
                expectedMessage,
                exception.Message
            );
        }

        [Theory]
        [InlineData("Errors must not contain null entries")]
        [InlineData("A successful result cannot contain errors")]
        [InlineData("UpdatedAt must not be earlier than CreatedAt")]
        public void BrokenState_RuleDescription_IsUsedInContextAndDefaultMessage(string ruleDescription)
        {
            var stateContext = new Dictionary<string, object?>();

            var exception = DomainInvariantViolationException.BrokenState<Error>(
                ruleDescription: ruleDescription,
                stateContext: stateContext
            );

            Assert.Equal(
                ruleDescription,
                exception.Context["Rule"]
            );

            Assert.Contains(
                $"Broken rule: {ruleDescription}.",
                exception.Message
            );
        }

        [Fact]
        public void BrokenState_OwnerType_IsUsedInContextAndDefaultMessage()
        {
            const string ruleDescription = "Errors must not contain null entries";

            var stateContext = new Dictionary<string, object?>();

            var exception = DomainInvariantViolationException.BrokenState<Email>(
                ruleDescription: ruleDescription,
                stateContext: stateContext
            );

            Assert.Equal(
                "Email",
                exception.Context["Type"]
            );

            Assert.Contains(
                "in type 'Email'",
                exception.Message
            );

            Assert.Equal(
                ruleDescription,
                exception.Context["Rule"]
            );
        }

        [Theory]
        [InlineData("Custom diagnostic message")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\r\n")]
        public void BrokenState_CustomMessage_IsPreserved(string customMessage)
        {
            const string ruleDescription = "Errors must not contain null entries";

            var stateContext = new Dictionary<string, object?>();

            var exception = DomainInvariantViolationException.BrokenState<Error>(
                ruleDescription: ruleDescription,
                stateContext: stateContext,
                message: customMessage
            );

            Assert.Equal(
                customMessage,
                exception.Message
            );
        }

        [Fact]
        public void BrokenState_EmptyStateContext_ContainsOnlyStandardFields()
        {
            const string ruleDescription = "Errors must not contain null entries";

            var stateContext = new Dictionary<string, object?>();

            var exception = DomainInvariantViolationException.BrokenState<Error>(
                ruleDescription: ruleDescription,
                stateContext: stateContext
            );

            Assert.Equal(
                4,
                exception.Context.Count
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "Error",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                ruleDescription,
                exception.Context["Rule"]
            );
        }

        [Theory]
        [InlineData(42)]
        [InlineData(0)]
        [InlineData("text")]
        [InlineData("")]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(3.14)]
        public void BrokenState_StateContext_PrefixesKeysAndPreservesNonNullValues(object value)
        {
            const string ruleDescription = "Errors must not contain null entries";

            var stateContext = new Dictionary<string, object?>
            {
                ["Value"] = value
            };

            var exception = DomainInvariantViolationException.BrokenState<Error>(
                ruleDescription: ruleDescription,
                stateContext: stateContext
            );

            Assert.Equal(
                value,
                exception.Context["State_Value"]
            );

            Assert.IsType(
                value.GetType(),
                exception.Context["State_Value"]
            );
        }

        // У контекст потрапляє рядок "null", а не null-посилання.
        // The context stores the string "null", not a null reference.
        [Fact]
        public void BrokenState_StateContext_NullValue_IsStoredAsStringNull()
        {
            const string ruleDescription = "Errors must not contain null entries";

            var stateContext = new Dictionary<string, object?>
            {
                ["UpdatedAt"] = null
            };

            var exception = DomainInvariantViolationException.BrokenState<Error>(
                ruleDescription: ruleDescription,
                stateContext: stateContext
            );

            Assert.Equal(
                "null",
                exception.Context["State_UpdatedAt"]
            );

            Assert.IsType<string>(exception.Context["State_UpdatedAt"]);
        }

        [Fact]
        public void BrokenState_StateContext_DoesNotOverrideStandardFields()
        {
            const string ruleDescription = "Errors must not contain null entries";

            var stateContext = new Dictionary<string, object?>
            {
                ["Layer"] = "Infrastructure",
                ["Type"] = "ShouldNotAppear",
                ["Operation"] = "ShouldNotAppear",
                ["Rule"] = "FakeRule"
            };

            var exception = DomainInvariantViolationException.BrokenState<Error>(
                ruleDescription: ruleDescription,
                stateContext: stateContext,
                operationType: OperationType.Create
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "Error",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Create",
                exception.Context["Operation"]
            );

            Assert.Equal(
                ruleDescription,
                exception.Context["Rule"]
            );

            Assert.Equal(
                "Infrastructure",
                exception.Context["State_Layer"]
            );

            Assert.Equal(
                "ShouldNotAppear",
                exception.Context["State_Type"]
            );

            Assert.Equal(
                "ShouldNotAppear",
                exception.Context["State_Operation"]
            );

            Assert.Equal(
                "FakeRule",
                exception.Context["State_Rule"]
            );
        }

        // Префікс додається знову навіть до вже префіксованого ключа.
        // The prefix is applied again even to an already-prefixed key.
        [Fact]
        public void BrokenState_AlreadyPrefixedStateKey_AddsPrefixAgain()
        {
            const string ruleDescription = "Errors must not contain null entries";

            var stateContext = new Dictionary<string, object?>
            {
                ["State_TraceId"] = "trace-001"
            };

            var exception = DomainInvariantViolationException.BrokenState<Error>(
                ruleDescription: ruleDescription,
                stateContext: stateContext
            );

            Assert.True(exception.Context.ContainsKey("State_State_TraceId"));

            Assert.Equal(
                "trace-001",
                exception.Context["State_State_TraceId"]
            );

            Assert.False(exception.Context.ContainsKey("State_TraceId"));
        }

        [Fact]
        public void BrokenState_StateContext_DoesNotModifyInputDictionary()
        {
            const string ruleDescription = "Errors must not contain null entries";

            var original = new Dictionary<string, object?>
            {
                ["UserId"] = 100,
                ["Source"] = "API",
                ["UpdatedAt"] = null
            };

            var snapshot = new Dictionary<string, object?>(original);

            _ = DomainInvariantViolationException.BrokenState<Error>(
                ruleDescription: ruleDescription,
                stateContext: original
            );

            Assert.Equal(
                snapshot,
                original
            );

            Assert.Null(original["UpdatedAt"]);

            Assert.DoesNotContain(
                "Layer",
                original.Keys
            );

            Assert.DoesNotContain(
                "State_UserId",
                original.Keys
            );
        }

        [Fact]
        public void BrokenState_StateContext_CopiesDictionaryEntries()
        {
            const string ruleDescription = "Errors must not contain null entries";

            var stateContext = new Dictionary<string, object?>
            {
                ["UserId"] = 42,
                ["Trace"] = "original"
            };

            var exception = DomainInvariantViolationException.BrokenState<Error>(
                ruleDescription: ruleDescription,
                stateContext: stateContext
            );

            stateContext["UserId"] = 999;
            stateContext["NewKey"] = "added";
            stateContext.Remove("Trace");

            Assert.Equal(
                42,
                exception.Context["State_UserId"]
            );

            Assert.Equal(
                "original",
                exception.Context["State_Trace"]
            );

            Assert.False(exception.Context.ContainsKey("State_NewKey"));
        }

        [Fact]
        public void BrokenState_WithInnerException_PreservesReference()
        {
            const string ruleDescription = "Errors must not contain null entries";

            var stateContext = new Dictionary<string, object?>();

            var inner = new InvalidOperationException("root cause");

            var exception = DomainInvariantViolationException.BrokenState<Error>(
                ruleDescription: ruleDescription,
                stateContext: stateContext,
                innerException: inner
            );

            Assert.Same(
                inner,
                exception.InnerException
            );
        }

        [Fact]
        public void BrokenState_SeparateCalls_CreateIndependentInstances()
        {
            const string firstRule = "Errors must not contain null entries";
            const string secondRule = "A successful result cannot contain errors";

            var first = DomainInvariantViolationException.BrokenState<Error>(
                ruleDescription: firstRule,
                stateContext: new Dictionary<string, object?> { ["A"] = 1 }
            );

            var second = DomainInvariantViolationException.BrokenState<Error>(
                ruleDescription: secondRule,
                stateContext: new Dictionary<string, object?> { ["B"] = 2 }
            );

            Assert.NotSame(
                first,
                second
            );

            Assert.NotSame(
                first.Context,
                second.Context
            );

            Assert.Equal(
                firstRule,
                first.Context["Rule"]
            );

            Assert.Equal(
                1,
                first.Context["State_A"]
            );

            Assert.Equal(
                secondRule,
                second.Context["Rule"]
            );

            Assert.Equal(
                2,
                second.Context["State_B"]
            );

            Assert.False(first.Context.ContainsKey("State_B"));

            Assert.False(second.Context.ContainsKey("State_A"));
        }
        #endregion
    }
}
