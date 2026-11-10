using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.EmailUser;

namespace Authorization.Domain.UnitTests.Common.Exceptions.DomainException
{
    public partial class DomainDataInconsistencyExceptionTests
    {
        #region InvalidFieldFormat
        [Fact]
        public void InvalidFieldFormat_DefaultArguments_SetsExceptionMetadata()
        {
            const string fieldName = "Code";
            const string value = "invalid";

            var exception = DomainDataInconsistencyException.InvalidFieldFormat<Error>(
                fieldName: fieldName,
                value: value
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.INVALID_FORMAT",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.DataInconsistency,
                exception.ExceptionType
            );

            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void InvalidFieldFormat_DefaultArguments_BuildsContext()
        {
            const string fieldName = "Code";
            const string value = "invalid";

            var exception = DomainDataInconsistencyException.InvalidFieldFormat<Error>(
                fieldName: fieldName,
                value: value
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
                "Code",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "String",
                exception.Context["ValueType"]
            );
        }

        [Fact]
        public void InvalidFieldFormat_DefaultArguments_GeneratesDefaultMessage()
        {
            const string fieldName = "Code";
            const string value = "invalid";

            const string expected =
                "An error occurred during the 'Restore' operation. The field 'Code' in " +
                "type 'Error' has an invalid format!";

            var exception = DomainDataInconsistencyException.InvalidFieldFormat<Error>(
                fieldName: fieldName,
                value: value
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
        public void InvalidFieldFormat_ExplicitOperation_UsesSpecifiedOperation(OperationType operationType)
        {
            const string fieldName = "Code";
            const string value = "invalid";

            var exception = DomainDataInconsistencyException.InvalidFieldFormat<Error>(
                fieldName: fieldName,
                value: value,
                operationType: operationType
            );

            Assert.Equal(
                operationType.ToString(),
                exception.Context["Operation"]
            );

            Assert.Contains(
                $"'{operationType}'",
                exception.Message
            );
        }

        [Theory]
        [InlineData("Code")]
        [InlineData("Login")]
        [InlineData("Profile.Email")]
        public void InvalidFieldFormat_FieldName_IsUsedInContextAndDefaultMessage(string fieldName)
        {
            const string value = "invalid";

            var exception = DomainDataInconsistencyException.InvalidFieldFormat<Error>(
                fieldName: fieldName,
                value: value
            );

            Assert.Equal(
                fieldName,
                exception.Context["Field"]
            );

            Assert.Contains(
                $"The field '{fieldName}'",
                exception.Message
            );
        }

        [Fact]
        public void InvalidFieldFormat_OwnerType_IsUsedInContextAndDefaultMessage()
        {
            const string fieldName = "Address";
            const string value = "not-an-email";

            var exception = DomainDataInconsistencyException.InvalidFieldFormat<Email>(
                fieldName: fieldName,
                value: value
            );

            Assert.Equal(
                fieldName,
                exception.Context["Field"]
            );

            Assert.Equal(
                nameof(Email),
                exception.Context["Type"]
            );

            Assert.Contains(
                "in type 'Email'",
                exception.Message
            );

            Assert.Equal(
                "String",
                exception.Context["ValueType"]
            );
        }

        [Theory]
        [InlineData(null, "null")]
        [InlineData(42, "Int32")]
        [InlineData("text", "String")]
        [InlineData("", "String")]
        [InlineData(true, "Boolean")]
        [InlineData(3.14, "Double")]
        public void InvalidFieldFormat_Value_SetsRuntimeTypeName(
            object? value,
            string expectedValueType)
        {
            const string fieldName = "Code";

            var exception = DomainDataInconsistencyException.InvalidFieldFormat<Error>(
                fieldName: fieldName,
                value: value
            );

            Assert.Equal(
                expectedValueType,
                exception.Context["ValueType"]
            );
        }

        [Theory]
        [InlineData("Custom diagnostic message")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        public void InvalidFieldFormat_CustomMessage_IsPreserved(string customMessage)
        {
            const string fieldName = "Code";
            const string value = "invalid";

            var exception = DomainDataInconsistencyException.InvalidFieldFormat<Error>(
                fieldName: fieldName,
                value: value,
                message: customMessage
            );

            Assert.Equal(
                customMessage,
                exception.Message
            );
        }

        [Fact]
        public void InvalidFieldFormat_EmptyContext_ContainsOnlyStandardFields()
        {
            const string fieldName = "Code";
            const string value = "invalid";

            var exception = DomainDataInconsistencyException.InvalidFieldFormat<Error>(
            fieldName: fieldName,
            value: value,
            context: new Dictionary<string, object>());

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
                fieldName,
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "String",
                exception.Context["ValueType"]
            );
        }

        [Fact]
        public void InvalidFieldFormat_CustomContext_PrefixesKeysAndPreservesValues()
        {
            const string fieldName = "Code";
            const string value = "invalid";

            var customContext = new Dictionary<string, object>
            {
                ["UserId"] = 42,
                ["Trace"] = "abc-123"
            };

            var exception = DomainDataInconsistencyException.InvalidFieldFormat<Error>(
                fieldName: fieldName,
                value: value,
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
        public void InvalidFieldFormat_CustomContext_DoesNotOverrideStandardFields()
        {
            const string fieldName = "Code";
            const string value = "invalid";

            var customContext = new Dictionary<string, object>
            {
                ["Layer"] = "Infrastructure",
                ["Type"] = "ShouldNotAppear",
                ["Field"] = "ShouldNotAppear",
                ["Operation"] = "ShouldNotAppear",
                ["ValueType"] = "ShouldNotAppear"
            };

            var exception = DomainDataInconsistencyException.InvalidFieldFormat<Error>(
                fieldName: fieldName,
                value: value,
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
                fieldName,
                exception.Context["Field"]
            );

            Assert.Equal(
                "Create",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "String",
                exception.Context["ValueType"]
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

            Assert.Equal(
                "ShouldNotAppear",
                exception.Context["Custom_ValueType"]
            );
        }

        // Префікс додається знову навіть до вже префіксованого ключа.
        // The prefix is applied again even to an already-prefixed key.
        [Fact]
        public void InvalidFieldFormat_AlreadyPrefixedContextKey_AddsPrefixAgain()
        {
            const string fieldName = "Code";
            const string value = "invalid";

            var customContext = new Dictionary<string, object>
            {
                ["Custom_TraceId"] = "trace-001"
            };

            var exception = DomainDataInconsistencyException.InvalidFieldFormat<Error>(
                fieldName: fieldName,
                value: value,
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
        public void InvalidFieldFormat_CustomContext_DoesNotModifyInputDictionary()
        {
            const string fieldName = "Code";
            const string value = "invalid";

            var original = new Dictionary<string, object>
            {
                ["UserId"] = 100,
                ["Source"] = "API"
            };

            var snapshot = new Dictionary<string, object>(original);

            _ = DomainDataInconsistencyException.InvalidFieldFormat<Error>(
                fieldName: fieldName,
                value: value,
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
                "ValueType",
                original.Keys
            );

            Assert.DoesNotContain(
                "Custom_UserId",
                original.Keys
            );
        }

        [Fact]
        public void InvalidFieldFormat_CustomContext_CopiesDictionaryEntries()
        {
            const string fieldName = "Code";
            const string value = "invalid";

            var customContext = new Dictionary<string, object>
            {
                ["UserId"] = 42,
                ["Trace"] = "original"
            };

            var exception = DomainDataInconsistencyException.InvalidFieldFormat<Error>(
                fieldName: fieldName,
                value: value,
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
        public void InvalidFieldFormat_WithInnerException_PreservesReference()
        {
            const string fieldName = "Code";
            const string value = "invalid";

            var inner = new InvalidOperationException("root cause");

            var exception = DomainDataInconsistencyException.InvalidFieldFormat<Error>(
                fieldName: fieldName,
                value: value,
                innerException: inner
            );

            Assert.Same(inner, exception.InnerException);
        }

        [Fact]
        public void InvalidFieldFormat_SeparateCalls_CreateIndependentInstances()
        {
            var first = DomainDataInconsistencyException.InvalidFieldFormat<Error>(
                fieldName: "Code",
                value: 42,
                context: new Dictionary<string, object> { ["A"] = 1 }
            );

            var second = DomainDataInconsistencyException.InvalidFieldFormat<Error>(
                fieldName: "Login",
                value: "text",
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
                "Int32",
                first.Context["ValueType"]
            );

            Assert.Equal(
                1,
                first.Context["Custom_A"]
            );

            Assert.Equal(
                "Login",
                second.Context["Field"]
            );

            Assert.Equal(
                "String",
                second.Context["ValueType"]
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
