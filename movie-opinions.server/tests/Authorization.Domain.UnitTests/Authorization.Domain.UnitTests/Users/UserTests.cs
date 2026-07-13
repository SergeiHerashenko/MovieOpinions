using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.DomainEvents.User;
using Authorization.Domain.Users;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.EmailUser;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersRefreshToken.ValueObjects;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.Users
{
    public class UserTests
    {
        private readonly Login _validLogin = Login.From(Email.Create("test@test.com").Value);
        private readonly Password _validPassword = Password.Create("StrongPass123!").Value;
        private readonly IpAddress _validIp = IpAddress.Create("192.168.1.1").Value;
        private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

        #region Create
        [Fact]
        public void Create_ShouldReturnSuccess_WhenDataIsValid()
        {
            var result = User.Create(_validLogin, _validPassword, Role.User, _validIp);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Login.Should().Be(_validLogin);
            result.Value.Password.Should().Be(_validPassword);
            result.Value.Role.Should().Be(Role.User);
            result.Value.IsLoginConfirmed.Should().BeTrue();
            result.Value.FailedLoginAttempts.Should().Be(0);
            result.Value.IsBlocked.Should().BeFalse();
            result.Value.IsDeleted.Should().BeFalse();

            result.Value.DomainEvents.Should().ContainSingle(e => e is UserRegisteredEvent);
        }

        [Fact]
        public void Create_ShouldReturnReturnFailure_WhenLoginIsNull()
        {
            var result = User.Create(null!, _validPassword, Role.User, _validIp);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Code == "LOGIN_EMPTY");
        }

        [Fact]
        public void Create_ShouldReturnFailure_WhenPasswordIsNull()
        {
            var result = User.Create(_validLogin, null!, Role.User, _validIp);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Code == "PASSWORD_EMPTY");
        }

        [Fact]
        public void Create_ShouldReturnFailure_WhenRoleIsNotUser()
        {
            var result = User.Create(_validLogin, _validPassword, Role.Admin, _validIp);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Code == "OPERATION_IS_NOT_ALLOWED");
        }
        #endregion

        #region Restore
        [Fact]
        public void Restore_ShouldCreateUser_WithAllFields()
        {
            var userId = UserId.CreateUnique();
            var createdAt = DateTimeOffset.UtcNow.AddDays(-1);

            var user = User.Restore(
                userId, createdAt, _validLogin, _validPassword, Role.User,
                _now, _now, _validIp, true, 2, true, false);

            user.Id.Should().Be(userId);
            user.CreatedAt.Should().Be(createdAt);
            user.Login.Should().Be(_validLogin);
            user.Password.Should().Be(_validPassword);
            user.Role.Should().Be(Role.User);
            user.UpdatedAt.Should().Be(_now);
            user.LastLoginAt.Should().Be(_now);
            user.LastLoginIp.Should().Be(_validIp);
            user.IsLoginConfirmed.Should().BeTrue();
            user.FailedLoginAttempts.Should().Be(2);
            user.IsBlocked.Should().BeTrue();
            user.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public void Restore_ShouldThrow_WhenRequiredFieldIsNull()
        {
            var userId = UserId.CreateUnique();

            Action act = () => User.Restore(userId, _now, null!, _validPassword, Role.User,
                null, null, null!, true, 0, false, false);

            act.Should().Throw<DomainDataInconsistencyException>();
        }

        [Fact]
        public void Restore_ShouldThrow_WhenRoleIsInvalid()
        {
            var userId = UserId.CreateUnique();

            Action act = () => User.Restore(userId, _now, _validLogin, _validPassword, (Role)999,
                null, null, null!, true, 0, false, false);

            act.Should().Throw<DomainDataInconsistencyException>();
        }
        #endregion

        #region ChangeLogin
        [Fact]
        public void ChangeLogin_ShouldResultSucceed_AndRaiseEvent()
        {
            var user = CreateValidUser();
            var newLogin = Login.From(Email.Create("newuser@gmail.com").Value);

            var result = user.ChangeLogin(newLogin, _now);

            result.IsSuccess.Should().BeTrue();
            user.Login.Should().Be(newLogin);
            user.IsLoginConfirmed.Should().BeFalse();
            user.UpdatedAt.Should().Be(_now);

            user.DomainEvents.Should().ContainSingle(e => e is UserLoginChangedEvent);
        }

        [Fact]
        public void ChangeLogin_ShouldReturnFailure_WhenUserIsBlocked()
        {
            var user = CreateBlockedUser();

            var result = user.ChangeLogin(Login.From(Email.Create("newuser@gmail.com").Value), _now);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "ACCOUNT_BLOCKED");
        }

        [Fact]
        public void ChangeLogin_ShouldReturnFailure_WhenUserIsDeleted()
        {
            var user = CreateDeletedUser();

            var result = user.ChangeLogin(Login.From(Email.Create("newuser@gmail.com").Value), _now);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "ACCOUNT_DELETED");
        }

        [Fact]
        public void ChangeLogin_ShouldReturnFailure_WhenNewLoginIsSame()
        {
            var user = CreateValidUser();

            var result = user.ChangeLogin(_validLogin, _now);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "NO_CHANGES_DETECTED");
        }

        [Fact]
        public void ChangeLogin_ShouldReturnFailure_WhenLoginIsNull()
        {
            var user = CreateValidUser();

            var result = user.ChangeLogin(null!, _now);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "LOGIN_EMPTY");
        }
        #endregion

        #region ChangePassword
        [Fact]
        public void ChangePassword_ShouldResultSucceed_AndRaiseEvent()
        {
            var user = CreateValidUser();
            var newPassword = Password.Create("newpass").Value;

            var result = user.ChangePassword(newPassword, _now);

            result.IsSuccess.Should().BeTrue();
            user.Password.Should().Be(newPassword);
            user.UpdatedAt.Should().Be(_now);

            user.DomainEvents.Should().ContainSingle(e => e is UserPasswordChangedEvent);
        }

        [Fact]
        public void ChangePassword_ShouldReturnFailure_WhenUserIsBlocked()
        {
            var user = CreateBlockedUser();

            var result = user.ChangePassword(Password.Create("test1").Value, _now);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "ACCOUNT_BLOCKED");
        }

        [Fact]
        public void ChangePassword_ShouldReturnFailure_WhenUserIsDeleted()
        {
            var user = CreateDeletedUser();

            var result = user.ChangePassword(Password.Create("test1").Value, _now);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "ACCOUNT_DELETED");
        }

        [Fact]
        public void ChangePassword_ShouldReturnFailure_WhenNewLoginIsSame()
        {
            var user = CreateValidUser();

            var result = user.ChangePassword(_validPassword, _now);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "NO_CHANGES_DETECTED");
        }

        [Fact]
        public void ChangePassword_ShouldReturnFailure_WhenLoginIsNull()
        {
            var user = CreateValidUser();

            var result = user.ChangePassword(null!, _now);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "PASSWORD_EMPTY");
        }
        #endregion

        #region ConfirmLogin
        [Fact]
        public void ConfirmLogin_ShouldReturnSucceed()
        {
            var user = CreateValidUser();
            user.ChangeLogin(Login.From(Email.Create("newuser@gmail.com").Value), _now);

            var result = user.ConfirmLogin(_now);

            result.IsSuccess.Should().BeTrue();
            user.IsLoginConfirmed.Should().BeTrue();
        }

        [Fact]
        public void ConfirmLogin_ShouldReturnFailure_WhenAlreadyConfirmed()
        {
            var user = CreateValidUser();

            var result = user.ConfirmLogin(_now);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "ALREADY_CONFIRMED");
        }

        [Fact]
        public void ConfirmLogin_ShouldReturnFailure_WhenUserIsDeleted()
        {
            var user = CreateDeletedUser();

            var result = user.ConfirmLogin(_now);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "ACCOUNT_DELETED");
        }

        [Fact]
        public void ConfirmLogin_ShouldReturnFailure_WhenUserIsBlocked()
        {
            var user = CreateBlockedUser();

            var result = user.ConfirmLogin(_now);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "ACCOUNT_BLOCKED");
        }
        #endregion

        #region Block/RemoveBlock
        [Fact]
        public void Block_ShouldReturnSucceed_AndRaiseEvent()
        {
            var user = CreateValidUser();

            var result = user.Block(_now, "Test reason");

            result.IsSuccess.Should().BeTrue();
            user.IsBlocked.Should().BeTrue();
            user.DomainEvents.Should().ContainSingle(e => e is UserBlockedEvent);
        }

        [Fact]
        public void Block_ShouldReturnSucceed_WhenUserIsBlocked()
        {
            var user = CreateBlockedUser();

            var result = user.Block(_now, "Test reason");

            result.IsSuccess.Should().BeTrue();
            user.IsBlocked.Should().BeTrue();
        }

        [Fact]
        public void RemoveBlock_ShouldReturnSucceed_AndRaiseEvent()
        {
            var user = CreateBlockedUser();

            var result = user.RemoveBlock(_now);

            result.IsSuccess.Should().BeTrue();
            user.IsBlocked.Should().BeFalse();
            user.DomainEvents.Should().ContainSingle(e => e is UserRemoveBlockedEvent);
        }

        [Fact]
        public void RemoveBlock_ShouldReturnSucceed_WhenUserIsUnBlocked()
        {
            var user = CreateValidUser();

            var result = user.RemoveBlock(_now);

            result.IsSuccess.Should().BeTrue();
            user.IsBlocked.Should().BeFalse();
        }
        #endregion

        #region Failed Login Attempts
        [Fact]
        public void RecordFailedLoginAttempt_ShouldIncrement_AndBlockAfterMax()
        {
            var user = CreateValidUser();

            for (int i = 0; i < 3; i++)
            {
                user.RecordFailedLoginAttempt(_now);
            }

            user.FailedLoginAttempts.Should().Be(3);
            user.IsBlocked.Should().BeTrue();
            user.DomainEvents.Should().Contain(e => e is UserBlockedEvent);
        }
        #endregion

        #region Delete / Undelete
        [Fact]
        public void Delete_ShouldReturnSucceed_AndRaiseEvent()
        {
            var user = CreateValidUser();

            var result = user.Delete(_now);

            result.IsSuccess.Should().BeTrue();
            user.IsDeleted.Should().BeTrue();
            user.DomainEvents.Should().ContainSingle(e => e is UserDeletedEvent);
        }

        [Fact]
        public void Undelete_ShouldSucceed_WhenWithinRestoreWindow()
        {
            var user = CreateDeletedUser();

            var result = user.Undelete(_now.AddDays(30), _now);

            result.IsSuccess.Should().BeTrue();
            user.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public void Undelete_ShouldFail_WhenRestoreWindowExpired()
        {
            var user = CreateDeletedUser();

            var result = user.Undelete(_now.AddDays(-1), _now);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "RESTORE_IS_NOT_ALLOWED");
        }
        #endregion

        #region LoginSuccess
        [Fact]
        public void LoginSuccess_ShouldResetAttempts_AndUpdateInfo()
        {
            var user = CreateValidUser();
            user.RecordFailedLoginAttempt(_now);

            var result = user.LoginSuccess(_validIp, _now);

            result.IsSuccess.Should().BeTrue();
            user.FailedLoginAttempts.Should().Be(0);
            user.LastLoginAt.Should().Be(_now);
            user.LastLoginIp.Should().Be(_validIp);
            user.DomainEvents.Should().ContainSingle(e => e is UserLoggedInEvent);
        }
        #endregion

        #region ProvideAccess Guard
        [Fact]
        public void Operations_ShouldFail_WhenUserIsBlocked()
        {
            var user = CreateBlockedUser();

            user.ChangeLogin(_validLogin, _now).IsFailure.Should().BeTrue();
            user.ChangePassword(_validPassword, _now).IsFailure.Should().BeTrue();
            user.LoginSuccess(_validIp, _now).IsFailure.Should().BeTrue();
        }

        [Fact]
        public void Operations_ShouldFail_WhenUserIsDeleted()
        {
            var user = CreateDeletedUser();

            user.ChangeLogin(_validLogin, _now).IsFailure.Should().BeTrue();
        }
        #endregion

        #region Helpers
        private User CreateValidUser()
        {
            var result = User.Create(_validLogin, _validPassword, Role.User, _validIp);
            return result.Value;
        }

        private User CreateBlockedUser()
        {
            var user = CreateValidUser();

            user.Block(_now, "Test");

            return user;
        }

        private User CreateDeletedUser()
        {
            var user = CreateValidUser();

            user.Delete(_now);

            return user;
        }
        #endregion
    }
}