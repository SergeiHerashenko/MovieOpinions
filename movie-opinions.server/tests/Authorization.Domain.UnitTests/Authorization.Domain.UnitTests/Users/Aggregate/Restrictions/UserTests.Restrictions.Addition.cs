using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Users.Aggregate;
using Authorization.Domain.Users.AggregateChanges.Restriction;
using Authorization.Domain.Users.AggregateChanges.SessionRestriction;
using Authorization.Domain.Users.AggregateChanges.Tokens;
using Authorization.Domain.Users.Contracts;
using Authorization.Domain.Users.DomainEvents.SessionRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction.Enums;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction;
using Authorization.Domain.Users.Enums;

namespace Authorization.Domain.UnitTests.Users.Aggregate
{
    public partial class UserTests
    {
        #region AddRestrictions: new and existing sessions
        [Theory]
        [InlineData(RestrictionType.Ban, 1)]
        [InlineData(RestrictionType.Ban, 2)]
        [InlineData(RestrictionType.CommentMute, 1)]
        [InlineData(RestrictionType.CommentMute, 2)]
        [InlineData(RestrictionType.ChatMute, 1)]
        [InlineData(RestrictionType.ChatMute, 2)]
        [InlineData(RestrictionType.AllMute, 1)]
        [InlineData(RestrictionType.AllMute, 2)]
        public void AddRestrictions_NewGroup_CreatesRestrictionsSessionEventAndChanges(RestrictionType type, int count)
        {
            var user = new FlowState().Restore();

            var inputs = Enumerable
                .Range(1, count)
                .Select(i => new RestrictionData
                {
                    RestrictionType = type,
                    RestrictionRule = RestrictionRule.Restore($"Rule {i}", i * 3),
                    ImposedBy = $"Admin {i}",
                    Reason = i == 1 ? null : "Reason",
                })
                .ToArray();

            var now = FlowTime.AddMinutes(1).ToOffset(TimeSpan.FromHours(2));

            FlowSuccess(user.AddRestrictions(
                inputs,
                now)
            );

            var restrictions = user.Restrictions.ToArray();

            Assert.Equal(
                count,
                restrictions.Length
            );

            for (var i = 0; i < count; i++)
            {
                Assert.Same(
                    user.Id,
                    restrictions[i].UserId
                );

                Assert.Same(
                    inputs[i].RestrictionRule,
                    restrictions[i].RestrictionRule
                );

                Assert.Equal(
                    inputs[i].ImposedBy,
                    restrictions[i].ImposedBy
                );

                Assert.Equal(
                    inputs[i].Reason,
                    restrictions[i].Reason
                );

                Assert.Equal(
                    type,
                    restrictions[i].RestrictionType
                );

                Assert.Equal(
                    RestrictionStatus.Active,
                    restrictions[i].Status
                );

                Assert.Null(restrictions[i].RevokedAt);

                Assert.Equal(
                    now,
                    restrictions[i].CreatedAt
                );

                Assert.Equal(
                    now.Offset,
                    restrictions[i].CreatedAt.Offset
                );
            }

            var expectedMinutes = count == 1 ? 3 : 9;
            var session = Assert.Single(user.RestrictionSessions);

            Assert.Same(
                user.Id,
                session.UserId
            );

            Assert.Equal(
                type,
                session.RestrictionType
            );

            Assert.Equal(
                now,
                session.CreatedAt
            );

            Assert.Equal(
                expectedMinutes,
                session.TotalBlockedMinutes
            );

            Assert.True(restrictions.Select(x => x.Id).ToHashSet().SetEquals(session.ActiveRestrictionIds));

            var ev = Assert.IsType<UserRestrictionSessionCreatedEvent>(Assert.Single(user.DomainEvents));

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
                expectedMinutes,
                ev.TotalBlockedMinutes
            );

            Assert.Equal(
                now,
                ev.OccurredOn
            );

            Assert.Equal(
                count,
                ev.RestrictionDescriptions.Count
            );

            for (var i = 0; i < count; i++)
            {
                Assert.Same(
                    inputs[i].RestrictionRule,
                    ev.RestrictionDescriptions.ElementAt(i).Rule
                );

                Assert.Equal(
                    inputs[i].Reason,
                    ev.RestrictionDescriptions.ElementAt(i).Reason
                );
            }

            Assert.Equal(
                count + 1,
                user.AggregateChanges.Count
            );

            Assert.Same(
                session,
                Assert.IsType<UserRestrictionSessionCreated>(user.AggregateChanges[0]).UserRestrictionSession
            );

            for (var i = 0; i < count; i++)
            {
                Assert.Same(
                    restrictions[i],
                    Assert.IsType<UserRestrictionCreated>(user.AggregateChanges[i + 1]).Restriction
                );
            }

            Assert.All(
                user.AggregateChanges,
                x => Assert.Equal(now, x.OccurredOn)
            );

            Assert.Equal(
                now,
                user.UpdatedAt
            );
        }

        [Fact]
        public void AddRestrictions_InterleavedTypes_GroupsDescriptionsAndEnumeratesSourceOnce()
        {
            var user = new FlowState().Restore();

            var values = new[]
            {
                FlowData(RestrictionType.ChatMute, 3),
                FlowData(RestrictionType.Ban, 5),
                FlowData(RestrictionType.ChatMute, 7),
                FlowData(RestrictionType.AllMute, 11),
                FlowData(RestrictionType.CommentMute, 13),
            };

            var input = new FlowSingleUse<RestrictionData>(values);

            FlowSuccess(user.AddRestrictions(
                input,
                FlowTime)
            );

            Assert.Equal(
                1,
                input.EnumerationCount
            );

            Assert.Equal(
                5,
                user.Restrictions.Count
            );

            Assert.Equal(
                4,
                user.RestrictionSessions.Count
            );

            Assert.Equal(
                4,
                user.DomainEvents.Count
            );

            Assert.Equal(
                9,
                user.AggregateChanges.Count
            );

            foreach (
                var expected in new[]
                {
                    (RestrictionType.ChatMute, 10, 2),
                    (RestrictionType.Ban, 5, 1),
                    (RestrictionType.AllMute, 11, 1),
                    (RestrictionType.CommentMute, 13, 1),
                }
            )
            {
                var session = Assert.Single(user.RestrictionSessions.Where(x => x.RestrictionType == expected.Item1));

                Assert.Equal(
                    expected.Item2,
                    session.TotalBlockedMinutes
                );

                Assert.Equal(
                    expected.Item3,
                    session.ActiveRestrictionIds.Count
                );

                var ev = Assert.Single(
                    user.DomainEvents.OfType<UserRestrictionSessionCreatedEvent>()
                        .Where(x => x.RestrictionType == expected.Item1)
                );

                Assert.Equal(
                    expected.Item3,
                    ev.RestrictionDescriptions.Count
                );

                Assert.Equal(
                    expected.Item2,
                    ev.TotalBlockedMinutes
                );
            }
        }

        [Fact]
        public void AddRestrictions_RepeatedDataObject_CreatesSeparateRestrictionIdentifiers()
        {
            var user = new FlowState().Restore();
            var data = FlowData();

            FlowSuccess(user.AddRestrictions(
                new[] { data, data },
                FlowTime)
            );

            Assert.Equal(
                2,
                user.Restrictions.Count
            );

            Assert.Equal(
                2,
                user.Restrictions.Select(x => x.Id).Distinct().Count()
            );

            Assert.Equal(
                20,
                Assert.Single(user.RestrictionSessions).TotalBlockedMinutes
            );
        }

        [Theory]
        [InlineData(RestrictionType.Ban)]
        [InlineData(RestrictionType.ChatMute)]
        public void AddRestrictions_ActiveSession_AppendsRestrictionsWithoutReplacingSession(RestrictionType type)
        {
            var data = new FlowState();
            var session = data.AddGroup(type);
            var existing = Assert.Single(data.Restrictions);
            var user = data.Restore();
            var now = FlowTime.AddMinutes(5);
            var rule1 = FlowData(type, 3);
            var rule2 = FlowData(type, 7);

            FlowSuccess(user.AddRestrictions(
                new[] { rule1, rule2 },
                now)
            );

            Assert.Same(
                session,
                Assert.Single(user.RestrictionSessions)
            );

            Assert.Equal(
                FlowTime,
                session.CreatedAt
            );

            Assert.Equal(
                20,
                session.TotalBlockedMinutes
            );

            Assert.Equal(
                3,
                session.ActiveRestrictionIds.Count
            );

            Assert.Same(
                existing,
                user.Restrictions.First()
            );

            Assert.Equal(
                RestrictionStatus.Active,
                existing.Status
            );

            Assert.Equal(
                3,
                user.Restrictions.Count
            );

            var ev = Assert.IsType<UserRestrictionsAddedToSessionEvent>(Assert.Single(user.DomainEvents));

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
                20,
                ev.TotalBlockedMinutes
            );

            Assert.Equal(
                2,
                ev.RestrictionDescriptions.Count
            );

            Assert.Same(
                rule1.RestrictionRule,
                ev.RestrictionDescriptions.First().Rule
            );

            Assert.Same(
                rule2.RestrictionRule,
                ev.RestrictionDescriptions.Last().Rule
            );

            Assert.Equal(
                now,
                ev.OccurredOn
            );
            Assert.Equal(
                3,
                user.AggregateChanges.Count
            );

            Assert.Same(
                session,
                Assert.IsType<UserRestrictionSessionUpdated>(user.AggregateChanges[0]).UserRestrictionSession
            );

            Assert.Equal(
                2,
                user.AggregateChanges.OfType<UserRestrictionCreated>().Count()
            );

            Assert.Equal(
                now,
                user.UpdatedAt
            );
        }

        [Theory]
        [InlineData(-1L)]
        [InlineData(0L)]
        [InlineData(1L)]
        public void AddRestrictions_ExpirationBoundary_UpdatesOrReplacesSession(long ticks)
        {
            var data = new FlowState();
            var oldSession = data.AddGroup();
            var oldRestriction = Assert.Single(data.Restrictions);
            var user = data.Restore();
            var now = FlowTime.AddMinutes(10).AddTicks(ticks);

            FlowSuccess(user.AddRestrictions(new[] { FlowData(minutes: 5) }, now));

            var session = Assert.Single(user.RestrictionSessions);

            if (ticks < 0)
            {
                Assert.Same(
                    oldSession,
                    session
                );

                Assert.Equal(
                    15,
                    session.TotalBlockedMinutes
                );

                Assert.Equal(
                    RestrictionStatus.Active,
                    oldRestriction.Status
                );

                Assert.Equal(
                    2,
                    user.Restrictions.Count
                );

                Assert.IsType<UserRestrictionsAddedToSessionEvent>(Assert.Single(user.DomainEvents));

                Assert.Equal(
                    2,
                    user.AggregateChanges.Count
                );
            }
            else
            {
                Assert.NotSame(
                    oldSession,
                    session
                );

                Assert.NotEqual(
                    oldSession.Id,
                    session.Id
                );

                Assert.Equal(
                    now,
                    session.CreatedAt
                );

                Assert.Equal(
                    5,
                    session.TotalBlockedMinutes
                );

                Assert.Equal(
                    RestrictionStatus.Completed,
                    oldRestriction.Status
                );

                Assert.Null(oldRestriction.RevokedAt);

                Assert.Single(user.Restrictions);

                Assert.Equal(
                    2,
                    user.DomainEvents.Count
                );

                var completed = Assert.IsType<UserRestrictionSessionCompletedEvent>(user.DomainEvents[0]);

                Assert.Same(
                    oldSession.Id,
                    completed.UserRestrictionSessionId
                );

                Assert.Equal(
                    now,
                    completed.OccurredOn
                );

                Assert.IsType<UserRestrictionSessionCreatedEvent>(user.DomainEvents[1]);

                Assert.Equal(
                    4,
                    user.AggregateChanges.Count
                );

                Assert.Same(
                    oldRestriction,
                    Assert.IsType<UserRestrictionUpdated>(user.AggregateChanges[0]).UserRestriction
                );

                Assert.Same(
                    oldSession.Id,
                    Assert.IsType<UserRestrictionSessionDeleted>(user.AggregateChanges[1]).UserRestrictionSessionId
                );

                Assert.Same(
                    session,
                    Assert.IsType<UserRestrictionSessionCreated>(user.AggregateChanges[2]).UserRestrictionSession
                );

                Assert.IsType<UserRestrictionCreated>(user.AggregateChanges[3]);
            }

            Assert.Equal(
                now,
                user.UpdatedAt
            );
        }

        [Fact]
        public void AddRestrictions_MixedSessionPlans_AppliesAllGroupsAndPreservesOtherHistory()
        {
            var user = User.Create(
                FlowLogin(),
                FlowPassword(),
                FlowTime
            );

            var registered = Assert.Single(user.DomainEvents);

            FlowSuccess(
                user.AddRestrictions(
                    new[] { FlowData(minutes: 30), FlowData(RestrictionType.CommentMute, 10) },
                    FlowTime
                )
            );

            var active = user.RestrictionSessions.Single(x => x.RestrictionType == RestrictionType.ChatMute);
            var expired = user.RestrictionSessions.Single(x => x.RestrictionType == RestrictionType.CommentMute);
            var oldChanges = user.AggregateChanges.ToArray();
            var oldEvents = user.DomainEvents.ToArray();

            FlowSuccess(
                user.AddRestrictions(
                    new[]
                    {
                        FlowData(minutes: 5),
                        FlowData(RestrictionType.CommentMute, 5),
                        FlowData(RestrictionType.AllMute, 5),
                    },
                    FlowTime.AddMinutes(10)
                )
            );

            Assert.Same(
                active,
                user.RestrictionSessions.Single(x => x.RestrictionType == RestrictionType.ChatMute)
            );

            Assert.Equal(
                35,
                active.TotalBlockedMinutes
            );

            Assert.DoesNotContain(
                user.RestrictionSessions,
                x => ReferenceEquals(x, expired)
            );

            Assert.Equal(
                3,
                user.RestrictionSessions.Count
            );

            Assert.Equal(
                4,
                user.Restrictions.Count
            );

            Assert.Equal(
                8,
                user.AggregateChanges.Count - oldChanges.Length
            );

            Assert.Equal(
                4,
                user.DomainEvents.Count - oldEvents.Length
            );

            FlowSameItems(
                oldChanges,
                user.AggregateChanges.Take(oldChanges.Length)
            );

            FlowSameItems(
                oldEvents,
                user.DomainEvents.Take(oldEvents.Length)
            );

            Assert.Same(
                registered,
                user.DomainEvents[0]
            );
        }
        #endregion

        #region AddRestrictions: access, validation and preparation atomicity
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void AddRestrictions_NullCollection_PrecedesTimeValidation(bool invalidTime)
        {
            var user = new FlowState().Restore();
            var before = new FlowSnapshot(user);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                user.AddRestrictions(
                    null!,
                    invalidTime ? FlowTime.AddTicks(-1) : FlowTime
                )
            );

            FlowEmpty(
                exception,
                "restrictionDataCollection"
            );

            before.AssertUnchanged();
        }

        [Fact]
        public void AddRestrictions_EmptyCollection_ReturnsValidationFailureWithoutChanges()
        {
            var user = new FlowState().Restore();
            var before = new FlowSnapshot(user);

            FlowFailure(
                user.AddRestrictions(Array.Empty<RestrictionData>(), FlowTime),
                "RESTRICTION_USER.EMPTY_RESTRICTION_LIST",
                ErrorType.Validation
            );

            before.AssertUnchanged();
        }

        [Fact]
        public void AddRestrictions_NullItems_ReportsCountsWithoutApplyingEarlierItems()
        {
            var user = new FlowState().Restore();
            var before = new FlowSnapshot(user);
            var values = new[] { FlowData(), null!, null! };

            var exception = Assert.Throws<DomainInvalidOperationException>(() =>
                user.AddRestrictions(
                    values,
                    FlowTime
                )
            );

            FlowPrecondition(
                exception,
                "AddRestrictions"
            );

            Assert.Equal(
                3, exception.Context["Custom_RestrictionDataCount"]
            );

            Assert.Equal(
                2, exception.Context["Custom_NullItemCount"]
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void AddRestrictions_InvalidLaterItem_PropagatesEntityGuardWithoutChanges(int kind)
        {
            var user = new FlowState().Restore();
            var invalid = FlowData(RestrictionType.AllMute);

            if (kind == 0)
            {
                invalid.RestrictionRule = null!;
            }
            else if (kind == 1)
            {
                invalid.RestrictionType = (RestrictionType)999;
            }
            else
            {
                invalid.ImposedBy = kind == 2 ? null! : " ";
            }

            var before = new FlowSnapshot(user);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                user.AddRestrictions(
                    new[] { FlowData(), invalid },
                    FlowTime
                )
            );

            if (kind == 1)
            {
                Assert.Equal(
                    "DOMAIN.DATA_INCONSISTENCY.UNSUPPORTED_TYPE",
                    exception.ExceptionCode
                );

                Assert.Equal(
                    "restrictionType",
                    exception.Context["Field"]
                );

                Assert.Equal(
                    "UserRestriction",
                    exception.Context["Type"]
                );

                Assert.Equal(
                    "Create",
                    exception.Context["Operation"]
                );
            }
            else
            {
                FlowEmpty(
                    exception,
                    kind == 0 ? "restrictionRule" : "imposedBy", "UserRestriction", "Create"
                );
            }

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void AddRestrictions_DurationOverflow_IsAtomicAcrossPreparedGroups(bool existing)
        {
            var data = new FlowState();
            data.AddGroup();

            if (existing)
            {
                data.AddGroup(RestrictionType.AllMute, new[] { int.MaxValue });
            }

            var user = data.Restore();
            var before = new FlowSnapshot(user);

            var values = existing
                ? new[] { FlowData(minutes: 1), FlowData(RestrictionType.AllMute, 1) }
                : new[]
                {
                    FlowData(minutes: 1),
                    FlowData(RestrictionType.AllMute, int.MaxValue),
                    FlowData(RestrictionType.AllMute, 1),
                };

            Assert.Throws<OverflowException>(() => user.AddRestrictions(values, FlowTime));

            before.AssertUnchanged();
        }

        [Fact]
        public void AddRestrictions_ExpiredSessionIdMismatch_ThrowsBeforeApplyingPlans()
        {
            var data = new FlowState();
            var session = data.AddGroup();
            var user = data.Restore();

            var extra = UserRestriction.Create(
                data.Id,
                RestrictionType.ChatMute,
                RestrictionRule.Restore("Extra", 1),
                "Admin",
                null,
                FlowTime
            );

            session.AddRestrictions(
                new[] { extra },
                FlowTime
            );

            var before = new FlowSnapshot(user);

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                user.AddRestrictions(
                    new[] { FlowData(RestrictionType.AllMute), FlowData() },
                    FlowTime.AddMinutes(11)
                )
            );

            FlowInvariant(exception);

            Assert.Equal(
                "The expired restriction session does not exactly match the active aggregate restrictions.",
                exception.Context["Rule"]
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void AddRestrictions_ExpiredSessionWithTerminalRestriction_ThrowsWithoutChanges(bool revoked)
        {
            var data = new FlowState();
            data.AddGroup();
            var restriction = Assert.Single(data.Restrictions);
            var user = data.Restore();

            if (revoked)
            {
                restriction.RevokeRestriction(FlowTime);
            }
            else
            {
                restriction.MarkAsCompleted(FlowTime);
            }

            var before = new FlowSnapshot(user);

            var exception = Assert.Throws<DomainInvariantViolationException>(() =>
                user.AddRestrictions(
                    new[] { FlowData() },
                    FlowTime.AddMinutes(10)
                )
            );

            FlowInvariant(
                exception,
                "UserRestriction"
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void AddRestrictions_DeletedUser_AllowsRestrictionAndOnlyBanTerminatesTokens(bool ban)
        {
            var data = new FlowState();
            data.AddDeletion();
            var token = data.AddToken();
            var user = data.Restore();
            var type = ban ? RestrictionType.Ban : RestrictionType.ChatMute;

            FlowSuccess(user.AddRestrictions(new[] { FlowData(type) }, FlowTime));

            Assert.True(user.IsDeleted);

            Assert.Equal(
                ban ? TokenStatus.Revoked : TokenStatus.Active,
                token.TokenStatus
            );

            Assert.Equal(
                ban ? 0 : 1,
                user.RefreshTokens.Count
            );

            Assert.Equal(
                ban ? 1 : 0,
                user.AggregateChanges.OfType<UserRefreshTokenUpdated>().Count()
            );
        }

        [Fact]
        public void AddRestrictions_Ban_TerminatesElapsedAndUsableTokensWithCorrectUpdates()
        {
            var data = new FlowState();
            var now = FlowTime.AddMinutes(1);
            var elapsed = data.AddToken(1, expires: now);
            var usable = data.AddToken(2);
            var user = data.Restore();

            FlowSuccess(user.AddRestrictions(new[] { FlowData(RestrictionType.Ban) }, now));

            Assert.Empty(user.RefreshTokens);

            Assert.Equal(
                TokenStatus.Expired,
                elapsed.TokenStatus
            );

            Assert.Equal(
                TokenStatus.Revoked,
                usable.TokenStatus
            );

            var updates = user.AggregateChanges.OfType<UserRefreshTokenUpdated>().ToArray();

            Assert.Equal(
                2,
                updates.Length
            );

            Assert.Equal(
                TokenStatus.Expired,
                updates[0].TokenStatus
            );

            Assert.Null(updates[0].RevokedAt);

            Assert.Equal(
                TokenStatus.Revoked,
                updates[1].TokenStatus
            );

            Assert.Equal(
                now,
                updates[1].RevokedAt
            );

            Assert.All(
                updates,
                x => Assert.Equal(now, x.Now)
            );
        }

        private static IEnumerable<RestrictionData> FlowThrowingRestrictionData(Exception failure)
        {
            yield return FlowData();
            throw failure;
        }

        [Fact]
        public void AddRestrictions_EnumerationThrows_PropagatesSameExceptionWithoutChanges()
        {
            var user = new FlowState().Restore();
            var before = new FlowSnapshot(user);
            var expected = new InvalidOperationException("Source failure.");

            var actual = Assert.Throws<InvalidOperationException>(() =>
                user.AddRestrictions(
                    FlowThrowingRestrictionData(expected),
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
        public void AddRestrictions_TimeBeforeAggregateTimestamp_ThrowsWithoutChanges(bool afterCreation)
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
                user.AddRestrictions(
                    new[] { FlowData() },
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
