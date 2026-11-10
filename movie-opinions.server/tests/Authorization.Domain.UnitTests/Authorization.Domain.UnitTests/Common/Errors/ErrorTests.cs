using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;

namespace Authorization.Domain.UnitTests.Common.Errors
{
    public class ErrorTests
    {
        #region Constructor
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\r\n")]
        public void Constructor_InvalidCode_ThrowsDomainDataInconsistencyException(string? code)
        {
            const string message = "Test error message";
            const ErrorType errorType = ErrorType.Validation;

            Action action = () => new Error(code!, message, errorType);

            var exception = Assert.Throws<DomainDataInconsistencyException>(action);

            Assert.Equal(
                "code",
                exception.Context["Field"]
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.EMPTY_VALUE",
                exception.ExceptionCode
            );

            Assert.Equal(
                "Create",
                exception.Context["Operation"]
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\r\n")]
        public void Constructor_InvalidMessage_ThrowsDomainDataInconsistencyException(string? message)
        {
            const string code = "TEST.ERROR";
            const ErrorType errorType = ErrorType.Validation;

            Action action = () => new Error(code, message!, errorType);

            var exception = Assert.Throws<DomainDataInconsistencyException>(action);

            Assert.Equal(
                "message",
                exception.Context["Field"]
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.EMPTY_VALUE",
                exception.ExceptionCode
            );

            Assert.Equal(
                "Create",
                exception.Context["Operation"]
            );
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        public void Constructor_UndefinedErrorType_ThrowsDomainDataInconsistencyException(
            int rawErrorType)
        {
            const string code = "TEST.ERROR";
            const string message = "Test error message";
            var errorType = (ErrorType)rawErrorType;

            Action action = () => new Error(code, message, errorType);

            var exception = Assert.Throws<DomainDataInconsistencyException>(action);

            Assert.Equal(
                "errorType",
                exception.Context["Field"]
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.UNSUPPORTED_TYPE",
                exception.ExceptionCode
            );

            Assert.Equal(
                "Create",
                exception.Context["Operation"]
            );
        }

        [Theory]
        [InlineData(ErrorType.Validation)]
        [InlineData(ErrorType.NotFound)]
        [InlineData(ErrorType.Conflict)]
        [InlineData(ErrorType.Forbidden)]
        [InlineData(ErrorType.BusinessRule)]
        public void Constructor_ValidArguments_SetsProperties(ErrorType errorType)
        {
            const string code = "TEST.ERROR";
            const string message = "Test error message";

            var error = new Error(code, message, errorType);

            Assert.Equal(
                code,
                error.Code
            );

            Assert.Equal(
                message,
                error.Message
            );

            Assert.Equal(
                errorType,
                error.ErrorType
            );
        }
        #endregion
    }
}
