using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.EmailUser;

namespace Authorization.Domain.UnitTests.Common.Exceptions.DomainException
{
    public partial class DomainDataInconsistencyExceptionTests
    {
        #region Empty
        [Fact]
        public void Empty_DefaultArguments_SetsExceptionMetadata()
        {
            const string fieldName = "Code";

            var exception = DomainDataInconsistencyException.Empty<Error>(fieldName);

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.EMPTY_VALUE",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.DataInconsistency,
                exception.ExceptionType
            );

            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void Empty_DefaultArguments_BuildsContext()
        {
            const string fieldName = "Code";

            var exception = DomainDataInconsistencyException.Empty<Error>(fieldName);

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                nameof(Error),
                exception.Context["Type"]
            );

            Assert.Equal(
                fieldName,
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );
        }

        [Fact]
        public void Empty_DefaultArguments_GeneratesDefaultMessage()
        {
            const string fieldName = "Code";

            const string expectedMessage =
                "An error occurred during the 'Restore' operation. " +
                "The field 'Code' in type 'Error' is empty!";

            var exception = DomainDataInconsistencyException.Empty<Error>(fieldName);

            Assert.Equal(
                expectedMessage,
                exception.Message
            );
        }

        [Theory]
        [InlineData(OperationType.Restore, "Restore")]
        [InlineData(OperationType.Create, "Create")]
        [InlineData(OperationType.Update, "Update")]
        [InlineData(OperationType.Delete, "Delete")]
        [InlineData(OperationType.Read, "Read")]
        [InlineData(OperationType.Compare, "Compare")]
        public void Empty_ExplicitOperation_UsesSpecifiedOperation(
            OperationType operationType,
            string expectedOperation)
        {
            const string fieldName = "Code";

            var exception = DomainDataInconsistencyException.Empty<Error>(
                fieldName,
                operationType
            );

            Assert.Equal(
                expectedOperation,
                exception.Context["Operation"]
            );

            var expectedMessage =
                $"An error occurred during the '{expectedOperation}' operation. " +
                "The field 'Code' in type 'Error' is empty!";

            Assert.Equal(
                expectedMessage,
                exception.Message
            );
        }

        [Theory]
        [InlineData("Code")]
        [InlineData("Login")]
        [InlineData("Profile.Email")]
        public void Empty_FieldName_IsUsedInContextAndDefaultMessage(string fieldName)
        {
            var exception = DomainDataInconsistencyException.Empty<Error>(fieldName);

            Assert.Equal(
                fieldName,
                exception.Context["Field"]
            );

            Assert.Contains(
                $"The field '{fieldName}'",
                exception.Message
            );

            Assert.Contains(
                $"in type '{typeof(Error).Name}'",
                exception.Message
            );

            Assert.Contains(
                "Restore",
                exception.Message
            );
        }

        [Fact]
        public void Empty_OwnerType_IsUsedInContextAndDefaultMessage()
        {
            const string fieldName = "Address";

            var exception = DomainDataInconsistencyException.Empty<Email>(fieldName);

            Assert.Equal(
                "Email",
                exception.Context["Type"]
            );

            Assert.Contains(
                "in type 'Email'",
                exception.Message
            );

            Assert.Equal(
                fieldName,
                exception.Context["Field"]
            );
        }

        [Theory]
        [InlineData("Custom diagnostic message")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        public void Empty_CustomMessage_IsPreserved(string customMessage)
        {
            const string fieldName = "Code";

            var exception = DomainDataInconsistencyException.Empty<Error>(
                fieldName,
                message: customMessage
            );

            Assert.Equal(
                customMessage,
                exception.Message);
        }

        [Fact]
        public void Empty_EmptyContext_ContainsOnlyStandardFields()
        {
            const string fieldName = "Code";

            var exception = DomainDataInconsistencyException.Empty<Error>(
                fieldName,
                context: new Dictionary<string, object>()
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
                nameof(Error),
                exception.Context["Type"]
            );

            Assert.Equal(
                fieldName,
                exception.Context["Field"]
            );

            Assert.Equal(
                nameof(OperationType.Restore),
                exception.Context["Operation"]
            );
        }

        [Fact]
        public void Empty_CustomContext_PrefixesKeysAndPreservesValues()
        {
            var customContext = new Dictionary<string, object>
            {
                ["UserId"] = 42,
                ["Trace"] = "abc-123"
            };

            const string fieldName = "Code";

            var exception = DomainDataInconsistencyException.Empty<Error>(
                fieldName,
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
        public void Empty_CustomContext_DoesNotOverrideStandardFields()
        {
            var customContext = new Dictionary<string, object>
            {
                ["Layer"] = "Infrastructure",
                ["Type"] = "ShouldNotAppear",
                ["Field"] = "ShouldNotAppear",
                ["Operation"] = "ShouldNotAppear"
            };

            const string fieldName = "Code";

            var exception = DomainDataInconsistencyException.Empty<Error>(
                fieldName,
                operationType: OperationType.Create,
                context: customContext
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                nameof(Error),
                exception.Context["Type"]
            );

            Assert.Equal(
                fieldName,
                exception.Context["Field"]
            );

            Assert.Equal(
                nameof(OperationType.Create),
                exception.Context["Operation"]
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
                exception.Context["Custom_Field"]
            );

            Assert.Equal(
                "ShouldNotAppear",
                exception.Context["Custom_Operation"]
            );
        }

        // Префікс додається знову навіть до вже префіксованого ключа.
        // The prefix is applied again even to an already-prefixed key.
        [Fact]
        public void Empty_AlreadyPrefixedContextKey_AddsPrefixAgain()
        {
            var customContext = new Dictionary<string, object>
            {
                ["Custom_TraceId"] = "trace-001"
            };

            const string fieldName = "Code";

            var exception = DomainDataInconsistencyException.Empty<Error>(
                fieldName,
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
        public void Empty_CustomContext_DoesNotModifyInputDictionary()
        {
            var original = new Dictionary<string, object>
            {
                ["UserId"] = 43,
                ["Source"] = "API"
            };

            var snapshot = new Dictionary<string, object>(original);

            const string fieldName = "Code";

            _ = DomainDataInconsistencyException.Empty<Error>(
                fieldName,
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
                "Custom_UserId",
                original.Keys
            );
        }

        [Fact]
        public void Empty_CustomContext_CopiesDictionaryEntries()
        {
            var customContext = new Dictionary<string, object>
            {
                ["UserId"] = 43,
                ["Trace"] = "original"
            };

            const string fieldName = "Code";

            var exception = DomainDataInconsistencyException.Empty<Error>(
                fieldName,
                context: customContext
            );

            customContext["UserId"] = 999;
            customContext["NewKey"] = "added";
            customContext.Remove("Trace");

            Assert.Equal(
                43,
                exception.Context["Custom_UserId"]
            );

            Assert.Equal(
                "original",
                exception.Context["Custom_Trace"]
            );

            Assert.False(exception.Context.ContainsKey("Custom_NewKey"));
        }

        [Fact]
        public void Empty_WithInnerException_PreservesReference()
        {
            var inner = new InvalidOperationException("root cause");

            const string fieldName = "Code";

            var exception = DomainDataInconsistencyException.Empty<Error>(
                fieldName,
                innerException: inner
            );

            Assert.Same(
                inner,
                exception.InnerException
            );
        }

        [Fact]
        public void Empty_SeparateCalls_CreateIndependentInstances()
        {
            var first = DomainDataInconsistencyException.Empty<Error>(
                fieldName: "Code",
                context: new Dictionary<string, object> { ["A"] = 1 }
            );

            var second = DomainDataInconsistencyException.Empty<Email>(
                fieldName: "Email",
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
                "Code",
                first.Context["Field"]
            );

            Assert.Equal(
                1,
                first.Context["Custom_A"]
            );

            Assert.Equal(
                "Email",
                second.Context["Field"]
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
