using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.DomainEvents.UserPendingRegistration;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.EmailUser;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersPendingRegistration;
using Authorization.Domain.UsersPendingRegistration.ValueObjects;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.UsersPendingRegistrations
{
    public class UserPendingRegistrationTests
    {
        private readonly Login _validLogin = Login.From(Email.Create("test@gmail.com").Value);
        private readonly Password _validPassword = Password.Create("StrongPass123!").Value;
        private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

        #region Creation
        [Fact]
        public void Create_ShouldReturnSucceed_WithValidData()
        {
            var result = UserPendingRegistration.Create(_validLogin, _validPassword);

            result.IsSuccess.Should().BeTrue();
            var pending = result.Value;

            pending.Should().NotBeNull();
            pending.UserId.Should().NotBeNull();
            pending.Login.Should().Be(_validLogin);
            pending.Password.Should().Be(_validPassword);
            pending.RegistrationToken.Should().NotBeNull();
            pending.ExpiresAt.Should().BeCloseTo(pending.CreatedAt.AddHours(12), precision: TimeSpan.FromSeconds(5));

            pending.DomainEvents.Should().ContainSingle(e => e is UserPendingRegistrationRequestedEvent);
        }

        [Fact]
        public void Create_ShouldReturnFailure_WhenLoginIsNull()
        {
            var result = UserPendingRegistration.Create(null!, _validPassword);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Code == "LOGIN_EMPTY");
        }

        [Fact]
        public void Create_ShouldReturnFailure_WhenPasswordIsNull()
        {
            var result = UserPendingRegistration.Create(_validLogin, null!);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Code == "PASSWORD_EMPTY");
        }
        #endregion

        #region Restore
        [Fact]
        public void Restore_ShouldCreateCorrectInstance()
        {
            var id = UserPendingRegistrationId.CreateUnique();
            var userId = UserId.CreateUnique();
            var token = RegistrationToken.CreateUnique();
            var createdAt = _now.AddHours(-5);
            var expiresAt = createdAt.AddHours(12);

            var pending = UserPendingRegistration.Restore(id, userId, _validLogin, _validPassword, token, createdAt, expiresAt);

            pending.Id.Should().Be(id);
            pending.UserId.Should().Be(userId);
            pending.Login.Should().Be(_validLogin);
            pending.Password.Should().Be(_validPassword);
            pending.RegistrationToken.Should().Be(token);
            pending.CreatedAt.Should().Be(createdAt);
            pending.ExpiresAt.Should().Be(expiresAt);
        }

        [Fact]
        public void Restore_ShouldReturnThrow_WhenRequiredFieldIsNull()
        {
            var id = UserPendingRegistrationId.CreateUnique();
            var userId = UserId.CreateUnique();
            var token = RegistrationToken.CreateUnique();

            Action act = () => UserPendingRegistration.Restore(id, userId, null!, _validPassword, token, _now, _now.AddHours(12));

            act.Should().Throw<DomainDataInconsistencyException>();
        }
        #endregion

        #region Refresh
        [Fact]
        public void Refresh_ShouldUpdatePasswordTokenAndExpiration_AndRaiseEvent()
        {
            var pending = CreateValidPendingRegistration();
            var newPassword = Password.Create("NewStrongPass456!").Value;
            var refreshTime = _now.AddHours(6);

            var result = pending.Refresh(newPassword, refreshTime);

            result.IsSuccess.Should().BeTrue();

            pending.Password.Should().Be(newPassword);
            pending.RegistrationToken.Should().NotBeNull();
            pending.ExpiresAt.Should().BeCloseTo(refreshTime.AddHours(12), precision: TimeSpan.FromSeconds(5));

            pending.DomainEvents.Should().Contain(e => e is UserPendingRegistrationRequestedEvent);
        }

        [Fact]
        public void Refresh_ShouldFail_WhenPasswordIsNull()
        {
            var pending = CreateValidPendingRegistration();

            var result = pending.Refresh(null!, _now);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Code == "PASSWORD_EMPTY");
        }
        #endregion

        #region IsExpired
        [Fact]
        public void IsExpired_ShouldReturnTrue_WhenPastExpiration()
        {
            var pending = CreateValidPendingRegistration();
            var futureTime = pending.ExpiresAt.AddMinutes(1);

            pending.IsExpired(futureTime).Should().BeTrue();
        }

        [Fact]
        public void IsExpired_ShouldReturnFalse_WhenNotYetExpired()
        {
            var pending = CreateValidPendingRegistration();
            var beforeTime = pending.ExpiresAt.AddMinutes(-1);

            pending.IsExpired(beforeTime).Should().BeFalse();
        }

        [Fact]
        public void IsExpired_ShouldReturnFalse_ExactlyAtExpiration()
        {
            var pending = CreateValidPendingRegistration();

            pending.IsExpired(pending.ExpiresAt).Should().BeFalse();
        }
        #endregion

        #region Helper
        private UserPendingRegistration CreateValidPendingRegistration()
        {
            var result = UserPendingRegistration.Create(_validLogin, _validPassword);
            return result.Value;
        }
        #endregion
    }
}
