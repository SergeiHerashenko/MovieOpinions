using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.ValueObjects.PasswordUser;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Aggregate;
using Authorization.Domain.Users.AggregateChanges.Action;
using Authorization.Domain.Users.DomainEvents.Actions;
using Authorization.Domain.Users.Entities.UsersPendingAction;
using Authorization.Domain.Users.Entities.UsersPendingAction.Actions;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;
using Authorization.Domain.Users.Enums;

namespace Authorization.Domain.UnitTests.Users.Aggregate
{
    public partial class UserTests
    {
        #region Pending action request helpers
        // Вибір операції не формує очікуваний результат — очікування задані окремо в тестах.
        // Operation selection does not construct expected results; tests state expectations separately.
        private static Result<UserPendingAction> FlowRequest(
            User user,
            int kind,
            DateTimeOffset now) =>
            kind switch
            {
                0 => user.RequestLoginChange(FlowLogin("next@example.com"), now),

                1 => user.RequestPasswordChange(
                    FlowPlain(),
                    FlowPassword("NEW.HASH"),
                    (_, hash) => hash.Value == "NEW.HASH",
                    now
                ),

                2 => user.RequestAccountDeletion("Close account", now),

                _ => throw new ArgumentOutOfRangeException(nameof(kind)),
            };

        private static void FlowRequestEvent(
            User user,
            UserPendingAction action,
            int kind,
            DateTimeOffset now)
        {
            if (kind == 0)
            {
                var ev = Assert.IsType<UserLoginChangeRequestedEvent>(Assert.Single(user.DomainEvents));

                Assert.Same(
                    user.Id,
                    ev.UserId
                );

                Assert.Equal(
                    action.Id,
                    ev.UserPendingActionId
                );

                Assert.Same(
                    user.Login,
                    ev.CurrentLogin
                );

                Assert.Equal(
                    "next@example.com",
                    ev.RequestedLogin.Value
                );

                Assert.Same(
                    action.ConfirmationToken,
                    ev.ConfirmationFlowToken
                );

                Assert.Equal(
                    now.AddMinutes(30),
                    ev.ExpiresAt
                );

                Assert.Equal(
                    now,
                    ev.OccurredOn
                );
            }
            else if (kind == 1)
            {
                var ev = Assert.IsType<UserPasswordChangeRequestedEvent>(Assert.Single(user.DomainEvents));

                Assert.Same(
                    user.Id,
                    ev.UserId
                );

                Assert.Equal(
                    action.Id,
                    ev.UserPendingActionId
                );

                Assert.Same(
                    user.Login,
                    ev.Login
                );

                Assert.Same(
                    action.ConfirmationToken,
                    ev.ConfirmationFlowToken
                );

                Assert.Equal(
                    now.AddMinutes(30),
                    ev.ExpiresAt
                );

                Assert.Equal(
                    now,
                    ev.OccurredOn
                );
            }
            else
            {
                var ev = Assert.IsType<UserAccountDeletionRequestedEvent>(Assert.Single(user.DomainEvents));

                Assert.Same(
                    user.Id,
                    ev.UserId
                );

                Assert.Equal(
                    action.Id,
                    ev.UserPendingActionId
                );

                Assert.Same(
                    user.Login,
                    ev.Login
                );

                Assert.Same(
                    action.ConfirmationToken,
                    ev.ConfirmationFlowToken
                );

                Assert.Equal(
                    now.AddMinutes(30),
                    ev.ExpiresAt
                );

                Assert.Equal(
                    now,
                    ev.OccurredOn
                );
            }
        }

        private static void FlowMakeActionTerminal(
            UserPendingAction action,
            ActionStatus status)
        {
            if (status == ActionStatus.Confirmed)
            {
                action.MarkAsConfirmed(
                    action.ConfirmationToken,
                    action.CreatedAt
                );
            }
            else if (status == ActionStatus.Cancelled)
            {
                action.MarkAsCancelled(
                    action.ConfirmationToken,
                    action.CreatedAt
                );
            }
            else
            {
                action.MarkAsExpired(action.ExpiresAt);
            }
        }
        #endregion

        #region Pending action creation and replacement
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void RequestPendingAction_ValidInput_RecordsCreationAndRequestWithoutApplyingAction(int kind)
        {
            var data = new FlowState();
            var token = data.AddToken();
            var user = data.Restore();
            var now = FlowTime.AddMinutes(1).ToOffset(TimeSpan.FromHours(2));

            var result = FlowRequest(
                user,
                kind,
                now
            );

            FlowSuccess(result);

            var action = result.Value;

            Assert.True(user.IsAction);
            Assert.False(user.IsDeleted);

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
                user.Id,
                action.UserId
            );

            Assert.Equal(
                ActionStatus.Active,
                action.Status
            );

            Assert.Equal(
                now,
                action.CreatedAt
            );

            Assert.Equal(
                now.Offset,
                action.CreatedAt.Offset
            );

            Assert.Equal(
                now.AddMinutes(30),
                action.ExpiresAt
            );

            Assert.Null(action.ConfirmationTime);
            Assert.Null(action.ExpiredAt);
            Assert.Null(action.CancelledTime);

            Assert.Equal(
                kind == 0 ? UserActionType.ChangeLogin
                    : kind == 1 ? UserActionType.ChangePassword
                    : UserActionType.DeleteUser,
                action.UserAction.ActionType
            );

            Assert.Equal(
                kind == 0 ? "next@example.com"
                    : kind == 1 ? "NEW.HASH"
                    : "Close account",
                action.UserAction.Value
            );

            Assert.Equal(
                now,
                user.UpdatedAt
            );

            var change = Assert.IsType<UserPendingActionCreated>(Assert.Single(user.AggregateChanges));

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

            FlowRequestEvent(
                user,
                action,
                kind,
                now
            );

            FlowSuccess(user.CancelledAction(
                action.ConfirmationToken,
                action.Id,
                now)
            );

            Assert.False(user.IsAction);
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
        public void RequestPendingAction_ActiveActionOfAnyType_ReturnsConflictWithoutReplacement(
            int oldKind,
            int newKind
        )
        {
            var data = new FlowState();
            var old = data.AddAction(oldKind);
            var user = data.Restore();

            var before = new FlowSnapshot(
                user,
                old
            );

            FlowFailure(
                FlowRequest(
                    user,
                    newKind,
                    FlowTime
                ),
                "PENDING_ACTION.ACTION_ALREADY_EXISTS",
                ErrorType.Conflict
            );
            before.AssertUnchanged();

            FlowSuccess(user.CancelledAction(
                old.ConfirmationToken,
                old.Id,
                FlowTime)
            );
        }

        [Theory]
        [InlineData(0, 0, -1L)]
        [InlineData(0, 0, 0L)]
        [InlineData(0, 0, 1L)]
        [InlineData(0, 1, -1L)]
        [InlineData(0, 1, 0L)]
        [InlineData(0, 1, 1L)]
        [InlineData(0, 2, -1L)]
        [InlineData(0, 2, 0L)]
        [InlineData(0, 2, 1L)]
        [InlineData(1, 0, -1L)]
        [InlineData(1, 0, 0L)]
        [InlineData(1, 0, 1L)]
        [InlineData(1, 1, -1L)]
        [InlineData(1, 1, 0L)]
        [InlineData(1, 1, 1L)]
        [InlineData(1, 2, -1L)]
        [InlineData(1, 2, 0L)]
        [InlineData(1, 2, 1L)]
        [InlineData(2, 0, -1L)]
        [InlineData(2, 0, 0L)]
        [InlineData(2, 0, 1L)]
        [InlineData(2, 1, -1L)]
        [InlineData(2, 1, 0L)]
        [InlineData(2, 1, 1L)]
        [InlineData(2, 2, -1L)]
        [InlineData(2, 2, 0L)]
        [InlineData(2, 2, 1L)]
        public void RequestPendingAction_ExpirationBoundary_ReplacesOnlyElapsedAction(
            int oldKind,
            int newKind,
            long ticks
        )
        {
            var data = new FlowState();
            var old = data.AddAction(oldKind);
            var user = data.Restore();
            var now = old.ExpiresAt.AddTicks(ticks);

            var before = new FlowSnapshot(
                user,
                old
            );

            var result = FlowRequest(
                user,
                newKind,
                now
            );

            if (ticks < 0)
            {
                FlowFailure(
                    result,
                    "PENDING_ACTION.ACTION_ALREADY_EXISTS",
                    ErrorType.Conflict
                );

                before.AssertUnchanged();

                return;
            }

            FlowSuccess(result);

            var replacement = result.Value;

            Assert.NotSame(
                old,
                replacement
            );

            Assert.NotEqual(
                old.Id,
                replacement.Id
            );

            Assert.Equal(
                ActionStatus.Expired,
                old.Status
            );

            Assert.Equal(
                now,
                old.ExpiredAt
            );

            Assert.Equal(
                ActionStatus.Active,
                replacement.Status
            );

            Assert.Equal(
                now.AddMinutes(30),
                replacement.ExpiresAt
            );

            Assert.Equal(
                2,
                user.AggregateChanges.Count
            );

            var updated = Assert.IsType<UserPendingActionUpdated>(user.AggregateChanges[0]);

            Assert.Same(
                old,
                updated.UserPendingAction
            );

            Assert.Equal(
                now,
                updated.Now
            );

            Assert.Same(
                replacement,
                Assert.IsType<UserPendingActionCreated>(user.AggregateChanges[1]).UserPendingAction
            );

            FlowRequestEvent(
                user,
                replacement,
                newKind,
                now
            );

            Assert.Equal(
                now,
                user.UpdatedAt
            );

            FlowFailure(
                user.CancelledAction(
                    old.ConfirmationToken,
                    old.Id,
                    now
                ),
                "PENDING_ACTION.NOT_FOUND_ACTION",
                ErrorType.NotFound
            );

            FlowSuccess(user.CancelledAction(
                replacement.ConfirmationToken,
                replacement.Id,
                now)
            );
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
        public void RequestPendingAction_TerminalAttachedAction_ThrowsWithoutChanges(
            int kind,
            ActionStatus status)
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

            var exception = Assert.Throws<DomainInvariantViolationException>(() => FlowRequest(user, kind, now));

            FlowInvariant(
                exception,
                "UserPendingAction"
            );

            before.AssertUnchanged();
        }

        // Стару дію можна завершити лише після успішного створення заміни.
        // Expire the old action only after its replacement is successfully created.
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void RequestPendingAction_ReplacementCreationOverflows_DoesNotExpireOldAction(int kind)
        {
            var data = new FlowState();
            var action = data.AddAction();
            var user = data.Restore();
            var before = new FlowSnapshot(user, action);
            var now = DateTimeOffset.MaxValue.AddMinutes(-30).AddTicks(1);

            Assert.Throws<ArgumentOutOfRangeException>(() => FlowRequest(user, kind, now));

            before.AssertUnchanged();
        }

        #endregion

        #region RequestLoginChange and RequestAccountDeletion
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RequestLoginChange_EquivalentCurrentLogin_ReturnsNoUpdateBeforePendingActionCheck(bool hasAction)
        {
            var data = new FlowState();
            var old = hasAction ? data.AddAction() : null;
            var user = data.Restore();

            var before = new FlowSnapshot(
                user,
                old
            );

            var equivalent = FlowLogin();

            Assert.NotSame(
                user.Login,
                equivalent
            );

            FlowFailure(
                user.RequestLoginChange(equivalent, FlowTime),
                "PENDING_ACTION.NO_UPDATE_NEEDED",
                ErrorType.Conflict
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RequestLoginChange_NewPhoneOrRestoredHistoricalLogin_PreservesOriginalInstance(bool phone)
        {
            var user = new FlowState().Restore();
            var login = phone ? FlowPhoneLogin() : FlowLogin("serhii@mail.ru");

            var result = user.RequestLoginChange(
                login,
                FlowTime
            );

            FlowSuccess(result);

            Assert.Same(
                login,
                Assert.IsType<ChangeLoginAction>(result.Value.UserAction).NewLogin
            );

            var ev = Assert.IsType<UserLoginChangeRequestedEvent>(Assert.Single(user.DomainEvents));

            Assert.Same(
                login,
                ev.RequestedLogin
            );
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RequestLoginChange_NullLogin_PrecedesTimeAndAccessValidation(bool invalidTime)
        {
            var user = FlowAccessState(1).Restore();
            var before = new FlowSnapshot(user);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                user.RequestLoginChange(
                    null!,
                    invalidTime ? FlowTime.AddTicks(-1) : FlowTime
                )
            );

            FlowEmpty(
                exception,
                "newLogin"
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(null, "No deletion reason was provided.")]
        [InlineData("", "No deletion reason was provided.")]
        [InlineData(" ", "No deletion reason was provided.")]
        [InlineData(" Close account ", "Close account")]
        public void RequestAccountDeletion_OptionalReason_NormalizesThroughDeletionReason(
            string? raw,
            string expected)
        {
            var user = new FlowState().Restore();

            var result = user.RequestAccountDeletion(raw, FlowTime);

            FlowSuccess(result);

            var deletion = Assert.IsType<DeleteUserAction>(result.Value.UserAction);

            Assert.Equal(
                expected,
                deletion.Reason.Value
            );

            Assert.False(user.IsDeleted);
        }

        [Theory]
        [InlineData(600)]
        [InlineData(601)]
        public void RequestAccountDeletion_ReasonLengthBoundary_ValidatesBeforeReplacingExistingAction(int length)
        {
            var data = new FlowState();
            var old = data.AddAction();
            var user = data.Restore();
            var before = new FlowSnapshot(user, old);

            var result = user.RequestAccountDeletion(
                new string('x', length),
                old.ExpiresAt
            );

            if (length == 600)
            {
                FlowSuccess(result);

                Assert.Equal(
                    new string('x', 600),
                    Assert.IsType<DeleteUserAction>(result.Value.UserAction).Reason.Value
                );

                Assert.Equal(
                    ActionStatus.Expired,
                    old.Status
                );
            }
            else
            {
                FlowFailure(
                    result,
                    "USER_DELETION.TOO_LONG_REASON",
                    ErrorType.Validation
                );

                before.AssertUnchanged();
            }
        }
        #endregion

        #region RequestPasswordChange: verifier contract
        [Fact]
        public void RequestPasswordChange_ValidHash_PassesOriginalPlainAndBothHashesInOrder()
        {
            var originalHash = PasswordHash.Restore("OLD.HASH");
            var proposedHash = PasswordHash.Restore("NEW.HASH");
            var data = new FlowState { Password = Password.Restore(originalHash) };
            var user = data.Restore();
            var proposed = Password.Create(proposedHash);
            var plain = FlowPlain();
            var calls = new List<(PlainPassword Plain, PasswordHash Hash)>();

            var result = user.RequestPasswordChange(
                plain,
                proposed,
                (p, h) =>
                {
                    calls.Add((p, h));
                    return ReferenceEquals(h, proposedHash);
                },
                FlowTime
            );

            FlowSuccess(result);

            Assert.Equal(
                2,
                calls.Count
            );

            Assert.Same(
                plain,
                calls[0].Plain
            );

            Assert.Same(
                plain,
                calls[1].Plain
            );

            Assert.Same(
                proposedHash,
                calls[0].Hash
            );

            Assert.Same(
                originalHash,
                calls[1].Hash
            );

            Assert.Same(
                proposed,
                Assert.IsType<ChangePasswordAction>(result.Value.UserAction).NewPassword
            );

            Assert.Same(
                data.Password,
                user.Password
            );
        }

        [Theory]
        [InlineData(1, "plainNewPassword")]
        [InlineData(2, "newPassword")]
        [InlineData(3, "plainNewPassword")]
        [InlineData(4, "verifier")]
        [InlineData(5, "plainNewPassword")]
        [InlineData(6, "newPassword")]
        [InlineData(7, "plainNewPassword")]
        public void RequestPasswordChange_NullArguments_ReportFirstFieldWithoutCallingVerifier(
            int mask,
            string field)
        {
            var user = FlowAccessState(1).Restore();
            var calls = 0;

            Func<PlainPassword, PasswordHash, bool> verifier = (_, _) =>
            {
                calls++;
                return true;
            };

            var before = new FlowSnapshot(user);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                user.RequestPasswordChange(
                    (mask & 1) != 0 ? null! : FlowPlain(),
                    (mask & 2) != 0 ? null! : FlowPassword("NEW.HASH"),
                    (mask & 4) != 0 ? null! : verifier,
                    FlowTime.AddTicks(-1)
                )
            );

            FlowEmpty(
                exception,
                field
            );

            Assert.Equal(
                0,
                calls
            );

            before.AssertUnchanged();
        }

        [Fact]
        public void RequestPasswordChange_MismatchedProposedHash_ThrowsBeforeCheckingCurrentPassword()
        {
            var user = new FlowState().Restore();
            var before = new FlowSnapshot(user);
            var calls = 0;

            var exception = Assert.Throws<DomainInvalidOperationException>(() =>
                user.RequestPasswordChange(
                    FlowPlain(),
                    FlowPassword("NEW.HASH"),
                    (_, _) =>
                    {
                        calls++;
                        return false;
                    },
                    FlowTime
                )
            );

            FlowPrecondition(
                exception,
                "RequestPasswordChange"
            );

            Assert.Equal(
                "The supplied password hash must correspond to the supplied plain password.",
                exception.Context["Prerequisite"]
            );

            Assert.Equal(
                1,
                calls
            );

            before.AssertUnchanged();
        }

        [Fact]
        public void RequestPasswordChange_CurrentPasswordMatches_ReturnsNoUpdateBeforeActionCollision()
        {
            var data = new FlowState();
            var action = data.AddAction();
            var user = data.Restore();
            var before = new FlowSnapshot(user, action);
            var calls = 0;

            var result = user.RequestPasswordChange(
                FlowPlain(),
                FlowPassword("NEW.HASH"),
                (_, _) =>
                {
                    calls++;
                    return true;
                },
                FlowTime
            );

            FlowFailure(
                result,
                "PENDING_ACTION.NO_UPDATE_NEEDED",
                ErrorType.Conflict
            );

            Assert.Equal(
                2,
                calls
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void RequestPasswordChange_VerifierThrows_PropagatesSameExceptionWithoutMutation(int throwOnCall)
        {
            var user = new FlowState().Restore();
            var before = new FlowSnapshot(user);
            var expected = new InvalidOperationException("Verifier failed.");
            var calls = 0;

            var actual = Assert.Throws<InvalidOperationException>(() =>
                user.RequestPasswordChange(
                    FlowPlain(),
                    FlowPassword("NEW.HASH"),
                    (_, _) =>
                    {
                        calls++;

                        if (calls == throwOnCall)
                        {
                            throw expected;
                        }

                        return true;
                    },
                    FlowTime
                )
            );

            Assert.Same(
                expected,
                actual
            );

            Assert.Equal(
                throwOnCall,
                calls
            );

            before.AssertUnchanged();
        }
        #endregion

        #region Pending action request access and operation time
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
        public void RequestPendingAction_DeniedAccess_PrecedesOtherBusinessChecks(
            int kind,
            int accessState)
        {
            var data = FlowAccessState(accessState);
            var action = data.AddAction();
            var user = data.Restore();

            var before = new FlowSnapshot(
                user,
                action
            );

            FlowFailure(
                FlowRequest(user, kind, FlowTime),
                accessState == 2 ? "ACCESS_USER.USER_BLOCKED" : "ACCESS_USER.USER_DELETED",
                ErrorType.Forbidden
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RequestPasswordChange_DeniedAccessOrInvalidTime_DoesNotInvokeVerifier(bool invalidTime)
        {
            var user = (invalidTime ? new FlowState() : FlowAccessState(1)).Restore();
            var calls = 0;

            Func<PlainPassword, PasswordHash, bool> verifier = (_, _) =>
            {
                calls++;
                return false;
            };

            if (invalidTime)
            {
                Assert.Throws<DomainDataInconsistencyException>(() =>
                    user.RequestPasswordChange(
                        FlowPlain(),
                        FlowPassword("NEW.HASH"),
                        verifier,
                        FlowTime.AddTicks(-1)
                    )
                );
            }
            else
            {
                FlowFailure(
                    user.RequestPasswordChange(
                        FlowPlain(),
                        FlowPassword("NEW.HASH"),
                        verifier,
                        FlowTime
                    ),
                    "ACCESS_USER.USER_DELETED",
                    ErrorType.Forbidden
                );
            }

            Assert.Equal(0, calls);
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
        public void RequestPendingAction_MuteOrElapsedBan_AllowsCreation(
            int kind,
            RestrictionType type)
        {
            var data = new FlowState();
            data.AddGroup(type);
            var user = data.Restore();
            var now = type == RestrictionType.Ban ? FlowTime.AddMinutes(10) : FlowTime;

            FlowSuccess(FlowRequest(
                user,
                kind,
                now)
            );

            Assert.Single(user.RestrictionSessions);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RequestLoginChange_TimeBeforeAggregateTimestamp_ThrowsWithoutChanges(bool afterCreation)
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
                user.RequestLoginChange(FlowLogin(
                    "next@example.com"),
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
        public void RequestPasswordChange_TimeBeforeAggregateTimestamp_ThrowsWithoutChanges(bool afterCreation)
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
                FlowRequest(
                    user,
                    1,
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
        public void RequestAccountDeletion_TimeBeforeAggregateTimestamp_ThrowsWithoutChanges(bool afterCreation)
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
                user.RequestAccountDeletion(
                    "Reason",
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
