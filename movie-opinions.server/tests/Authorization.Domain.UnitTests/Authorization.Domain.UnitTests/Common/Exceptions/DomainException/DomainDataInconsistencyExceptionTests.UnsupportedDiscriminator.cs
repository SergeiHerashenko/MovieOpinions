using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.EmailUser;

namespace Authorization.Domain.UnitTests.Common.Exceptions.DomainException
{
    public partial class DomainDataInconsistencyExceptionTests
    {
        [Fact]
        public void UnsupportedDiscriminator_DefaultArguments_SetsExceptionMetadata()
        {
            const string fieldName = "Code";
            const string discriminatorValue = "UnsupportedKind";

            var exception = DomainDataInconsistencyException.UnsupportedDiscriminator<Error>(
                fieldName: fieldName,
                discriminatorValue: discriminatorValue
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.UNSUPPORTED_TYPE",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.DataInconsistency,
                exception.ExceptionType
            );

            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void UnsupportedDiscriminator_DefaultArguments_BuildsContext()
        {
            const string fieldName = "Code";
            const string discriminatorValue = "UnsupportedKind";

            var exception = DomainDataInconsistencyException.UnsupportedDiscriminator<Error>(
                fieldName: fieldName,
                discriminatorValue: discriminatorValue
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
                "UnsupportedKind",
                exception.Context["Value"]
            );
        }

        [Fact]
        public void UnsupportedDiscriminator_DefaultArguments_GeneratesDefaultMessage()
        {
            const string expected =
                "An error occurred during the 'Restore' operation. Unsupported discriminator " +
                "value 'UnsupportedKind' for type 'Error'!";

            const string fieldName = "Code";
            const string discriminatorValue = "UnsupportedKind";

            var exception = DomainDataInconsistencyException.UnsupportedDiscriminator<Error>(
                fieldName: fieldName,
                discriminatorValue: discriminatorValue
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
        public void UnsupportedDiscriminator_ExplicitOperation_UsesSpecifiedOperation(OperationType operationType)
        {
            const string fieldName = "Code";
            const string discriminatorValue = "UnsupportedKind";

            var exception = DomainDataInconsistencyException.UnsupportedDiscriminator<Error>(
                fieldName: fieldName,
                discriminatorValue: discriminatorValue,
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
        [InlineData("Profile.Type")]
        public void UnsupportedDiscriminator_FieldName_IsUsedInContext(string fieldName)
        {
            const string discriminatorValue = "UnsupportedKind";

            var exception = DomainDataInconsistencyException.UnsupportedDiscriminator<Error>(
                fieldName: fieldName,
                discriminatorValue: discriminatorValue
            );

            Assert.Equal(
                fieldName,
                exception.Context["Field"]
            );
        }

        [Fact]
        public void UnsupportedDiscriminator_OwnerType_IsUsedInContextAndDefaultMessage()
        {
            const string fieldName = "Address";
            const string discriminatorValue = "UnsupportedKind";

            var exception = DomainDataInconsistencyException.UnsupportedDiscriminator<Email>(
                fieldName: fieldName,
                discriminatorValue: discriminatorValue
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
                fieldName,
                exception.Context["Field"]
            );
        }

        [Theory]
        [InlineData("UnsupportedKind")]
        [InlineData(42)]
        [InlineData(0)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData("")]
        [InlineData(3.14)]
        [InlineData((ErrorType)999)]
        public void UnsupportedDiscriminator_Value_IsPreservedAndIncludedInMessage(object discriminatorValue)
        {
            const string fieldName = "Code";

            var exception = DomainDataInconsistencyException.UnsupportedDiscriminator<Error>(
                fieldName: fieldName,
                discriminatorValue: discriminatorValue
            );

            Assert.Equal(
                discriminatorValue,
                exception.Context["Value"]
            );

            Assert.Contains(
                $"value '{discriminatorValue}'",
                exception.Message
            );
        }

        [Fact]
        public void UnsupportedDiscriminator_NullValue_UsesUnknown()
        {
            const string fieldName = "Code";

            var exception = DomainDataInconsistencyException.UnsupportedDiscriminator<Error>(
                fieldName: fieldName,
                discriminatorValue: null
            );

            Assert.Equal(
                "Unknown",
                exception.Context["Value"]
            );

            Assert.Contains(
                "value 'Unknown'",
                exception.Message
            );
        }

        [Fact]
        public void UnsupportedDiscriminator_ReferenceValue_PreservesReference()
        {
            var reference = new object();

            const string fieldName = "Code";

            var exception = DomainDataInconsistencyException.UnsupportedDiscriminator<Error>(
                fieldName: fieldName,
                discriminatorValue: reference
            );
            
            Assert.Same(
                reference,
                exception.Context["Value"]
            );
        }

        [Theory]
        [InlineData("Custom diagnostic message")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        public void UnsupportedDiscriminator_CustomMessage_IsPreserved(string customMessage)
        {
            const string fieldName = "Code";
            const string discriminatorValue = "UnsupportedKind";

            var exception = DomainDataInconsistencyException.UnsupportedDiscriminator<Error>(
                fieldName: fieldName,
                discriminatorValue: discriminatorValue,
                message: customMessage
            );

            Assert.Equal(
                customMessage,
                exception.Message
            );
        }

        [Fact]
        public void UnsupportedDiscriminator_EmptyContext_ContainsOnlyStandardFields()
        {
            const string fieldName = "Code";
            const string discriminatorValue = "UnsupportedKind";

            var exception = DomainDataInconsistencyException.UnsupportedDiscriminator<Error>(
                fieldName: fieldName,
                discriminatorValue: discriminatorValue,
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
                "Code",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "UnsupportedKind",
                exception.Context["Value"]
            );
        }

        [Fact]
        public void UnsupportedDiscriminator_CustomContext_PrefixesKeysAndPreservesValues()
        {
            var customContext = new Dictionary<string, object>
            {
                ["UserId"] = 42,
                ["Trace"] = "abc-123"
            };

            const string fieldName = "Code";
            const string discriminatorValue = "UnsupportedKind";

            var exception = DomainDataInconsistencyException.UnsupportedDiscriminator<Error>(
                fieldName: fieldName,
                discriminatorValue: discriminatorValue,
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
        public void UnsupportedDiscriminator_CustomContext_DoesNotOverrideStandardFields()
        {
            var customContext = new Dictionary<string, object>
            {
                ["Layer"] = "Infrastructure",
                ["Type"] = "ShouldNotAppear",
                ["Field"] = "ShouldNotAppear",
                ["Operation"] = "ShouldNotAppear",
                ["Value"] = "ShouldNotAppear"
            };

            const string fieldName = "Code";
            const string discriminatorValue = "UnsupportedKind";

            var exception = DomainDataInconsistencyException.UnsupportedDiscriminator<Error>(
                fieldName: fieldName,
                discriminatorValue: discriminatorValue,
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
                discriminatorValue,
                exception.Context["Value"]
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
                exception.Context["Custom_Value"]
            );
        }

        [Fact]
        public void UnsupportedDiscriminator_AlreadyPrefixedContextKey_AddsPrefixAgain()
        {
            var customContext = new Dictionary<string, object>
            {
                ["Custom_TraceId"] = "trace-001"
            };

            const string fieldName = "Code";
            const string discriminatorValue = "UnsupportedKind";

            var exception = DomainDataInconsistencyException.UnsupportedDiscriminator<Error>(
                fieldName: fieldName,
                discriminatorValue: discriminatorValue,
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
        public void UnsupportedDiscriminator_CustomContext_DoesNotModifyInputDictionary()
        {
            var original = new Dictionary<string, object>
            {
                ["UserId"] = 100,
                ["Source"] = "API"
            };

            var snapshot = new Dictionary<string, object>(original);

            const string fieldName = "Code";
            const string discriminatorValue = "UnsupportedKind";

            _ = DomainDataInconsistencyException.UnsupportedDiscriminator<Error>(
                fieldName: fieldName,
                discriminatorValue: discriminatorValue,
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
                "Value",
                original.Keys
            );

            Assert.DoesNotContain(
                "Custom_UserId",
                original.Keys
            );
        }

        [Fact]
        public void UnsupportedDiscriminator_CustomContext_CopiesDictionaryEntries()
        {
            var customContext = new Dictionary<string, object>
            {
                ["UserId"] = 42,
                ["Trace"] = "original"
            };

            const string fieldName = "Code";
            const string discriminatorValue = "UnsupportedKind";

            var exception = DomainDataInconsistencyException.UnsupportedDiscriminator<Error>(
                fieldName: fieldName,
                discriminatorValue: discriminatorValue,
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
        public void UnsupportedDiscriminator_WithInnerException_PreservesReference()
        {
            var inner = new InvalidOperationException("root cause");

            const string fieldName = "Code";
            const string discriminatorValue = "UnsupportedKind";

            var exception = DomainDataInconsistencyException.UnsupportedDiscriminator<Error>(
                fieldName: fieldName,
                discriminatorValue: discriminatorValue,
                innerException: inner
            );

            Assert.Same(
                inner,
                exception.InnerException
            );
        }

        [Fact]
        public void UnsupportedDiscriminator_SeparateCalls_CreateIndependentInstances()
        {
            const string fieldNameFirst = "Code";
            const string discriminatorValueFirst = "KindA";

            var first = DomainDataInconsistencyException.UnsupportedDiscriminator<Error>(
                fieldName: fieldNameFirst,
                discriminatorValue: discriminatorValueFirst,
                context: new Dictionary<string, object> { ["A"] = 1 }
             );

            const string fieldNameSecond = "Login";
            const string discriminatorValueSecond = "KindB";

            var second = DomainDataInconsistencyException.UnsupportedDiscriminator<Error>(
                fieldName: fieldNameSecond,
                discriminatorValue: discriminatorValueSecond,
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
                discriminatorValueFirst,
                first.Context["Value"]
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
                discriminatorValueSecond,
                second.Context["Value"]
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
