using Authorization.Domain.Common.Exceptions;
using Authorization.Domain.Common.Exceptions.Enums;

namespace Authorization.Domain.UnitTests.Common.Exceptions
{
    public class BaseExceptionTests
    {
        private sealed class TestException : BaseException
        {
            public TestException(
                string exceptionCode,
                ExceptionType exceptionType,
                string message,
                IReadOnlyDictionary<string, object>? context = null,
                Exception? innerException = null)
                : base(
                    exceptionCode,
                    exceptionType,
                    message,
                    context,
                    innerException)
            {
            }
        }

        [Theory]
        [InlineData(ExceptionType.DataInconsistency)]
        [InlineData(ExceptionType.InvalidOperation)]
        [InlineData(ExceptionType.InvariantViolation)]
        public void Constructor_ValidArguments_SetsProperties(ExceptionType exceptionType)
        {
            const string exceptionCode = "TEST.EXCEPTION";
            const string message = "Test exception message";

            var exception = new TestException(
                exceptionCode: exceptionCode,
                exceptionType: exceptionType,
                message: message
            );

            Assert.Equal(
                exceptionCode,
                exception.ExceptionCode
            );

            Assert.Equal(
                exceptionType,
                exception.ExceptionType
            );

            Assert.Equal(
                message,
                exception.Message
            );
        }

        [Fact]
        public void Constructor_WithoutContext_CreatesEmptyContext()
        {
            const string exceptionCode = "TEST.EXCEPTION";
            const string message = "Test exception message";

            var exception = new TestException(
                exceptionCode: exceptionCode,
                exceptionType: ExceptionType.DataInconsistency,
                message: message
            );

            Assert.NotNull(exception.Context);

            Assert.Empty(exception.Context);
        }

        [Fact]
        public void Constructor_WithContext_PreservesReference()
        {
            const string exceptionCode = "TEST.EXCEPTION";
            const string message = "Test exception message";

            var context = new Dictionary<string, object>
            {
                ["UserId"] = 42,
                ["Source"] = "API"
            };

            var exception = new TestException(
                exceptionCode: exceptionCode,
                exceptionType: ExceptionType.DataInconsistency,
                message: message,
                context: context
            );

            Assert.Same(
                context,
                exception.Context
            );

            Assert.Equal(
                42,
                exception.Context["UserId"]
            );

            Assert.Equal(
                "API",
                exception.Context["Source"]
            );
        }

        [Fact]
        public void Constructor_WithoutInnerException_SetsNullInnerException()
        {
            const string exceptionCode = "TEST.EXCEPTION";
            const string message = "Test exception message";

            var exception = new TestException(
                exceptionCode: exceptionCode,
                exceptionType: ExceptionType.DataInconsistency,
                message: message
            );

            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void Constructor_WithInnerException_PreservesReference()
        {
            const string exceptionCode = "TEST.EXCEPTION";
            const string message = "Test exception message";

            var innerException = new InvalidOperationException("root cause");

            var exception = new TestException(
                exceptionCode: exceptionCode,
                exceptionType: ExceptionType.DataInconsistency,
                message: message,
                innerException: innerException
            );

            Assert.Same(
                innerException,
                exception.InnerException
            );
        }

        [Fact]
        public void Constructor_SeparateCallsWithoutContext_CreateIndependentContexts()
        {
            const string exceptionCode = "TEST.EXCEPTION";
            const string message = "Test exception message";

            var first = new TestException(
                exceptionCode: exceptionCode,
                exceptionType: ExceptionType.DataInconsistency,
                message: message
            );

            var second = new TestException(
                exceptionCode: exceptionCode,
                exceptionType: ExceptionType.InvalidOperation,
                message: message
            );

            Assert.Empty(first.Context);

            Assert.Empty(second.Context);

            Assert.NotSame(
                first.Context,
                second.Context
            );
        }

        [Theory]
        [InlineData("Test exception message")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\r\n")]
        public void Constructor_Message_IsPreserved(string message)
        {
            const string exceptionCode = "TEST.EXCEPTION";

            var exception = new TestException(
                exceptionCode: exceptionCode,
                exceptionType: ExceptionType.DataInconsistency,
                message: message
            );

            Assert.Equal(
                message,
                exception.Message
            );
        }
    }
}
