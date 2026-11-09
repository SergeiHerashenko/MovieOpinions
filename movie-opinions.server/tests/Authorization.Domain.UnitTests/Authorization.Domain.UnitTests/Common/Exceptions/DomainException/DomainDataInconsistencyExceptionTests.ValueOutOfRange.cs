using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.EmailUser;

namespace Authorization.Domain.UnitTests.Common.Exceptions.DomainException
{
    public partial class DomainDataInconsistencyExceptionTests
    {
        [Fact]
        public void ValueOutOfRange_DefaultArguments_SetsExceptionMetadata()
        {
            const string fieldName = "Code";

            var exception = DomainDataInconsistencyException.ValueOutOfRange<Error>(
                fieldName: fieldName
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.OUT_OF_RANGE",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.DataInconsistency,
                exception.ExceptionType
            );

            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void ValueOutOfRange_DefaultArguments_BuildsContext()
        {
            const string fieldName = "Code";

            var exception = DomainDataInconsistencyException.ValueOutOfRange<Error>(
                fieldName: fieldName
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
                "null",
                exception.Context["ValueType"]
            );
        }

        [Fact]
        public void ValueOutOfRange_DefaultArguments_GeneratesDefaultMessage()
        {
            const string expected =
                "An error occurred during the 'Restore' operation. The field 'Code' " +
                "in type 'Error' has a value outside the allowed range!";

            const string fieldName = "Code";

            var exception = DomainDataInconsistencyException.ValueOutOfRange<Error>(
                fieldName: fieldName
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
        public void ValueOutOfRange_ExplicitOperation_UsesSpecifiedOperation(OperationType operationType)
        {
            const string fieldName = "Code";

            var exception = DomainDataInconsistencyException.ValueOutOfRange<Error>(
                fieldName: fieldName,
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
        [InlineData("Profile.Age")]
        public void ValueOutOfRange_FieldName_IsUsedInContextAndDefaultMessage(string fieldName)
        {
            var exception = DomainDataInconsistencyException.ValueOutOfRange<Error>(
                fieldName: fieldName
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
        public void ValueOutOfRange_OwnerType_IsUsedInContextAndDefaultMessage()
        {
            const string fieldName = "Address";

            var exception = DomainDataInconsistencyException.ValueOutOfRange<Email>(
                fieldName: fieldName
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
                fieldName,
                exception.Context["Field"]
            );
        }

        [Theory]
        [InlineData(null, "null")]
        [InlineData("", "String")]
        [InlineData("text", "String")]
        [InlineData(123, "Int32")]
        [InlineData(true, "Boolean")]
        [InlineData(3.14, "Double")]
        public void ValueOutOfRange_Value_SetsRuntimeTypeName(
            object? value,
            string expectedValueType)
        {
            const string fieldName = "Address";

            var exception = DomainDataInconsistencyException.ValueOutOfRange<Error>(
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
        public void ValueOutOfRange_CustomMessage_IsPreserved(string customMessage)
        {
            const string fieldName = "Code";

            var exception = DomainDataInconsistencyException.ValueOutOfRange<Error>(
                fieldName: fieldName,
                message: customMessage
            );

            Assert.Equal(
                customMessage,
                exception.Message
            );
        }

        [Fact]
        public void ValueOutOfRange_EmptyContext_ContainsOnlyStandardFields()
        {
            const string fieldName = "Code";
            const int value = 123;

            var exception = DomainDataInconsistencyException.ValueOutOfRange<Error>(
                fieldName: fieldName,
                value: value,
                context: new Dictionary<string, object>()
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
                fieldName,
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "Int32",
                exception.Context["ValueType"]
            );
        }

        [Fact]
        public void ValueOutOfRange_CustomContext_PrefixesKeysAndPreservesValues()
        {
            var customContext = new Dictionary<string, object>
            {
                ["UserId"] = 42,
                ["Trace"] = "abc-123"
            };

            const string fieldName = "Code";
            const int value = 123;

            var exception = DomainDataInconsistencyException.ValueOutOfRange<Error>(
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
        public void ValueOutOfRange_CustomContext_DoesNotOverrideStandardFields()
        {
            var customContext = new Dictionary<string, object>
            {
                ["Layer"] = "Infrastructure",
                ["Type"] = "ShouldNotAppear",
                ["Field"] = "ShouldNotAppear",
                ["Operation"] = "ShouldNotAppear",
                ["ValueType"] = "FakeType"
            };

            const string fieldName = "Code";
            const int value = 123;

            var exception = DomainDataInconsistencyException.ValueOutOfRange<Error>(
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
                "Int32",
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
                "FakeType",
                exception.Context["Custom_ValueType"]
            );
        }

        [Fact]
        public void ValueOutOfRange_AlreadyPrefixedContextKey_AddsPrefixAgain()
        {
            var customContext = new Dictionary<string, object>
            {
                ["Custom_TraceId"] = "trace-001"
            };

            const string fieldName = "Code";
            const int value = 123;

            var exception = DomainDataInconsistencyException.ValueOutOfRange<Error>(
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
        public void ValueOutOfRange_CustomContext_DoesNotModifyInputDictionary()
        {
            var original = new Dictionary<string, object>
            {
                ["UserId"] = 100,
                ["Source"] = "API"
            };

            var snapshot = new Dictionary<string, object>(original);

            const string fieldName = "Code";
            const int value = 123;

            _ = DomainDataInconsistencyException.ValueOutOfRange<Error>(
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
        public void ValueOutOfRange_CustomContext_CopiesDictionaryEntries()
        {
            var customContext = new Dictionary<string, object>
            {
                ["UserId"] = 42,
                ["Trace"] = "original"
            };

            const string fieldName = "Code";
            const int value = 123;

            var exception = DomainDataInconsistencyException.ValueOutOfRange<Error>(
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
        public void ValueOutOfRange_WithInnerException_PreservesReference()
        {
            var inner = new InvalidOperationException("root cause");

            const string fieldName = "Code";
            const int value = 123;

            var exception = DomainDataInconsistencyException.ValueOutOfRange<Error>(
                fieldName: fieldName,
                value: value,
                innerException: inner
            );

            Assert.Same(
                inner,
                exception.InnerException
            );
        }

        [Fact]
        public void ValueOutOfRange_SeparateCalls_CreateIndependentInstances()
        {
            const string fieldNameFirst = "Code";
            const int valueFirst = 123;

            var first = DomainDataInconsistencyException.ValueOutOfRange<Error>(
                fieldName: fieldNameFirst,
                value: valueFirst,
                context: new Dictionary<string, object> { ["A"] = 1 }
            );

            const string fieldNameSecond = "Login";
            const string valueSecond = "text";

            var second = DomainDataInconsistencyException.ValueOutOfRange<Error>(
                fieldName: fieldNameSecond,
                value: valueSecond,
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
                fieldNameFirst,
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
                fieldNameSecond,
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
    }
}
