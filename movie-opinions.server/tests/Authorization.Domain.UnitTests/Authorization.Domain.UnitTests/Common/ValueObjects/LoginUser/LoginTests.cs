using Authorization.Domain.Common.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.EmailUser;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Common.ValueObjects.PhoneUser;

namespace Authorization.Domain.UnitTests.Common.ValueObjects.LoginUser
{
    public class LoginTests
    {
        [Theory]
        [InlineData("serhii@example.com")]
        [InlineData("a.b@sub.example.com")]
        public void From_ValidEmail_ReturnsEmailLogin(string address)
        {
            var email = Email.Restore(address);

            var login = Login.From(email);

            var emailLogin = Assert.IsType<EmailLogin>(login);

            Assert.Same(
                email,
                emailLogin.Email
            );

            Assert.Equal(
                LoginType.Email,
                login.Type
            );

            Assert.Equal(
                address,
                login.Value
            );
        }

        [Theory]
        [InlineData("+380", "981234567", "+380981234567")]
        [InlineData("+1", "2025550123", "+12025550123")]
        public void From_ValidPhone_ReturnsPhoneLogin(
            string code,
            string number,
            string expectedValue)
        {
            var phone = Phone.Restore(
                PhoneCountryCode.Restore(code),
                PhoneNationalNumber.Restore(number)
            );

            var login = Login.From(phone);

            var phoneLogin = Assert.IsType<PhoneLogin>(login);

            Assert.Same(
                phone,
                phoneLogin.Phone
            );

            Assert.Equal(
                LoginType.Phone,
                login.Type
            );

            Assert.Equal(
                expectedValue,
                login.Value
            );
        }

        [Fact]
        public void From_NullEmail_ThrowsDomainDataInconsistencyException()
        {
            Email? email = null;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                Login.From(email!)
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
                "Login",
                exception.Context["Type"]
            );

            Assert.Equal(
                "email",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Create",
                exception.Context["Operation"]
            );
        }

        [Fact]
        public void From_NullPhone_ThrowsDomainDataInconsistencyException()
        {
            Phone? phone = null;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                Login.From(phone!)
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
                "Login",
                exception.Context["Type"]
            );

            Assert.Equal(
                "phone",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Create",
                exception.Context["Operation"]
            );
        }

        [Fact]
        public void From_RestoredBlockedEmail_ReturnsEmailLogin()
        {
            var email = Email.Restore("serhii@mail.ru");

            var login = Login.From(email);

            var emailLogin = Assert.IsType<EmailLogin>(login);

            Assert.Same(
                email,
                emailLogin.Email
            );

            Assert.Equal(
                LoginType.Email,
                login.Type
            );


            Assert.Equal(
                "serhii@mail.ru",
                login.Value
            );
        }

        [Fact]
        public void From_RestoredBlockedPhone_ReturnsPhoneLogin()
        {
            var phone = Phone.Restore(
                PhoneCountryCode.Restore("+7"),
                PhoneNationalNumber.Restore("9012345678")
            );

            var login = Login.From(phone);

            var phoneLogin = Assert.IsType<PhoneLogin>(login);

            Assert.Same(
                phone,
                phoneLogin.Phone
            );

            Assert.Equal(
                LoginType.Phone,
                login.Type
            );

            Assert.Equal(
                "+79012345678",
                login.Value
            );
        }
    }
}
