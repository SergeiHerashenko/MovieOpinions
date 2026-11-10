using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.EmailUser;

namespace Authorization.Domain.UnitTests.Common.Exceptions.DomainException
{
    public partial class DomainInvalidOperationExceptionTests
    {
        #region ValueAccessOnFailure
        [Fact]
        public void ValueAccessOnFailure_DefaultArguments_SetsExceptionMetadata()
        {
            const string valueName = "Value";

            var exception = DomainInvalidOperationException.ValueAccessOnFailure<Error>(
                valueName: valueName
            );

            Assert.Equal(
                "DOMAIN.INVALID_OPERATION.VALUE_ACCESS_ON_FAILURE",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.InvalidOperation,
                exception.ExceptionType
            );

            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void ValueAccessOnFailure_DefaultArguments_BuildsContext()
        {
            const string valueName = "Value";

            var exception = DomainInvalidOperationException.ValueAccessOnFailure<Error>(
                valueName: valueName
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
                valueName,
                exception.Context["ValueName"]
            );
        }

        [Fact]
        public void ValueAccessOnFailure_DefaultArguments_GeneratesDefaultMessage()
        {
            const string valueName = "Value";

            const string expected =
                "Unable to access the value 'Value' in type 'Error'!";

            var exception = DomainInvalidOperationException.ValueAccessOnFailure<Error>(
                valueName: valueName
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
        public void ValueAccessOnFailure_ExplicitOperation_SetsOperationInContext(OperationType operationType)
        {
            const string valueName = "Value";

            const string expectedMessage =
                "Unable to access the value 'Value' in type 'Error'!";

            var exception = DomainInvalidOperationException.ValueAccessOnFailure<Error>(
                valueName: valueName,
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
        [InlineData("Value")]
        [InlineData("PasswordHash")]
        [InlineData("UserId")]
        public void ValueAccessOnFailure_ValueName_IsUsedInContextAndDefaultMessage(string valueName)
        {
            var exception = DomainInvalidOperationException.ValueAccessOnFailure<Error>(
                valueName: valueName
            );

            Assert.Equal(
                valueName,
                exception.Context["ValueName"]
            );

            Assert.Contains(
                $"value '{valueName}'",
                exception.Message
            );
        }

        [Fact]
        public void ValueAccessOnFailure_OwnerType_IsUsedInContextAndDefaultMessage()
        {
            const string valueName = "Address";

            var exception = DomainInvalidOperationException.ValueAccessOnFailure<Email>(
                valueName: valueName
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
                valueName,
                exception.Context["ValueName"]
            );
        }

        [Theory]
        [InlineData("Custom diagnostic message")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        public void ValueAccessOnFailure_CustomMessage_IsPreserved(string customMessage)
        {
            const string valueName = "Value";

            var exception = DomainInvalidOperationException.ValueAccessOnFailure<Error>(
                valueName: valueName,
                message: customMessage
            );

            Assert.Equal(
                customMessage,
                exception.Message
            );
        }

        [Fact]
        public void ValueAccessOnFailure_EmptyContext_ContainsOnlyStandardFields()
        {
            const string valueName = "Value";

            var emptyContext = new Dictionary<string, object>();

            var exception = DomainInvalidOperationException.ValueAccessOnFailure<Error>(
                valueName: valueName,
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
                valueName,
                exception.Context["ValueName"]
            );
        }

        [Fact]
        public void ValueAccessOnFailure_CustomContext_PrefixesKeysAndPreservesValues()
        {
            const string valueName = "Value";

            var customContext = new Dictionary<string, object>
            {
                ["UserId"] = 42,
                ["Trace"] = "abc-123"
            };

            var exception = DomainInvalidOperationException.ValueAccessOnFailure<Error>(
                valueName: valueName,
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
        public void ValueAccessOnFailure_CustomContext_DoesNotOverrideStandardFields()
        {
            const string valueName = "Value";

            var customContext = new Dictionary<string, object>
            {
                ["Layer"] = "Infrastructure",
                ["Type"] = "ShouldNotAppear",
                ["Operation"] = "ShouldNotAppear",
                ["ValueName"] = "FakeName"
            };

            var exception = DomainInvalidOperationException.ValueAccessOnFailure<Error>(
                valueName: valueName,
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
                valueName,
                exception.Context["ValueName"]
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
                "FakeName",
                exception.Context["Custom_ValueName"]
            );
        }

        // Префікс додається знову навіть до вже префіксованого ключа.
        // The prefix is applied again even to an already-prefixed key.
        [Fact]
        public void ValueAccessOnFailure_AlreadyPrefixedContextKey_AddsPrefixAgain()
        {
            const string valueName = "Value";

            var customContext = new Dictionary<string, object>
            {
                ["Custom_TraceId"] = "trace-001"
            };

            var exception = DomainInvalidOperationException.ValueAccessOnFailure<Error>(
                valueName: valueName,
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
        public void ValueAccessOnFailure_CustomContext_DoesNotModifyInputDictionary()
        {
            const string valueName = "Value";

            var original = new Dictionary<string, object>
            {
                ["UserId"] = 100,
                ["Source"] = "API"
            };

            var snapshot = new Dictionary<string, object>(original);

            _ = DomainInvalidOperationException.ValueAccessOnFailure<Error>(
                valueName: valueName,
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
                "ValueName",
                original.Keys
            );

            Assert.DoesNotContain(
                "Custom_UserId",
                original.Keys
            );
        }

        [Fact]
        public void ValueAccessOnFailure_CustomContext_CopiesDictionaryEntries()
        {
            const string valueName = "Value";

            var customContext = new Dictionary<string, object>
            {
                ["UserId"] = 42,
                ["Trace"] = "original"
            };

            var exception = DomainInvalidOperationException.ValueAccessOnFailure<Error>(
                valueName: valueName,
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
        public void ValueAccessOnFailure_WithInnerException_PreservesReference()
        {
            const string valueName = "Value";

            var inner = new InvalidOperationException("root cause");

            var exception = DomainInvalidOperationException.ValueAccessOnFailure<Error>(
                valueName: valueName,
                innerException: inner
            );

            Assert.Same(
                inner,
                exception.InnerException
            );
        }

        [Fact]
        public void ValueAccessOnFailure_SeparateCalls_CreateIndependentInstances()
        {
            const string firstValueName = "Value";
            const string secondValueName = "PasswordHash";

            var first = DomainInvalidOperationException.ValueAccessOnFailure<Error>(
                valueName: firstValueName,
                context: new Dictionary<string, object> { ["A"] = 1 }
            );

            var second = DomainInvalidOperationException.ValueAccessOnFailure<Error>(
                valueName: secondValueName,
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
                firstValueName,
                first.Context["ValueName"]
            );

            Assert.Equal(
                1,
                first.Context["Custom_A"]
            );

            Assert.Equal(
                secondValueName,
                second.Context["ValueName"]
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
