using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.ValueObjects.EmailUser;

namespace Authorization.Domain.UnitTests.Common.Guard
{
    public partial class DomainGuardTests
    {
        #region AgainstUndefinedEnum
        [Fact]
        public void AgainstUndefinedEnum_NoFields_DoesNotThrow()
        {
            DomainGuard.AgainstUndefinedEnum<Error>(
                OperationType.Create
            );
        }

        [Theory]
        [InlineData(ErrorType.Validation)]
        [InlineData(ErrorType.NotFound)]
        [InlineData(ErrorType.Conflict)]
        [InlineData(ErrorType.Forbidden)]
        [InlineData(ErrorType.BusinessRule)]
        public void AgainstUndefinedEnum_DefinedValue_DoesNotThrow(ErrorType errorType)
        {
            DomainGuard.AgainstUndefinedEnum<Error>(
                OperationType.Create,
                (errorType, "ErrorType")
            );
        }

        [Fact]
        public void AgainstUndefinedEnum_AllFieldsDefined_DoesNotThrow()
        {
            DomainGuard.AgainstUndefinedEnum<Error>(
                OperationType.Create,
                (ErrorType.Validation, "ErrorType"),
                (OperationType.Compare, "OperationType"),
                (ExceptionType.InvariantViolation, "ExceptionType")
            );
        }

        [Theory]
        [InlineData((ErrorType)(-1))]
        [InlineData((ErrorType)999)]
        [InlineData((OperationType)(-1))]
        [InlineData((OperationType)999)]
        [InlineData((ExceptionType)(-1))]
        [InlineData((ExceptionType)999)]
        public void AgainstUndefinedEnum_UndefinedValue_ThrowsDomainDataInconsistencyException(Enum value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DomainGuard.AgainstUndefinedEnum<Error>(
                    OperationType.Create,
                    (value, "Value")
                )
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.UNSUPPORTED_TYPE",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.DataInconsistency,
                exception.ExceptionType
            );

            Assert.Equal(
                "Value",
                exception.Context["Field"]
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
                value,
                exception.Context["Value"]
            );

            Assert.IsType(
                value.GetType(),
                exception.Context["Value"]
            );
        }

        [Fact]
        public void AgainstUndefinedEnum_UndefinedValueAfterDefinedFields_ReportsUndefinedField()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DomainGuard.AgainstUndefinedEnum<Error>(
                    OperationType.Create,
                    (ErrorType.Validation, "ErrorType"),
                    (OperationType.Compare, "OperationType"),
                    ((ExceptionType)999, "InvalidExceptionType")
                )
            );

            Assert.Equal(
                "InvalidExceptionType",
                exception.Context["Field"]
            );

            Assert.Equal(
                (ExceptionType)999,
                exception.Context["Value"]
            );
        }

        [Fact]
        public void AgainstUndefinedEnum_MultipleUndefinedFields_ReportsFirstUndefinedField()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DomainGuard.AgainstUndefinedEnum<Error>(
                    OperationType.Create,
                    (ErrorType.Validation, "ValidField"),
                    ((ErrorType)999, "FirstInvalid"),
                    ((OperationType)(-1), "SecondInvalid")
                )
            );

            Assert.Equal(
                "FirstInvalid",
                exception.Context["Field"]
            );

            Assert.Equal(
                (ErrorType)999,
                exception.Context["Value"]
            );
        }

        [Theory]
        [InlineData("ErrorType")]
        [InlineData("FailureCategory")]
        [InlineData("RequestedErrorType")]
        public void AgainstUndefinedEnum_FieldName_IsUsedInExceptionContext(string fieldName)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DomainGuard.AgainstUndefinedEnum<Error>(
                    OperationType.Create,
                    ((ErrorType)999, fieldName)
                )
            );

            Assert.Equal(
                fieldName,
                exception.Context["Field"]
            );
        }

        [Theory]
        [InlineData(OperationType.Restore)]
        [InlineData(OperationType.Create)]
        [InlineData(OperationType.Update)]
        [InlineData(OperationType.Delete)]
        [InlineData(OperationType.Read)]
        [InlineData(OperationType.Compare)]
        public void AgainstUndefinedEnum_ExplicitOperation_IsUsedInExceptionContext(OperationType operationType)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DomainGuard.AgainstUndefinedEnum<Error>(
                    operationType,
                    ((ErrorType)999, "Value")
                )
            );

            Assert.Equal(
                operationType.ToString(),
                exception.Context["Operation"]
            );
        }

        [Fact]
        public void AgainstUndefinedEnum_OwnerType_IsUsedInExceptionContext()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DomainGuard.AgainstUndefinedEnum<Email>(
                    OperationType.Create,
                    ((ErrorType)999, "Value")
                )
            );

            Assert.Equal(
                "Email",
                exception.Context["Type"]
            );
        }
        #endregion
    }
}
