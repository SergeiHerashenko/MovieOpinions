using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.EmailUser;

namespace Authorization.Domain.UnitTests.Common.Exceptions.DomainException
{
    public partial class DomainInvalidOperationExceptionTests
    {
        #region PreconditionFailed
        [Fact]
        public void PreconditionFailed_DefaultArguments_SetsExceptionMetadata()
        {
            const string sourceContext = "ValidatePhone";
            const string prerequisite = "RequiredRule";

            var exception = DomainInvalidOperationException.PreconditionFailed<Error>(
                sourceContext: sourceContext,
                prerequisite: prerequisite
            );

            Assert.Equal(
                "DOMAIN.INVALID_OPERATION.PRECONDITION_FAILED",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.InvalidOperation,
                exception.ExceptionType
            );

            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void PreconditionFailed_DefaultArguments_BuildsContext()
        {
            const string sourceContext = "ValidatePhone";
            const string prerequisite = "RequiredRule";

            var exception = DomainInvalidOperationException.PreconditionFailed<Error>(
                sourceContext: sourceContext,
                prerequisite: prerequisite
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
                "Read",
                exception.Context["Operation"]
            );

            Assert.Equal(
                sourceContext,
                exception.Context["SourceContext"]
            );

            Assert.Equal(
                prerequisite,
                exception.Context["Prerequisite"]
            );
        }

        [Fact]
        public void PreconditionFailed_DefaultArguments_GeneratesDefaultMessage()
        {
            const string sourceContext = "ValidatePhone";
            const string prerequisite = "RequiredRule";
            const string expected =
                "Precondition failed in 'ValidatePhone' for type 'Error'. " +
                "Required prerequisite 'RequiredRule' was not satisfied.";

            var exception = DomainInvalidOperationException.PreconditionFailed<Error>(
                sourceContext: sourceContext,
                prerequisite: prerequisite
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
        public void PreconditionFailed_ExplicitOperation_SetsOperationInContext(OperationType operationType)
        {
            const string sourceContext = "ValidatePhone";
            const string prerequisite = "RequiredRule";
            const string expectedMessage =
                "Precondition failed in 'ValidatePhone' for type 'Error'. " +
                "Required prerequisite 'RequiredRule' was not satisfied.";

            var exception = DomainInvalidOperationException.PreconditionFailed<Error>(
                sourceContext: sourceContext,
                prerequisite: prerequisite,
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
        [InlineData("ValidatePhone")]
        [InlineData("ValidateLogin")]
        [InlineData("ValidatePassword")]
        public void PreconditionFailed_SourceContext_IsUsedInContextAndDefaultMessage(string sourceContext)
        {
            const string prerequisite = "RequiredRule";

            var exception = DomainInvalidOperationException.PreconditionFailed<Error>(
                sourceContext: sourceContext,
                prerequisite: prerequisite
            );

            Assert.Equal(
                sourceContext,
                exception.Context["SourceContext"]
            );

            Assert.Contains(
                $"Precondition failed in '{sourceContext}'",
                exception.Message
            );
        }

        [Theory]
        [InlineData("RequiredRule")]
        [InlineData("FormatRule")]
        [InlineData("NumberingPlanRule")]
        public void PreconditionFailed_Prerequisite_IsUsedInContextAndDefaultMessage(string prerequisite)
        {
            const string sourceContext = "ValidatePhone";

            var exception = DomainInvalidOperationException.PreconditionFailed<Error>(
                sourceContext: sourceContext,
                prerequisite: prerequisite
            );

            Assert.Equal(
                prerequisite,
                exception.Context["Prerequisite"]
            );

            Assert.Contains(
                $"Required prerequisite '{prerequisite}' was not satisfied.",
                exception.Message
            );
        }

        [Fact]
        public void PreconditionFailed_OwnerType_IsUsedInContextAndDefaultMessage()
        {
            const string sourceContext = "ValidatePhone";
            const string prerequisite = "RequiredRule";

            var exception = DomainInvalidOperationException.PreconditionFailed<Email>(
                sourceContext: sourceContext,
                prerequisite: prerequisite
            );

            Assert.Equal(
                "Email",
                exception.Context["Type"]
            );

            Assert.Contains(
                "for type 'Email'",
                exception.Message
            );

            Assert.Equal(
                sourceContext,
                exception.Context["SourceContext"]
            );

            Assert.Equal(
                prerequisite,
                exception.Context["Prerequisite"]
            );
        }

        [Theory]
        [InlineData("Custom diagnostic message")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\r\n")]
        public void PreconditionFailed_CustomMessage_IsPreserved(string customMessage)
        {
            const string sourceContext = "ValidatePhone";
            const string prerequisite = "RequiredRule";

            var exception = DomainInvalidOperationException.PreconditionFailed<Error>(
                sourceContext: sourceContext,
                prerequisite: prerequisite,
                message: customMessage
            );

            Assert.Equal(
                customMessage,
                exception.Message
            );
        }

        [Fact]
        public void PreconditionFailed_EmptyContext_ContainsOnlyStandardFields()
        {
            const string sourceContext = "ValidatePhone";
            const string prerequisite = "RequiredRule";
            var emptyContext = new Dictionary<string, object>();

            var exception = DomainInvalidOperationException.PreconditionFailed<Error>(
                sourceContext: sourceContext,
                prerequisite: prerequisite,
                context: emptyContext
            );

            Assert.Equal(
                5,
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
                "Read",
                exception.Context["Operation"]
            );

            Assert.Equal(
                sourceContext,
                exception.Context["SourceContext"]
            );

            Assert.Equal(
                prerequisite,
                exception.Context["Prerequisite"]
            );
        }

        [Fact]
        public void PreconditionFailed_CustomContext_PrefixesKeysAndPreservesValues()
        {
            const string sourceContext = "ValidatePhone";
            const string prerequisite = "RequiredRule";

            var customContext = new Dictionary<string, object>
            {
                ["UserId"] = 42,
                ["Trace"] = "abc-123"
            };

            var exception = DomainInvalidOperationException.PreconditionFailed<Error>(
                sourceContext: sourceContext,
                prerequisite: prerequisite,
                context: customContext
            );

            Assert.Equal(
                42,
                exception.Context["Custom_UserId"]
            );

            Assert.Equal(
                "abc-123",
                exception.Context["Custom_Trace"]
            );

            Assert.IsType<int>(exception.Context["Custom_UserId"]);

            Assert.IsType<string>(exception.Context["Custom_Trace"]);
        }

        [Fact]
        public void PreconditionFailed_CustomContext_DoesNotOverrideStandardFields()
        {
            const string sourceContext = "ValidatePhone";
            const string prerequisite = "RequiredRule";

            var customContext = new Dictionary<string, object>
            {
                ["Layer"] = "Infrastructure",
                ["Type"] = "ShouldNotAppear",
                ["Operation"] = "ShouldNotAppear",
                ["SourceContext"] = "FakeSource",
                ["Prerequisite"] = "FakePrerequisite"
            };

            var exception = DomainInvalidOperationException.PreconditionFailed<Error>(
                sourceContext: sourceContext,
                prerequisite: prerequisite,
                operationType: OperationType.Create,
                context: customContext
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
                sourceContext,
                exception.Context["SourceContext"]
            );

            Assert.Equal(
                prerequisite,
                exception.Context["Prerequisite"]
            );

            Assert.Equal(
                "Infrastructure",
                exception.Context["Custom_Layer"]
            );

            Assert.Equal(
                "ShouldNotAppear",
                exception.Context["Custom_Type"]
            );

            Assert.Equal(
                "ShouldNotAppear",
                exception.Context["Custom_Operation"]
            );

            Assert.Equal(
                "FakeSource",
                exception.Context["Custom_SourceContext"]
            );

            Assert.Equal(
                "FakePrerequisite",
                exception.Context["Custom_Prerequisite"]
            );
        }

        // Префікс додається знову навіть до вже префіксованого ключа.
        // The prefix is applied again even to an already-prefixed key.
        [Fact]
        public void PreconditionFailed_AlreadyPrefixedContextKey_AddsPrefixAgain()
        {
            const string sourceContext = "ValidatePhone";
            const string prerequisite = "RequiredRule";

            var customContext = new Dictionary<string, object>
            {
                ["Custom_TraceId"] = "trace-001"
            };

            var exception = DomainInvalidOperationException.PreconditionFailed<Error>(
                sourceContext: sourceContext,
                prerequisite: prerequisite,
                context: customContext
            );

            Assert.True(exception.Context.ContainsKey("Custom_Custom_TraceId"));

            Assert.Equal(
                "trace-001",
                exception.Context["Custom_Custom_TraceId"]
            );

            Assert.False(exception.Context.ContainsKey("Custom_TraceId"));
        }

        [Fact]
        public void PreconditionFailed_CustomContext_DoesNotModifyInputDictionary()
        {
            const string sourceContext = "ValidatePhone";
            const string prerequisite = "RequiredRule";

            var original = new Dictionary<string, object>
            {
                ["UserId"] = 100,
                ["Source"] = "API"
            };

            var snapshot = new Dictionary<string, object>(original);

            _ = DomainInvalidOperationException.PreconditionFailed<Error>(
                sourceContext: sourceContext,
                prerequisite: prerequisite,
                context: original
            );

            Assert.Equal(
                snapshot,
                original
            );

            Assert.DoesNotContain(
                "Layer",
                original.Keys
            );

            Assert.DoesNotContain(
                "SourceContext",
                original.Keys
            );

            Assert.DoesNotContain(
                "Prerequisite",
                original.Keys
            );

            Assert.DoesNotContain(
                "Custom_UserId",
                original.Keys
            );
        }

        [Fact]
        public void PreconditionFailed_CustomContext_CopiesDictionaryEntries()
        {
            const string sourceContext = "ValidatePhone";
            const string prerequisite = "RequiredRule";

            var customContext = new Dictionary<string, object>
            {
                ["UserId"] = 42,
                ["Trace"] = "original"
            };

            var exception = DomainInvalidOperationException.PreconditionFailed<Error>(
                sourceContext: sourceContext,
                prerequisite: prerequisite,
                context: customContext
            );

            customContext["UserId"] = 999;
            customContext["NewKey"] = "added";
            customContext.Remove("Trace");

            Assert.Equal(
                42,
                exception.Context["Custom_UserId"]
            );

            Assert.Equal(
                "original",
                exception.Context["Custom_Trace"]
            );

            Assert.False(exception.Context.ContainsKey("Custom_NewKey"));
        }

        [Fact]
        public void PreconditionFailed_WithInnerException_PreservesReference()
        {
            const string sourceContext = "ValidatePhone";
            const string prerequisite = "RequiredRule";

            var inner = new InvalidOperationException("root cause");

            var exception = DomainInvalidOperationException.PreconditionFailed<Error>(
                sourceContext: sourceContext,
                prerequisite: prerequisite,
                innerException: inner
            );

            Assert.Same(
                inner,
                exception.InnerException
            );
        }

        [Fact]
        public void PreconditionFailed_SeparateCalls_CreateIndependentInstances()
        {
            const string firstSourceContext = "ValidatePhone";
            const string firstPrerequisite = "RequiredRule";
            const string secondSourceContext = "ValidateLogin";
            const string secondPrerequisite = "FormatRule";

            var first = DomainInvalidOperationException.PreconditionFailed<Error>(
                sourceContext: firstSourceContext,
                prerequisite: firstPrerequisite,
                context: new Dictionary<string, object> { ["A"] = 1 }
            );

            var second = DomainInvalidOperationException.PreconditionFailed<Error>(
                sourceContext: secondSourceContext,
                prerequisite: secondPrerequisite,
                context: new Dictionary<string, object> { ["B"] = 2 }
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
                firstSourceContext,
                first.Context["SourceContext"]
            );

            Assert.Equal(
                firstPrerequisite,
                first.Context["Prerequisite"]
            );

            Assert.Equal(
                1,
                first.Context["Custom_A"]
            );

            Assert.Equal(
                secondSourceContext,
                second.Context["SourceContext"]
            );

            Assert.Equal(
                secondPrerequisite,
                second.Context["Prerequisite"]
            );

            Assert.Equal(
                2,
                second.Context["Custom_B"]
            );

            Assert.False(first.Context.ContainsKey("Custom_B"));

            Assert.False(second.Context.ContainsKey("Custom_A"));
        }
        #endregion
    }
}
