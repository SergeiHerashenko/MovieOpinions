using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.DomainEvents.UserDeletion;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.EmailUser;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersDeletion;
using Authorization.Domain.UsersDeletion.Enums;
using Authorization.Domain.UsersDeletion.ValueObjects;
using FluentAssertions;

namespace Authorization.Domain.UnitTests.UsersDeletion
{
    public class UserDeletionTests
    {
        private readonly UserId _validUserId = UserId.CreateUnique();
        private readonly Login _validLogin = Login.From(Email.Create("test@gmail.com").Value);
        private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;
        private readonly string _reason = "Requested by user";

        #region Creation
        [Fact]
        public void Create_ShouldReturnSuccess_WhenDataIsValid()
        {
            var result = UserDeletion.Create(_validUserId, _validLogin, _reason, _now);

            result.IsSuccess.Should().BeTrue();
            var deletion = result.Value;

            deletion.UserId.Should().Be(_validUserId);
            deletion.Login.Should().Be(_validLogin);
            deletion.Reason.Should().Be(_reason);
            deletion.Status.Should().Be(DeletionStatus.Deleted);
            deletion.RestoreUntil.Should().BeCloseTo(_now.AddDays(30), precision: TimeSpan.FromSeconds(5));
            deletion.DeletedAt.Should().Be(_now);

            deletion.DomainEvents.Should().ContainSingle(e => e is UserAccountDeletionRequestedEvent);
        }

        [Fact]
        public void Create_ShouldReturnFailure_WhenUserIdIsNull()
        {
            var result = UserDeletion.Create(null!, _validLogin, _reason, _now);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "EMPTY_IDENTIFIER");
        }

        [Fact]
        public void Create_ShouldReturnFailure_WhenLoginIsNull()
        {
            var result = UserDeletion.Create(_validUserId, null!, _reason, _now);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "LOGIN_EMPTY");
        }
        #endregion

        #region Restore
        [Fact]
        public void Restore_ShouldReturnInstance_WithAllFields()
        {
            var id = UserDeletionId.CreateUnique();
            var createdAt = _now.AddDays(-10);

            var deletion = UserDeletion.Restore(
                id, _validUserId, _validLogin, _reason,
                _now, createdAt, _now.AddDays(30), null,
                DeletionStatus.Deleted, null);

            deletion.Id.Should().Be(id);
            deletion.UserId.Should().Be(_validUserId);
            deletion.Login.Should().Be(_validLogin);
            deletion.Status.Should().Be(DeletionStatus.Deleted);
        }

        [Fact]
        public void Restore_ShouldThrowException_WhenRequiredFieldIsNull()
        {
            var id = UserDeletionId.CreateUnique();

            Action act = () => UserDeletion.Restore(
                id, null!, _validLogin, _reason, _now, _now, _now.AddDays(30), null, DeletionStatus.Deleted, null);

            act.Should().Throw<DomainDataInconsistencyException>();
        }
        #endregion

        #region Undelete
        [Fact]
        public void Undelete_ShouldReturnSuccess_WhenWithinRestorePeriod()
        {
            var deletion = CreateValidUserDeletion();
            var restoreTime = _now.AddDays(15);

            var result = deletion.Undelete(restoreTime);

            result.IsSuccess.Should().BeTrue();
            deletion.Status.Should().Be(DeletionStatus.Restored);
            deletion.RestoredAt.Should().Be(restoreTime);
            deletion.UpdatedAt.Should().Be(restoreTime);
        }

        [Fact]
        public void Undelete_ShouldReturnFailure_WhenAlreadyRestored()
        {
            var deletion = CreateValidUserDeletion();
            deletion.Undelete(_now.AddDays(10));

            var result = deletion.Undelete(_now.AddDays(15));

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "ALREADY_RESTORED");
        }

        [Fact]
        public void Undelete_ShouldReturnFailure_WhenRestorePeriodExpired()
        {
            var deletion = CreateValidUserDeletion();
            var expiredTime = deletion.RestoreUntil.AddMinutes(1);

            var result = deletion.Undelete(expiredTime);

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().Contain(e => e.Code == "RESTORE_IS_NOT_ALLOWED");
        }
        #endregion

        #region MarkAsExpired
        [Fact]
        public void MarkAsExpired_ShouldChangeStatusToPermanentlyDeleted_WhenPeriodExpired()
        {
            var deletion = CreateValidUserDeletion();
            var expiredTime = deletion.RestoreUntil.AddDays(1);

            deletion.MarkAsExpired(expiredTime);

            deletion.Status.Should().Be(DeletionStatus.PermanentlyDeleted);
            deletion.UpdatedAt.Should().Be(expiredTime);
        }

        [Fact]
        public void MarkAsExpired_ShouldDoNothing_WhenAlreadyRestored()
        {
            var deletion = CreateValidUserDeletion();
            deletion.Undelete(_now.AddDays(5));

            var originalStatus = deletion.Status;
            deletion.MarkAsExpired(_now.AddDays(40));

            deletion.Status.Should().Be(originalStatus);
        }

        [Fact]
        public void MarkAsExpired_ShouldDoNothing_WhenStillWithinRestorePeriod()
        {
            var deletion = CreateValidUserDeletion();
            var beforeExpiry = deletion.RestoreUntil.AddMinutes(-10);

            deletion.MarkAsExpired(beforeExpiry);

            deletion.Status.Should().Be(DeletionStatus.Deleted);
        }
        #endregion

        #region Helpers
        private UserDeletion CreateValidUserDeletion()
        {
            var result = UserDeletion.Create(_validUserId, _validLogin, _reason, _now);
            return result.Value;
        }
        #endregion
    }
}
