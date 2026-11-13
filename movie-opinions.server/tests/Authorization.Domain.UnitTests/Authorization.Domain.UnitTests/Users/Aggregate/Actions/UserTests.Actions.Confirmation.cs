using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Aggregate;
using Authorization.Domain.Users.AggregateChanges.Action;
using Authorization.Domain.Users.AggregateChanges.Deletion;
using Authorization.Domain.Users.DomainEvents.Actions;
using Authorization.Domain.Users.Entities.UsersPendingAction.Actions;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.Enums;

namespace Authorization.Domain.UnitTests.Users.Aggregate
{
    public partial class UserTests
    {
        #region Confirmation helpers
        private static Result FlowConfirm(
            User user,
            int kind,
            ConfirmationFlowToken token,
            UserPendingActionId id,
            DateTimeOffset now
        ) =>
            kind switch
            {
                0 => user.ConfirmLoginChange(token, id, now),
                1 => user.ConfirmPasswordChange(token, id, now),
                2 => user.ConfirmAccountDeletion(token, id, now),
                _ => throw new ArgumentOutOfRangeException(nameof(kind)),
            };
        #endregion

        #region Confirmation: applying the action and recording changes
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void ConfirmPendingAction_ValidArguments_AppliesOnlyRequestedChangeAndConfirmsAction(int kind)
        {
            var data = new FlowState();
            var action = data.AddAction(kind);
            var token = data.AddToken();
            var session = data.AddGroup();
            var user = data.Restore();
            var now = FlowTime.ToOffset(TimeSpan.FromHours(2));

            FlowSuccess(FlowConfirm(
                user,
                kind,
                FlowConfirmation(),
                UserPendingActionId.Restore(action.Id.Value), now)
            );

            Assert.False(user.IsAction);

            Assert.Equal(
                ActionStatus.Confirmed,
                action.Status
            );

            Assert.Equal(
                now,
                action.ConfirmationTime
            );

            Assert.Equal(
                now.Offset,
                action.ConfirmationTime!.Value.Offset
            );

            Assert.Null(action.CancelledTime);
            Assert.Null(action.ExpiredAt);

            Assert.Equal(
                now,
                user.UpdatedAt
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

            var updated = Assert.Single(user.AggregateChanges.OfType<UserPendingActionUpdated>());

            Assert.Same(
                action,
                updated.UserPendingAction
            );

            Assert.Equal(
                now,
                updated.Now
            );

            Assert.Equal(
                now,
                updated.OccurredOn
            );

            if (kind == 0)
            {
                var change = Assert.IsType<ChangeLoginAction>(action.UserAction);

                Assert.Same(change.NewLogin,
                    user.Login
                );

                Assert.Same(
                    data.Password,
                    user.Password
                );

                Assert.False(user.IsDeleted);
                Assert.Single(user.AggregateChanges);

                var ev = Assert.IsType<UserLoginChangeConfirmedEvent>(Assert.Single(user.DomainEvents));

                Assert.Same(
                    data.Login,
                    ev.PreviousLogin
                );

                Assert.Same(
                    change.NewLogin,
                    ev.CurrentLogin
                );

                Assert.Same(
                    user.Id,
                    ev.UserId
                );

                Assert.Equal(
                    action.Id,
                    ev.UserPendingActionId
                );

                Assert.Equal(
                    now,
                    ev.OccurredOn
                );
            }
            else if (kind == 1)
            {
                var change = Assert.IsType<ChangePasswordAction>(action.UserAction);

                Assert.Same(
                    change.NewPassword,
                    user.Password
                );

                Assert.Same(
                    data.Login,
                    user.Login
                );

                Assert.False(user.IsDeleted);
                Assert.Single(user.AggregateChanges);

                var ev = Assert.IsType<UserPasswordChangeConfirmedEvent>(Assert.Single(user.DomainEvents));

                Assert.Same(
                    user.Id,
                    ev.UserId
                );

                Assert.Equal(
                    action.Id,
                    ev.UserPendingActionId
                );

                Assert.Same(
                    data.Login,
                    ev.Login
                );

                Assert.Equal(
                    now,
                    ev.OccurredOn
                );
            }
            else
            {
                Assert.Same(
                    data.Login,
                    user.Login
                );

                Assert.Same(
                    data.Password,
                    user.Password
                );

                Assert.True(user.IsDeleted);

                var deletion = user.GetDeletion().Value;

                Assert.Same(
                    Assert.IsType<DeleteUserAction>(action.UserAction).Reason,
                    deletion.Reason
                );

                Assert.Same(
                    data.Login,
                    deletion.Login
                );

                Assert.Equal(
                    now.AddDays(30),
                    deletion.RestoreUntil
                );

                Assert.Equal(
                    2,
                    user.AggregateChanges.Count
                );

                Assert.Same(
                    deletion,
                    Assert.IsType<UserDeletionCreated>(user.AggregateChanges[0]).UserDeletion
                );

                Assert.Same(
                    updated,
                    user.AggregateChanges[1]
                );

                var ev = Assert.IsType<UserAccountDeletionConfirmedEvent>(Assert.Single(user.DomainEvents));

                Assert.Same(
                    user.Id,
                    ev.UserId
                );

                Assert.Equal(
                    action.Id,
                    ev.UserPendingActionId
                );

                Assert.Same(
                    data.Login,
                    ev.Login
                );

                Assert.Equal(
                    now.AddDays(30),
                    ev.RestoreUntil
                );

                Assert.Equal(
                    now,
                    ev.OccurredOn
                );
            }
        }

        [Fact]
        public void ConfirmLoginChange_EmailToPhone_PreservesRequestedLoginInstance()
        {
            var user = new FlowState().Restore();
            var previous = user.Login;
            var requested = FlowPhoneLogin();

            var action = user.RequestLoginChange(
                requested,
                FlowTime
            ).Value;

            FlowSuccess(user.ConfirmLoginChange(
                action.ConfirmationToken,
                action.Id,
                FlowTime)
            );

            Assert.Same(
                requested,
                user.Login
            );

            Assert.IsType<PhoneLogin>(user.Login);

            var ev = Assert.Single(user.DomainEvents.OfType<UserLoginChangeConfirmedEvent>());

            Assert.Same(
                previous,
                ev.PreviousLogin
            );

            Assert.Same(
                requested,
                ev.CurrentLogin
            );
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 1)]
        [InlineData(0, 2)]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(1, 2)]
        [InlineData(2, 0)]
        [InlineData(2, 1)]
        [InlineData(2, 2)]
        public void ConfirmPendingAction_PreviousHistoryAndRepeatedCalls_ArePreserved(
            int kind,
            int nextKind)
        {
            var user = User.Create(
                FlowLogin(),
                FlowPassword(),
                FlowTime
            );

            var action = FlowRequest(user, kind, FlowTime).Value;
            var events = user.DomainEvents.ToArray();
            var changes = user.AggregateChanges.ToArray();

            FlowSuccess(FlowConfirm(
                user,
                kind,
                action.ConfirmationToken,
                action.Id,
                FlowTime)
            );

            FlowSameItems(
                events,
                user.DomainEvents.Take(events.Length)
            );

            FlowSameItems(
                changes,
                user.AggregateChanges.Take(changes.Length)
            );

            Assert.Equal(
                events.Length + 1,
                user.DomainEvents.Count
            );

            Assert.Equal(
                changes.Length + (kind == 2 ? 2 : 1),
                user.AggregateChanges.Count
            );

            var before = new FlowSnapshot(user, action);

            var result = FlowConfirm(
                user,
                nextKind,
                action.ConfirmationToken,
                action.Id,
                FlowTime.AddTicks(1)
            );

            FlowFailure(
                result,
                kind == 2 ? "ACCESS_USER.USER_DELETED" : "PENDING_ACTION.EMPTY_ACTION",
                kind == 2 ? ErrorType.Forbidden : ErrorType.NotFound
            );

            before.AssertUnchanged();
        }
        #endregion

        #region Confirmation: lookup, type, token and expiration precedence
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void ConfirmPendingAction_NoAction_ReturnsEmptyActionWithoutChanges(int kind)
        {
            var user = new FlowState().Restore();
            var before = new FlowSnapshot(user);

            FlowFailure(
                FlowConfirm(
                    user,
                    kind,
                    FlowConfirmation(),
                    UserPendingActionId.Restore(FlowGuid(104)),
                    FlowTime
                ),
                "PENDING_ACTION.EMPTY_ACTION",
                ErrorType.NotFound
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void ConfirmPendingAction_WrongIdentifier_PrecedesTypeTokenAndExpirationChecks(int kind)
        {
            var data = new FlowState();
            var action = data.AddAction((kind + 1) % 3);
            var user = data.Restore();
            var before = new FlowSnapshot(user, action);

            FlowFailure(
                FlowConfirm(
                    user,
                    kind,
                    FlowConfirmation(11),
                    UserPendingActionId.Restore(FlowGuid(104, 99)),
                    action.ExpiresAt
                ),
                "PENDING_ACTION.NOT_FOUND_ACTION",
                ErrorType.NotFound
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(0, 2)]
        [InlineData(1, 0)]
        [InlineData(1, 2)]
        [InlineData(2, 0)]
        [InlineData(2, 1)]
        public void ConfirmPendingAction_WrongActionType_PrecedesTokenAndExpirationChecks(
            int storedKind,
            int requestedKind
        )
        {
            var data = new FlowState();
            var action = data.AddAction(storedKind);
            var user = data.Restore();
            var before = new FlowSnapshot(user, action);

            FlowFailure(
                FlowConfirm(
                    user,
                    requestedKind,
                    FlowConfirmation(11),
                    action.Id,
                    action.ExpiresAt
                ),
                "PENDING_ACTION.INVALID_ACTION_TYPE",
                ErrorType.Conflict
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(0, -1L, false)]
        [InlineData(0, -1L, true)]
        [InlineData(0, 0L, false)]
        [InlineData(0, 0L, true)]
        [InlineData(0, 1L, false)]
        [InlineData(0, 1L, true)]
        [InlineData(1, -1L, false)]
        [InlineData(1, -1L, true)]
        [InlineData(1, 0L, false)]
        [InlineData(1, 0L, true)]
        [InlineData(1, 1L, false)]
        [InlineData(1, 1L, true)]
        [InlineData(2, -1L, false)]
        [InlineData(2, -1L, true)]
        [InlineData(2, 0L, false)]
        [InlineData(2, 0L, true)]
        [InlineData(2, 1L, false)]
        [InlineData(2, 1L, true)]
        public void ConfirmPendingAction_DeadlineAndTokenBoundary_DoesNotMutateOnFailure(
            int kind,
            long ticks,
            bool validToken
        )
        {
            var data = new FlowState();
            var action = data.AddAction(kind);
            var user = data.Restore();
            var before = new FlowSnapshot(user, action);
            var now = action.ExpiresAt.AddTicks(ticks);

            var result = FlowConfirm(
                user,
                kind,
                FlowConfirmation(validToken ? (byte)10 : (byte)11),
                action.Id,
                now
            );

            if (ticks < 0 && validToken)
            {
                FlowSuccess(result);

                Assert.Equal(
                    ActionStatus.Confirmed,
                    action.Status
                );

                Assert.Equal(
                    now,
                    action.ConfirmationTime
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
        [InlineData(0, ActionStatus.Confirmed)]
        [InlineData(0, ActionStatus.Cancelled)]
        [InlineData(0, ActionStatus.Expired)]
        [InlineData(1, ActionStatus.Confirmed)]
        [InlineData(1, ActionStatus.Cancelled)]
        [InlineData(1, ActionStatus.Expired)]
        [InlineData(2, ActionStatus.Confirmed)]
        [InlineData(2, ActionStatus.Cancelled)]
        [InlineData(2, ActionStatus.Expired)]
        public void ConfirmPendingAction_TerminalAttachedAction_ThrowsWithoutMutation(
            int kind,
            ActionStatus status)
        {
            var data = new FlowState();
            var action = data.AddAction(kind);
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
                FlowConfirm(
                    user,
                    kind,
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

        [Fact]
        public void ConfirmAccountDeletion_DeadlineOverflow_DoesNotConfirmOrAttachDeletion()
        {
            var time = DateTimeOffset.MaxValue.AddMinutes(-30);
            var data = new FlowState { CreatedAt = time, UpdatedAt = time };
            var action = data.AddAction(2);
            var user = data.Restore();
            var before = new FlowSnapshot(user, action);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                user.ConfirmAccountDeletion(
                    action.ConfirmationToken,
                    action.Id,
                    DateTimeOffset.MaxValue.AddTicks(-1)
                )
            );

            before.AssertUnchanged();
        }
        #endregion

        #region Confirmation: guards and access policy
        [Theory]
        [InlineData(0, 1, false)]
        [InlineData(0, 1, true)]
        [InlineData(0, 2, false)]
        [InlineData(0, 2, true)]
        [InlineData(0, 3, false)]
        [InlineData(0, 3, true)]
        [InlineData(1, 1, false)]
        [InlineData(1, 1, true)]
        [InlineData(1, 2, false)]
        [InlineData(1, 2, true)]
        [InlineData(1, 3, false)]
        [InlineData(1, 3, true)]
        [InlineData(2, 1, false)]
        [InlineData(2, 1, true)]
        [InlineData(2, 2, false)]
        [InlineData(2, 2, true)]
        [InlineData(2, 3, false)]
        [InlineData(2, 3, true)]
        public void ConfirmPendingAction_NullArguments_PrecedeTimeAndAccessValidation(
            int kind,
            int mask,
            bool invalidTime
        )
        {
            var user = FlowAccessState(1).Restore();
            var before = new FlowSnapshot(user);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                FlowConfirm(
                    user,
                    kind,
                    (mask & 1) != 0 ? null! : FlowConfirmation(),
                    (mask & 2) != 0 ? null! : UserPendingActionId.Restore(FlowGuid(104)),
                    invalidTime ? FlowTime.AddTicks(-1) : FlowTime
                )
            );

            FlowEmpty(
                exception, (mask & 1) != 0 ? "confirmationToken" : "actionId"
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(0, 2)]
        [InlineData(0, 3)]
        [InlineData(1, 1)]
        [InlineData(1, 2)]
        [InlineData(1, 3)]
        [InlineData(2, 1)]
        [InlineData(2, 2)]
        [InlineData(2, 3)]
        public void ConfirmPendingAction_DeniedAccess_PrecedesActionLookup(int kind, int accessState)
        {
            var data = FlowAccessState(accessState);
            var action = data.AddAction(kind);
            var user = data.Restore();
            var before = new FlowSnapshot(user, action);

            FlowFailure(
                FlowConfirm(
                    user,
                    kind,
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
        [InlineData(0, RestrictionType.Ban)]
        [InlineData(0, RestrictionType.CommentMute)]
        [InlineData(0, RestrictionType.ChatMute)]
        [InlineData(0, RestrictionType.AllMute)]
        [InlineData(1, RestrictionType.Ban)]
        [InlineData(1, RestrictionType.CommentMute)]
        [InlineData(1, RestrictionType.ChatMute)]
        [InlineData(1, RestrictionType.AllMute)]
        [InlineData(2, RestrictionType.Ban)]
        [InlineData(2, RestrictionType.CommentMute)]
        [InlineData(2, RestrictionType.ChatMute)]
        [InlineData(2, RestrictionType.AllMute)]
        public void ConfirmPendingAction_MuteOrExpiredBan_AllowsConfirmation(
            int kind,
            RestrictionType type)
        {
            var data = new FlowState();
            data.AddGroup(type);
            var action = data.AddAction(kind);
            var user = data.Restore();
            var now = type == RestrictionType.Ban ? FlowTime.AddMinutes(10) : FlowTime;

            FlowSuccess(FlowConfirm(
                user,
                kind,
                action.ConfirmationToken,
                action.Id,
                now)
            );

            Assert.Single(user.RestrictionSessions);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ConfirmLoginChange_TimeBeforeAggregateTimestamp_ThrowsWithoutChanges(bool afterCreation)
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
                FlowConfirm(
                    user,
                    0,
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

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ConfirmPasswordChange_TimeBeforeAggregateTimestamp_ThrowsWithoutChanges(bool afterCreation)
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
                FlowConfirm(
                    user,
                    1,
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

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ConfirmAccountDeletion_TimeBeforeAggregateTimestamp_ThrowsWithoutChanges(bool afterCreation)
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
                FlowConfirm(
                    user,
                    2,
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
