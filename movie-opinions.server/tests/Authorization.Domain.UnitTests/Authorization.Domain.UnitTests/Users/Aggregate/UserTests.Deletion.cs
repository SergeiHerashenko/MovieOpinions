using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Users.Aggregate;
using Authorization.Domain.Users.AggregateChanges.Deletion;
using Authorization.Domain.Users.DomainEvents.Deletion;
using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;
using Authorization.Domain.Users.Enums;

namespace Authorization.Domain.UnitTests.Users.Aggregate
{
    public partial class UserTests
    {
        #region Delete
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Delete_ValidReason_AttachesDeletionAndPreservesUnrelatedState(bool phone)
        {
            var data = new FlowState { Login = phone ? FlowPhoneLogin() : FlowLogin() };
            var token = data.AddToken();
            var session = data.AddGroup();
            var action = data.AddAction();
            var user = data.Restore();
            var reason = DeletionReason.Restore("Close account");
            var now = FlowTime.AddMinutes(1).ToOffset(TimeSpan.FromHours(2));

            FlowSuccess(user.Delete(
                now,
                reason)
            );

            Assert.True(user.IsDeleted);

            var deletionResult = user.GetDeletion();

            FlowSuccess(deletionResult);

            var deletion = deletionResult.Value;

            Assert.Same(
                user.Id,
                deletion.UserId
            );

            Assert.Same(
                data.Login,
                deletion.Login
            );

            Assert.Same(
                reason,
                deletion.Reason
            );

            Assert.Equal(
                DeletionStatus.Deleted,
                deletion.Status
            );

            Assert.Equal(
                now,
                deletion.CreatedAt
            );

            Assert.Equal(
                now.Offset,
                deletion.CreatedAt.Offset
            );

            Assert.Equal(
                now.AddDays(30),
                deletion.RestoreUntil
            );

            Assert.Null(deletion.RestoredAt);
            Assert.Null(deletion.UpdatedAt);

            Assert.Equal(
                now,
                user.UpdatedAt
            );

            var change = Assert.IsType<UserDeletionCreated>(Assert.Single(user.AggregateChanges));

            Assert.Same(
                deletion,
                change.UserDeletion
            );

            Assert.Equal(
                now,
                change.OccurredOn
            );

            Assert.Empty(user.DomainEvents);

            Assert.Same(
                token,
                Assert.Single(user.RefreshTokens)
            );

            Assert.Equal(
                TokenStatus.Active,
                token.TokenStatus
            );

            Assert.Same(
                session,
                Assert.Single(user.RestrictionSessions)
            );

            Assert.True(user.IsAction);

            Assert.Equal(
                ActionStatus.Active,
                action.Status
            );
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Delete_NullReason_PrecedesTimestampValidation(bool invalidTime)
        {
            var user = new FlowState().Restore();
            var before = new FlowSnapshot(user);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                user.Delete(invalidTime ? FlowTime.AddTicks(-1) : FlowTime, null!)
            );

            FlowEmpty(
                exception,
                "reason"
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Delete_ExistingDeletion_ReturnsConflictWithoutReplacement(bool permanent)
        {
            var data = new FlowState();
            var deletion = data.AddDeletion();

            if (permanent)
            {
                Assert.True(deletion.TryMarkAsPermanentlyDeleted(deletion.RestoreUntil));
                data.UpdatedAt = deletion.RestoreUntil;
            }

            var user = data.Restore();
            var before = new FlowSnapshot(user);
            var now = permanent ? deletion.RestoreUntil : FlowTime;

            FlowFailure(
                user.Delete(now, DeletionReason.Restore("Other reason")),
                "USER_DELETION.USER_ALREADY_DELETED",
                ErrorType.Conflict
            );

            before.AssertUnchanged();
        }

        [Fact]
        public void Delete_RestorationDeadlineOverflow_DoesNotAttachPartialDeletion()
        {
            var user = new FlowState().Restore();
            var before = new FlowSnapshot(user);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                user.Delete(
                    DateTimeOffset.MaxValue.AddDays(-30).AddTicks(1),
                    DeletionReason.Restore("Reason")
                )
            );

            before.AssertUnchanged();
        }
        #endregion

        #region Undelete
        [Fact]
        public void Undelete_MissingDeletion_ReturnsConflictWithoutChanges()
        {
            var user = new FlowState().Restore();
            var before = new FlowSnapshot(user);

            FlowFailure(
                user.Undelete(FlowTime),
                "USER_DELETION.USER_NOT_DELETED",
                ErrorType.Conflict
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Undelete_RestorationBoundary_RestoresOnlyBeforeDeadline(int position)
        {
            var data = new FlowState();
            var deletion = data.AddDeletion();
            var user = data.Restore();
            var now = position == 0 ? FlowTime : deletion.RestoreUntil.AddTicks(position == 1 ? -1 : position - 2);
            var before = new FlowSnapshot(user);

            var result = user.Undelete(now);

            if (position >= 2)
            {
                FlowFailure(
                    result,
                    "USER_DELETION.RESTORATION_PERIOD_EXPIRED",
                    ErrorType.Forbidden
                );

                before.AssertUnchanged();

                return;
            }

            FlowSuccess(result);

            Assert.False(user.IsDeleted);

            Assert.Equal(
                DeletionStatus.Restored,
                deletion.Status
            );

            Assert.Equal(
                now,
                deletion.RestoredAt
            );

            Assert.Equal(
                now,
                deletion.UpdatedAt
            );

            Assert.Equal(
                now,
                user.UpdatedAt
            );

            var change = Assert.IsType<UserDeletionUpdated>(Assert.Single(user.AggregateChanges));

            Assert.Same(
                deletion,
                change.UserDeletion
            );

            Assert.Equal(
                now,
                change.OccurredOn
            );

            var ev = Assert.IsType<UserAccountRestoredEvent>(Assert.Single(user.DomainEvents));

            Assert.Same(
                user.Id,
                ev.UserId
            );

            Assert.Same(
                deletion.Id,
                ev.UserDeletionId
            );

            Assert.Same(
                deletion.Login,
                ev.Login
            );

            Assert.Equal(
                now,
                ev.OccurredOn
            );

            var after = new FlowSnapshot(user);

            FlowFailure(
                user.Undelete(now.AddTicks(1)),
                "USER_DELETION.USER_NOT_DELETED",
                ErrorType.Conflict
            );

            after.AssertUnchanged();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Undelete_AlreadyTerminalDeletion_PropagatesEntityFailure(bool restored)
        {
            var data = new FlowState();
            var deletion = data.AddDeletion();
            var user = data.Restore();

            if (restored)
            {
                FlowSuccess(deletion.Undelete(FlowTime));
            }
            else
            {
                Assert.True(deletion.TryMarkAsPermanentlyDeleted(deletion.RestoreUntil));
            }

            var before = new FlowSnapshot(user);
            var now = restored ? FlowTime : deletion.RestoreUntil;

            FlowFailure(
                user.Undelete(now),
                restored ? "USER_DELETION.USER_ALREADY_RESTORED" : "USER_DELETION.RESTORATION_PERIOD_EXPIRED",
                restored ? ErrorType.Conflict : ErrorType.Forbidden
            );

            before.AssertUnchanged();
        }

        [Fact]
        public void Undelete_ActiveBan_DoesNotPreventRestoration()
        {
            var data = FlowAccessState(3);
            var user = data.Restore();

            FlowSuccess(user.Undelete(FlowTime));

            Assert.False(user.IsDeleted);
            Assert.Single(user.RestrictionSessions);
        }
        #endregion

        #region TryFinalizeDeletion and GetDeletion
        [Fact]
        public void TryFinalizeDeletion_MissingDeletion_ReturnsFalseWithoutChanges()
        {
            var user = new FlowState().Restore();
            var before = new FlowSnapshot(user);

            Assert.False(user.TryFinalizeDeletion(FlowTime));
            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(-1L)]
        [InlineData(0L)]
        [InlineData(1L)]
        public void TryFinalizeDeletion_DeadlineBoundary_FinalizesOnceAndKeepsDeletionAttached(long ticks)
        {
            var data = new FlowState();
            var deletion = data.AddDeletion();
            var user = data.Restore();
            var now = deletion.RestoreUntil.AddTicks(ticks);
            var before = new FlowSnapshot(user);

            var result = user.TryFinalizeDeletion(now);

            Assert.Equal(
                ticks >= 0,
                result
            );

            if (ticks < 0)
            {
                before.AssertUnchanged();
                return;
            }

            Assert.True(user.IsDeleted);

            Assert.Same(
                deletion,
                user.GetDeletion().Value
            );

            Assert.Equal(
                DeletionStatus.PermanentlyDeleted,
                deletion.Status
            );

            Assert.Equal(
                now,
                deletion.UpdatedAt
            );

            Assert.Null(deletion.RestoredAt);

            Assert.Equal(
                now,
                user.UpdatedAt
            );

            var change = Assert.IsType<UserDeletionUpdated>(Assert.Single(user.AggregateChanges));

            Assert.Same(
                deletion,
                change.UserDeletion
            );

            Assert.Equal(
                now,
                change.OccurredOn
            );

            var ev = Assert.IsType<UserAccountDeletionFinalizedEvent>(Assert.Single(user.DomainEvents));

            Assert.Same(
                user.Id,
                ev.UserId
            );

            Assert.Same(
                deletion.Id,
                ev.UserDeletionId
            );

            Assert.Same(
                deletion.Login,
                ev.Login
            );

            Assert.Equal(
                now,
                ev.OccurredOn
            );

            var after = new FlowSnapshot(user);

            Assert.False(user.TryFinalizeDeletion(now.AddTicks(1)));

            after.AssertUnchanged();
        }

        [Fact]
        public void TryFinalizeDeletion_RestoredAttachedEntity_ReturnsFalseWithoutChanges()
        {
            var data = new FlowState();
            var deletion = data.AddDeletion();
            var user = data.Restore();
            FlowSuccess(deletion.Undelete(FlowTime));
            var before = new FlowSnapshot(user);

            Assert.False(user.TryFinalizeDeletion(deletion.RestoreUntil));

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void GetDeletion_MissingOrPresent_ReturnsExpectedResultWithoutMutation(bool present)
        {
            var data = new FlowState();

            if (present)
            {
                data.AddDeletion();
            }

            var user = data.Restore();
            var before = new FlowSnapshot(user);

            for (var i = 0; i < 2; i++)
            {
                var result = user.GetDeletion();

                if (present)
                {
                    FlowSuccess(result);
                    Assert.Same(
                        data.Deletion,
                        result.Value
                    );
                }
                else
                {
                    FlowFailure(
                        result,
                        "USER_DELETION.USER_NOT_DELETED",
                        ErrorType.Conflict
                    );
                }
            }

            before.AssertUnchanged();
        }

        [Fact]
        public void DeleteUndeleteDelete_RecordsNewDeletionAndPreservesEarlierHistory()
        {
            var user = User.Create(
                FlowLogin(),
                FlowPassword(),
                FlowTime
            );

            var registered = Assert.Single(user.DomainEvents);

            FlowSuccess(
                user.Delete(FlowTime,
                DeletionReason.Restore("First"))
            );

            var first = user.GetDeletion().Value;
            var firstChange = user.AggregateChanges[0];

            FlowSuccess(user.Undelete(FlowTime.AddMinutes(1)));

            FlowSuccess(
                user.Delete(FlowTime.AddMinutes(2),
                DeletionReason.Restore("Second"))
            );

            var second = user.GetDeletion().Value;

            Assert.NotSame(
                first,
                second
            );

            Assert.NotEqual(
                first.Id,
                second.Id
            );

            Assert.Equal(
                DeletionStatus.Restored,
                first.Status
            );

            Assert.Equal(
                "Second",
                second.Reason.Value
            );

            Assert.Equal(
                3,
                user.AggregateChanges.Count
            );

            Assert.Same(
                firstChange,
                user.AggregateChanges[0]
            );

            Assert.Same(
                registered,
                user.DomainEvents[0]
            );

            Assert.Equal(
                2,
                user.DomainEvents.Count
            );
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Delete_TimeBeforeAggregateTimestamp_ThrowsWithoutChanges(bool afterCreation)
        {
            var state = new FlowState();

            if (afterCreation)
            {
                state.UpdatedAt = FlowTime.AddMinutes(5);
                state.LastLoginAt = FlowTime.AddMinutes(4);
            }

            var user = state.Restore();
            var before = new FlowSnapshot(user);
            var minimum = afterCreation ? state.UpdatedAt!.Value : state.CreatedAt;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                user.Delete(
                    minimum.AddTicks(-1),
                    DeletionReason.Restore("Reason")
                )
            );

            FlowRange(
                exception,
                afterCreation ? "UpdatedAt" : "CreatedAt",
                minimum
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Undelete_TimeBeforeAggregateTimestamp_ThrowsWithoutChanges(bool afterCreation)
        {
            var state = new FlowState();

            if (afterCreation)
            {
                state.UpdatedAt = FlowTime.AddMinutes(5);
                state.LastLoginAt = FlowTime.AddMinutes(4);
            }

            var user = state.Restore();
            var before = new FlowSnapshot(user);
            var minimum = afterCreation ? state.UpdatedAt!.Value : state.CreatedAt;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() => user.Undelete(minimum.AddTicks(-1)));

            FlowRange(
                exception,
                afterCreation ? "UpdatedAt" : "CreatedAt",
                minimum
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TryFinalizeDeletion_TimeBeforeAggregateTimestamp_ThrowsWithoutChanges(bool afterCreation)
        {
            var state = new FlowState();

            if (afterCreation)
            {
                state.UpdatedAt = FlowTime.AddMinutes(5);
                state.LastLoginAt = FlowTime.AddMinutes(4);
            }

            var user = state.Restore();
            var before = new FlowSnapshot(user);
            var minimum = afterCreation ? state.UpdatedAt!.Value : state.CreatedAt;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                user.TryFinalizeDeletion(minimum.AddTicks(-1))
            );

            FlowRange(
                exception,
                afterCreation ? "UpdatedAt" : "CreatedAt",
                minimum
            );

            before.AssertUnchanged();
        }
        #endregion
    }
}
