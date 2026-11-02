using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.ValueObjects.EmailUser;

namespace Authorization.Domain.UnitTests.Common.Guard
{
    public partial class DomainGuardTests
    {
        [Fact]
        public void AgainstNull_NoFields_DoesNotThrow()
        {
            DomainGuard.AgainstNull<Error>(
                OperationType.Create
            );
        }

        [Theory]
        [InlineData("text")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\r\n")]
        [InlineData(0)]
        [InlineData(42)]
        [InlineData(false)]
        [InlineData(true)]
        public void AgainstNull_NonNullValue_DoesNotThrow(object value)
        {
            DomainGuard.AgainstNull<Error>(
                OperationType.Create,
                (value, "Field")
            );
        }

        [Fact]
        public void AgainstNull_AllFieldsNonNull_DoesNotThrow()
        {
            DomainGuard.AgainstNull<Error>(
                OperationType.Create,
                ("user@example.com", "Login"),
                (0, "Attempts"),
                (new object(), "State")
            );
        }

        [Fact]
        public void AgainstNull_NullField_ThrowsDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DomainGuard.AgainstNull<Error>(
                    OperationType.Create,
                    (null, "Login")
                )
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.EMPTY_VALUE",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.DataInconsistency,
                exception.ExceptionType
            );

            Assert.Equal(
                "Login",
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
        }

        [Fact]
        public void AgainstNull_NullFieldAfterValidFields_ReportsNullField()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DomainGuard.AgainstNull<Error>(
                    OperationType.Create,
                    ("user@example.com", "Login"),
                    (42, "Attempts"),
                    (null, "Password")
                )
            );

            Assert.Equal(
                "Password",
                exception.Context["Field"]
            );
        }

        [Fact]
        public void AgainstNull_MultipleNullFields_ReportsFirstNullField()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DomainGuard.AgainstNull<Error>(
                    OperationType.Create,
                    ("user@example.com", "Login"),
                    (null, "Password"),
                    (null, "RefreshToken")
                )
            );

            Assert.Equal(
                "Password",
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
        public void AgainstNull_ExplicitOperation_IsUsedInExceptionContext(OperationType operationType)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DomainGuard.AgainstNull<Error>(
                    operationType,
                    (null, "Login")
                )
            );

            Assert.Equal(
                operationType.ToString(),
                exception.Context["Operation"]
            );
        }

        [Fact]
        public void AgainstNull_OwnerType_IsUsedInExceptionContext()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DomainGuard.AgainstNull<Email>(
                    OperationType.Create,
                    (null, "Address")
                )
            );

            Assert.Equal("Email", exception.Context["Type"]);
        }

        [Fact]
        public void AgainstNull_NullableValueWithValue_DoesNotThrow()
        {
            int? attempts = 0;

            DomainGuard.AgainstNull<Error>(
                OperationType.Create,
                (attempts, "Attempts")
            );
        }

        [Fact]
        public void AgainstNull_NullableValueWithoutValue_ThrowsDomainDataInconsistencyException()
        {
            int? attempts = null;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                DomainGuard.AgainstNull<Error>(
                    OperationType.Create,
                    (attempts, "Attempts")
                )
            );

            Assert.Equal(
                "Attempts",
                exception.Context["Field"]
            );
        }
    }
}
