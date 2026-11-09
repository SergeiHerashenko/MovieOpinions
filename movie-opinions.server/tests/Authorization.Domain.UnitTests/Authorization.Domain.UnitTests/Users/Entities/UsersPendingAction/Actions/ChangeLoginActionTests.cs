using Authorization.Domain.Common.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.EmailUser;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Common.ValueObjects.PhoneUser;
using Authorization.Domain.Users.Entities.UsersPendingAction.Actions;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;

namespace Authorization.Domain.UnitTests.Users.Entities.UsersPendingAction.Actions
{
    public class ChangeLoginActionTests
    {
        private static Login CreateEmailLogin(string address = "serhii@example.com")
            => Login.From(Email.Restore(address));

        private static Login CreatePhoneLogin(
            string code = "+380",
            string number = "981234567")
            => Login.From(
                Phone.Restore(
                    PhoneCountryCode.Restore(code),
                    PhoneNationalNumber.Restore(number)
                )
            );

        private static void AssertEmptyField(
            DomainDataInconsistencyException exception,
            string field,
            string operation)
        {
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
                "ChangeLoginAction",
                exception.Context["Type"]
            );

            Assert.Equal(
                field,
                exception.Context["Field"]
            );

            Assert.Equal(
                operation,
                exception.Context["Operation"]
            );
        }

        [Theory]
        [InlineData(true, "serhii@example.com", null, null, "serhii@example.com")]
        [InlineData(false, null, "+380", "981234567", "+380981234567")]
        public void Create_ValidLogin_PreservesLoginAndSetsProperties(
            bool useEmail,
            string? address,
            string? code,
            string? number,
            string expectedValue)
        {
            var login = useEmail
                ? CreateEmailLogin(address!)
                : CreatePhoneLogin(code!, number!);

            var action = ChangeLoginAction.Create(login);

            Assert.Same(
                login,
                action.NewLogin
            );

            Assert.Equal(
                UserActionType.ChangeLogin,
                action.ActionType
            );

            Assert.Equal(
                useEmail ? LoginType.Email : LoginType.Phone,
                action.LoginType
            );

            Assert.Equal(
                expectedValue,
                action.Value
            );
        }

        [Fact]
        public void Create_NullLogin_ThrowsDomainDataInconsistencyException()
        {
            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                ChangeLoginAction.Create(null!)
            );

            AssertEmptyField(
                exception,
                "login",
                "Create"
            );
        }

        [Theory]
        [InlineData("email", "serhii@mail.ru", null, null, "serhii@mail.ru")]
        [InlineData("phone", null, "+7", "9012345678", "+79012345678")]
        public void Create_RestoredBlockedLogin_PreservesLogin(
            string kind,
            string? address,
            string? code,
            string? number,
            string expectedValue)
        {
            var login = kind == "email"
                ? CreateEmailLogin(address!)
                : CreatePhoneLogin(code!, number!);

            var action = ChangeLoginAction.Create(login);

            Assert.Same(
                login,
                action.NewLogin
            );

            Assert.Equal(
                UserActionType.ChangeLogin,
                action.ActionType
            );

            Assert.Equal(
                kind == "email" ? LoginType.Email : LoginType.Phone,
                action.LoginType
            );

            Assert.Equal(
                expectedValue,
                action.Value
            );
        }

        [Theory]
        [InlineData("serhii@example.com")]
        [InlineData("a.b@sub.example.com")]
        public void Restore_ValidEmail_CreatesEmailLoginAndSetsProperties(string address)
        {
            var email = Email.Restore(address);

            var action = ChangeLoginAction.Restore(email);

            var emailLogin = Assert.IsType<EmailLogin>(action.NewLogin);

            Assert.Same(
                email,
                emailLogin.Email
            );

            Assert.Equal(
                UserActionType.ChangeLogin,
                action.ActionType
            );

            Assert.Equal(
                LoginType.Email,
                action.LoginType
            );

            Assert.Equal(
                address,
                action.Value
            );
        }

        [Fact]
        public void Restore_NullEmail_ThrowsDomainDataInconsistencyException()
        {
            Email? email = null;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                ChangeLoginAction.Restore(email!)
            );

            AssertEmptyField(
                exception,
                "email",
                "Restore"
            );
        }

        [Fact]
        public void Restore_RestoredBlockedEmail_PreservesEmail()
        {
            var email = Email.Restore("serhii@mail.ru");

            var action = ChangeLoginAction.Restore(email);

            var emailLogin = Assert.IsType<EmailLogin>(action.NewLogin);

            Assert.Same(
                email,
                emailLogin.Email
            );

            Assert.Equal(
                UserActionType.ChangeLogin,
                action.ActionType
            );

            Assert.Equal(
                LoginType.Email,
                action.LoginType
            );

            Assert.Equal(
                "serhii@mail.ru",
                action.Value
            );
        }

        [Theory]
        [InlineData("+380", "981234567", "+380981234567")]
        [InlineData("+1", "2025550123", "+12025550123")]
        public void Restore_ValidPhone_CreatesPhoneLoginAndSetsProperties(
            string code,
            string number,
            string expectedValue)
        {
            var phone = Phone.Restore(
                PhoneCountryCode.Restore(code),
                PhoneNationalNumber.Restore(number)
            );

            var action = ChangeLoginAction.Restore(phone);

            var phoneLogin = Assert.IsType<PhoneLogin>(action.NewLogin);

            Assert.Same(
                phone,
                phoneLogin.Phone
            );

            Assert.Equal(
                UserActionType.ChangeLogin,
                action.ActionType
            );

            Assert.Equal(
                LoginType.Phone,
                action.LoginType
            );

            Assert.Equal(
                expectedValue,
                action.Value
            );
        }

        [Fact]
        public void Restore_NullPhone_ThrowsDomainDataInconsistencyException()
        {
            Phone? phone = null;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                ChangeLoginAction.Restore(phone!)
            );

            AssertEmptyField(
                exception,
                "phone",
                "Restore"
            );
        }

        [Fact]
        public void Restore_RestoredBlockedPhone_PreservesPhone()
        {
            var phone = Phone.Restore(
                PhoneCountryCode.Restore("+7"),
                PhoneNationalNumber.Restore("9012345678")
            );

            var action = ChangeLoginAction.Restore(phone);

            var phoneLogin = Assert.IsType<PhoneLogin>(action.NewLogin);

            Assert.Same(
                phone,
                phoneLogin.Phone
            );

            Assert.Equal(
                UserActionType.ChangeLogin,
                action.ActionType
            );

            Assert.Equal(
                LoginType.Phone,
                action.LoginType
            );

            Assert.Equal(
                "+79012345678",
                action.Value
            );
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetEqualityComponents_ValidAction_ReturnsSingleLoginComponent(bool useEmail)
        {
            var login = useEmail ? CreateEmailLogin() : CreatePhoneLogin();
            var action = ChangeLoginAction.Create(login);

            var component = Assert.Single(action.GetEqualityComponents());

            Assert.Same(
                login,
                component
            );
        }

        [Fact]
        public void CreateThenRestore_EquivalentEmail_PreservesEqualityAndHashCode()
        {
            var firstEmail = Email.Restore("serhii@example.com");
            var secondEmail = Email.Restore("serhii@example.com");

            var created = ChangeLoginAction.Create(Login.From(firstEmail));
            var restored = ChangeLoginAction.Restore(secondEmail);

            Assert.NotSame(
                created,
                restored
            );

            Assert.NotSame(
                created.NewLogin,
                restored.NewLogin
            );

            Assert.Equal(
                created.Value,
                restored.Value
            );

            Assert.True(created.Equals((object)restored));
            Assert.True(restored.Equals((object)created));

            Assert.Equal(
                created.GetHashCode(),
                restored.GetHashCode()
            );
        }

        [Fact]
        public void CreateThenRestore_EquivalentPhone_PreservesEqualityAndHashCode()
        {
            var firstPhone = Phone.Restore(
                PhoneCountryCode.Restore("+380"),
                PhoneNationalNumber.Restore("981234567")
            );
            var secondPhone = Phone.Restore(
                PhoneCountryCode.Restore("+380"),
                PhoneNationalNumber.Restore("981234567")
            );

            var created = ChangeLoginAction.Create(Login.From(firstPhone));
            var restored = ChangeLoginAction.Restore(secondPhone);

            Assert.NotSame(
                created,
                restored
            );

            Assert.NotSame(
                created.NewLogin,
                restored.NewLogin
            );

            Assert.Equal(
                created.Value,
                restored.Value
            );

            Assert.True(created.Equals((object)restored));
            Assert.True(restored.Equals((object)created));

            Assert.Equal(
                created.GetHashCode(),
                restored.GetHashCode()
            );
        }

        [Theory]
        [InlineData("serhii@example.com", "other@example.com")]
        [InlineData("serhii@example.com", "serhii@example.org")]
        public void Equals_DifferentEmailValues_ReturnsFalse(
            string firstAddress,
            string secondAddress)
        {
            var first = ChangeLoginAction.Create(CreateEmailLogin(firstAddress));
            var second = ChangeLoginAction.Create(CreateEmailLogin(secondAddress));

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }

        [Theory]
        [InlineData("+380", "981234567", "+380", "991234567")]
        [InlineData("+380", "981234567", "+1", "2025550123")]
        public void Equals_DifferentPhoneValues_ReturnsFalse(
            string firstCode,
            string firstNumber,
            string secondCode,
            string secondNumber)
        {
            var first = ChangeLoginAction.Create(
                CreatePhoneLogin(firstCode, firstNumber)
            );

            var second = ChangeLoginAction.Create(
                CreatePhoneLogin(secondCode, secondNumber)
            );

            Assert.False(first.Equals((object)second));
            Assert.False(second.Equals((object)first));
        }

        [Fact]
        public void Equals_EmailAndPhoneActions_ReturnsFalse()
        {
            var emailAction = ChangeLoginAction.Create(CreateEmailLogin());
            var phoneAction = ChangeLoginAction.Create(CreatePhoneLogin());

            Assert.False(emailAction.Equals((object)phoneAction));
            Assert.False(phoneAction.Equals((object)emailAction));
        }
    }
}
