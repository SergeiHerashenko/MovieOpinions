using Authorization.Domain.Common.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.EmailUser;
using Authorization.Domain.Common.ValueObjects.LoginUser;

namespace Authorization.Domain.UnitTests.Common.ValueObjects.LoginUser
{
    public class EmailLoginTests
    {
        #region Restore: value and component validation
        [Theory]
        [InlineData("serhii@example.com", "serhii", "example.com")]
        [InlineData("a.b@sub.example.com", "a.b", "sub.example.com")]
        public void Restore_ValidEmail_SetsProperties(
            string address,
            string expectedLocal,
            string expectedDomain)
        {
            var login = EmailLogin.Restore(address);

            Assert.Equal(
                expectedLocal,
                login.Email.EmailLocalPart.Value
            );

            Assert.Equal(
                expectedDomain,
                login.Email.EmailDomainPart.Value
            );

            Assert.Equal(
                address,
                login.Value
            );

            Assert.Equal(
                LoginType.Email,
                login.Type
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\r\n")]
        public void Restore_EmptyEmail_PropagatesDomainDataInconsistencyException(string? storedEmail)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                EmailLogin.Restore(storedEmail!)
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
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "Email",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Email",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );
        }

        [Theory]
        [InlineData("example.com")]
        [InlineData("@example.com")]
        [InlineData("serhii@")]
        [InlineData("serhii@@example.com")]
        public void Restore_InvalidEmailStructure_PropagatesDomainDataInconsistencyException(string value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                EmailLogin.Restore(value)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.INVALID_FORMAT",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.DataInconsistency,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "Email",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Email",
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

        [Theory]
        [InlineData(" serhii@example.com ")]
        [InlineData("ser hii@example.com")]
        [InlineData("serhii@ example.com")]
        [InlineData("serhii@example.com\n")]
        public void Restore_Whitespace_PropagatesDomainDataInconsistencyException(string value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                EmailLogin.Restore(value)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.INVALID_FORMAT",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.DataInconsistency,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                "Email",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Email",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );
        }

        [Theory]
        [InlineData("SERHII@example.com", "EmailLocalPart")]
        [InlineData("serhii@EXAMPLE.COM", "EmailDomainPart")]
        public void Restore_NonCanonicalEmail_PropagatesDomainDataInconsistencyException(
            string value,
            string expectedType)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                EmailLogin.Restore(value)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.INVALID_FORMAT",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.DataInconsistency,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                expectedType,
                exception.Context["Type"]
            );

            Assert.Equal(
                "Value",
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
        public void Restore_InvalidLocalFormat_PropagatesDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                EmailLogin.Restore("a..b@example.com")
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.INVALID_FORMAT",
                exception.ExceptionCode
            );

            Assert.Equal(
                "EmailLocalPart",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Value",
                exception.Context["Field"]
            );

            Assert.Equal(
                "String",
                exception.Context["ValueType"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );
        }

        [Fact]
        public void Restore_TooShortLocalPart_PropagatesDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                EmailLogin.Restore("a@example.com")
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.OUT_OF_RANGE",
                exception.ExceptionCode
            );

            Assert.Equal(
                "EmailLocalPart",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Value",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Int32",
                exception.Context["ValueType"]
            );

            Assert.Equal(
                1,
                exception.Context["Custom_ActualLength"]
            );

            Assert.Equal(
                2,
                exception.Context["Custom_MinimumLength"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );
        }

        [Fact]
        public void Restore_TooLongEmail_PropagatesDomainDataInconsistencyException()
        {
            var address = new string('a', 243) + "@example.com";

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                EmailLogin.Restore(address)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.OUT_OF_RANGE",
                exception.ExceptionCode
            );

            Assert.Equal(
                "Email",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Email",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Int32",
                exception.Context["ValueType"]
            );

            Assert.Equal(
                255,
                exception.Context["Custom_ActualLength"]
            );

            Assert.Equal(
                254,
                exception.Context["Custom_MaximumLength"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );
        }

        [Theory]
        [InlineData("serhii@example")]
        [InlineData("serhii@-example.com")]
        public void Restore_InvalidDomainFormat_PropagatesDomainDataInconsistencyException(string value)
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                EmailLogin.Restore(value)
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.INVALID_FORMAT",
                exception.ExceptionCode
            );

            Assert.Equal(
                "EmailDomainPart",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Value",
                exception.Context["Field"]
            );

            Assert.Equal(
                "String",
                exception.Context["ValueType"]
            );

            Assert.Equal(
                "Restore",
                exception.Context["Operation"]
            );
        }
        #endregion

        #region Restore: stored business state
        // Відновлення логіна допускає історичну адресу із заблокованим доменом.
        // Login restoration accepts a historical address with a blocked domain.
        [Theory]
        [InlineData("serhii@mail.ru", "serhii", "mail.ru")]
        [InlineData("serhii@yandex.ru", "serhii", "yandex.ru")]
        public void Restore_BlockedDomain_PreservesEmailLogin(
            string address,
            string expectedLocal,
            string expectedDomain)
        {
            var login = EmailLogin.Restore(address);

            Assert.Equal(
                expectedLocal,
                login.Email.EmailLocalPart.Value
            );

            Assert.Equal(
                expectedDomain,
                login.Email.EmailDomainPart.Value
            );

            Assert.Equal(
                address,
                login.Value
            );

            Assert.Equal(
                LoginType.Email,
                login.Type
            );
        }
        #endregion

        #region Equality and hash code
        [Fact]
        public void GetEqualityComponents_ValidLogin_ReturnsSingleEmailComponent()
        {
            var login = EmailLogin.Restore("serhii@example.com");

            var component = Assert.Single(login.GetEqualityComponents());

            Assert.Same(
                login.Email,
                component
            );
        }

        [Theory]
        [InlineData("serhii@example.com", "petrenko@example.com")]
        [InlineData("serhii@example.com", "serhii@example.org")]
        public void Equals_DifferentEmails_ReturnsFalse(
            string firstAddress,
            string secondAddress)
        {
            var first = EmailLogin.Restore(firstAddress);
            var second = EmailLogin.Restore(secondAddress);

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }
        #endregion

        #region From and Restore
        [Fact]
        public void FromThenRestore_NormalizedEmail_PreservesEqualityAndHashCode()
        {
            var emailResult = Email.Create(" SeRhIi@ExAmPlE.CoM ");

            Assert.True(emailResult.IsSuccess);

            var created = Login.From(emailResult.Value);
            var restored = EmailLogin.Restore(created.Value);

            Assert.Equal(
                "serhii@example.com",
                created.Value
            );

            Assert.Equal(
                "serhii@example.com",
                restored.Value
            );

            Assert.Equal(
                LoginType.Email,
                created.Type
            );

            Assert.Equal(
                LoginType.Email,
                restored.Type
            );

            Assert.NotSame(
                created,
                restored
            );

            Assert.True(created.Equals((object)restored));
            Assert.True(restored.Equals((object)created));

            Assert.Equal(
                created.GetHashCode(),
                restored.GetHashCode()
            );
        }
        #endregion
    }
}
