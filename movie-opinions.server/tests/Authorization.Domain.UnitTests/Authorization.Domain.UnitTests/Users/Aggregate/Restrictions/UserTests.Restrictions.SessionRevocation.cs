using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Users.Aggregate;
using Authorization.Domain.Users.AggregateChanges.Restriction;
using Authorization.Domain.Users.AggregateChanges.SessionRestriction;
using Authorization.Domain.Users.DomainEvents.SessionRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction.Enums;
using Authorization.Domain.Users.Enums;

namespace Authorization.Domain.UnitTests.Users.Aggregate
{
    public partial class UserTests
    {
        #region RevokeRestrictionSession: active and elapsed sessions
        [Theory]
        [InlineData(RestrictionType.Ban, 1, -1L, false)]
        [InlineData(RestrictionType.Ban, 1, 0L, false)]
        [InlineData(RestrictionType.Ban, 1, 1L, false)]
        [InlineData(RestrictionType.Ban, 3, -1L, false)]
        [InlineData(RestrictionType.Ban, 3, 0L, false)]
        [InlineData(RestrictionType.Ban, 3, 1L, false)]
        [InlineData(RestrictionType.CommentMute, 1, -1L, false)]
        [InlineData(RestrictionType.CommentMute, 1, 0L, false)]
        [InlineData(RestrictionType.CommentMute, 1, 1L, false)]
        [InlineData(RestrictionType.CommentMute, 3, -1L, false)]
        [InlineData(RestrictionType.CommentMute, 3, 0L, false)]
        [InlineData(RestrictionType.CommentMute, 3, 1L, false)]
        [InlineData(RestrictionType.ChatMute, 1, -1L, false)]
        [InlineData(RestrictionType.ChatMute, 1, 0L, false)]
        [InlineData(RestrictionType.ChatMute, 1, 1L, false)]
        [InlineData(RestrictionType.ChatMute, 3, -1L, false)]
        [InlineData(RestrictionType.ChatMute, 3, 0L, false)]
        [InlineData(RestrictionType.ChatMute, 3, 1L, false)]
        [InlineData(RestrictionType.AllMute, 1, -1L, false)]
        [InlineData(RestrictionType.AllMute, 1, 0L, false)]
        [InlineData(RestrictionType.AllMute, 1, 1L, false)]
        [InlineData(RestrictionType.AllMute, 3, -1L, false)]
        [InlineData(RestrictionType.AllMute, 3, 0L, false)]
        [InlineData(RestrictionType.AllMute, 3, 1L, false)]
        [InlineData(RestrictionType.ChatMute, 1, 0L, true)]
        public void RevokeRestrictionSession_ExpirationBoundary_RevokesOrCompletesEntireGroup(
            RestrictionType type,
            int count,
            long ticks,
            bool atCreation
        )
        {
            var data = new FlowState();
            var session = data.AddGroup(
                type,
                Enumerable.Repeat(10, count).ToArray()
            );

            var targets = data.Restrictions.ToArray();
            var otherType = (RestrictionType)(((int)type + 1) % 4);
            var otherSession = data.AddGroup(otherType, new[] { 60 });
            var otherRestriction = data.Restrictions.Last();
            var user = data.Restore();
            var now = atCreation ? FlowTime : FlowTime.AddMinutes(count * 10).AddTicks(ticks);

            FlowSuccess(user.RevokeRestrictionSession(
                type,
                now)
            );

            var expired = !atCreation && ticks >= 0;

            Assert.All(
                targets,
                x =>
                {
                    Assert.Equal(expired ? RestrictionStatus.Completed : RestrictionStatus.Revoked, x.Status);
                    Assert.Equal(expired ? (DateTimeOffset?)null : now, x.RevokedAt);
                }
            );

            Assert.Same(
                otherRestriction,
                Assert.Single(user.Restrictions)
            );

            Assert.Same(
                otherSession,
                Assert.Single(user.RestrictionSessions)
            );

            Assert.Equal(
                RestrictionStatus.Active,
                otherRestriction.Status
            );

            Assert.Equal(
                60,
                otherSession.TotalBlockedMinutes
            );

            if (expired)
            {
                var ev = Assert.IsType<UserRestrictionSessionCompletedEvent>(Assert.Single(user.DomainEvents));

                Assert.Same(
                    session.Id,
                    ev.UserRestrictionSessionId
                );

                Assert.Same(
                    user.Login,
                    ev.Login
                );

                Assert.Equal(
                    type,
                    ev.RestrictionType
                );

                Assert.Equal(
                    now,
                    ev.OccurredOn
                );
            }
            else
            {
                var ev = Assert.IsType<UserRestrictionSessionRevokedEvent>(Assert.Single(user.DomainEvents));

                Assert.Same(
                    session.Id,
                    ev.UserRestrictionSessionId
                );

                Assert.Same(
                    user.Login,
                    ev.Login
                );

                Assert.Equal(
                    type,
                    ev.RestrictionType
                );

                Assert.Equal(
                    now,
                    ev.OccurredOn
                );
            }

            Assert.Equal(
                count + 1,
                user.AggregateChanges.Count
            );

            var changes = user.AggregateChanges.OfType<UserRestrictionUpdated>().ToArray();

            FlowSameItems(
                targets,
                changes.Select(x => x.UserRestriction)
            );

            var removed = Assert.Single(user.AggregateChanges.OfType<UserRestrictionSessionDeleted>());

            Assert.Same(
                session.Id,
                removed.UserRestrictionSessionId
            );

            Assert.All(
                user.AggregateChanges,
                x => Assert.Equal(now, x.OccurredOn)
            );

            Assert.Equal(
                now,
                user.UpdatedAt
            );

            var before = new FlowSnapshot(user);

            FlowFailure(
                user.RevokeRestrictionSession(type, now.AddTicks(1)),
                "RESTRICTION_USER.NOT_FOUND_SESSION",
                ErrorType.NotFound
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(RestrictionType.Ban)]
        [InlineData(RestrictionType.CommentMute)]
        [InlineData(RestrictionType.ChatMute)]
        [InlineData(RestrictionType.AllMute)]
        public void RevokeRestrictionSession_MissingType_ReturnsNotFoundWithoutChanges(RestrictionType type)
        {
            var user = new FlowState().Restore();
            var before = new FlowSnapshot(user);

            var result = user.RevokeRestrictionSession(
                type,
                FlowTime
            );

            FlowFailure(
                result,
                "RESTRICTION_USER.NOT_FOUND_SESSION",
                ErrorType.NotFound
            );

            Assert.Contains(
                type.ToString(),
                Assert.Single(result.Errors).Message
            );

            before.AssertUnchanged();
        }

        [Fact]
        public void RevokeRestrictionSession_DeletedUser_DoesNotPreventCleanup()
        {
            var data = FlowAccessState(3);
            var user = data.Restore();

            FlowSuccess(user.RevokeRestrictionSession(
                RestrictionType.Ban,
                FlowTime)
            );

            Assert.True(user.IsDeleted);
            Assert.Empty(user.Restrictions);
            Assert.Empty(user.RestrictionSessions);
            Assert.Empty(user.RefreshTokens);
        }
        #endregion

        #region RevokeRestrictionSession: guards and failed preparation
        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        [InlineData(2147483647)]
        public void RevokeRestrictionSession_UndefinedType_PrecedesTimestampValidation(int value)
        {
            var user = new FlowState().Restore();
            var before = new FlowSnapshot(user);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                user.RevokeRestrictionSession(
                    (RestrictionType)value,
                    FlowTime.AddTicks(-1)
                )
            );

            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.UNSUPPORTED_TYPE",
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
                "User",
                exception.Context["Type"]
            );

            Assert.Equal(
                "Delete",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "restrictionType",
                exception.Context["Field"]
            );

            before.AssertUnchanged();
        }

        [Fact]
        public void RevokeRestrictionSession_SessionIdsDiffer_ThrowsWithoutChangingAggregate()
        {
            var data = new FlowState();
            var session = data.AddGroup(minutes: new[] { 10, 20 });
            var user = data.Restore();

            session.RemoveRestrictions(
                new[] { data.Restrictions[1] },
                FlowTime
            );

            var before = new FlowSnapshot(user);

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                user.RevokeRestrictionSession(
                    RestrictionType.ChatMute,
                    FlowTime
                )
            );

            FlowInvariant(
                exception,
                operation: "Delete"
            );

            Assert.Equal(
                "The restriction session does not exactly match the active restrictions of its type.",
                exception.Context["Rule"]
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void RevokeRestrictionSession_TerminalRestriction_DoesNotPartiallyChangeEarlierItems(
            bool elapsed,
            bool completed
        )
        {
            var data = new FlowState();
            data.AddGroup(minutes: new[] { 10, 20 });
            var invalid = data.Restrictions[1];
            var user = data.Restore();

            if (completed)
            {
                invalid.MarkAsCompleted(FlowTime);
            }
            else
            {
                invalid.RevokeRestriction(FlowTime);
            }

            var before = new FlowSnapshot(user);
            var now = FlowTime.AddMinutes(elapsed ? 30 : 1);

            if (elapsed)
            {
                var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                    user.RevokeRestrictionSession(
                        RestrictionType.ChatMute,
                        now
                    )
                );

                FlowInvariant(
                    exception,
                    "UserRestriction"
                );
            }
            else
            {
                FlowFailure(
                    user.RevokeRestrictionSession(RestrictionType.ChatMute, now),
                    completed ? "RESTRICTION.ALREADY_COMPLETED" : "RESTRICTION.ALREADY_REVOKED",
                    ErrorType.Conflict
                );
            }

            before.AssertUnchanged();
        }

        [Fact]
        public void RevokeRestrictionSession_PreservesPreviouslyRecordedHistory()
        {
            var user = User.Create(FlowLogin(), FlowPassword(), FlowTime);

            FlowSuccess(user.AddRestrictions(new[] { FlowData() }, FlowTime));

            var events = user.DomainEvents.ToArray();
            var changes = user.AggregateChanges.ToArray();

            FlowSuccess(user.RevokeRestrictionSession(
                RestrictionType.ChatMute,
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
                changes.Length + 2,
                user.AggregateChanges.Count
            );
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RevokeRestrictionSession_TimeBeforeAggregateTimestamp_ThrowsWithoutChanges(bool afterCreation)
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
                user.RevokeRestrictionSession(
                    RestrictionType.ChatMute,
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
