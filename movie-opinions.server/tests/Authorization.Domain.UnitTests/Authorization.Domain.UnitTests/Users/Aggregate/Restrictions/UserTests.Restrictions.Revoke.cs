using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Users.AggregateChanges.Restriction;
using Authorization.Domain.Users.AggregateChanges.SessionRestriction;
using Authorization.Domain.Users.AggregateChanges.Tokens;
using Authorization.Domain.Users.DomainEvents.SessionRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction.Enums;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects;
using Authorization.Domain.Users.Enums;

namespace Authorization.Domain.UnitTests.Users.Aggregate
{
    public partial class UserTests
    {
        #region RemoveRestrictions: removal effects
        // Після скорочення тривалості перевіряємо нову та початкову межі завершення.
        // After shortening the duration, check both the new and original expiration boundaries.
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void RemoveRestrictions_ReducedExpirationBoundary_SelectsCorrectPlanAndStatuses(int boundary)
        {
            var data = new FlowState();
            var session = data.AddGroup(minutes: new[] { 10, 20 });
            var requested = data.Restrictions[0];
            var remaining = data.Restrictions[1];
            var otherSession = data.AddGroup(RestrictionType.AllMute, new[] { 60 });
            var other = data.Restrictions[2];
            var user = data.Restore();

            var now = boundary switch
            {
                0 => FlowTime.AddMinutes(20).AddTicks(-1),
                1 => FlowTime.AddMinutes(20),
                2 => FlowTime.AddMinutes(20).AddTicks(1),
                3 => FlowTime.AddMinutes(30).AddTicks(-1),
                4 => FlowTime.AddMinutes(30),
                _ => FlowTime.AddMinutes(30).AddTicks(1),
            };

            var id = UserRestrictionId.Restore(requested.Id.Value);

            FlowSuccess(user.RemoveRestrictions(new[] { id }, now));

            Assert.Equal(
                boundary >= 4 ? RestrictionStatus.Completed : RestrictionStatus.Revoked,
                requested.Status
            );

            Assert.Equal(
                boundary < 4 ? now : (DateTimeOffset?)null,
                requested.RevokedAt
            );

            Assert.Equal(
                boundary == 0 ? RestrictionStatus.Active : RestrictionStatus.Completed,
                remaining.Status
            );

            Assert.Null(remaining.RevokedAt);

            Assert.Same(
                other,
                user.Restrictions.Last()
            );

            Assert.Equal(
                RestrictionStatus.Active,
                other.Status
            );

            Assert.Contains(
                user.RestrictionSessions,
                x => ReferenceEquals(x, otherSession)
            );

            Assert.Equal(
                60,
                otherSession.TotalBlockedMinutes
            );

            if (boundary == 0)
            {
                Assert.Equal(
                    2,
                    user.Restrictions.Count
                );

                Assert.Equal(
                    2,
                    user.RestrictionSessions.Count
                );

                Assert.Equal(
                    20,
                    session.TotalBlockedMinutes
                );

                Assert.Equal(
                    remaining.Id,
                    Assert.Single(session.ActiveRestrictionIds)
                );

                var ev = Assert.IsType<UserRestrictionsRemovedFromSessionEvent>(Assert.Single(user.DomainEvents));

                Assert.Same(
                    session.Id,
                    ev.UserRestrictionSessionId
                );

                Assert.Same(
                    user.Login,
                    ev.Login
                );

                Assert.Equal
                    (RestrictionType.ChatMute,
                    ev.RestrictionType
                );

                Assert.Same(
                    requested,
                    Assert.Single(ev.UserRestrictions)
                );

                Assert.Equal(
                    20,
                    ev.TotalBlockedMinutes
                );

                Assert.Equal(
                    now,
                    ev.OccurredOn
                );

                Assert.Equal(
                    2,
                    user.AggregateChanges.Count
                );

                Assert.Same(
                    requested,
                    Assert.IsType<UserRestrictionUpdated>(user.AggregateChanges[0]).UserRestriction
                );

                Assert.Same(
                    session,
                    Assert.IsType<UserRestrictionSessionUpdated>(user.AggregateChanges[1]).UserRestrictionSession
                );
            }
            else
            {
                Assert.Same(
                    other,
                    Assert.Single(user.Restrictions)
                );

                Assert.Same(
                    otherSession,
                    Assert.Single(user.RestrictionSessions)
                );

                Assert.Equal(
                    0,
                    session.TotalBlockedMinutes
                );

                Assert.Empty(session.ActiveRestrictionIds);

                var ev = Assert.IsType<UserRestrictionSessionCompletedEvent>(Assert.Single(user.DomainEvents));

                Assert.Same(
                    session.Id,
                    ev.UserRestrictionSessionId
                );

                Assert.Equal(
                    now,
                    ev.OccurredOn
                );

                Assert.Equal(
                    3,
                    user.AggregateChanges.Count
                );

                var updates = user.AggregateChanges.OfType<UserRestrictionUpdated>().ToArray();

                Assert.Equal(
                    2,
                    updates.Length
                );

                Assert.True(
                    new[] { requested.Id, remaining.Id }
                        .ToHashSet()
                        .SetEquals(updates.Select(x => x.UserRestriction.Id))
                );

                var deleted = Assert.Single(user.AggregateChanges.OfType<UserRestrictionSessionDeleted>());

                Assert.Same(
                    session.Id,
                    deleted.UserRestrictionSessionId
                );
            }

            Assert.Equal(
                now,
                user.UpdatedAt
            );

            Assert.All(
                user.AggregateChanges,
                x => Assert.Equal(now, x.OccurredOn)
            );
        }

        [Theory]
        [InlineData(RestrictionType.Ban, 1)]
        [InlineData(RestrictionType.Ban, 3)]
        [InlineData(RestrictionType.CommentMute, 1)]
        [InlineData(RestrictionType.CommentMute, 3)]
        [InlineData(RestrictionType.ChatMute, 1)]
        [InlineData(RestrictionType.ChatMute, 3)]
        [InlineData(RestrictionType.AllMute, 1)]
        [InlineData(RestrictionType.AllMute, 3)]
        public void RemoveRestrictions_AllBeforeExpiration_RevokesSessionAndEveryRestriction(
            RestrictionType type,
            int count
        )
        {
            var data = new FlowState();
            var session = data.AddGroup(type, Enumerable.Repeat(10, count).ToArray());
            var targets = data.Restrictions.ToArray();
            var user = data.Restore();

            var ids = new FlowSingleUse<UserRestrictionId>(
                targets.Reverse().Select(x => UserRestrictionId.Restore(x.Id.Value))
            );

            var now = FlowTime.AddMinutes(1);

            FlowSuccess(user.RemoveRestrictions(
                ids,
                now)
            );

            Assert.Equal(
                1,
                ids.EnumerationCount
            );

            Assert.Empty(user.Restrictions);
            Assert.Empty(user.RestrictionSessions);
            Assert.Empty(session.ActiveRestrictionIds);

            Assert.Equal(
                0,
                session.TotalBlockedMinutes
            );

            Assert.All(
                targets,
                x =>
                {
                    Assert.Equal(RestrictionStatus.Revoked, x.Status);
                    Assert.Equal(now, x.RevokedAt);
                }
            );

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

            Assert.Equal(
                count + 1,
                user.AggregateChanges.Count
            );

            Assert.Equal(
                count,
                user.AggregateChanges.OfType<UserRestrictionUpdated>().Count()
            );

            Assert.Same(
                session.Id,
                Assert.Single(user.AggregateChanges.OfType<UserRestrictionSessionDeleted>()).UserRestrictionSessionId
            );

            Assert.Equal(
                now,
                user.UpdatedAt
            );
        }

        [Fact]
        public void RemoveRestrictions_MultipleTypes_AppliesIndependentRemovalPlans()
        {
            var data = new FlowState();
            var active = data.AddGroup(minutes: new[] { 10, 20 });
            var first = data.Restrictions[0];
            data.AddGroup(RestrictionType.AllMute, new[] { 5 });
            var expired = data.Restrictions[2];
            data.AddGroup(RestrictionType.CommentMute, new[] { 10 });
            var last = data.Restrictions[3];
            var user = data.Restore();

            FlowSuccess(user.RemoveRestrictions(new[] { first.Id, expired.Id, last.Id }, FlowTime.AddMinutes(5)));

            Assert.Single(user.Restrictions);

            Assert.Same(
                active,
                Assert.Single(user.RestrictionSessions)
            );

            Assert.Equal(
                20,
                active.TotalBlockedMinutes
            );

            Assert.Equal(
                RestrictionStatus.Revoked,
                first.Status
            );

            Assert.Equal(
                RestrictionStatus.Completed,
                expired.Status
            );

            Assert.Equal(
                RestrictionStatus.Revoked,
                last.Status
            );

            Assert.Equal(
                3,
                user.DomainEvents.Count
            );

            Assert.Single(user.DomainEvents.OfType<UserRestrictionsRemovedFromSessionEvent>());
            Assert.Single(user.DomainEvents.OfType<UserRestrictionSessionCompletedEvent>());
            Assert.Single(user.DomainEvents.OfType<UserRestrictionSessionRevokedEvent>());

            Assert.Equal(
                6,
                user.AggregateChanges.Count
            );
        }
        #endregion

        #region RemoveRestrictions: failures and atomicity
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RemoveRestrictions_NullCollection_PrecedesTimeValidation(bool invalidTime)
        {
            var user = new FlowState().Restore();
            var before = new FlowSnapshot(user);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                user.RemoveRestrictions(
                    null!,
                    invalidTime ? FlowTime.AddTicks(-1) : FlowTime
                )
            );

            FlowEmpty(
                exception,
                "restrictionIds"
            );

            before.AssertUnchanged();
        }

        [Fact]
        public void RemoveRestrictions_EmptyCollection_ReturnsValidationFailure()
        {
            var user = new FlowState().Restore();
            var before = new FlowSnapshot(user);

            FlowFailure(
                user.RemoveRestrictions(Array.Empty<UserRestrictionId>(), FlowTime),
                "RESTRICTION_USER.EMPTY_RESTRICTION_LIST",
                ErrorType.Validation
            );

            before.AssertUnchanged();
        }

        [Fact]
        public void RemoveRestrictions_NullIdentifiers_ReportCountsBeforeDuplicateOrLookupChecks()
        {
            var user = new FlowState().Restore();
            var id = UserRestrictionId.Restore(FlowGuid(102));
            var before = new FlowSnapshot(user);

            var exception = Assert.Throws<DomainInvalidOperationException>(() =>
                user.RemoveRestrictions(
                    new[] { id, id, null!, null! },
                    FlowTime
                )
            );

            FlowPrecondition(
                exception,
                "RemoveRestrictions"
            );

            Assert.Equal(
                4,
                exception.Context["Custom_RestrictionIdentifierCount"]
            );

            Assert.Equal(
                2,
                exception.Context["Custom_NullIdentifierCount"]
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RemoveRestrictions_EquivalentDuplicateIds_ReturnsDuplicateFailureBeforeLookup(bool exists)
        {
            var data = new FlowState();
            data.AddGroup();
            var user = data.Restore();
            var value = exists ? data.Restrictions[0].Id.Value : FlowGuid(102, 99);
            var ids = new[] { UserRestrictionId.Restore(value), UserRestrictionId.Restore(value) };
            var before = new FlowSnapshot(user);

            FlowFailure(
                user.RemoveRestrictions(ids, FlowTime),
                "RESTRICTION_USER.DUPLICATED_RESTRICTION_IDENTIFIERS",
                ErrorType.Validation
            );

            before.AssertUnchanged();
        }

        [Fact]
        public void RemoveRestrictions_MissingIdentifiers_DoesNotPartiallyRemoveMatchedItems()
        {
            var data = new FlowState();
            data.AddGroup();
            var known = Assert.Single(data.Restrictions).Id;
            var missing1 = UserRestrictionId.Restore(FlowGuid(102, 98));
            var missing2 = UserRestrictionId.Restore(FlowGuid(102, 99));
            var user = data.Restore();
            var before = new FlowSnapshot(user);

            var result = user.RemoveRestrictions(new[] { known, missing1, missing2 }, FlowTime);

            FlowFailure(
                result,
                "RESTRICTION_USER.NOT_FOUND_RESTRICTION",
                ErrorType.NotFound
            );

            var message = Assert.Single(result.Errors).Message;

            Assert.Contains(
                missing1.Value.ToString(),
                message
            );

            Assert.Contains(
                missing2.Value.ToString(),
                message
            );

            Assert.DoesNotContain(
                known.Value.ToString(),
                message
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(0, true)]
        [InlineData(1, false)]
        [InlineData(1, true)]
        [InlineData(2, false)]
        [InlineData(2, true)]
        [InlineData(3, false)]
        [InlineData(3, true)]
        public void RemoveRestrictions_TerminalRestriction_FailsWithoutApplyingAnyPlan(int plan, bool completed)
        {
            var data = new FlowState();
            data.AddGroup(minutes: new[] { 10, 20 });
            var target = data.Restrictions[0];
            var user = data.Restore();

            if (completed)
            {
                target.MarkAsCompleted(FlowTime);
            }
            else
            {
                target.RevokeRestriction(FlowTime);
            }

            var before = new FlowSnapshot(user);

            var now = FlowTime.AddMinutes(
                plan == 2 ? 20
                : plan == 3 ? 30
                : 5
            );

            var ids = plan == 1 ? data.Restrictions.Select(x => x.Id).ToArray() : new[] { target.Id };

            if (plan == 3)
            {
                var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                    user.RemoveRestrictions(
                        ids,
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
                    user.RemoveRestrictions(ids, now),
                    completed ? "RESTRICTION.ALREADY_COMPLETED" : "RESTRICTION.ALREADY_REVOKED",
                    ErrorType.Conflict
                );
            }

            before.AssertUnchanged();
        }

        // Помилка пізнішої групи не повинна застосувати вже підготовлену попередню.
        // A later group failure must not apply an earlier prepared group.
        [Fact]
        public void RemoveRestrictions_LaterGroupFails_PreparedEarlierGroupIsUnchanged()
        {
            var data = new FlowState();
            data.AddGroup(minutes: new[] { 10, 20 });
            data.AddGroup(RestrictionType.AllMute, new[] { 10, 20 });
            var first = data.Restrictions[0];
            var invalid = data.Restrictions[2];
            var user = data.Restore();
            invalid.RevokeRestriction(FlowTime);
            var before = new FlowSnapshot(user);

            FlowFailure(
                user.RemoveRestrictions(new[] { first.Id, invalid.Id }, FlowTime.AddMinutes(5)),
                "RESTRICTION.ALREADY_REVOKED",
                ErrorType.Conflict
            );

            before.AssertUnchanged();
        }

        [Fact]
        public void RemoveRestrictions_SessionIdsDiffer_ThrowsBeforeChangingRestrictions()
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
                user.RemoveRestrictions(
                    new[] { data.Restrictions[0].Id },
                    FlowTime
                )
            );

            FlowInvariant(exception);

            Assert.Equal(
                "The restriction session does not exactly match the active restrictions of its type.",
                exception.Context["Rule"]
            );

            before.AssertUnchanged();
        }

        [Fact]
        public void RemoveRestrictions_DeletedAndBannedUser_AllowsCleanupWithoutResurrectingTokens()
        {
            var data = FlowAccessState(3);
            var user = data.Restore();

            FlowSuccess(user.RemoveRestrictions(
                data.Restrictions.Select(x => x.Id),
                FlowTime)
            );

            Assert.True(user.IsDeleted);
            Assert.Empty(user.Restrictions);
            Assert.Empty(user.RestrictionSessions);
            Assert.Empty(user.RefreshTokens);
            Assert.Empty(user.AggregateChanges.OfType<UserRefreshTokenCreated>());
        }

        private static IEnumerable<UserRestrictionId> FlowThrowingRestrictionIds(
            UserRestrictionId id,
            Exception failure
        )
        {
            yield return id;
            throw failure;
        }

        [Fact]
        public void RemoveRestrictions_EnumerationThrows_PropagatesSameExceptionWithoutChanges()
        {
            var data = new FlowState();
            data.AddGroup();
            var user = data.Restore();
            var before = new FlowSnapshot(user);
            var expected = new InvalidOperationException("Source failure.");

            var actual = Assert.Throws<InvalidOperationException>(() =>
                user.RemoveRestrictions(
                    FlowThrowingRestrictionIds(
                        data.Restrictions[0].Id,
                        expected
                    ),
                    FlowTime
                )
            );

            Assert.Same(
                expected,
                actual
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RemoveRestrictions_TimeBeforeAggregateTimestamp_ThrowsWithoutChanges(bool afterCreation)
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
                user.RemoveRestrictions(
                    Array.Empty<UserRestrictionId>(),
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
