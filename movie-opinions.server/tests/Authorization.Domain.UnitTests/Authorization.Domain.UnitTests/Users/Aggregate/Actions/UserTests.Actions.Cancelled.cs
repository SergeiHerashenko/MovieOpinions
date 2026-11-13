using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Users.Aggregate;
using Authorization.Domain.Users.AggregateChanges.Action;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.Enums;

namespace Authorization.Domain.UnitTests.Users.Aggregate
{
    public partial class UserTests
    {
        #region CancelledAction: successful cancellation
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void CancelledAction_AnyActionType_CancelsAndRemovesOnlyPendingAction(int kind)
        {
            var data = new FlowState();
            var action = data.AddAction(kind);
            var token = data.AddToken();
            var session = data.AddGroup();
            var user = data.Restore();
            var now = action.ExpiresAt.AddTicks(-1).ToOffset(TimeSpan.FromHours(2));
            var suppliedToken = FlowConfirmation();
            var suppliedId = UserPendingActionId.Restore(action.Id.Value);

            Assert.NotSame(
                action.ConfirmationToken,
                suppliedToken
            );

            FlowSuccess(user.CancelledAction(
                suppliedToken,
                suppliedId,
                now)
            );

            Assert.False(user.IsAction);
            Assert.False(user.IsDeleted);

            Assert.Equal(
                ActionStatus.Cancelled,
                action.Status
            );

            Assert.Equal(
                now,
                action.CancelledTime
            );

            Assert.Equal(
                now.Offset,
                action.CancelledTime!.Value.Offset
            );

            Assert.Null(action.ConfirmationTime);
            Assert.Null(action.ExpiredAt);

            Assert.Same(
                data.Login,
                user.Login
            );

            Assert.Same(
                data.Password,
                user.Password
            );

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

            Assert.Equal(
                now,
                user.UpdatedAt
            );

            var change = Assert.IsType<UserPendingActionUpdated>(Assert.Single(user.AggregateChanges));

            Assert.Same(
                action,
                change.UserPendingAction
            );

            Assert.Equal(
                now,
                change.Now
            );

            Assert.Equal(
                now,
                change.OccurredOn
            );

            Assert.Empty(user.DomainEvents);

            var before = new FlowSnapshot(user, action);

            FlowFailure(
                user.CancelledAction(
                    suppliedToken,
                    suppliedId,
                    now
                ),
                "PENDING_ACTION.EMPTY_ACTION",
                ErrorType.NotFound
            );

            before.AssertUnchanged();
        }
        #endregion

        #region CancelledAction: validation and access
        [Fact]
        public void CancelledAction_NoAction_ReturnsEmptyActionWithoutChanges()
        {
            var user = new FlowState().Restore();
            var before = new FlowSnapshot(user);

            FlowFailure(
                user.CancelledAction(
                    FlowConfirmation(),
                    UserPendingActionId.Restore(FlowGuid(104)),
                    FlowTime
                ),
                "PENDING_ACTION.EMPTY_ACTION",
                ErrorType.NotFound
            );

            before.AssertUnchanged();
        }

        [Fact]
        public void CancelledAction_WrongIdentifier_PrecedesExpirationAndTokenValidation()
        {
            var data = new FlowState();
            var action = data.AddAction();
            var user = data.Restore();
            var before = new FlowSnapshot(user, action);

            FlowFailure(
                user.CancelledAction(
                    FlowConfirmation(11),
                    UserPendingActionId.Restore(FlowGuid(104, 99)),
                    action.ExpiresAt
                ),
                "PENDING_ACTION.NOT_FOUND_ACTION",
                ErrorType.NotFound
            );

            before.AssertUnchanged();
        }

        // Після дедлайну помилка строку має пріоритет; сама перевірка не змінює статус.
        // After the deadline, expiration takes precedence; validation itself does not change the status.
        [Theory]
        [InlineData(-1L, false)]
        [InlineData(-1L, true)]
        [InlineData(0L, false)]
        [InlineData(0L, true)]
        [InlineData(1L, false)]
        [InlineData(1L, true)]
        public void CancelledAction_DeadlineAndTokenBoundary_ReturnsFirstValidationFailureWithoutExpirationMutation(
            long ticks,
            bool validToken
        )
        {
            var data = new FlowState();
            var action = data.AddAction();
            var user = data.Restore();
            var before = new FlowSnapshot(user, action);
            var now = action.ExpiresAt.AddTicks(ticks);
            var supplied = validToken ? FlowConfirmation() : FlowConfirmation(11);

            var result = user.CancelledAction(supplied, action.Id, now);

            if (ticks < 0 && validToken)
            {
                FlowSuccess(result);

                Assert.Equal(
                    ActionStatus.Cancelled,
                    action.Status
                );

                Assert.Equal(
                    now,
                    action.CancelledTime
                );
            }
            else
            {
                FlowFailure(
                    result,
                    ticks >= 0 ? "PENDING_ACTION.EXPIRED_ACTION" : "PENDING_ACTION.INVALID_CONFIRMATION_TOKEN",
                    ticks >= 0 ? ErrorType.Conflict : ErrorType.Forbidden
                );

                before.AssertUnchanged();
            }
        }

        [Theory]
        [InlineData(ActionStatus.Confirmed)]
        [InlineData(ActionStatus.Cancelled)]
        [InlineData(ActionStatus.Expired)]
        public void CancelledAction_TerminalAttachedAction_ThrowsWithoutMutation(ActionStatus status)
        {
            var data = new FlowState();
            var action = data.AddAction();
            var user = data.Restore();

            FlowMakeActionTerminal(
                action,
                status
            );

            var before = new FlowSnapshot(
                user,
                action
            );

            var now = status == ActionStatus.Expired ? action.ExpiresAt : FlowTime;

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                user.CancelledAction(
                    action.ConfirmationToken,
                    action.Id,
                    now
                )
            );

            FlowInvariant(
                exception,
                "UserPendingAction"
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(1, false)]
        [InlineData(1, true)]
        [InlineData(2, false)]
        [InlineData(2, true)]
        [InlineData(3, false)]
        [InlineData(3, true)]
        public void CancelledAction_NullArguments_PrecedeTimeAndAccessChecks(
            int mask,
            bool invalidTime)
        {
            var user = FlowAccessState(1).Restore();
            var before = new FlowSnapshot(user);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                user.CancelledAction(
                    (mask & 1) != 0 ? null! : FlowConfirmation(),
                    (mask & 2) != 0 ? null! : UserPendingActionId.Restore(FlowGuid(104)),
                    invalidTime ? FlowTime.AddTicks(-1) : FlowTime
                )
            );

            FlowEmpty(
                exception,
                (mask & 1) != 0 ? "confirmationToken" : "actionId"
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void CancelledAction_DeniedAccess_PrecedesActionLookup(int accessState)
        {
            var data = FlowAccessState(accessState);
            var action = data.AddAction();
            var user = data.Restore();
            var before = new FlowSnapshot(user, action);

            FlowFailure(
                user.CancelledAction(
                    FlowConfirmation(11),
                    UserPendingActionId.Restore(FlowGuid(104, 99)),
                    FlowTime
                ),
                accessState == 2 ? "ACCESS_USER.USER_BLOCKED" : "ACCESS_USER.USER_DELETED",
                ErrorType.Forbidden
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(RestrictionType.Ban)]
        [InlineData(RestrictionType.CommentMute)]
        [InlineData(RestrictionType.ChatMute)]
        [InlineData(RestrictionType.AllMute)]
        public void CancelledAction_MuteOrExpiredBan_AllowsCancellation(RestrictionType type)
        {
            var data = new FlowState();
            data.AddGroup(type);
            var action = data.AddAction();
            var user = data.Restore();
            var now = type == RestrictionType.Ban ? FlowTime.AddMinutes(10) : FlowTime;

            FlowSuccess(user.CancelledAction(
                action.ConfirmationToken,
                action.Id,
                now)
            );

            Assert.Single(user.RestrictionSessions);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void CancelledAction_ExistingHistory_IsPreservedAndConfirmationThenReportsEmptyAction(int kind)
        {
            var user = User.Create(
                FlowLogin(),
                FlowPassword(),
                FlowTime
            );

            var action = FlowRequest(
                user,
                kind,
                FlowTime
            ).Value;

            var changes = user.AggregateChanges.ToArray();
            var events = user.DomainEvents.ToArray();

            FlowSuccess(user.CancelledAction(
                action.ConfirmationToken,
                action.Id,
                FlowTime)
            );

            FlowSameItems(
                changes,
                user.AggregateChanges.Take(changes.Length)
            );

            FlowSameItems(
                events,
                user.DomainEvents
            );

            Assert.Equal(
                changes.Length + 1,
                user.AggregateChanges.Count
            );

            var before = new FlowSnapshot(
                user,
                action
            );

            FlowFailure(
                FlowConfirm(
                    user,
                    kind,
                    action.ConfirmationToken,
                    action.Id,
                    FlowTime
                ),
                "PENDING_ACTION.EMPTY_ACTION",
                ErrorType.NotFound
            );
            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CancelledAction_TimeBeforeAggregateTimestamp_ThrowsWithoutChanges(bool afterCreation)
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
                user.CancelledAction(
                    FlowConfirmation(),
                    UserPendingActionId.Restore(FlowGuid(104)),
                    minimum.AddTicks(-1)
                )
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
