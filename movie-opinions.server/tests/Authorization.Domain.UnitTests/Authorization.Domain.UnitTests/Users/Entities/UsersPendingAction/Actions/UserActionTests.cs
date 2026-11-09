using Authorization.Domain.Common.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.EmailUser;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Common.ValueObjects.PasswordUser;
using Authorization.Domain.Common.ValueObjects.PhoneUser;
using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;
using Authorization.Domain.Users.Entities.UsersPendingAction.Actions;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;

namespace Authorization.Domain.UnitTests.Users.Entities.UsersPendingAction.Actions
{
    public class UserActionTests
    {
        [Theory]
        [InlineData("TEST.HASH")]
        [InlineData("AbCdEf0123+/=")]
        [InlineData(" TEST.HASH ")]
        public void From_ValidPassword_ReturnsChangePasswordAction(string hashValue)
        {
            var password = Password.Create(PasswordHash.Create(hashValue));

            var action = UserAction.From(password);

            var changePassword = Assert.IsType<ChangePasswordAction>(action);

            Assert.Same(
                password,
                changePassword.NewPassword
            );

            Assert.Equal(
                UserActionType.ChangePassword,
                action.ActionType
            );

            Assert.Equal(
                hashValue,
                action.Value
            );
        }

        [Fact]
        public void From_NullPassword_ThrowsDomainDataInconsistencyException()
        {
            Password? password = null;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserAction.From(password!)
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
                "ChangePasswordAction",
                exception.Context["Type"]
            );

            Assert.Equal(
                "passwordHash",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Create",
                exception.Context["Operation"]
            );
        }

        [Theory]
        [InlineData("serhii@example.com")]
        [InlineData("a.b@sub.example.com")]
        public void From_ValidEmailLogin_ReturnsChangeLoginAction(string address)
        {
            var login = Login.From(Email.Restore(address));

            var action = UserAction.From(login);

            var changeLogin = Assert.IsType<ChangeLoginAction>(action);

            Assert.Same(
                login,
                changeLogin.NewLogin
            );

            Assert.Equal(
                UserActionType.ChangeLogin,
                action.ActionType
            );

            Assert.Equal(
                LoginType.Email,
                changeLogin.LoginType
            );

            Assert.Equal(
                address,
                action.Value
            );
        }

        [Theory]
        [InlineData("+380", "981234567", "+380981234567")]
        [InlineData("+1", "2025550123", "+12025550123")]
        public void From_ValidPhoneLogin_ReturnsChangeLoginAction(
            string code,
            string number,
            string expectedValue)
        {
            var login = Login.From(
                Phone.Restore(
                    PhoneCountryCode.Restore(code),
                    PhoneNationalNumber.Restore(number)
                )
            );

            var action = UserAction.From(login);

            var changeLogin = Assert.IsType<ChangeLoginAction>(action);

            Assert.Same(
                login,
                changeLogin.NewLogin
            );

            Assert.Equal(
                UserActionType.ChangeLogin,
                action.ActionType
            );

            Assert.Equal(
                LoginType.Phone,
                changeLogin.LoginType
            );

            Assert.Equal(
                expectedValue,
                action.Value
            );
        }

        [Fact]
        public void From_NullLogin_ThrowsDomainDataInconsistencyException()
        {
            Login? login = null;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserAction.From(login!)
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
                "ChangeLoginAction",
                exception.Context["Type"]
            );

            Assert.Equal(
                "login",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Create",
                exception.Context["Operation"]
            );
        }

        [Theory]
        [InlineData("email")]
        [InlineData("phone")]
        public void From_RestoredBlockedLogin_ReturnsChangeLoginAction(string kind)
        {
            Login login = kind == "email"
                ? Login.From(Email.Restore("serhii@mail.ru"))
                : Login.From(
                    Phone.Restore(
                        PhoneCountryCode.Restore("+7"),
                        PhoneNationalNumber.Restore("9012345678")
                    )
                );

            var expectedValue = kind == "email"
                ? "serhii@mail.ru"
                : "+79012345678";

            var expectedLoginType = kind == "email"
                ? LoginType.Email
                : LoginType.Phone;

            var action = UserAction.From(login);

            var changeLogin = Assert.IsType<ChangeLoginAction>(action);

            Assert.Same(
                login,
                changeLogin.NewLogin
            );

            Assert.Equal(
                UserActionType.ChangeLogin,
                action.ActionType
            );

            Assert.Equal(
                expectedLoginType,
                changeLogin.LoginType
            );

            Assert.Equal(
                expectedValue,
                action.Value
            );
        }

        [Theory]
        [InlineData("Account closed", "Account closed")]
        [InlineData(null, "No deletion reason was provided.")]
        public void From_ValidDeletionReason_ReturnsDeleteUserAction(
            string? rawReason,
            string expectedValue)
        {
            var reasonResult = DeletionReason.Create(rawReason);

            Assert.True(reasonResult.IsSuccess);

            var reason = reasonResult.Value;

            var action = UserAction.From(reason);

            var deleteUser = Assert.IsType<DeleteUserAction>(action);

            Assert.Same(
                reason,
                deleteUser.Reason
            );

            Assert.Equal(
                UserActionType.DeleteUser,
                action.ActionType
            );

            Assert.Equal(
                expectedValue,
                action.Value
            );
        }

        [Fact]
        public void From_NullDeletionReason_ThrowsDomainDataInconsistencyException()
        {
            DeletionReason? reason = null;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                UserAction.From(reason!)
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
                "DeleteUserAction",
                exception.Context["Type"]
            );

            Assert.Equal(
                "reason",
                exception.Context["Field"]
            );

            Assert.Equal(
                "Create",
                exception.Context["Operation"]
            );
        }
    }
}
