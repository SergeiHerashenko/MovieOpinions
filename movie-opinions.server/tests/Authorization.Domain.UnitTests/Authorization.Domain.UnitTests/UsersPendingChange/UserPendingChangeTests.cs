using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.DomainEvents.UserPendingChange;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.EmailUser;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersPendingChange;
using Authorization.Domain.UsersPendingChange.Changes;
using Authorization.Domain.UsersPendingChange.Enums;
using Authorization.Domain.UsersPendingChange.ValueObjects;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.UsersPendingChange
{
    public class UserPendingChangeTests
    {
        private readonly UserId _validUserId = UserId.CreateUnique();
        private readonly Login _validLogin = Login.From(Email.Create("test@gmail.com").Value);
        private readonly Password _validPassword = Password.Create("StrongPass123!").Value;
        private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

        #region Creation
        [Fact]
        public void Create_ShouldReturnSuccess_WhenDataIsValid()
        {
            var change = UserChange.From(_validLogin);
            var result = UserPendingChange.Create(_validUserId, change);

            result.IsSuccess.Should().BeTrue();
            var pending = result.Value;

            pending.UserId.Should().Be(_validUserId);
            pending.Change.Should().BeOfType<LoginChange>();
            pending.ConfirmationToken.Should().NotBeNull();
            pending.Status.Should().Be(ChangeStatus.Active);
            pending.ExpiresAt.Should().BeCloseTo(_now.AddMinutes(30), TimeSpan.FromSeconds(5));

            pending.DomainEvents.Should().ContainSingle(e => e is UserPendingChangeRequestedEvent);
        }

        [Fact]
        public void Create_ShouldReturnFailure_WhenUserIdIsNull()
        {
            var change = UserChange.From(_validLogin);
            var result = UserPendingChange.Create(null!, change);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "EMPTY_IDENTIFIER");
        }

        [Fact]
        public void Create_ShouldReturnFailure_WhenUserChangeIsNull()
        {
            var result = UserPendingChange.Create(_validUserId, null!);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "EMPTY_IDENTIFIER");
        }
        #endregion

        #region Restore
        [Fact]
        public void Restore_ShouldCreateInstance_WithValidState()
        {
            var id = UserPendingChangeId.CreateUnique();
            var token = ConfirmationToken.CreateUnique();
            var change = UserChange.From(_validLogin);
            var createdAt = _now.AddMinutes(-10);

            var pending = UserPendingChange.Restore(
                id, _validUserId, token, change,
                createdAt.AddMinutes(30), null, null, ChangeStatus.Active, createdAt);

            pending.Id.Should().Be(id);
            pending.UserId.Should().Be(_validUserId);
            pending.Status.Should().Be(ChangeStatus.Active);
        }

        [Fact]
        public void Restore_ShouldThrowException_WhenRequiredFieldIsNull()
        {
            var id = UserPendingChangeId.CreateUnique();
            var token = ConfirmationToken.CreateUnique();
            var change = UserChange.From(_validLogin);

            Action act = () => UserPendingChange.Restore(
                id, null!, token, change, _now.AddMinutes(30), null, null, ChangeStatus.Active, _now);

            act.Should().Throw<DomainDataInconsistencyException>();
        }

        [Fact]
        public void Restore_ShouldThrowException_WhenStateIsInvalid()
        {
            var id = UserPendingChangeId.CreateUnique();
            var token = ConfirmationToken.CreateUnique();
            var change = UserChange.From(_validLogin);

            Action act = () => UserPendingChange.Restore(
                id, _validUserId, token, change,
                _now.AddMinutes(30), null, _now, ChangeStatus.Active, _now);

            act.Should().Throw<DomainInvariantViolationException>();
        }
        #endregion

        #region ConfirmChange
        [Fact]
        public void ConfirmChange_ShouldReturnSuccess_WhenActive()
        {
            var pending = CreateValidPendingChange();
            var confirmTime = _now.AddMinutes(5);

            var result = pending.ConfirmChange(confirmTime);

            result.IsSuccess.Should().BeTrue();
            pending.Status.Should().Be(ChangeStatus.Confirmed);
            pending.ConfirmationTime.Should().Be(confirmTime);
        }

        [Fact]
        public void ConfirmChange_ShouldReturnFailure_WhenAlreadyConfirmed()
        {
            var pending = CreateValidPendingChange();
            pending.ConfirmChange(_now);

            var result = pending.ConfirmChange(_now.AddMinutes(1));

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "ALREADY_CONFIRMED");
        }

        [Fact]
        public void ConfirmChange_ShouldReturnFailure_WhenExpired()
        {
            var pending = CreateValidPendingChange();
            pending.MarkAsExpired(_now.AddMinutes(40));

            var result = pending.ConfirmChange(_now);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "OPERATION_IS_NOT_ALLOWED");
        }
        #endregion

        #region MarkAsExpired
        [Fact]
        public void MarkAsExpired_ShouldUpdateStatus_WhenTimeExceeded()
        {
            var pending = CreateValidPendingChange();
            var expiredTime = pending.ExpiresAt.AddMinutes(1);

            pending.MarkAsExpired(expiredTime);

            pending.Status.Should().Be(ChangeStatus.Expired);
            pending.ExpiredAt.Should().Be(expiredTime);
        }

        [Fact]
        public void MarkAsExpired_ShouldDoNothing_WhenStillActive()
        {
            var pending = CreateValidPendingChange();
            pending.MarkAsExpired(pending.ExpiresAt.AddMinutes(-5));

            pending.Status.Should().Be(ChangeStatus.Active);
        }
        #endregion

        #region Helpers
        private UserPendingChange CreateValidPendingChange()
        {
            var change = UserChange.From(_validLogin);
            var result = UserPendingChange.Create(_validUserId, change);
            return result.Value;
        }
        #endregion
    }
}
