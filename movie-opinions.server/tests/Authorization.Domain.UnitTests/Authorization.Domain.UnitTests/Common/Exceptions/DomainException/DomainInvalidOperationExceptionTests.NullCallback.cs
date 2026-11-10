using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.EmailUser;

namespace Authorization.Domain.UnitTests.Common.Exceptions.DomainException
{
    public partial class DomainInvalidOperationExceptionTests
    {
        #region NullCallback
        [Fact]
        public void NullCallback_DefaultArguments_SetsExceptionMetadata()
        {
            const string callbackName = "callback";

            var exception = DomainInvalidOperationException.NullCallback<Error>(
                callbackName: callbackName
            );

            Assert.Equal(
                "DOMAIN.INVALID_OPERATION.NULL_CALLBACK",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.InvalidOperation,
                exception.ExceptionType
            );

            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void NullCallback_DefaultArguments_BuildsContext()
        {
            const string callbackName = "callback";

            var exception = DomainInvalidOperationException.NullCallback<Error>(
                callbackName: callbackName
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
                callbackName,
                exception.Context["CallbackName"]
            );
        }

        [Fact]
        public void NullCallback_DefaultArguments_GeneratesDefaultMessage()
        {
            const string callbackName = "callback";

            const string expected =
                "The callback 'callback' is null during Read for type 'Error'!";

            var exception = DomainInvalidOperationException.NullCallback<Error>(
                callbackName: callbackName
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
        public void NullCallback_ExplicitOperation_IsUsedInContextAndDefaultMessage(OperationType operationType)
        {
            const string callbackName = "callback";

            var expectedMessage =
                $"The callback 'callback' is null during {operationType} for type 'Error'!";

            var exception = DomainInvalidOperationException.NullCallback<Error>(
                callbackName: callbackName,
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
        [InlineData("callback")]
        [InlineData("onSuccess")]
        [InlineData("tokenOperation")]
        public void NullCallback_CallbackName_IsUsedInContextAndDefaultMessage(string callbackName)
        {
            var exception = DomainInvalidOperationException.NullCallback<Error>(
                callbackName: callbackName
            );

            Assert.Equal(
                callbackName,
                exception.Context["CallbackName"]
            );

            Assert.Contains(
                $"The callback '{callbackName}'",
                exception.Message
            );
        }

        [Fact]
        public void NullCallback_OwnerType_IsUsedInContextAndDefaultMessage()
        {
            const string callbackName = "onCompleted";

            var exception = DomainInvalidOperationException.NullCallback<Email>(
                callbackName: callbackName
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
                callbackName,
                exception.Context["CallbackName"]
            );
        }

        [Theory]
        [InlineData("Custom diagnostic message")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        public void NullCallback_CustomMessage_IsPreserved(string customMessage)
        {
            const string callbackName = "callback";

            var exception = DomainInvalidOperationException.NullCallback<Error>(
                callbackName: callbackName,
                message: customMessage
            );

            Assert.Equal(
                customMessage,
                exception.Message
            );
        }

        [Fact]
        public void NullCallback_EmptyContext_ContainsOnlyStandardFields()
        {
            const string callbackName = "callback";

            var emptyContext = new Dictionary<string, object>();

            var exception = DomainInvalidOperationException.NullCallback<Error>(
                callbackName: callbackName,
                context: emptyContext
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
                "Read",
                exception.Context["Operation"]
            );

            Assert.Equal(
                callbackName,
                exception.Context["CallbackName"]
            );
        }

        [Fact]
        public void NullCallback_CustomContext_PrefixesKeysAndPreservesValues()
        {
            const string callbackName = "callback";

            var customContext = new Dictionary<string, object>
            {
                ["UserId"] = 42,
                ["Trace"] = "abc-123"
            };

            var exception = DomainInvalidOperationException.NullCallback<Error>(
                callbackName: callbackName,
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
        public void NullCallback_CustomContext_DoesNotOverrideStandardFields()
        {
            const string callbackName = "callback";

            var customContext = new Dictionary<string, object>
            {
                ["Layer"] = "Infrastructure",
                ["Type"] = "ShouldNotAppear",
                ["Operation"] = "ShouldNotAppear",
                ["CallbackName"] = "FakeCallback"
            };

            var exception = DomainInvalidOperationException.NullCallback<Error>(
                callbackName: callbackName,
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
                callbackName,
                exception.Context["CallbackName"]
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
                "FakeCallback",
                exception.Context["Custom_CallbackName"]
            );
        }

        // Префікс додається знову навіть до вже префіксованого ключа.
        // The prefix is applied again even to an already-prefixed key.
        [Fact]
        public void NullCallback_AlreadyPrefixedContextKey_AddsPrefixAgain()
        {
            const string callbackName = "callback";

            var customContext = new Dictionary<string, object>
            {
                ["Custom_TraceId"] = "trace-001"
            };

            var exception = DomainInvalidOperationException.NullCallback<Error>(
                callbackName: callbackName,
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
        public void NullCallback_CustomContext_DoesNotModifyInputDictionary()
        {
            const string callbackName = "callback";

            var original = new Dictionary<string, object>
            {
                ["UserId"] = 100,
                ["Source"] = "API"
            };

            var snapshot = new Dictionary<string, object>(original);

            _ = DomainInvalidOperationException.NullCallback<Error>(
                callbackName: callbackName,
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
                "CallbackName",
                original.Keys
            );

            Assert.DoesNotContain(
                "Custom_UserId",
                original.Keys
            );
        }

        [Fact]
        public void NullCallback_CustomContext_CopiesDictionaryEntries()
        {
            const string callbackName = "callback";

            var customContext = new Dictionary<string, object>
            {
                ["UserId"] = 42,
                ["Trace"] = "original"
            };

            var exception = DomainInvalidOperationException.NullCallback<Error>(
                callbackName: callbackName,
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
        public void NullCallback_WithInnerException_PreservesReference()
        {
            const string callbackName = "callback";

            var inner = new InvalidOperationException("root cause");

            var exception = DomainInvalidOperationException.NullCallback<Error>(
                callbackName: callbackName,
                innerException: inner
            );

            Assert.Same(
                inner,
                exception.InnerException
            );
        }

        [Fact]
        public void NullCallback_SeparateCalls_CreateIndependentInstances()
        {
            const string firstCallbackName = "callback";
            const string secondCallbackName = "onSuccess";

            var first = DomainInvalidOperationException.NullCallback<Error>(
                callbackName: firstCallbackName,
                context: new Dictionary<string, object> { ["A"] = 1 }
            );

            var second = DomainInvalidOperationException.NullCallback<Error>(
                callbackName: secondCallbackName,
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
                firstCallbackName,
                first.Context["CallbackName"]
            );

            Assert.Equal(
                1,
                first.Context["Custom_A"]
            );

            Assert.Equal(
                secondCallbackName,
                second.Context["CallbackName"]
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
