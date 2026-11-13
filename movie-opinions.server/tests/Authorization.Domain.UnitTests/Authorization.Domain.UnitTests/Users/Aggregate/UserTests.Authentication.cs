using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Users.Aggregate;
using Authorization.Domain.Users.AggregateChanges.Restriction;
using Authorization.Domain.Users.AggregateChanges.SessionRestriction;
using Authorization.Domain.Users.AggregateChanges.Tokens;
using Authorization.Domain.Users.DomainEvents;
using Authorization.Domain.Users.DomainEvents.SessionRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction.Enums;
using Authorization.Domain.Users.Enums;

namespace Authorization.Domain.UnitTests.Users.Aggregate
{
    public partial class UserTests
    {
        #region RecordFailedPasswordAttempt
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void RecordFailedPasswordAttempt_BelowThreshold_IncrementsWithoutRevokingTokens(int attempts)
        {
            var data = new FlowState { FailedAttempts = attempts };
            var token = data.AddToken();
            var user = data.Restore();
            var now = FlowTime.AddMinutes(1);

            FlowSuccess(user.RecordFailedPasswordAttempt(now));

            Assert.Equal(
                attempts + 1,
                user.FailedPasswordAttempts
            );

            Assert.Equal(
                now,
                user.UpdatedAt
            );

            Assert.Null(user.LastLoginAt);

            Assert.Same(
                token,
                Assert.Single(user.RefreshTokens)
            );

            Assert.Equal(
                TokenStatus.Active,
                token.TokenStatus
            );

            Assert.Empty(user.Restrictions);
            Assert.Empty(user.RestrictionSessions);
            Assert.Empty(user.AggregateChanges);
            Assert.Empty(user.DomainEvents);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RecordFailedPasswordAttempt_ThirdFailure_AddsSystemBanAndTerminatesEveryToken(bool hasMute)
        {
            var data = new FlowState { FailedAttempts = 2 };
            var now = FlowTime.AddMinutes(1);
            var elapsed = data.AddToken(1, expires: now);
            var usable = data.AddToken(2);
            if (hasMute)
            {
                data.AddGroup(RestrictionType.ChatMute);
            }
            var user = data.Restore();

            FlowSuccess(user.RecordFailedPasswordAttempt(now));

            Assert.Equal(
                0,
                user.FailedPasswordAttempts
            );

            Assert.Equal(
                now,
                user.UpdatedAt
            );

            Assert.Empty(user.RefreshTokens);

            Assert.Equal(
                TokenStatus.Expired,
                elapsed.TokenStatus
            );

            Assert.Null(elapsed.RevokedAt);

            Assert.Equal(
                TokenStatus.Revoked,
                usable.TokenStatus
            );

            Assert.Equal(
                now,
                usable.RevokedAt
            );

            var ban = Assert.Single(user.Restrictions.Where(x => x.RestrictionType == RestrictionType.Ban));

            Assert.Equal(
                "System lock",
                ban.RestrictionRule.Name
            );

            Assert.Equal(
                10,
                ban.RestrictionRule.DurationMinutes
            );

            Assert.Equal(
                "System",
                ban.ImposedBy
            );

            Assert.Equal(
                "Too many failed login attempts",
                ban.Reason
            );

            Assert.Equal(
                now,
                ban.CreatedAt
            );

            var session = Assert.Single(user.RestrictionSessions.Where(x => x.RestrictionType == RestrictionType.Ban));

            Assert.Equal(
                10,
                session.TotalBlockedMinutes
            );

            Assert.Equal(
                ban.Id,
                Assert.Single(session.ActiveRestrictionIds)
            );

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
                RestrictionType.Ban,
                ev.RestrictionType
            );

            Assert.Equal(
                10,
                ev.TotalBlockedMinutes
            );

            Assert.Equal(
                now,
                ev.OccurredOn
            );

            Assert.Equal(
                4,
                user.AggregateChanges.Count
            );

            Assert.IsType<UserRestrictionSessionCreated>(user.AggregateChanges[0]);
            Assert.IsType<UserRestrictionCreated>(user.AggregateChanges[1]);

            Assert.Equal(
                2,
                user.AggregateChanges.OfType<UserRefreshTokenUpdated>().Count()
            );

            Assert.Equal(
                hasMute ? 2 : 1,
                user.Restrictions.Count
            );

            Assert.Equal(
                hasMute ? 2 : 1,
                user.RestrictionSessions.Count
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        public void RecordFailedPasswordAttempt_ExpiredBanAtBoundary_AllowsNewAttempt(int attempts)
        {
            var data = new FlowState { FailedAttempts = attempts };
            var oldSession = data.AddGroup(RestrictionType.Ban, new[] { 10 });
            var oldRestriction = Assert.Single(data.Restrictions);
            var user = data.Restore();
            var now = FlowTime.AddMinutes(10);

            FlowSuccess(user.RecordFailedPasswordAttempt(now));

            if (attempts == 2)
            {
                Assert.Equal(
                    0,
                    user.FailedPasswordAttempts
                );

                Assert.NotSame(
                    oldSession,
                    Assert.Single(user.RestrictionSessions)
                );

                Assert.Equal(
                    RestrictionStatus.Completed,
                    oldRestriction.Status
                );

                Assert.Equal(
                    2,
                    user.DomainEvents.Count
                );
            }
            else
            {
                Assert.Equal(
                    attempts + 1,
                    user.FailedPasswordAttempts
                );

                Assert.Same(
                    oldSession,
                    Assert.Single(user.RestrictionSessions)
                );

                Assert.Equal(
                    RestrictionStatus.Active,
                    oldRestriction.Status
                );

                Assert.Empty(user.DomainEvents);
            }
        }

        #endregion

        #region LoginSuccess
        // Поточний контракт визначає новизну за пристроєм та IP, навіть якщо строк токена минув.
        // The current contract checks device and IP even when the stored token has elapsed.
        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, false, true)]
        [InlineData(false, true, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, false)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        [InlineData(true, true, true)]
        public void LoginSuccess_DeviceAndIpPair_DeterminesNewDeviceEventWithoutChangingTokens(
            bool sameDevice,
            bool sameIp,
            bool elapsed
        )
        {
            var data = new FlowState { FailedAttempts = 2 };
            data.AddToken(1, device: FlowDevice("Other"), ip: FlowIp("192.168.1.2"));
            var matching = data.AddToken(2);
            var device = FlowDevice(sameDevice ? "Chrome" : "Firefox");
            var ip = FlowIp(sameIp ? "192.168.1.1" : "192.168.1.3");
            var user = data.Restore();
            var now = elapsed ? matching.ExpiresAt : FlowTime;

            FlowSuccess(user.LoginSuccess(
                device,
                ip,
                now)
            );

            Assert.Equal(
                0,
                user.FailedPasswordAttempts
            );

            Assert.Equal(
                now,
                user.LastLoginAt
            );

            Assert.Equal(
                now,
                user.UpdatedAt
            );

            FlowSameItems(
                data.Tokens,
                user.RefreshTokens
            );

            Assert.All(
                user.RefreshTokens,
                x => Assert.Equal(TokenStatus.Active, x.TokenStatus)
            );

            Assert.Empty(user.AggregateChanges);

            if (sameDevice && sameIp)
            {
                Assert.Empty(user.DomainEvents);
            }
            else
            {
                var ev = Assert.IsType<UserSignedInFromNewDeviceEvent>(Assert.Single(user.DomainEvents));

                Assert.Same(
                    user.Id,
                    ev.UserId
                );

                Assert.Same(
                    user.Login,
                    ev.Login
                );

                Assert.Same(
                    device,
                    ev.DeviceInfo
                );

                Assert.Same(
                    ip,
                    ev.IpAddress
                );

                Assert.Equal(
                    now,
                    ev.OccurredOn
                );
            }
        }

        [Theory]
        [InlineData(-5)]
        [InlineData(0)]
        [InlineData(2)]
        public void LoginSuccess_NoTokens_RecordsNewDeviceAndPreservesTimestampOffset(int offset)
        {
            var user = new FlowState().Restore();
            var now = FlowTime.ToOffset(TimeSpan.FromHours(offset));

            FlowSuccess(user.LoginSuccess(
                FlowDevice(),
                FlowIp(),
                now)
            );

            Assert.IsType<UserSignedInFromNewDeviceEvent>(Assert.Single(user.DomainEvents));

            Assert.Empty(user.RefreshTokens);
            Assert.Empty(user.AggregateChanges);

            Assert.Equal(
                now.Offset,
                user.LastLoginAt!.Value.Offset
            );

            Assert.Equal(
                now.Offset,
                user.UpdatedAt!.Value.Offset
            );
        }

        [Theory]
        [InlineData(1, "deviceInfo", false)]
        [InlineData(1, "deviceInfo", true)]
        [InlineData(2, "ipAddress", false)]
        [InlineData(2, "ipAddress", true)]
        [InlineData(3, "deviceInfo", false)]
        [InlineData(3, "deviceInfo", true)]
        public void LoginSuccess_NullArguments_ReportFirstFieldBeforeTimeOrAccess(
            int mask,
            string field,
            bool invalidTime
        )
        {
            var user = FlowAccessState(1).Restore();
            var before = new FlowSnapshot(user);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                user.LoginSuccess(
                    (mask & 1) != 0 ? null! : FlowDevice(),
                    (mask & 2) != 0 ? null! : FlowIp(),
                    invalidTime ? FlowTime.AddTicks(-1) : FlowTime
                )
            );

            FlowEmpty(
                exception,
                field
            );

            before.AssertUnchanged();
        }
        #endregion

        #region Authentication access and operation time
        [Theory]
        [InlineData(false, 1)]
        [InlineData(false, 2)]
        [InlineData(false, 3)]
        [InlineData(true, 1)]
        [InlineData(true, 2)]
        [InlineData(true, 3)]
        public void AuthenticationOperations_DeniedAccess_DoNotChangeState(
            bool login,
            int accessState)
        {
            var data = FlowAccessState(accessState);
            data.FailedAttempts = 2;
            var user = data.Restore();
            var before = new FlowSnapshot(user);

            var result = login
                ? user.LoginSuccess(FlowDevice(), FlowIp(), FlowTime)
                : user.RecordFailedPasswordAttempt(FlowTime);

            FlowFailure(
                result,
                accessState == 2 ? "ACCESS_USER.USER_BLOCKED" : "ACCESS_USER.USER_DELETED",
                ErrorType.Forbidden
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(false, RestrictionType.CommentMute)]
        [InlineData(false, RestrictionType.ChatMute)]
        [InlineData(false, RestrictionType.AllMute)]
        [InlineData(true, RestrictionType.CommentMute)]
        [InlineData(true, RestrictionType.ChatMute)]
        [InlineData(true, RestrictionType.AllMute)]
        public void AuthenticationOperations_MuteDoesNotDenyAccess(
            bool login,
            RestrictionType type)
        {
            var data = new FlowState();
            var session = data.AddGroup(type);
            var user = data.Restore();

            FlowSuccess(
                login ? user.LoginSuccess(
                    FlowDevice(),
                    FlowIp(), FlowTime
                ) : user.RecordFailedPasswordAttempt(FlowTime)
            );

            Assert.Same(
                session,
                Assert.Single(user.RestrictionSessions)
            );

            Assert.Equal(
                RestrictionStatus.Active,
                Assert.Single(user.Restrictions).Status
            );
        }

        [Fact]
        public void AuthenticationOperations_ExistingHistory_IsPreserved()
        {
            var user = User.Create(
                FlowLogin(),
                FlowPassword(),
                FlowTime
            );

            var ev = Assert.Single(user.DomainEvents);

            FlowSuccess(user.CreateRefreshToken(
                FlowDevice(),
                FlowIp(),
                FlowTime)
            );

            var change = Assert.Single(user.AggregateChanges);

            FlowSuccess(user.RecordFailedPasswordAttempt(FlowTime));

            FlowSuccess(user.LoginSuccess(
                FlowDevice("Firefox"),
                FlowIp(),
                FlowTime.AddTicks(1))
            );

            Assert.Same(
                ev,
                user.DomainEvents[0]
            );

            Assert.Equal(
                2,
                user.DomainEvents.Count
            );

            Assert.Same(
                change,
                Assert.Single(user.AggregateChanges)
            );
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RecordFailedPasswordAttempt_TimeBeforeAggregateTimestamp_ThrowsWithoutChanges(bool afterCreation)
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
                user.RecordFailedPasswordAttempt(minimum.AddTicks(-1))
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
        public void LoginSuccess_TimeBeforeAggregateTimestamp_ThrowsWithoutChanges(bool afterCreation)
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
                user.LoginSuccess(
                    FlowDevice(),
                    FlowIp(),
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
