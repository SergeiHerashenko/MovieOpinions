using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.EmailUser;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Common.ValueObjects.PasswordUser;
using Authorization.Domain.Common.ValueObjects.PhoneUser;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Aggregate;
using Authorization.Domain.Users.AggregateChanges.Tokens;
using Authorization.Domain.Users.Contracts;
using Authorization.Domain.Users.Entities.UsersDeletion;
using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;
using Authorization.Domain.Users.Entities.UsersPendingAction;
using Authorization.Domain.Users.Entities.UsersPendingAction.Actions;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;
using Authorization.Domain.Users.Entities.UsersPendingAction.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRefreshToken;
using Authorization.Domain.Users.Entities.UsersRefreshToken.Enums;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;
using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction.Enums;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction;
using Authorization.Domain.Users.Entities.UsersRestrictionSession;
using Authorization.Domain.Users.Entities.UsersRestrictionSession.ValueObjects;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;
using System.Collections;

namespace Authorization.Domain.UnitTests.Users.Aggregate
{
    public partial class UserTests
    {
        #region Shared aggregate test data and helpers
        // Ці допоміжні типи спільні для всіх дев'яти partial-файлів UserTests.
        // These helpers are shared by all nine partial UserTests files.
        private static readonly DateTimeOffset FlowTime = new(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);

        private static Guid FlowGuid(int family, int number = 1) =>
            Guid.Parse($"{family:x8}-0000-4000-8000-{number:x12}");

        private static string FlowBase64(byte value)
            => Convert.ToBase64String(Enumerable.Repeat(value, 64).ToArray());

        private static ConfirmationFlowToken FlowConfirmation(byte value = 10)
        {
            var result = ConfirmationFlowToken.Parse(FlowBase64(value));
            FlowSuccess(result);
            return result.Value;
        }

        private static DeviceInfo FlowDevice(string browser = "Chrome")
        {
            return DeviceInfo.Restore(
                DeviceType.Desktop,
                "Windows 11",
                browser,
                "Desktop"
            );
        }

        private static IpAddress FlowIp(string value = "192.168.1.1")
            => IpAddress.Restore(value);

        private static Login FlowLogin(string address = "serhii@example.com")
            => Login.From(Email.Restore(address));

        private static Login FlowPhoneLogin() =>
            Login.From(Phone.Restore(PhoneCountryCode.Restore("+380"), PhoneNationalNumber.Restore("981234567")));

        private static Password FlowPassword(string value = "TEST.HASH") =>
            Password.Restore(PasswordHash.Restore(value));

        private static PlainPassword FlowPlain()
        {
            var result = PlainPassword.Create("Abcdefg1");
            Assert.True(result.IsSuccess);
            return result.Value;
        }

        private static RestrictionData FlowData(RestrictionType type = RestrictionType.ChatMute, int minutes = 10) =>
            new()
            {
                RestrictionType = type,
                RestrictionRule = RestrictionRule.Restore("Rule", minutes),
                ImposedBy = "Admin",
                Reason = "Reason",
            };

        // Початковий стан проходить справжній Restore; некоректність вводиться лише після нього.
        // Initial fixtures pass the real Restore; defensive corruption is introduced only afterwards.
        private sealed class FlowState
        {
            public UserId Id { get; set; } = UserId.Restore(FlowGuid(100));
            public DateTimeOffset CreatedAt { get; set; } = FlowTime;
            public DateTimeOffset? UpdatedAt { get; set; } = FlowTime;
            public DateTimeOffset? LastLoginAt { get; set; }
            public int FailedAttempts { get; set; }
            public Role Role { get; set; } = Role.User;
            public Login Login { get; set; } = FlowLogin();
            public Password Password { get; set; } = FlowPassword();
            public List<UserRefreshToken> Tokens { get; } = new();
            public List<UserRestriction> Restrictions { get; } = new();
            public List<UserRestrictionSession> Sessions { get; } = new();
            public UserPendingAction? Action { get; set; }
            public UserDeletion? Deletion { get; set; }

            public User Restore() =>
                User.Restore(
                    Id,
                    CreatedAt,
                    Login,
                    Password,
                    Role,
                    UpdatedAt,
                    LastLoginAt,
                    FailedAttempts,
                    Restrictions,
                    Sessions,
                    Tokens,
                    Action,
                    Deletion
                );

            public UserRefreshToken AddToken(
                int number = 1,
                DateTimeOffset? created = null,
                DateTimeOffset? expires = null,
                DeviceInfo? device = null,
                IpAddress? ip = null
            )
            {
                var time = created ?? CreatedAt;

                var token = UserRefreshToken.Restore(
                    UserRefreshTokenId.Restore(FlowGuid(101, number)),
                    Id,
                    RefreshToken.Restore(FlowBase64((byte)number)),
                    device ?? FlowDevice(),
                    ip ?? FlowIp(),
                    "Kyiv",
                    TokenStatus.Active,
                    expires ?? time.AddDays(7),
                    null,
                    time,
                    null
                );

                Tokens.Add(token);
                Advance(time);
                return token;
            }

            public UserRestrictionSession AddGroup(
                RestrictionType type = RestrictionType.ChatMute,
                int[]? minutes = null,
                DateTimeOffset? created = null
            )
            {
                var time = created ?? CreatedAt;
                var group = new List<UserRestriction>();
                foreach (var duration in minutes ?? new[] { 10 })
                {
                    var number = Restrictions.Count + 1;
                    var restriction = UserRestriction.Restore(
                        UserRestrictionId.Restore(FlowGuid(102, number)),
                        Id,
                        RestrictionRule.Restore($"Rule {number}", duration),
                        type,
                        "Admin",
                        $"Reason {number}",
                        RestrictionStatus.Active,
                        time,
                        null
                    );
                    Restrictions.Add(restriction);
                    group.Add(restriction);
                }

                var session = UserRestrictionSession.Restore(
                    UserRestrictionSessionId.Restore(FlowGuid(103, Sessions.Count + 1)),
                    Id,
                    group.Select(x => x.Id).ToArray(),
                    type,
                    group.Sum(x => x.RestrictionRule.DurationMinutes),
                    time
                );

                Sessions.Add(session);
                Advance(time);
                return session;
            }

            // kind: 0 — логін, 1 — пароль, 2 — видалення; це номери сценаріїв, а не значення enum.
            // kind: 0 = login, 1 = password, 2 = deletion; these are scenario numbers, not enum values.
            public UserPendingAction AddAction(int kind = 0, DateTimeOffset? created = null)
            {
                var time = created ?? CreatedAt;

                UserAction action = kind switch
                {
                    0 => UserAction.From(FlowLogin("next@example.com")),
                    1 => UserAction.From(FlowPassword("NEW.HASH")),
                    2 => UserAction.From(DeletionReason.Restore("Close account")),
                    _ => throw new ArgumentOutOfRangeException(nameof(kind)),
                };

                Action = UserPendingAction.Restore(
                    UserPendingActionId.Restore(FlowGuid(104)),
                    Id,
                    FlowConfirmation(),
                    action,
                    time.AddMinutes(30),
                    null,
                    null,
                    null,
                    ActionStatus.Active,
                    time
                );

                Advance(time);
                return Action;
            }

            public UserDeletion AddDeletion(DateTimeOffset? created = null)
            {
                var time = created ?? CreatedAt;
                Deletion = UserDeletion.Restore(
                    UserDeletionId.Restore(FlowGuid(105)),
                    Id,
                    Login,
                    DeletionReason.Restore("Close account"),
                    time,
                    time.AddDays(30),
                    null,
                    DeletionStatus.Deleted,
                    null
                );

                Advance(time);
                return Deletion;
            }

            private void Advance(DateTimeOffset time)
            {
                if (UpdatedAt is null || UpdatedAt < time)
                {
                    UpdatedAt = time;
                }
            }
        }

        private static FlowState FlowAccessState(int state)
        {
            // state: 0 — звичайний, 1 — видалений, 2 — заблокований, 3 — обидва обмеження.
            // state: 0 = normal, 1 = deleted, 2 = banned, 3 = both conditions.
            var data = new FlowState();

            if (state is 1 or 3)
            {
                data.AddDeletion();
            }

            if (state is 2 or 3)
            {
                data.AddGroup(RestrictionType.Ban);
            }

            return data;
        }

        private static void FlowSuccess(Result result)
        {
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Empty(result.Errors);
        }

        private static void FlowFailure(Result result, string code, ErrorType errorType)
        {
            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);

            var error = Assert.Single(result.Errors);

            Assert.Equal(
                code,
                error.Code
            );

            Assert.Equal(
                errorType,
                error.ErrorType
            );

            Assert.False(string.IsNullOrWhiteSpace(error.Message));
        }

        private static void FlowEmpty(
            DomainDataInconsistencyException exception,
            string field,
            string type = "User",
            string operation = "Update"
        )
        {
            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.EMPTY_VALUE",
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
                type,
                exception.Context["Type"]
            );

            Assert.Equal(
                operation,
                exception.Context["Operation"]
            );

            Assert.Equal(
                field,
                exception.Context["Field"]
            );
        }

        private static void FlowRange(
            DomainDataInconsistencyException exception,
            string minimumField,
            DateTimeOffset minimum,
            string type = "User"
        )
        {
            Assert.Equal(
                "DOMAIN.DATA_INCONSISTENCY.OUT_OF_RANGE",
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
                type,
                exception.Context["Type"]
            );

            Assert.Equal(
                "Update",
                exception.Context["Operation"]
            );

            Assert.Equal(
                "now",
                exception.Context["Field"]
            );

            Assert.Equal(
                "DateTimeOffset",
                exception.Context["ValueType"]
            );

            Assert.Equal(
                minimumField,
                exception.Context["Custom_MinimumField"]
            );

            Assert.Equal(
                minimum,
                exception.Context["Custom_MinimumValue"]
            );
        }

        private static void FlowInvariant(
            DomainInvariantViolationException exception,
            string type = "User",
            string operation = "Update"
        )
        {
            Assert.Equal(
                "DOMAIN.INVARIANT_VIOLATION.INVALID_STATE",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.InvariantViolation,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                type,
                exception.Context["Type"]
            );

            Assert.Equal(
                operation,
                exception.Context["Operation"]
            );

            Assert.False(string.IsNullOrWhiteSpace((string)exception.Context["Rule"]));
        }

        private static void FlowPrecondition(
            DomainInvalidOperationException exception,
            string source,
            string type = "User"
        )
        {
            Assert.Equal(
                "DOMAIN.INVALID_OPERATION.PRECONDITION_FAILED",
                exception.ExceptionCode
            );

            Assert.Equal(
                ExceptionType.InvalidOperation,
                exception.ExceptionType
            );

            Assert.Equal(
                "Domain",
                exception.Context["Layer"]
            );

            Assert.Equal(
                type,
                exception.Context["Type"]
            );

            Assert.Equal(
                "Update",
                exception.Context["Operation"]
            );

            Assert.Equal(
                source,
                exception.Context["SourceContext"]
            );
        }

        private static void FlowSameItems<T>(
            IEnumerable<T> expected,
            IEnumerable<T> actual)
            where T : class
        {
            var first = expected.ToArray();
            var second = actual.ToArray();

            Assert.Equal(
                first.Length,
                second.Length
            );

            for (var i = 0; i < first.Length; i++)
            {
                Assert.Same(
                    first[i],
                    second[i]
                );
            }
        }

        // Скалярні копії виявляють мутації, яких не видно через рівність Entity за Id.
        // Scalar copies detect mutations hidden by Entity equality based only on Id.
        private sealed class FlowSnapshot
        {
            private readonly User user;
            private readonly Login login;
            private readonly Password password;
            private readonly DateTimeOffset? updated;
            private readonly DateTimeOffset? lastLogin;
            private readonly int attempts;
            private readonly Role role;
            private readonly bool isDeleted;
            private readonly bool isAction;
            private readonly UserRefreshToken[] tokens;
            private readonly (TokenStatus, DateTimeOffset?, DateTimeOffset?)[] tokenStates;
            private readonly UserRestriction[] restrictions;
            private readonly (RestrictionStatus, DateTimeOffset?)[] restrictionStates;
            private readonly UserRestrictionSession[] sessions;
            private readonly int[] durations;
            private readonly Guid[][] sessionIds;
            private readonly object[] events;
            private readonly object[] changes;
            private readonly UserPendingAction? action;
            private readonly (ActionStatus?, DateTimeOffset?, DateTimeOffset?, DateTimeOffset?) actionState;
            private readonly UserDeletion? deletion;
            private readonly (DeletionStatus?, DateTimeOffset?, DateTimeOffset?) deletionState;

            public FlowSnapshot(User user, UserPendingAction? action = null)
            {
                this.user = user;
                login = user.Login;
                password = user.Password;
                updated = user.UpdatedAt;
                lastLogin = user.LastLoginAt;
                attempts = user.FailedPasswordAttempts;
                role = user.Role;
                isDeleted = user.IsDeleted;
                isAction = user.IsAction;
                tokens = user.RefreshTokens.ToArray();
                tokenStates = tokens.Select(x => (x.TokenStatus, x.ConsumedAt, x.RevokedAt)).ToArray();
                restrictions = user.Restrictions.ToArray();
                restrictionStates = restrictions.Select(x => (x.Status, x.RevokedAt)).ToArray();
                sessions = user.RestrictionSessions.ToArray();
                durations = sessions.Select(x => x.TotalBlockedMinutes).ToArray();

                sessionIds = sessions
                    .Select(x => x.ActiveRestrictionIds.Select(i => i.Value).OrderBy(i => i).ToArray())
                    .ToArray();

                events = user.DomainEvents.Cast<object>().ToArray();
                changes = user.AggregateChanges.Cast<object>().ToArray();
                this.action = action;
                actionState = (action?.Status, action?.ConfirmationTime, action?.ExpiredAt, action?.CancelledTime);
                var deletionResult = user.GetDeletion();
                deletion = deletionResult.IsSuccess ? deletionResult.Value : null;
                deletionState = (deletion?.Status, deletion?.UpdatedAt, deletion?.RestoredAt);
            }

            public void AssertUnchanged()
            {
                Assert.Same(
                    login,
                    user.Login
                );

                Assert.Same(
                    password,
                    user.Password
                );

                Assert.Equal(
                    updated,
                    user.UpdatedAt
                );

                Assert.Equal(
                    lastLogin,
                    user.LastLoginAt
                );

                Assert.Equal(
                    attempts,
                    user.FailedPasswordAttempts
                );

                Assert.Equal(
                    role,
                    user.Role
                );

                Assert.Equal(
                    isDeleted,
                    user.IsDeleted
                );

                Assert.Equal(
                    isAction,
                    user.IsAction
                );

                FlowSameItems(
                    tokens,
                    user.RefreshTokens
                );

                Assert.Equal(
                    tokenStates,
                    user.RefreshTokens.Select(x => (x.TokenStatus, x.ConsumedAt, x.RevokedAt)).ToArray()
                );

                FlowSameItems(
                    restrictions,
                    user.Restrictions
                );

                Assert.Equal(
                    restrictionStates,
                    user.Restrictions.Select(x => (x.Status, x.RevokedAt)).ToArray()
                );

                FlowSameItems(
                    sessions,
                    user.RestrictionSessions
                );

                Assert.Equal(
                    durations,
                    user.RestrictionSessions.Select(x => x.TotalBlockedMinutes).ToArray()
                );

                for (var i = 0; i < sessions.Length; i++)
                {
                    Assert.Equal(
                        sessionIds[i],
                        sessions[i].ActiveRestrictionIds.Select(x => x.Value).OrderBy(x => x).ToArray()
                    );
                }

                FlowSameItems(
                    events,
                    user.DomainEvents.Cast<object>()
                );

                FlowSameItems(
                    changes,
                    user.AggregateChanges.Cast<object>()
                );

                Assert.Equal(
                    actionState,
                    (action?.Status, action?.ConfirmationTime, action?.ExpiredAt, action?.CancelledTime)
                );

                var currentDeletion = user.GetDeletion();

                Assert.Same(
                    deletion,
                    currentDeletion.IsSuccess ? currentDeletion.Value : null
                );

                Assert.Equal(
                    deletionState,
                    (deletion?.Status, deletion?.UpdatedAt, deletion?.RestoredAt)
                );
            }
        }

        // Одноразовий перелік допомагає перевірити матеріалізацію вхідних даних перед змінами.
        // A single-use enumerable verifies that inputs are materialized before state changes.
        private sealed class FlowSingleUse<T> : IEnumerable<T>
        {
            private readonly IEnumerable<T> values;
            public int EnumerationCount { get; private set; }

            public FlowSingleUse(IEnumerable<T> values) => this.values = values;

            public IEnumerator<T> GetEnumerator()
            {
                EnumerationCount++;

                if (EnumerationCount > 1)
                {
                    throw new InvalidOperationException("The source was enumerated more than once.");
                }

                return values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        #endregion

        #region CreateRefreshToken
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("Kyiv")]
        [InlineData(" Київ ")]
        public void CreateRefreshToken_ValidArguments_AddsOriginalTokenAndCreatedChange(string? city)
        {
            var user = new FlowState().Restore();
            var device = FlowDevice();
            var ip = FlowIp();
            var now = FlowTime.AddMinutes(1);

            var result = user.CreateRefreshToken(
                device,
                ip,
                now,
                city
            );

            FlowSuccess(result);

            var token = Assert.Single(user.RefreshTokens);

            Assert.Same(
                token,
                result.Value
            );

            Assert.Same(
                user.Id,
                token.UserId
            );

            Assert.Same(
                device,
                token.DeviceInfo
            );

            Assert.Same(
                ip,
                token.IpAddress
            );

            Assert.Equal(
                city,
                token.City
            );

            Assert.Equal(
                TokenStatus.Active,
                token.TokenStatus
            );

            Assert.Equal(
                now,
                token.CreatedAt
            );

            Assert.Equal(
                now.AddDays(7),
                token.ExpiresAt
            );

            Assert.Null(token.ConsumedAt);
            Assert.Null(token.RevokedAt);

            Assert.Equal(
                now,
                user.UpdatedAt
            );

            var change = Assert.IsType<UserRefreshTokenCreated>(Assert.Single(user.AggregateChanges));

            Assert.Same(
                token,
                change.UserRefreshToken
            );

            Assert.Equal(
                now,
                change.OccurredOn
            );

            Assert.Empty(user.DomainEvents);
        }

        [Fact]
        public void CreateRefreshToken_OmittedCity_UsesNull()
        {
            var user = new FlowState().Restore();

            var result = user.CreateRefreshToken(
                FlowDevice(),
                FlowIp(),
                FlowTime
            );

            FlowSuccess(result);

            Assert.Null(result.Value.City);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CreateRefreshToken_RepeatedCalls_PreserveExistingTokensEventsAndChanges(bool later)
        {
            var user = User.Create(
                FlowLogin(),
                FlowPassword(),
                FlowTime
            );

            var registered = Assert.Single(user.DomainEvents);

            var first = user.CreateRefreshToken(
                FlowDevice(),
                FlowIp(),
                FlowTime
            ).Value;

            var firstChange = Assert.Single(user.AggregateChanges);
            var now = later ? FlowTime.AddMinutes(1) : FlowTime;

            var second = user.CreateRefreshToken(
                FlowDevice(),
                FlowIp(),
                now
            ).Value;

            Assert.Equal(
                2,
                user.RefreshTokens.Count
            );

            Assert.Same(
                first,
                user.RefreshTokens.First()
            );

            Assert.Same(
                second,
                user.RefreshTokens.Last()
            );

            Assert.NotEqual(
                first.Id,
                second.Id
            );

            Assert.NotEqual(
                first.RefreshToken,
                second.RefreshToken
            );

            Assert.Same(
                firstChange,
                user.AggregateChanges[0]
            );

            Assert.Equal(
                2,
                user.AggregateChanges.Count
            );

            Assert.Same(
                registered,
                Assert.Single(user.DomainEvents)
            );

            Assert.Equal(
                now,
                user.UpdatedAt
            );
        }

        [Theory]
        [InlineData(1, "deviceInfo", false)]
        [InlineData(1, "deviceInfo", true)]
        [InlineData(2, "ipAddress", false)]
        [InlineData(2, "ipAddress", true)]
        [InlineData(3, "deviceInfo", false)]
        [InlineData(3, "deviceInfo", true)]
        public void CreateRefreshToken_NullArguments_ReportFirstMissingFieldWithoutChanges(
            int mask,
            string field,
            bool invalidTime
        )
        {
            var user = new FlowState().Restore();
            var before = new FlowSnapshot(user);

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                user.CreateRefreshToken(
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

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CreateRefreshToken_BoundaryCreationTimes_PreserveLifetime(bool maximum)
        {
            var now = maximum ? DateTimeOffset.MaxValue.AddDays(-7) : DateTimeOffset.MinValue;
            var user = new FlowState { CreatedAt = now, UpdatedAt = now }.Restore();

            var result = user.CreateRefreshToken(
                FlowDevice(),
                FlowIp(),
                now
            );

            FlowSuccess(result);

            Assert.Equal(
                now,
                result.Value.CreatedAt
            );

            Assert.Equal(
                now.AddDays(7),
                result.Value.ExpiresAt
            );
        }

        [Fact]
        public void CreateRefreshToken_ExpirationOverflow_LeavesAggregateUnchanged()
        {
            var user = new FlowState().Restore();
            var before = new FlowSnapshot(user);
            var now = DateTimeOffset.MaxValue.AddDays(-7).AddTicks(1);

            Assert.Throws<ArgumentOutOfRangeException>(()
                => user.CreateRefreshToken(
                    FlowDevice(),
                    FlowIp(),
                    now
                )
            );

            before.AssertUnchanged();
        }
        #endregion

        #region Refresh token failure ordering
        // Відмова в доступі має зупинити обробку токена до зміни його статусу.
        // Denied access must stop token processing before its status changes.
        [Fact]
        public void ConsumeRefreshToken_DeletedUserWithElapsedToken_DoesNotProcessOrRemoveToken()
        {
            var data = FlowAccessState(1);
            var token = data.AddToken(expires: FlowTime.AddMinutes(1));
            var user = data.Restore();
            var before = new FlowSnapshot(user);

            var result = user.ConsumeRefreshToken(token.Id, token.ExpiresAt);

            FlowFailure(
                result,
                "ACCESS_USER.USER_DELETED",
                ErrorType.Forbidden
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(TokenStatus.Consumed)]
        [InlineData(TokenStatus.Revoked)]
        [InlineData(TokenStatus.Expired)]
        public void TerminateAllRefreshToken_FirstItemIsInactive_ThrowsWithoutChanges(TokenStatus status)
        {
            var data = new FlowState();
            var invalid = data.AddToken(1);
            data.AddToken(2);
            var user = data.Restore();

            if (status == TokenStatus.Consumed)
            {
                FlowSuccess(invalid.Consume(FlowTime));
            }
            else if (status == TokenStatus.Revoked)
            {
                FlowSuccess(invalid.Revoke(FlowTime));
            }
            else
            {
                invalid.Terminate(invalid.ExpiresAt);
            }

            var before = new FlowSnapshot(user);

            var exception = Assert.Throws<DomainInvalidOperationException>(() =>
                user.TerminateAllRefreshToken(FlowTime.AddMinutes(1))
            );

            FlowPrecondition(
                exception,
                "Terminate",
                "UserRefreshToken"
            );

            before.AssertUnchanged();
        }
        #endregion

        #region ConsumeRefreshToken and RevokeRefreshToken
        [Theory]
        [InlineData(false, 0, 0)]
        [InlineData(false, 0, 1)]
        [InlineData(false, 0, 2)]
        [InlineData(false, 0, 3)]
        [InlineData(false, 1, 0)]
        [InlineData(false, 1, 1)]
        [InlineData(false, 1, 2)]
        [InlineData(false, 1, 3)]
        [InlineData(false, 2, 0)]
        [InlineData(false, 2, 1)]
        [InlineData(false, 2, 2)]
        [InlineData(false, 2, 3)]
        [InlineData(true, 0, 0)]
        [InlineData(true, 0, 1)]
        [InlineData(true, 0, 2)]
        [InlineData(true, 0, 3)]
        [InlineData(true, 1, 0)]
        [InlineData(true, 1, 1)]
        [InlineData(true, 1, 2)]
        [InlineData(true, 1, 3)]
        [InlineData(true, 2, 0)]
        [InlineData(true, 2, 1)]
        [InlineData(true, 2, 2)]
        [InlineData(true, 2, 3)]
        public void ProcessRefreshToken_ValidOrElapsedToken_RecordsExactTransitionAndPreservesOtherTokens(
            bool consume,
            int position,
            int boundary
        )
        {
            var state = new FlowState();
            var tokens = Enumerable
                .Range(1, 3)
                .Select(i => state.AddToken(i, expires: FlowTime.AddMinutes(10)))
                .ToArray();

            var target = tokens[position];
            var user = state.Restore();

            var now = boundary switch
            {
                0 => FlowTime,
                1 => target.ExpiresAt.AddTicks(-1),
                2 => target.ExpiresAt,
                _ => target.ExpiresAt.AddTicks(1),
            };

            var id = UserRefreshTokenId.Restore(target.Id.Value);

            var result = consume ? user.ConsumeRefreshToken(id, now) : user.RevokeRefreshToken(id, now);
            var expired = boundary >= 2;

            if (expired)
            {
                FlowFailure(
                    result,
                    "TOKEN_STATUS.EXPIRED_TOKEN",
                    ErrorType.Forbidden
                );

                Assert.Contains(
                    "UserRefreshToken",
                    Assert.Single(result.Errors).Message
                );
            }
            else
            {
                FlowSuccess(result);
            }

            var status =
                expired ? TokenStatus.Expired
                : consume ? TokenStatus.Consumed
                : TokenStatus.Revoked;

            Assert.Equal(
                status,
                target.TokenStatus
            );

            Assert.Equal(
                !expired && consume ? now : (DateTimeOffset?)null,
                target.ConsumedAt
            );

            Assert.Equal(
                !expired && !consume ? now : (DateTimeOffset?)null,
                target.RevokedAt
            );

            FlowSameItems(
                tokens.Where(x => !ReferenceEquals(x, target)),
                user.RefreshTokens
            );

            Assert.All(
                user.RefreshTokens,
                x => Assert.Equal(TokenStatus.Active, x.TokenStatus)
            );

            var change = Assert.IsType<UserRefreshTokenUpdated>(Assert.Single(user.AggregateChanges));

            Assert.Same(
                target.Id,
                change.RefreshTokenId
            );

            Assert.Same(
                user.Id,
                change.UserId
            );

            Assert.Equal(
                status,
                change.TokenStatus
            );

            Assert.Equal(
                target.ConsumedAt,
                change.ConsumedAt
            );

            Assert.Equal(
                target.RevokedAt,
                change.RevokedAt
            );

            Assert.Equal(
                now,
                change.Now
            );

            Assert.Equal(
                now,
                change.OccurredOn
            );

            Assert.Equal(
                now,
                user.UpdatedAt
            );

            Assert.Empty(user.DomainEvents);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void ProcessRefreshToken_MissingId_ReturnsNotFoundWithoutChanges(
            bool consume,
            bool hasOtherToken)
        {
            var state = new FlowState();

            if (hasOtherToken)
            {
                state.AddToken();
            }

            var user = state.Restore();
            var before = new FlowSnapshot(user);
            var id = UserRefreshTokenId.Restore(FlowGuid(101, 99));

            var result = consume ? user.ConsumeRefreshToken(id, FlowTime) : user.RevokeRefreshToken(id, FlowTime);

            FlowFailure(
                result,
                "TOKEN_STATUS.NOT_FOUND_TOKEN",
                ErrorType.NotFound
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void ProcessRefreshToken_NullId_PrecedesTimestampValidation(
            bool consume,
            bool invalidTime)
        {
            var user = new FlowState().Restore();
            var before = new FlowSnapshot(user);
            var now = invalidTime ? FlowTime.AddTicks(-1) : FlowTime;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                consume ? user.ConsumeRefreshToken(null!, now) : user.RevokeRefreshToken(null!, now)
            );

            FlowEmpty(
                exception,
                "userRefreshTokenId"
            );

            before.AssertUnchanged();
        }

        // Імітуємо зміну вкладеної сутності через збережене посилання після Restore.
        // Simulate a nested entity mutation through a retained reference after Restore.
        [Theory]
        [InlineData(false, TokenStatus.Consumed, false)]
        [InlineData(false, TokenStatus.Consumed, true)]
        [InlineData(false, TokenStatus.Revoked, false)]
        [InlineData(false, TokenStatus.Revoked, true)]
        [InlineData(false, TokenStatus.Expired, false)]
        [InlineData(false, TokenStatus.Expired, true)]
        [InlineData(true, TokenStatus.Consumed, false)]
        [InlineData(true, TokenStatus.Consumed, true)]
        [InlineData(true, TokenStatus.Revoked, false)]
        [InlineData(true, TokenStatus.Revoked, true)]
        [InlineData(true, TokenStatus.Expired, false)]
        [InlineData(true, TokenStatus.Expired, true)]
        public void ProcessRefreshToken_AlreadyTerminalToken_ReturnsConflictWithoutChanges(
            bool consume,
            TokenStatus status,
            bool afterExpiration
        )
        {
            var state = new FlowState();
            var token = state.AddToken();
            var user = state.Restore();

            if (status == TokenStatus.Consumed)
            {
                FlowSuccess(token.Consume(FlowTime));
            }
            else if (status == TokenStatus.Revoked)
            {
                FlowSuccess(token.Revoke(FlowTime));
            }
            else
            {
                token.Terminate(token.ExpiresAt);
            }

            var before = new FlowSnapshot(user);
            var now = afterExpiration ? token.ExpiresAt.AddTicks(1) : FlowTime.AddTicks(1);

            var result = consume ? user.ConsumeRefreshToken(token.Id, now) : user.RevokeRefreshToken(token.Id, now);

            FlowFailure(
                result,
                "TOKEN_STATUS.INVALID_STATUS_TRANSITION",
                ErrorType.Conflict
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, false, true)]
        [InlineData(false, true, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, false)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        [InlineData(true, true, true)]
        public void ProcessRefreshToken_RemovedToken_ReturnsNotFoundOnRepeatedOrCrossOperation(
            bool firstConsume,
            bool secondConsume,
            bool expired
        )
        {
            var state = new FlowState();
            var token = state.AddToken();
            var user = state.Restore();
            var now = expired ? token.ExpiresAt : FlowTime;

            var first = firstConsume
                ? user.ConsumeRefreshToken(token.Id, now)
                : user.RevokeRefreshToken(token.Id, now);

            if (expired)
            {
                FlowFailure(
                    first,
                    "TOKEN_STATUS.EXPIRED_TOKEN",
                    ErrorType.Forbidden
                );
            }
            else
            {
                FlowSuccess(first);
            }

            var before = new FlowSnapshot(user);

            var again = secondConsume
                ? user.ConsumeRefreshToken(token.Id, now.AddTicks(1))
                : user.RevokeRefreshToken(token.Id, now.AddTicks(1));

            FlowFailure(
                again,
                "TOKEN_STATUS.NOT_FOUND_TOKEN",
                ErrorType.NotFound
            );

            before.AssertUnchanged();
        }
        #endregion

        #region Access policy and timestamps
        [Theory]
        [InlineData(false, 1)]
        [InlineData(false, 2)]
        [InlineData(false, 3)]
        [InlineData(true, 1)]
        [InlineData(true, 2)]
        [InlineData(true, 3)]
        public void CreateOrConsumeRefreshToken_DeniedAccess_ReturnsExpectedFailureWithoutChanges(
            bool create,
            int accessState
        )
        {
            var data = FlowAccessState(accessState);
            var user = data.Restore();
            var before = new FlowSnapshot(user);

            Result result = create
                ? user.CreateRefreshToken(FlowDevice(), FlowIp(), FlowTime)
                : user.ConsumeRefreshToken(UserRefreshTokenId.Restore(FlowGuid(101)), FlowTime);

            FlowFailure(
                result,
                accessState == 2 ? "ACCESS_USER.USER_BLOCKED" : "ACCESS_USER.USER_DELETED",
                ErrorType.Forbidden
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(-1L)]
        [InlineData(0L)]
        [InlineData(1L)]
        public void CreateRefreshToken_BanExpirationBoundary_UsesCurrentTimeWithoutCleaningRestrictions(long ticks)
        {
            var data = new FlowState();
            var session = data.AddGroup(RestrictionType.Ban, new[] { 10 });
            var user = data.Restore();
            var before = new FlowSnapshot(user);
            var now = FlowTime.AddMinutes(10).AddTicks(ticks);

            var result = user.CreateRefreshToken(FlowDevice(), FlowIp(), now);

            if (ticks < 0)
            {
                FlowFailure(
                    result,
                    "ACCESS_USER.USER_BLOCKED",
                    ErrorType.Forbidden
                );

                before.AssertUnchanged();
            }
            else
            {
                FlowSuccess(result);

                Assert.Equal(
                    now,
                    result.Value.CreatedAt
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
        }

        [Theory]
        [InlineData(0L)]
        [InlineData(1L)]
        public void ConsumeRefreshToken_ExpiredBan_AllowsTokenCreatedAfterBan(long ticks)
        {
            var data = new FlowState();
            var session = data.AddGroup(RestrictionType.Ban, new[] { 10 });
            var end = FlowTime.AddMinutes(10);
            var token = data.AddToken(created: end);
            var user = data.Restore();

            FlowSuccess(user.ConsumeRefreshToken(
                token.Id,
                end.AddTicks(ticks))
            );

            Assert.Equal(
                TokenStatus.Consumed,
                token.TokenStatus
            );

            Assert.Same(
                session,
                Assert.Single(user.RestrictionSessions)
            );
        }

        [Theory]
        [InlineData(false, RestrictionType.CommentMute)]
        [InlineData(false, RestrictionType.ChatMute)]
        [InlineData(false, RestrictionType.AllMute)]
        [InlineData(true, RestrictionType.CommentMute)]
        [InlineData(true, RestrictionType.ChatMute)]
        [InlineData(true, RestrictionType.AllMute)]
        public void CreateOrConsumeRefreshToken_MuteDoesNotDenyAccess(
            bool create,
            RestrictionType type)
        {
            var data = new FlowState();
            data.AddGroup(type);
            var token = data.AddToken();
            var user = data.Restore();

            Result result = create
                ? user.CreateRefreshToken(FlowDevice(), FlowIp(), FlowTime)
                : user.ConsumeRefreshToken(token.Id, FlowTime);

            FlowSuccess(result);

            Assert.Single(user.Restrictions);
            Assert.Single(user.RestrictionSessions);
        }

        [Fact]
        public void RevokeRefreshToken_ActiveBanWithMissingId_ReturnsNotFoundInsteadOfAccessError()
        {
            var user = FlowAccessState(2).Restore();
            var before = new FlowSnapshot(user);

            var result = user.RevokeRefreshToken(
                UserRefreshTokenId.Restore(FlowGuid(101)),
                FlowTime
            );

            FlowFailure(
                result,
                "TOKEN_STATUS.NOT_FOUND_TOKEN",
                ErrorType.NotFound
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RevokeOrTerminateRefreshTokens_DeletedUser_AllowsCleanup(bool terminate)
        {
            var data = new FlowState();
            data.AddDeletion();
            var token = data.AddToken();
            var user = data.Restore();

            if (terminate)
            {
                user.TerminateAllRefreshToken(FlowTime);
            }
            else
            {
                FlowSuccess(user.RevokeRefreshToken(
                    token.Id,
                    FlowTime)
                );
            }

            Assert.True(user.IsDeleted);

            Assert.Equal(
                TokenStatus.Revoked,
                token.TokenStatus
            );

            Assert.Empty(user.RefreshTokens);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CreateRefreshToken_TimeBeforeAggregateTimestamp_ThrowsWithoutChanges(bool afterCreation)
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
                user.CreateRefreshToken(
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

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ConsumeRefreshToken_TimeBeforeAggregateTimestamp_ThrowsWithoutChanges(bool afterCreation)
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
                user.ConsumeRefreshToken(
                    UserRefreshTokenId.Restore(FlowGuid(101)),
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
        public void RevokeRefreshToken_TimeBeforeAggregateTimestamp_ThrowsWithoutChanges(bool afterCreation)
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
                user.RevokeRefreshToken(
                    UserRefreshTokenId.Restore(FlowGuid(101)),
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
        public void TerminateAllRefreshToken_TimeBeforeAggregateTimestamp_ThrowsWithoutChanges(bool afterCreation)
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
                user.TerminateAllRefreshToken(minimum.AddTicks(-1))
            );

            FlowRange(
                exception,
                afterCreation ? "UpdatedAt" : "CreatedAt",
                minimum
            );

            before.AssertUnchanged();
        }

        [Theory]
        [InlineData(0, -5)]
        [InlineData(0, 2)]
        [InlineData(1, -5)]
        [InlineData(1, 2)]
        [InlineData(2, -5)]
        [InlineData(2, 2)]
        [InlineData(3, -5)]
        [InlineData(3, 2)]
        public void RefreshTokenOperations_EqualInstantWithDifferentOffset_IsAcceptedAndPreserved(
            int operation,
            int offset
        )
        {
            var data = new FlowState { LastLoginAt = FlowTime };
            var token = data.AddToken();
            var user = data.Restore();
            var now = FlowTime.ToOffset(TimeSpan.FromHours(offset));

            switch (operation)
            {
                case 0:
                    var created = user.CreateRefreshToken(
                        FlowDevice(),
                        FlowIp(),
                        now
                    );

                    FlowSuccess(created);

                    Assert.Equal(
                        now.Offset,
                        created.Value.CreatedAt.Offset
                    );
                    break;

                case 1:
                    FlowSuccess(user.ConsumeRefreshToken(
                        token.Id,
                        now)
                    );

                    Assert.Equal(
                        now.Offset,
                        token.ConsumedAt!.Value.Offset
                    );
                    break;

                case 2:
                    FlowSuccess(user.RevokeRefreshToken(
                        token.Id,
                        now)
                    );

                    Assert.Equal(
                        now.Offset,
                        token.RevokedAt!.Value.Offset
                    );
                    break;

                default:
                    user.TerminateAllRefreshToken(now);

                    Assert.Equal(
                        now.Offset,
                        token.RevokedAt!.Value.Offset
                    );
                    break;
            }

            Assert.Equal(
                now.Offset,
                user.UpdatedAt!.Value.Offset
            );

            Assert.All(
                user.AggregateChanges,
                x => Assert.Equal(now.Offset, x.OccurredOn.Offset)
            );
        }
        #endregion

        #region TerminateAllRefreshToken
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TerminateAllRefreshToken_EmptyCollection_DoesNotAdvanceUpdatedAt(bool updated)
        {
            var user = new FlowState { UpdatedAt = updated ? FlowTime : null }.Restore();

            var before = new FlowSnapshot(user);

            user.TerminateAllRefreshToken(FlowTime.AddMinutes(5));

            before.AssertUnchanged();
        }

        [Fact]
        public void TerminateAllRefreshToken_MixedExpirations_UpdatesEachTokenAndPreservesHistory()
        {
            var data = new FlowState();
            var now = FlowTime.AddMinutes(10);

            var tokens = new[]
            {
                data.AddToken(1, expires: now.AddTicks(-1)),
                data.AddToken(2, expires: now),
                data.AddToken(3, expires: now.AddTicks(1)),
            };

            var user = data.Restore();

            user.TerminateAllRefreshToken(now);

            Assert.Empty(user.RefreshTokens);

            Assert.Equal(
                now,
                user.UpdatedAt
            );

            Assert.Equal(
                new[] { TokenStatus.Expired, TokenStatus.Expired, TokenStatus.Revoked },
                tokens.Select(x => x.TokenStatus).ToArray()
            );

            Assert.Equal(
                3
                , user.AggregateChanges.Count
            );

            for (var i = 0; i < tokens.Length; i++)
            {
                var change = Assert.IsType<UserRefreshTokenUpdated>(user.AggregateChanges[i]);

                Assert.Same(
                    tokens[i].Id,
                    change.RefreshTokenId
                );

                Assert.Same(
                    user.Id,
                    change.UserId
                );

                Assert.Equal(
                    i < 2 ? TokenStatus.Expired : TokenStatus.Revoked,
                    change.TokenStatus
                );

                Assert.Null(change.ConsumedAt);

                Assert.Equal(
                    i < 2 ? (DateTimeOffset?)null : now,
                    change.RevokedAt
                );

                Assert.Equal(
                    now,
                    change.Now
                );

                Assert.Equal(
                    now,
                    change.OccurredOn
                );
            }

            Assert.Empty(user.DomainEvents);

            var before = new FlowSnapshot(user);

            user.TerminateAllRefreshToken(now.AddMinutes(1));

            before.AssertUnchanged();
        }

        // Метод документує часткові зміни в пам’яті, якщо пізніший елемент спричинив виняток.
        // The method documents partial in-memory changes if a later item throws.
        [Theory]
        [InlineData(TokenStatus.Consumed)]
        [InlineData(TokenStatus.Revoked)]
        [InlineData(TokenStatus.Expired)]
        public void TerminateAllRefreshToken_NonActiveItem_DoesNotRollbackEarlierItems(TokenStatus status)
        {
            var data = new FlowState();
            var first = data.AddToken(1);
            var invalid = data.AddToken(2);
            var last = data.AddToken(3);
            var user = data.Restore();

            if (status == TokenStatus.Consumed)
            {
                FlowSuccess(invalid.Consume(FlowTime));
            }
            else if (status == TokenStatus.Revoked)
            {
                FlowSuccess(invalid.Revoke(FlowTime));
            }
            else
            {
                invalid.Terminate(invalid.ExpiresAt);
            }

            var now = FlowTime.AddMinutes(1);

            var exception = Assert.Throws<DomainInvalidOperationException>(() => user.TerminateAllRefreshToken(now));

            FlowPrecondition(
                exception,
                "Terminate",
                "UserRefreshToken"
            );

            Assert.Equal(
                TokenStatus.Revoked,
                first.TokenStatus
            );

            Assert.Equal(
                now,
                first.RevokedAt
            );

            FlowSameItems(
                new[] { invalid, last },
                user.RefreshTokens
            );

            Assert.Equal(
                status,
                invalid.TokenStatus
            );

            Assert.Equal(
                TokenStatus.Active,
                last.TokenStatus
            );

            Assert.Single(user.AggregateChanges);

            Assert.Equal(
                FlowTime,
                user.UpdatedAt
            );
        }
        #endregion
    }
}
