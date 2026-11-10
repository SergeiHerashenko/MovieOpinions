using Authorization.Domain.Common.Exceptions;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.ValueObjects.EmailUser;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Common.ValueObjects.PasswordUser;
using Authorization.Domain.Common.ValueObjects.PhoneUser;
using Authorization.Domain.Users.Aggregate;
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

namespace Authorization.Domain.UnitTests.Users.Aggregate
{
    public partial class UserTests
    {
        private static readonly DateTimeOffset RestorationTime = new(
            2026, 11, 10,
            12, 0, 0,
            TimeSpan.Zero
        );

        private sealed class RestorationState
        {
            public UserId UserId { get; set; } = RestorationUserId();
            public DateTimeOffset CreatedAt { get; set; } = RestorationTime;
            public Login Login { get; set; } = Login.From(Email.Restore("serhii@example.com"));
            public Password Password { get; set; } = Password.Restore(PasswordHash.Restore("TEST.HASH"));
            public Role Role { get; set; } = Role.User;
            public DateTimeOffset? UpdatedAt { get; set; }
            public DateTimeOffset? LastLoginAt { get; set; }
            public int FailedPasswordAttempts { get; set; }
            public IEnumerable<UserRestriction> Restrictions { get; set; } = Array.Empty<UserRestriction>();
            public IEnumerable<UserRestrictionSession> Sessions { get; set; } = Array.Empty<UserRestrictionSession>();
            public IEnumerable<UserRefreshToken> Tokens { get; set; } = Array.Empty<UserRefreshToken>();
            public UserPendingAction? Action { get; set; }
            public UserDeletion? Deletion { get; set; }

            public User Restore()
                => User.Restore(
                    UserId,
                    CreatedAt,
                    Login,
                    Password,
                    Role,
                    UpdatedAt,
                    LastLoginAt,
                    FailedPasswordAttempts,
                    Restrictions,
                    Sessions,
                    Tokens,
                    Action,
                    Deletion
                );
        }

        private static class RestorationRules
        {
            public const string UpdatedBeforeCreated =
                "UpdatedAt must not be earlier than CreatedAt.";

            public const string LoginBeforeCreated =
                "LastLoginAt must not be earlier than CreatedAt.";

            public const string UpdatedRequiredForLogin =
                "UpdatedAt must be set when LastLoginAt is set.";
            public const string UpdatedBeforeLogin =
                "UpdatedAt must not be earlier than LastLoginAt.";

            public const string NullRestrictions =
                "The active restriction collection contains null elements.";

            public const string DuplicateRestrictions =
                "The active restriction collection contains duplicate identifiers.";

            public const string ActiveRestrictions =
                "Only active restrictions can be restored in the user's active restriction collection.";

            public const string NullSessions =
                "The restriction session collection contains null elements.";

            public const string DuplicateSessions =
                "The restriction session collection contains duplicate identifiers.";

            public const string DuplicateSessionTypes =
                "The user contains more than one restriction session of the same type.";

            public const string TypesMismatch =
                "The active restriction types do not exactly match the restriction session types.";

            public const string IdsMismatch =
                "The restriction session identifiers do not exactly match " +
                "the active restrictions of its type.";

            public const string DurationMismatch =
                "The restriction session total blocked duration does not match " +
                "the duration of its active restrictions.";

            public const string BeforeSession =
                "A restriction cannot be created before the session to which it belongs.";

            public const string AtOrAfterSessionEnd =
                "A current restriction cannot be created at or after its session's expiration time.";

            public const string NullTokens =
                "The refresh token collection contains null elements.";

            public const string DuplicateTokenIds =
                "The 'RefreshToken' collection contains duplicate identifiers.";

            public const string DuplicateTokenValues =
                "The refresh token collection contains duplicate token values.";

            public const string ActiveTokens =
                "Only active refresh tokens can be restored as part of the user aggregate.";

            public const string ActiveAction =
                "Only active pending action can be restored as part of the user aggregate.";

            public const string RestoredDeletion =
                "A restored deletion record cannot be attached as the user's current deletion.";

            public const string TokenBeforeBanEnd =
                "A current refresh token must have been created " +
                "at or after the attached ban session's expiration time.";

            public const string UpdatedRequired =
                "User.UpdatedAt must be set when the aggregate contains " +
                "related state or recorded failed password attempts.";
        }

        #region Helpers: коректні вкладені сутності
        private static Guid RestorationGuid(int family, int number)
            => Guid.Parse($"{family:x8}-0000-4000-8000-{number:x12}");

        private static UserId RestorationUserId(int number = 1)
            => UserId.Restore(RestorationGuid(1, number));

        private static UserRestrictionId RestorationRestrictionId(int number)
            => UserRestrictionId.Restore(RestorationGuid(2, number));

        private static UserRestriction RestorationRestriction(
            int number = 1,
            int minutes = 10,
            RestrictionType type = RestrictionType.ChatMute,
            UserId? owner = null,
            DateTimeOffset? createdAt = null,
            RestrictionStatus status = RestrictionStatus.Active)
        {
            var created = createdAt ?? RestorationTime;

            return UserRestriction.Restore(
                RestorationRestrictionId(number),
                owner ?? RestorationUserId(),
                RestrictionRule.Restore($"Rule {number}", minutes),
                type,
                "System",
                null,
                status,
                created,
                status == RestrictionStatus.Revoked ? created.AddTicks(1) : null
            );
        }

        private static UserRestrictionSession RestorationSession(
            IEnumerable<UserRestrictionId> ids,
            int totalMinutes,
            int number = 1,
            RestrictionType type = RestrictionType.ChatMute,
            UserId? owner = null,
            DateTimeOffset? createdAt = null)
        {
            return UserRestrictionSession.Restore(
                UserRestrictionSessionId.Restore(RestorationGuid(3, number)),
                owner ?? RestorationUserId(),
                ids.ToArray(),
                type,
                totalMinutes,
                createdAt ?? RestorationTime
            );
        }

        private static string RestorationBase64(byte fill)
        {
            var bytes = new byte[64];
            Array.Fill(bytes, fill);
            return Convert.ToBase64String(bytes);
        }

        private static UserRefreshToken RestorationToken(
            int number = 1,
            byte tokenValue = 1,
            UserId? owner = null,
            DateTimeOffset? createdAt = null,
            TokenStatus status = TokenStatus.Active)
        {
            var created = createdAt ?? RestorationTime;

            return UserRefreshToken.Restore(
                UserRefreshTokenId.Restore(RestorationGuid(4, number)),
                owner ?? RestorationUserId(),
                RefreshToken.Restore(RestorationBase64(tokenValue)),
                DeviceInfo.Restore(DeviceType.Desktop, "Windows 11", "Chrome", "Model X"),
                IpAddress.Restore("192.168.1.1"),
                "Kyiv",
                status,
                created.AddHours(1),
                status == TokenStatus.Consumed ? created.AddMinutes(1) : null,
                created,
                status == TokenStatus.Revoked ? created.AddMinutes(1) : null
            );
        }

        private static UserPendingAction RestorationAction(
            UserId? owner = null,
            DateTimeOffset? createdAt = null,
            ActionStatus status = ActionStatus.Active)
        {
            var created = createdAt ?? RestorationTime;
            var expiration = created.AddMinutes(30);
            var token = ConfirmationFlowToken.Parse(RestorationBase64(7));

            Assert.True(token.IsSuccess);

            return UserPendingAction.Restore(
                UserPendingActionId.Restore(RestorationGuid(5, 1)),
                owner ?? RestorationUserId(),
                token.Value,
                ChangePasswordAction.Restore(PasswordHash.Restore("NEW.HASH")),
                expiration,
                status == ActionStatus.Confirmed ? created.AddMinutes(1) : null,
                status == ActionStatus.Expired ? expiration : null,
                status == ActionStatus.Cancelled ? created.AddMinutes(1) : null,
                status,
                created
            );
        }

        private static UserDeletion RestorationDeletion(
            UserId? owner = null,
            DateTimeOffset? createdAt = null,
            DeletionStatus status = DeletionStatus.Deleted)
        {
            var created = createdAt ?? RestorationTime;
            var updated = status switch
            {
                DeletionStatus.Restored => (DateTimeOffset?)created.AddMinutes(1),
                DeletionStatus.PermanentlyDeleted => created.AddHours(2),
                _ => null
            };

            return UserDeletion.Restore(
                UserDeletionId.Restore(RestorationGuid(6, 1)),
                owner ?? RestorationUserId(),
                Login.From(Email.Restore("serhii@example.com")),
                DeletionReason.Restore("Account closed"),
                created,
                created.AddHours(1),
                status == DeletionStatus.Restored ? updated : null,
                status,
                updated
            );
        }

        private static RestorationState RestorationRestrictedState(
            RestrictionType type = RestrictionType.ChatMute,
            int[]? durations = null,
            DateTimeOffset? sessionCreatedAt = null,
            DateTimeOffset? restrictionCreatedAt = null)
        {
            var state = new RestorationState();
            var sessionTime = sessionCreatedAt ?? state.CreatedAt;
            var restrictionTime = restrictionCreatedAt ?? sessionTime;
            var minutes = durations ?? new[] { 10 };
            var restrictions = minutes.Select((duration, index) => RestorationRestriction(
                index + 1,
                duration,
                type,
                state.UserId,
                restrictionTime
            )).ToArray();

            state.Restrictions = restrictions;
            state.Sessions = new[]
            {
                RestorationSession(
                    restrictions.Select(item => item.Id),
                    minutes.Sum(),
                    type: type,
                    owner: state.UserId,
                    createdAt: sessionTime
                )
            };
            state.UpdatedAt = restrictionTime >= sessionTime ? restrictionTime : sessionTime;

            return state;
        }

        private static void RestorationAttachEntity(
            RestorationState state,
            string kind,
            UserId? owner = null,
            DateTimeOffset? createdAt = null)
        {
            var entityOwner = owner ?? state.UserId;
            var created = createdAt ?? state.CreatedAt;

            switch (kind)
            {
                case "restriction":
                    var restriction = RestorationRestriction(
                        owner: entityOwner,
                        createdAt: created
                    );

                    state.Restrictions = new[] { restriction };
                    state.Sessions = new[]
                    {
                        RestorationSession(
                            new[] {restriction.Id },
                            10,
                            owner: state.UserId,
                            createdAt: state.CreatedAt
                        )
                    };
                    break;

                case "session":
                    var restrictionTime = created >= state.CreatedAt ? created : state.CreatedAt;
                    var sessionRestriction = RestorationRestriction(
                        owner: state.UserId,
                        createdAt: restrictionTime
                    );

                    state.Restrictions = new[] { sessionRestriction };
                    state.Sessions = new[]
                    {
                        RestorationSession(
                            new[] { sessionRestriction.Id },
                            10,
                            owner: entityOwner,
                            createdAt: created
                        )
                    };
                    break;

                case "token":
                    state.Tokens = new[]
                    {
                        RestorationToken(
                            owner: entityOwner,
                            createdAt: created
                        )
                    };
                    break;

                case "action":
                    state.Action = RestorationAction(
                        entityOwner,
                        created
                    );
                    break;

                case "deletion":
                    state.Deletion = RestorationDeletion(
                        entityOwner,
                        created
                    );
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(kind),
                        kind,
                        "Unknown test entity."
                    );
            }

            state.UpdatedAt = created >= state.CreatedAt ? created : state.CreatedAt;
        }

        private static IUserOwned RestorationOwnedEntity(
            RestorationState state,
            string kind)
            => kind switch
            {
                "restriction" => state.Restrictions.Single(),
                "session" => state.Sessions.Single(),
                "token" => state.Tokens.Single(),
                "action" => state.Action!,
                "deletion" => state.Deletion!,
                _ => throw new ArgumentOutOfRangeException(nameof(kind))
            };

        private static string RestorationEntityName(string kind)
            => kind switch
            {
                "restriction" => nameof(UserRestriction),
                "session" => nameof(UserRestrictionSession),
                "token" => nameof(UserRefreshToken),
                "action" => nameof(UserPendingAction),
                "deletion" => nameof(UserDeletion),
                _ => throw new ArgumentOutOfRangeException(nameof(kind))
            };

        private static string RestorationOwnerRule(string kind)
            => $"A '{RestorationEntityName(kind)}' belonging to another user cannot " +
                "be restored as part of this aggregate.";
        #endregion

        #region Helpers: перевірки стану та діагностики
        private static void RestorationAssertNoEvents(User user)
        {
            Assert.Empty(user.DomainEvents);
            Assert.Empty(user.AggregateChanges);
        }

        private static void RestorationAssertSameItems<T>(
            IEnumerable<T> expected,
            IEnumerable<T> actual)
            where T : class
        {
            var expectedItems = expected.ToArray();
            var actualItems = actual.ToArray();

            Assert.Equal(
                expectedItems.Length,
                actualItems.Length
            );

            foreach (var item in expectedItems)
            {
                Assert.Contains(
                    actualItems,
                    candidate => ReferenceEquals(
                        item,
                        candidate
                    )
                );
            }
        }

        private static void RestorationAssertState(
            RestorationState expected,
            User actual)
        {
            Assert.Same(
                expected.UserId,
                actual.Id
            );

            Assert.Same(
                expected.Login,
                actual.Login
            );

            Assert.Same(
                expected.Password,
                actual.Password
            );

            Assert.Equal(
                expected.Role,
                actual.Role
            );

            Assert.Equal(
                expected.CreatedAt,
                actual.CreatedAt
            );

            Assert.Equal(
                expected.CreatedAt.Offset,
                actual.CreatedAt.Offset
            );

            Assert.Equal(
                expected.UpdatedAt,
                actual.UpdatedAt
            );

            Assert.Equal(
                expected.UpdatedAt?.Offset,
                actual.UpdatedAt?.Offset
            );

            Assert.Equal(
                expected.LastLoginAt,
                actual.LastLoginAt
            );

            Assert.Equal(
                expected.LastLoginAt?.Offset,
                actual.LastLoginAt?.Offset
            );

            Assert.Equal(
                expected.FailedPasswordAttempts,
                actual.FailedPasswordAttempts
            );

            Assert.Equal(
                expected.Action is not null,
                actual.IsAction
            );

            Assert.Equal(
                expected.Deletion is not null,
                actual.IsDeleted
            );

            RestorationAssertSameItems(
                expected.Restrictions,
                actual.Restrictions
            );

            RestorationAssertSameItems(
                expected.Sessions,
                actual.RestrictionSessions
            );

            RestorationAssertSameItems(
                expected.Tokens,
                actual.RefreshTokens
            );

            if (expected.Deletion is not null)
            {
                var deletion = actual.GetDeletion();

                Assert.True(deletion.IsSuccess);

                Assert.Same(
                    expected.Deletion,
                    deletion.Value
                );
            }

            RestorationAssertNoEvents(actual);
        }

        private static void RestorationAssertCommon(
            BaseException exception,
            string code,
            ExceptionType type,
            string message)
        {
            Assert.Equal(
                code,
                exception.ExceptionCode
            );

            Assert.Equal(
                type,
                exception.ExceptionType
            );

            Assert.Equal(
                message,
                exception.Message
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
                "Restore",
                exception.Context["Operation"]
            );
        }

        private static void RestorationAssertContext(
            IReadOnlyDictionary<string, object> actual,
            string prefix,
            params (string Key, object? Value)[] expected)
        {
            foreach (var (key, value) in expected)
            {
                var actualValue = actual[prefix + key];

                if (value is Array expectedArray)
                {
                    var actualArray = Assert.IsAssignableFrom<Array>(actualValue);

                    Assert.Equal(
                        expectedArray.GetType(),
                        actualArray.GetType()
                    );

                    Assert.Equal(
                        expectedArray.Length,
                        actualArray.Length
                    );

                    Assert.True(expectedArray.Cast<object>().ToHashSet().SetEquals(actualArray.Cast<object>()));
                }
                else
                {
                    Assert.Equal(
                        value ?? "null",
                        actualValue
                    );
                }
            }
        }

        private static void RestorationAssertEmpty(
            DomainDataInconsistencyException exception,
            string field)
        {
            RestorationAssertCommon(
                exception,
                "DOMAIN.DATA_INCONSISTENCY.EMPTY_VALUE",
                ExceptionType.DataInconsistency,
                "An error occurred during the 'Restore' operation. " +
                $"The field '{field}' in type 'User' is empty!"
            );

            Assert.Equal(
                field,
                exception.Context["Field"]
            );
        }

        private static void RestorationAssertUnsupported(
            DomainDataInconsistencyException exception,
            Role role)
        {
            RestorationAssertCommon(
                exception,
                "DOMAIN.DATA_INCONSISTENCY.UNSUPPORTED_TYPE",
                ExceptionType.DataInconsistency,
                "An error occurred during the 'Restore' operation. " +
                $"Unsupported discriminator value '{role}' for type 'User'!"
            );

            Assert.Equal(
                "role",
                exception.Context["Field"]
            );

            Assert.Equal(
                role,
                exception.Context["Value"]
            );
        }

        private static void RestorationAssertRange(
            DomainDataInconsistencyException exception,
            string field,
            string valueType,
            params (string Key, object? Value)[] custom)
        {
            RestorationAssertCommon(
                exception,
                "DOMAIN.DATA_INCONSISTENCY.OUT_OF_RANGE",
                ExceptionType.DataInconsistency,
                "An error occurred during the 'Restore' operation. " +
                $"The field '{field}' in type 'User' has a value outside the allowed range!"
            );

            Assert.Equal(
                field,
                exception.Context["Field"]
            );

            Assert.Equal(
                valueType,
                exception.Context["ValueType"]
            );

            RestorationAssertContext(exception.Context, "Custom_", custom);
        }

        private static void RestorationAssertDateRange(
            DomainDataInconsistencyException exception,
            string field,
            string minimumField,
            DateTimeOffset minimum)
            => RestorationAssertRange(
                exception,
                field,
                "DateTimeOffset",
                ("MinimumField", minimumField),
                ("MinimumValue", minimum)
            );

        private static void RestorationAssertInvariant(
            DomainInvariantViolationException exception,
            string rule,
            params (string Key, object? Value)[] state)
        {
            RestorationAssertCommon(
                exception,
                "DOMAIN.INVARIANT_VIOLATION.INVALID_STATE",
                ExceptionType.InvariantViolation,
                $"Critical invariant violation in type 'User'. Broken rule: {rule}."
            );

            Assert.Equal(
                rule,
                exception.Context["Rule"]
            );

            RestorationAssertContext(
                exception.Context,
                "State_",
                state
            );
        }

        private static User RestorationRestoreWithNulls(
            RestorationState state,
            params string[] fields)
            => User.Restore(
                fields.Contains("userId") ? null! : state.UserId,
                state.CreatedAt,
                fields.Contains("login") ? null! : state.Login,
                fields.Contains("password") ? null! : state.Password,
                state.Role,
                state.UpdatedAt,
                state.LastLoginAt,
                state.FailedPasswordAttempts,
                fields.Contains("restrictions") ? null! : state.Restrictions,
                fields.Contains("restrictionSessions") ? null! : state.Sessions,
                fields.Contains("refreshTokens") ? null! : state.Tokens,
                state.Action,
                state.Deletion
            );
        #endregion

        #region Успішне відновлення та збереження даних
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Restore_MinimalValidState_PreservesProperties(bool emailLogin)
        {
            var state = new RestorationState
            {
                CreatedAt = RestorationTime.ToOffset(TimeSpan.FromHours(2)),
                Login = emailLogin
                    ? Login.From(Email.Restore("serhii@example.com"))
                    : Login.From(Phone.Restore(
                        PhoneCountryCode.Restore("+380"),
                        PhoneNationalNumber.Restore("981234567")
                    ))
            };

            var user = state.Restore();

            RestorationAssertState(state, user);

            Assert.Empty(user.Restrictions);
            Assert.Empty(user.RestrictionSessions);
            Assert.Empty(user.RefreshTokens);
            Assert.Null(user.UpdatedAt);
            Assert.Null(user.LastLoginAt);
            Assert.False(user.IsAction);
            Assert.False(user.IsDeleted);
        }

        [Theory]
        [InlineData(Role.Guest)]
        [InlineData(Role.User)]
        [InlineData(Role.Moderator)]
        [InlineData(Role.Admin)]
        public void Restore_ValidRole_PreservesRole(Role role)
        {
            var state = new RestorationState { Role = role };

            var user = state.Restore();

            RestorationAssertState(state, user);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void Restore_ValidFailedPasswordAttempts_PreservesCount(int attempts)
        {
            var state = new RestorationState
            {
                FailedPasswordAttempts = attempts,
                UpdatedAt = RestorationTime
            };

            var user = state.Restore();

            RestorationAssertState(
                state,
                user
            );
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(0L, null)]
        [InlineData(0L, 0L)]
        [InlineData(1L, 0L)]
        [InlineData(1L, 1L)]
        [InlineData(2L, 1L)]
        public void Restore_ValidUserTimestamps_PreservesValues(
            long? updatedTicks,
            long? loginTicks)
        {
            var state = new RestorationState
            {
                UpdatedAt = updatedTicks is null
                    ? null
                    : RestorationTime.AddTicks(updatedTicks.Value).ToOffset(TimeSpan.FromHours(2)),
                LastLoginAt = loginTicks is null
                    ? null
                    : RestorationTime.AddTicks(loginTicks.Value).ToOffset(TimeSpan.FromHours(-5))
            };

            var user = state.Restore();

            RestorationAssertState(
                state,
                user
            );
        }

        [Fact]
        public void Restore_ValidRelatedCollections_PreservesInstances()
        {
            var state = RestorationRestrictedState(durations: new[] { 10, 20 });
            state.Tokens = new[] { RestorationToken(), RestorationToken(2, 2) };
            state.Action = RestorationAction();
            state.Deletion = RestorationDeletion();

            var user = state.Restore();

            RestorationAssertState(
                state,
                user
            );
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public void Restore_OptionalRelatedState_SetsPresenceFlags(
            bool hasAction,
            bool hasDeletion)
        {
            var state = new RestorationState
            {
                Action = hasAction ? RestorationAction() : null,
                Deletion = hasDeletion ? RestorationDeletion() : null,
                UpdatedAt = RestorationTime
            };

            var user = state.Restore();

            RestorationAssertState(
                state,
                user
            );
        }

        [Theory]
        [InlineData("restrictions")]
        [InlineData("sessions")]
        [InlineData("tokens")]
        public void Restore_SourceCollectionsChanged_DoesNotChangeAggregateCollections(string kind)
        {
            var state = RestorationRestrictedState(durations: new[] { 10, 20 });
            var restrictions = state.Restrictions.ToList();
            var sessions = state.Sessions.ToList();
            var tokens = new List<UserRefreshToken> {
                RestorationToken(),
                RestorationToken(2, 2)
            };
            state.Restrictions = restrictions;
            state.Sessions = sessions;
            state.Tokens = tokens;
            var expectedRestrictions = restrictions.ToArray();
            var expectedSessions = sessions.ToArray();
            var expectedTokens = tokens.ToArray();

            var user = state.Restore();

            switch (kind)
            {
                case "restrictions":
                    restrictions.Clear();
                    restrictions.Add(RestorationRestriction(99));
                    break;
                case "sessions":
                    sessions.Clear();
                    sessions.Add(RestorationSession(new[] { RestorationRestrictionId(99) }, 10, 99));
                    break;
                case "tokens":
                    tokens.Clear();
                    tokens.Add(RestorationToken(99, 99));
                    break;
            }

            RestorationAssertSameItems(
                expectedRestrictions,
                user.Restrictions
            );

            RestorationAssertSameItems(
                expectedSessions,
                user.RestrictionSessions
            );

            RestorationAssertSameItems(
                expectedTokens,
                user.RefreshTokens
            );

            RestorationAssertNoEvents(user);
        }

        [Fact]
        public void Restore_InputEnumerables_AreEnumeratedOnce()
        {
            var state = RestorationRestrictedState();
            var restrictions = state.Restrictions.ToArray();
            var sessions = state.Sessions.ToArray();
            var tokens = new[] { RestorationToken() };
            var restrictionCount = 0;
            var sessionCount = 0;
            var tokenCount = 0;

            IEnumerable<T> Count<T>(
                IEnumerable<T> items,
                Action onEnumeration)
            {
                onEnumeration();

                foreach (var item in items)
                {
                    yield return item;
                }
            }

            state.Restrictions = Count(
                restrictions,
                () => restrictionCount++
            );

            state.Sessions = Count(
                sessions,
                () => sessionCount++
            );

            state.Tokens = Count(
                tokens,
                () => tokenCount++
            );

            var user = state.Restore();

            Assert.Equal(
                1,
                restrictionCount
            );

            Assert.Equal(
                1,
                sessionCount
            );

            Assert.Equal(
                1,
                tokenCount
            );

            RestorationAssertSameItems(
                restrictions,
                user.Restrictions
            );

            RestorationAssertSameItems(
                sessions,
                user.RestrictionSessions
            );

            RestorationAssertSameItems(
                tokens,
                user.RefreshTokens
            );

            RestorationAssertNoEvents(user);
        }

        [Theory]
        [InlineData("token")]
        [InlineData("action")]
        [InlineData("session")]
        public void Restore_ElapsedLifetimes_PreservesStoredActiveState(string kind)
        {
            var state = new RestorationState();

            RestorationAttachEntity(
                state,
                kind
            );

            var referenceTime = RestorationTime.AddHours(2);
            state.UpdatedAt = referenceTime;

            var user = state.Restore();

            RestorationAssertState(
                state,
                user
            );

            if (kind == "token")
            {
                Assert.True(state.Tokens.Single().ExpiresAt < referenceTime);

                Assert.Equal(
                    TokenStatus.Active,
                    user.RefreshTokens.Single().TokenStatus
                );
            }
            else if (kind == "action")
            {
                var action = state.Action!;
                Assert.True(action.ExpiresAt < referenceTime);

                Assert.Equal(
                    ActionStatus.Active,
                    action.Status
                );

                Assert.True(user.IsAction);
            }
            else
            {
                Assert.True(state.Sessions.Single().GetExpirationDate() < referenceTime);

                Assert.Equal(
                    RestrictionStatus.Active,
                    user.Restrictions.Single().Status
                );

                Assert.Single(user.RestrictionSessions);
            }
        }
        #endregion

        #region Обов'язкові аргументи та базові значення
        [Theory]
        [InlineData("userId")]
        [InlineData("login")]
        [InlineData("password")]
        [InlineData("restrictions")]
        [InlineData("restrictionSessions")]
        [InlineData("refreshTokens")]
        public void Restore_NullRequiredArgument_ThrowsDataInconsistency(string field)
        {
            var state = new RestorationState();

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                RestorationRestoreWithNulls(
                    state,
                    field
                )
            );

            RestorationAssertEmpty(
                exception,
                field
            );
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        public void Restore_UndefinedRole_ThrowsDataInconsistency(int roleValue)
        {
            var state = new RestorationState { Role = (Role)roleValue };

            var exception = Assert.Throws<DomainDataInconsistencyException>(() => state.Restore());

            RestorationAssertUnsupported(
                exception,
                state.Role
            );
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(int.MaxValue)]
        public void Restore_InvalidFailedPasswordAttempts_ThrowsDataInconsistency(int attempts)
        {
            var state = new RestorationState
            {
                FailedPasswordAttempts = attempts,
                UpdatedAt = RestorationTime
            };

            var exception = Assert.Throws<DomainDataInconsistencyException>(() => state.Restore());

            RestorationAssertRange(
                exception,
                "failedPasswordAttempts",
                "Int32",
                ("FailedPasswordAttempts", attempts),
                ("MaximumFailedPasswordAttempts", 3)
            );
        }
        #endregion

        #region Часові мітки користувача
        [Fact]
        public void Restore_UpdatedAtBeforeCreatedAt_ThrowsInvariant()
        {
            var state = new RestorationState { UpdatedAt = RestorationTime.AddTicks(-1) };

            var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

            RestorationAssertInvariant(
                exception,
                RestorationRules.UpdatedBeforeCreated,
                ("UpdatedAt", state.UpdatedAt),
                ("CreatedAt", state.CreatedAt)
            );
        }

        [Fact]
        public void Restore_LastLoginAtBeforeCreatedAt_ThrowsInvariant()
        {
            var state = new RestorationState
            {
                UpdatedAt = RestorationTime,
                LastLoginAt = RestorationTime.AddTicks(-1)
            };

            var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

            RestorationAssertInvariant(
                exception,
                RestorationRules.LoginBeforeCreated,
                ("LastLoginAt", state.LastLoginAt),
                ("CreatedAt", state.CreatedAt)
            );
        }

        [Fact]
        public void Restore_LastLoginAtWithoutUpdatedAt_ThrowsInvariant()
        {
            var state = new RestorationState { LastLoginAt = RestorationTime };

            var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

            RestorationAssertInvariant(
                exception,
                RestorationRules.UpdatedRequiredForLogin,
                ("CreatedAt", state.CreatedAt),
                ("UpdatedAt", null),
                ("LastLoginAt", state.LastLoginAt)
            );
        }

        [Fact]
        public void Restore_UpdatedAtBeforeLastLoginAt_ThrowsInvariant()
        {
            var state = new RestorationState
            {
                UpdatedAt = RestorationTime,
                LastLoginAt = RestorationTime.AddTicks(1)
            };

            var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

            RestorationAssertInvariant(
                exception,
                RestorationRules.UpdatedBeforeLogin,
                ("UpdatedAt", state.UpdatedAt),
                ("LastLoginAt", state.LastLoginAt)
            );
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Restore_UserTimestampsWithDifferentOffsets_CompareInstants(bool sameInstant)
        {
            var state = new RestorationState
            {
                UpdatedAt = sameInstant
                    ? RestorationTime.ToOffset(TimeSpan.FromHours(2))
                    : RestorationTime.AddHours(-1).ToOffset(TimeSpan.FromHours(2))
            };

            if (sameInstant)
            {
                var user = state.Restore();
                RestorationAssertState(state, user);
            }
            else
            {
                var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());
                RestorationAssertInvariant(
                    exception,
                    RestorationRules.UpdatedBeforeCreated,
                    ("UpdatedAt", state.UpdatedAt),
                    ("CreatedAt", state.CreatedAt)
                );
            }
        }
        #endregion

        #region Власник і час створення вкладених сутностей
        [Theory]
        [InlineData("restriction")]
        [InlineData("session")]
        [InlineData("token")]
        [InlineData("action")]
        [InlineData("deletion")]
        public void Restore_RelatedEntityOwnedByAnotherUser_ThrowsInvariant(string kind)
        {
            var state = new RestorationState();
            var foreignOwner = RestorationUserId(2);
            RestorationAttachEntity(
                state,
                kind,
                foreignOwner
            );

            var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

            RestorationAssertInvariant(
                exception,
                RestorationOwnerRule(kind),
                ("EntityType", RestorationEntityName(kind)),
                ("ExpectedUserId", state.UserId.Value),
                ("ActualUserId", foreignOwner.Value)
            );
        }

        [Theory]
        [InlineData("restriction")]
        [InlineData("session")]
        [InlineData("token")]
        [InlineData("action")]
        [InlineData("deletion")]
        public void Restore_RelatedEntityWithEquivalentOwnerId_Succeeds(string kind)
        {
            var state = new RestorationState();
            var equivalentOwner = UserId.Restore(state.UserId.Value);

            Assert.NotSame(
                state.UserId,
                equivalentOwner
            );

            RestorationAttachEntity(
                state,
                kind,
                equivalentOwner
            );

            Assert.Same(
                equivalentOwner,
                RestorationOwnedEntity(state, kind).UserId
            );

            var user = state.Restore();

            RestorationAssertState(
                state,
                user
            );
        }

        [Theory]
        [InlineData("restriction")]
        [InlineData("session")]
        [InlineData("token")]
        [InlineData("action")]
        [InlineData("deletion")]
        public void Restore_RelatedEntityCreatedBeforeUser_ThrowsDataInconsistency(string kind)
        {
            var state = new RestorationState();
            RestorationAttachEntity(
                state,
                kind,
                createdAt: state.CreatedAt.AddTicks(-1)
            );

            var exception = Assert.Throws<DomainDataInconsistencyException>(() => state.Restore());

            RestorationAssertDateRange(
                exception,
                "CreatedAt",
                "userCreatedAt",
                state.CreatedAt
            );
        }

        [Theory]
        [InlineData("restriction", 0)]
        [InlineData("restriction", 1)]
        [InlineData("session", 0)]
        [InlineData("session", 1)]
        [InlineData("token", 0)]
        [InlineData("token", 1)]
        [InlineData("action", 0)]
        [InlineData("action", 1)]
        [InlineData("deletion", 0)]
        [InlineData("deletion", 1)]
        public void Restore_RelatedEntityCreatedAtOrAfterUserCreation_Succeeds(string kind, int minutes)
        {
            var state = new RestorationState();
            RestorationAttachEntity(
                state,
                kind,
                createdAt: state.CreatedAt.AddMinutes(minutes)
            );

            var user = state.Restore();

            RestorationAssertState(
                state,
                user
            );
        }
        #endregion

        #region Склад колекцій і статуси
        [Theory]
        [InlineData("restrictions", 0, 1)]
        [InlineData("restrictions", 1, 1)]
        [InlineData("restrictions", 1, 2)]
        [InlineData("sessions", 0, 1)]
        [InlineData("sessions", 1, 1)]
        [InlineData("sessions", 1, 2)]
        [InlineData("tokens", 0, 1)]
        [InlineData("tokens", 1, 1)]
        [InlineData("tokens", 1, 2)]
        public void Restore_RelatedCollectionContainsNull_ThrowsInvariant(
            string kind,
            int index,
            int nullCount)
        {
            var state = RestorationRestrictedState();
            string rule;
            string countKey;
            string nullKey;

            T[] InsertNulls<T>(T item) where T : class
            {
                var items = new List<T> { item };

                for (var i = 0; i < nullCount; i++)
                {
                    items.Insert(index + i, null!);
                }

                return items.ToArray();
            }

            switch (kind)
            {
                case "restrictions":
                    state.Restrictions = InsertNulls(state.Restrictions.Single());
                    rule = RestorationRules.NullRestrictions;
                    countKey = "RestrictionCount";
                    nullKey = "NullRestrictionCount";
                    break;

                case "sessions":
                    state.Sessions = InsertNulls(state.Sessions.Single());
                    rule = RestorationRules.NullSessions;
                    countKey = "RestrictionSessionCount";
                    nullKey = "NullRestrictionSessionCount";
                    break;

                case "tokens":
                    state.Tokens = InsertNulls(RestorationToken());
                    rule = RestorationRules.NullTokens;
                    countKey = "RefreshTokenCount";
                    nullKey = "NullRefreshTokenCount";
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }

            var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

            RestorationAssertInvariant(
                exception,
                rule,
                ("UserId", state.UserId.Value),
                (countKey, 1 + nullCount),
                (nullKey, nullCount)
            );
        }

        [Theory]
        [InlineData("restrictions", true)]
        [InlineData("restrictions", false)]
        [InlineData("sessions", true)]
        [InlineData("sessions", false)]
        [InlineData("tokens", true)]
        [InlineData("tokens", false)]
        public void Restore_RelatedCollectionContainsDuplicateIds_ThrowsInvariant(
            string kind,
            bool sameInstance)
        {
            var state = RestorationRestrictedState();
            string rule;
            string countKey;
            string duplicateKey;
            Guid duplicateId;

            switch (kind)
            {
                case "restrictions":
                    var restriction = state.Restrictions.Single();
                    var duplicateRestriction = sameInstance ? restriction : RestorationRestriction(minutes: 20);
                    state.Restrictions = new[] { restriction, duplicateRestriction };
                    rule = RestorationRules.DuplicateRestrictions;
                    countKey = "RestrictionCount";
                    duplicateKey = "DuplicateRestrictionIds";
                    duplicateId = restriction.Id.Value;
                    break;

                case "sessions":
                    var session = state.Sessions.Single();
                    var duplicateSession = sameInstance
                        ? session
                        : RestorationSession(session.ActiveRestrictionIds, 10, type: RestrictionType.AllMute);
                    state.Sessions = new[] { session, duplicateSession };
                    rule = RestorationRules.DuplicateSessions;
                    countKey = "RestrictionSessionCount";
                    duplicateKey = "DuplicateRestrictionSessionIds";
                    duplicateId = session.Id.Value;
                    break;

                case "tokens":
                    var token = RestorationToken();
                    var duplicateToken = sameInstance ? token : RestorationToken(tokenValue: 2);
                    state.Tokens = new[] { token, duplicateToken };
                    rule = RestorationRules.DuplicateTokenIds;
                    countKey = "RefreshTokenCount";
                    duplicateKey = "DuplicateRefreshTokenIds";
                    duplicateId = token.Id.Value;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }

            var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

            RestorationAssertInvariant(
                exception,
                rule,
                ("UserId", state.UserId.Value),
                (countKey, 2),
                (duplicateKey, new[] { duplicateId })
            );
        }

        [Theory]
        [InlineData(RestrictionType.Ban)]
        [InlineData(RestrictionType.CommentMute)]
        [InlineData(RestrictionType.ChatMute)]
        [InlineData(RestrictionType.AllMute)]
        public void Restore_MultipleRestrictionSessionsOfSameType_ThrowsInvariant(RestrictionType type)
        {
            var state = RestorationRestrictedState(type);
            var first = state.Sessions.Single();
            state.Sessions = new[]
            {
                first,
                RestorationSession(first.ActiveRestrictionIds, 10, 2, type)
            };

            var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

            RestorationAssertInvariant(
                exception,
                RestorationRules.DuplicateSessionTypes,
                ("UserId", state.UserId.Value),
                ("DuplicateRestrictionTypes", new[] { type }),
                ("RestrictionSessionCount", 2)
            );
        }

        [Theory]
        [InlineData(RestrictionStatus.Completed)]
        [InlineData(RestrictionStatus.Revoked)]
        public void Restore_NonActiveRestriction_ThrowsInvariant(RestrictionStatus status)
        {
            var state = RestorationRestrictedState();
            var restriction = RestorationRestriction(status: status);
            state.Restrictions = new[] { restriction };

            var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

            RestorationAssertInvariant(
                exception,
                RestorationRules.ActiveRestrictions,
                ("UserId", state.UserId.Value),
                ("RestrictionId", restriction.Id.Value),
                ("RestrictionType", restriction.RestrictionType),
                ("ExpectedRestrictionStatus", RestrictionStatus.Active),
                ("ActualRestrictionStatus", status)
            );
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Restore_DuplicateRefreshTokenValues_ThrowsInvariant(bool includeUnrelatedToken)
        {
            var state = new RestorationState { UpdatedAt = RestorationTime };
            var first = RestorationToken();
            var second = RestorationToken(2, 1);
            var tokens = new List<UserRefreshToken> { first, second };

            if (includeUnrelatedToken)
            {
                tokens.Add(RestorationToken(3, 3));
            }

            state.Tokens = tokens;

            Assert.NotSame(
                first.RefreshToken,
                second.RefreshToken
            );

            Assert.Equal(
                first.RefreshToken.Value,
                second.RefreshToken.Value
            );

            var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

            RestorationAssertInvariant(
                exception,
                RestorationRules.DuplicateTokenValues,
                ("UserId", state.UserId.Value),
                ("RefreshTokenCount", tokens.Count),
                ("AffectedRefreshTokenIds", new[] { first.Id.Value, second.Id.Value })
            );
        }

        [Theory]
        [InlineData(TokenStatus.Expired)]
        [InlineData(TokenStatus.Consumed)]
        [InlineData(TokenStatus.Revoked)]
        public void Restore_NonActiveRefreshToken_ThrowsInvariant(TokenStatus status)
        {
            var token = RestorationToken(status: status);
            var state = new RestorationState
            {
                Tokens = new[] { token },
                UpdatedAt = RestorationTime.AddMinutes(1)
            };

            var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

            RestorationAssertInvariant(
                exception,
                RestorationRules.ActiveTokens,
                ("UserId", state.UserId.Value),
                ("RefreshTokenId", token.Id.Value),
                ("RefreshTokenStatus", status)
            );
        }

        [Theory]
        [InlineData(ActionStatus.Confirmed)]
        [InlineData(ActionStatus.Cancelled)]
        [InlineData(ActionStatus.Expired)]
        public void Restore_NonActivePendingAction_ThrowsInvariant(ActionStatus status)
        {
            var action = RestorationAction(status: status);
            var state = new RestorationState
            {
                Action = action,
                UpdatedAt = RestorationTime.AddHours(1)
            };

            var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

            RestorationAssertInvariant(
                exception,
                RestorationRules.ActiveAction,
                ("UserId", state.UserId.Value),
                ("PendingActionId", action.Id.Value),
                ("PendingActionStatus", status)
            );
        }

        [Fact]
        public void Restore_RestoredDeletion_ThrowsInvariant()
        {
            var deletion = RestorationDeletion(status: DeletionStatus.Restored);
            var state = new RestorationState
            {
                Deletion = deletion,
                UpdatedAt = deletion.UpdatedAt
            };

            var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

            RestorationAssertInvariant(
                exception,
                RestorationRules.RestoredDeletion,
                ("UserId", state.UserId.Value),
                ("DeletionId", deletion.Id.Value),
                ("DeletionStatus", DeletionStatus.Restored)
            );
        }

        [Theory]
        [InlineData(DeletionStatus.Deleted)]
        [InlineData(DeletionStatus.PermanentlyDeleted)]
        public void Restore_AllowedDeletionStatus_Succeeds(DeletionStatus status)
        {
            var deletion = RestorationDeletion(status: status);
            var state = new RestorationState
            {
                Deletion = deletion,
                UpdatedAt = deletion.UpdatedAt ?? deletion.CreatedAt
            };

            var user = state.Restore();

            RestorationAssertState(
                state,
                user
            );

            Assert.True(user.IsDeleted);

            Assert.Equal(
                status,
                deletion.Status
            );
        }
        #endregion

        #region Узгодженість обмежень із сесіями
        [Theory]
        [InlineData("missingSession")]
        [InlineData("extraSession")]
        [InlineData("differentTypes")]
        public void Restore_RestrictionTypesDoNotMatchSessionTypes_ThrowsInvariant(string scenario)
        {
            var state = RestorationRestrictedState();
            RestrictionType[] restrictionTypes;
            RestrictionType[] sessionTypes;
            RestrictionType[] withoutSession;
            RestrictionType[] withoutRestrictions;

            switch (scenario)
            {
                case "missingSession":
                    state.Sessions = Array.Empty<UserRestrictionSession>();
                    restrictionTypes = new[] { RestrictionType.ChatMute };
                    sessionTypes = Array.Empty<RestrictionType>();
                    withoutSession = new[] { RestrictionType.ChatMute };
                    withoutRestrictions = Array.Empty<RestrictionType>();
                    break;

                case "extraSession":
                    state.Restrictions = Array.Empty<UserRestriction>();
                    restrictionTypes = Array.Empty<RestrictionType>();
                    sessionTypes = new[] { RestrictionType.ChatMute };
                    withoutSession = Array.Empty<RestrictionType>();
                    withoutRestrictions = new[] { RestrictionType.ChatMute };
                    break;

                case "differentTypes":
                    state.Sessions = new[]
                    {
                        RestorationSession(
                            new[] { RestorationRestrictionId(1) },
                            10,
                            type: RestrictionType.AllMute
                        )
                    };
                    restrictionTypes = new[] { RestrictionType.ChatMute };
                    sessionTypes = new[] { RestrictionType.AllMute };
                    withoutSession = new[] { RestrictionType.ChatMute };
                    withoutRestrictions = new[] { RestrictionType.AllMute };
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(scenario));
            }

            var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

            RestorationAssertInvariant(
                exception,
                RestorationRules.TypesMismatch,
                ("RestrictionTypes", restrictionTypes),
                ("SessionTypes", sessionTypes),
                ("RestrictionTypesWithoutSession", withoutSession),
                ("SessionTypesWithoutRestrictions", withoutRestrictions)
            );
        }

        [Theory]
        [InlineData("missing")]
        [InlineData("extra")]
        [InlineData("different")]
        public void Restore_SessionRestrictionIdsDoNotMatch_ThrowsInvariant(string scenario)
        {
            var state = RestorationRestrictedState(durations: new[] { 10, 20 });
            var numbers = scenario switch
            {
                "missing" => new[] { 1 },
                "extra" => new[] { 1, 2, 3 },
                "different" => new[] { 3, 4 },
                _ => throw new ArgumentOutOfRangeException(nameof(scenario))
            };
            var session = RestorationSession(numbers.Select(RestorationRestrictionId), 30);
            state.Sessions = new[] { session };

            var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

            RestorationAssertInvariant(
                exception,
                RestorationRules.IdsMismatch,
                ("SessionId", session.Id.Value),
                ("RestrictionType", RestrictionType.ChatMute),
                ("AggregateRestrictionIds", new[] { RestorationGuid(2, 1), RestorationGuid(2, 2) }),
                ("SessionRestrictionIds", numbers.Select(number => RestorationGuid(2, number)).ToArray())
            );
        }

        [Theory]
        [InlineData(29)]
        [InlineData(31)]
        public void Restore_SessionDurationDoesNotMatchRestrictions_ThrowsInvariant(int actualMinutes)
        {
            var state = RestorationRestrictedState(durations: new[] { 10, 20 });
            var session = RestorationSession(state.Restrictions.Select(item => item.Id), actualMinutes);
            state.Sessions = new[] { session };

            var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

            RestorationAssertInvariant(
                exception,
                RestorationRules.DurationMismatch,
                ("SessionId", session.Id.Value),
                ("RestrictionType", RestrictionType.ChatMute),
                ("ExpectedTotalBlockedMinutes", 30),
                ("ActualTotalBlockedMinutes", actualMinutes),
                ("RestrictionCount", 2)
            );
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Restore_RestrictionCreatedBeforeSession_ThrowsInvariant(bool twoInvalid)
        {
            var sessionTime = RestorationTime.AddMinutes(2);
            var firstTime = sessionTime.AddTicks(-1);
            var secondTime = twoInvalid ? sessionTime.AddTicks(-2) : sessionTime;
            var state = RestorationRestrictedState(
                durations: new[] { 10, 20 },
                sessionCreatedAt: sessionTime
            );
            var first = RestorationRestriction(1, 10, createdAt: firstTime);
            var second = RestorationRestriction(2, 20, createdAt: secondTime);
            state.Restrictions = new[] { first, second };
            var session = state.Sessions.Single();

            var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

            RestorationAssertInvariant(
                exception,
                RestorationRules.BeforeSession,
                ("SessionId", session.Id.Value),
                ("RestrictionType", RestrictionType.ChatMute),
                ("SessionCreatedAt", sessionTime),
                ("InvalidRestrictionIds", twoInvalid
                    ? new[] { first.Id.Value, second.Id.Value }
                    : new[] { first.Id.Value }),
                ("InvalidRestrictionCreatedAtValues", twoInvalid
                    ? new[] { firstTime, secondTime }
                    : new[] { firstTime })
            );
        }

        [Theory]
        [InlineData(0L, false)]
        [InlineData(1L, false)]
        [InlineData(0L, true)]
        public void Restore_RestrictionCreatedAtOrAfterSessionExpiration_ThrowsInvariant(long ticks, bool twoInvalid)
        {
            var state = RestorationRestrictedState(durations: new[] { 10, 20 });
            var session = state.Sessions.Single();
            var expiration = RestorationTime.AddMinutes(30);
            var firstTime = twoInvalid ? expiration : RestorationTime;
            var secondTime = expiration.AddTicks(ticks + (twoInvalid ? 1 : 0));
            var first = RestorationRestriction(1, 10, createdAt: firstTime);
            var second = RestorationRestriction(2, 20, createdAt: secondTime);
            state.Restrictions = new[] { first, second };
            state.UpdatedAt = secondTime;

            var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

            RestorationAssertInvariant(
                exception,
                RestorationRules.AtOrAfterSessionEnd,
                ("SessionId", session.Id.Value),
                ("RestrictionType", RestrictionType.ChatMute),
                ("SessionCreatedAt", session.CreatedAt),
                ("SessionExpiresAt", expiration),
                ("InvalidRestrictionIds", twoInvalid
                    ? new[] { first.Id.Value, second.Id.Value }
                    : new[] { second.Id.Value }),
                ("InvalidRestrictionCreatedAtValues", twoInvalid
                    ? new[] { firstTime, secondTime }
                    : new[] { secondTime })
            );
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Restore_RestrictionCreatedWithinSessionLifetime_Succeeds(bool atStart)
        {
            var restrictionTime = atStart ? RestorationTime : RestorationTime.AddMinutes(10).AddTicks(-1);
            var state = RestorationRestrictedState(restrictionCreatedAt: restrictionTime);

            var user = state.Restore();

            RestorationAssertState(
                state,
                user
            );
        }

        [Fact]
        public void Restore_MultipleRestrictionGroups_Succeeds()
        {
            var state = new RestorationState { UpdatedAt = RestorationTime };
            var restrictions = new List<UserRestriction>();
            var sessions = new List<UserRestrictionSession>();
            var types = new[]
            {
                RestrictionType.Ban,
                RestrictionType.CommentMute,
                RestrictionType.ChatMute,
                RestrictionType.AllMute
            };

            for (var i = 0; i < types.Length; i++)
            {
                var first = RestorationRestriction(i * 10 + 1, 10, types[i]);
                var second = RestorationRestriction(i * 10 + 2, 20, types[i]);
                restrictions.Add(first);
                restrictions.Add(second);
                sessions.Add(RestorationSession(new[] { second.Id, first.Id }, 30, i + 1, types[i]));
            }

            state.Restrictions = restrictions.AsEnumerable().Reverse().ToArray();
            state.Sessions = sessions.ToArray();

            var user = state.Restore();

            RestorationAssertState(
                state,
                user
            );

            Assert.Equal(
                8,
                user.Restrictions.Count
            );

            Assert.Equal(
                4,
                user.RestrictionSessions.Count
            );
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Restore_RestrictionDurationSumAtInt32Limit_Succeeds(bool split)
        {
            var durations = split ? new[] { int.MaxValue - 1, 1 } : new[] { int.MaxValue };
            var state = RestorationRestrictedState(durations: durations);

            var user = state.Restore();

            RestorationAssertState(
                state,
                user
            );

            Assert.Equal(
                int.MaxValue,
                user.RestrictionSessions.Single().TotalBlockedMinutes
            );
        }

        [Fact]
        public void Restore_RestrictionDurationSumOverflow_ThrowsOverflowException()
        {
            var restrictions = new[]
            {
                RestorationRestriction(1, int.MaxValue),
                RestorationRestriction(2, 1)
            };
            var state = new RestorationState
            {
                Restrictions = restrictions,
                Sessions = new[]
                {
                    RestorationSession(restrictions.Select(item => item.Id), int.MaxValue)
                },
                UpdatedAt = RestorationTime
            };

            Assert.Throws<OverflowException>(() => state.Restore());
        }

        [Fact]
        public void Restore_SessionExpirationOverflow_ThrowsArgumentOutOfRangeException()
        {
            var state = RestorationRestrictedState(sessionCreatedAt: DateTimeOffset.MaxValue);

            Assert.Throws<ArgumentOutOfRangeException>(() => state.Restore());
        }
        #endregion

        #region Ban і час створення refresh-токенів
        [Fact]
        public void Restore_BanSessionWithoutRefreshTokens_Succeeds()
        {
            var state = RestorationRestrictedState(RestrictionType.Ban);

            var user = state.Restore();

            RestorationAssertState(
                state,
                user
            );

            Assert.Empty(user.RefreshTokens);
        }

        [Theory]
        [InlineData(RestrictionType.CommentMute)]
        [InlineData(RestrictionType.ChatMute)]
        [InlineData(RestrictionType.AllMute)]
        public void Restore_MuteSessionWithRefreshTokens_Succeeds(RestrictionType type)
        {
            var state = RestorationRestrictedState(type);
            var token = RestorationToken();
            state.Tokens = new[] { token };
            Assert.True(token.CreatedAt < state.Sessions.Single().GetExpirationDate());

            var user = state.Restore();

            RestorationAssertState(
                state,
                user
            );
        }

        [Theory]
        [InlineData(-1L)]
        [InlineData(0L)]
        [InlineData(TimeSpan.TicksPerMinute)]
        [InlineData(10L * TimeSpan.TicksPerMinute - 1)]
        public void Restore_RefreshTokenCreatedBeforeBanExpiration_ThrowsInvariant(long ticksFromBanStart)
        {
            var banStart = RestorationTime.AddMinutes(10);
            var state = RestorationRestrictedState(
                RestrictionType.Ban,
                sessionCreatedAt: banStart
            );
            var session = state.Sessions.Single();
            var tokenTime = banStart.AddTicks(ticksFromBanStart);
            var token = RestorationToken(createdAt: tokenTime);
            state.Tokens = new[] { token };
            state.UpdatedAt = tokenTime >= banStart ? tokenTime : banStart;

            var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

            RestorationAssertInvariant(
                exception,
                RestorationRules.TokenBeforeBanEnd,
                ("UserId", state.UserId.Value),
                ("BanSessionId", session.Id.Value),
                ("BanSessionCreatedAt", banStart),
                ("BanSessionExpiresAt", banStart.AddMinutes(10)),
                ("RefreshTokenId", token.Id.Value),
                ("RefreshTokenCreatedAt", tokenTime)
            );
        }

        [Theory]
        [InlineData(0L)]
        [InlineData(1L)]
        public void Restore_RefreshTokenCreatedAtOrAfterBanExpiration_Succeeds(long ticksAfterBanEnd)
        {
            var state = RestorationRestrictedState(RestrictionType.Ban);
            var tokenTime = RestorationTime.AddMinutes(10).AddTicks(ticksAfterBanEnd);
            state.Tokens = new[] { RestorationToken(createdAt: tokenTime) };
            state.UpdatedAt = tokenTime;

            var user = state.Restore();

            RestorationAssertState(state, user);
        }

        [Fact]
        public void Restore_ValidTokenFollowedByTokenCreatedBeforeBanExpiration_ThrowsInvariant()
        {
            var state = RestorationRestrictedState(RestrictionType.Ban);
            var session = state.Sessions.Single();
            var expiration = RestorationTime.AddMinutes(10);
            var valid = RestorationToken(createdAt: expiration);
            var invalid = RestorationToken(
                2,
                2,
                createdAt: expiration.AddTicks(-1)
            );
            state.Tokens = new[] { valid, invalid };
            state.UpdatedAt = expiration;

            var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

            RestorationAssertInvariant(
                exception,
                RestorationRules.TokenBeforeBanEnd,
                ("UserId", state.UserId.Value),
                ("BanSessionId", session.Id.Value),
                ("BanSessionCreatedAt", session.CreatedAt),
                ("BanSessionExpiresAt", expiration),
                ("RefreshTokenId", invalid.Id.Value),
                ("RefreshTokenCreatedAt", invalid.CreatedAt)
            );
        }
        #endregion

        #region UpdatedAt і пов'язаний стан
        [Theory]
        [InlineData("attempts1")]
        [InlineData("attempts2")]
        [InlineData("restriction")]
        [InlineData("token")]
        [InlineData("action")]
        [InlineData("deletion")]
        public void Restore_RelatedStateWithoutUpdatedAt_ThrowsInvariant(string kind)
        {
            var state = new RestorationState();

            if (kind == "attempts1" || kind == "attempts2")
            {
                state.FailedPasswordAttempts = kind == "attempts1" ? 1 : 2;
            }
            else
            {
                RestorationAttachEntity(
                    state,
                    kind
                );
            }

            state.UpdatedAt = null;

            var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

            RestorationAssertInvariant(
                exception,
                RestorationRules.UpdatedRequired,
                ("UserId", state.UserId.Value),
                ("UserUpdatedAt", null),
                ("FailedPasswordAttempts", state.FailedPasswordAttempts),
                ("RestrictionCount", state.Restrictions.Count()),
                ("RestrictionSessionCount", state.Sessions.Count()),
                ("RefreshTokenCount", state.Tokens.Count()),
                ("PendingActionId", state.Action?.Id.Value),
                ("DeletionId", state.Deletion?.Id.Value)
            );
        }

        [Theory]
        [InlineData("restriction")]
        [InlineData("token")]
        [InlineData("action")]
        [InlineData("deletion")]
        public void Restore_UpdatedAtBeforeRelatedEntityCreation_ThrowsDataInconsistency(string kind)
        {
            var state = new RestorationState();
            var childTime = RestorationTime.AddMinutes(1);
            RestorationAttachEntity(state, kind, createdAt: childTime);
            state.UpdatedAt = RestorationTime;

            var exception = Assert.Throws<DomainDataInconsistencyException>(() => state.Restore());

            RestorationAssertDateRange(
                exception,
                "aggregateUpdatedAt",
                "CreatedAt",
                childTime
            );
        }

        [Fact]
        public void Restore_UpdatedAtBeforeDeletionUpdatedAt_ThrowsDataInconsistency()
        {
            var deletion = RestorationDeletion(status: DeletionStatus.PermanentlyDeleted);
            var deletionUpdatedAt = deletion.UpdatedAt!.Value;
            var state = new RestorationState
            {
                Deletion = deletion,
                UpdatedAt = deletionUpdatedAt.AddTicks(-1)
            };

            var exception = Assert.Throws<DomainDataInconsistencyException>(() => state.Restore());

            RestorationAssertDateRange(
                exception,
                "aggregateUpdatedAt",
                "UpdatedAt",
                deletionUpdatedAt
            );
        }

        [Theory]
        [InlineData("restriction", 0L)]
        [InlineData("restriction", 1L)]
        [InlineData("token", 0L)]
        [InlineData("token", 1L)]
        [InlineData("action", 0L)]
        [InlineData("action", 1L)]
        [InlineData("deletion", 0L)]
        [InlineData("deletion", 1L)]
        [InlineData("deletionUpdated", 0L)]
        [InlineData("deletionUpdated", 1L)]
        public void Restore_UpdatedAtAtOrAfterLatestRelatedTimestamp_Succeeds(
            string kind,
            long ticks)
        {
            var state = new RestorationState();
            var childTime = RestorationTime.AddMinutes(1);
            DateTimeOffset latest;

            if (kind == "deletionUpdated")
            {
                var deletion = RestorationDeletion(
                    createdAt: childTime,
                    status: DeletionStatus.PermanentlyDeleted
                );
                state.Deletion = deletion;
                latest = deletion.UpdatedAt!.Value;
            }
            else
            {
                RestorationAttachEntity(
                    state,
                    kind,
                    createdAt: childTime
                );
                latest = childTime;
            }

            state.UpdatedAt = latest.AddTicks(ticks);

            var user = state.Restore();

            RestorationAssertState(
                state,
                user
            );
        }
        #endregion

        #region Пріоритети перевірок на конфліктних даних
        [Theory]
        [InlineData("userId", "login")]
        [InlineData("login", "password")]
        [InlineData("password", "restrictions")]
        [InlineData("restrictions", "restrictionSessions")]
        [InlineData("restrictionSessions", "refreshTokens")]
        public void Restore_RequiredArgumentValidationOrder_ReportsFirstNull(string first, string second)
        {
            var state = new RestorationState();

            var exception = Assert.Throws<DomainDataInconsistencyException>(() =>
                RestorationRestoreWithNulls(
                    state,
                    first,
                    second
                )
            );

            RestorationAssertEmpty(
                exception,
                first
            );
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Restore_CoreValidationOrder_ReportsFirstViolation(bool undefinedRole)
        {
            var state = new RestorationState
            {
                Role = undefinedRole ? (Role)999 : Role.User,
                FailedPasswordAttempts = 3,
                UpdatedAt = RestorationTime.AddTicks(-1)
            };

            var exception = Assert.Throws<DomainDataInconsistencyException>(() => state.Restore());

            if (undefinedRole)
            {
                RestorationAssertUnsupported(
                    exception,
                    (Role)999
                );
            }
            else
            {
                RestorationAssertRange(
                    exception,
                    "failedPasswordAttempts",
                    "Int32",
                    ("FailedPasswordAttempts", 3),
                    ("MaximumFailedPasswordAttempts", 3)
                );
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Restore_UserTimestampValidationOrder_ReportsFirstViolation(bool checkUpdatedFirst)
        {
            var state = new RestorationState
            {
                UpdatedAt = checkUpdatedFirst ? RestorationTime.AddTicks(-1) : null,
                LastLoginAt = RestorationTime.AddTicks(-2)
            };

            var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

            RestorationAssertInvariant(
                exception,
                checkUpdatedFirst ? RestorationRules.UpdatedBeforeCreated : RestorationRules.LoginBeforeCreated
            );
        }

        [Theory]
        [InlineData("restrictions", "nullBeforeDuplicate")]
        [InlineData("restrictions", "duplicateBeforeOwner")]
        [InlineData("sessions", "nullBeforeDuplicate")]
        [InlineData("sessions", "duplicateBeforeOwner")]
        [InlineData("sessions", "duplicateIdsBeforeTypes")]
        [InlineData("sessions", "duplicateTypesBeforeOwner")]
        [InlineData("tokens", "nullBeforeDuplicate")]
        [InlineData("tokens", "duplicateBeforeOwner")]
        [InlineData("tokens", "duplicateIdsBeforeValues")]
        [InlineData("tokens", "duplicateValuesBeforeOwner")]
        public void Restore_CollectionValidationOrder_ReportsFirstViolation(string kind, string scenario)
        {
            var state = RestorationRestrictedState();
            var withNull = scenario == "nullBeforeDuplicate";
            var owner = scenario.EndsWith("Owner", StringComparison.Ordinal)
                ? RestorationUserId(2)
                : state.UserId;
            string rule;

            switch (kind)
            {
                case "restrictions":
                    var firstRestriction = state.Restrictions.Single();
                    var secondRestriction = RestorationRestriction(owner: owner);
                    state.Restrictions = withNull
                        ? new[] { firstRestriction, secondRestriction, null! }
                        : new[] { firstRestriction, secondRestriction };
                    rule = withNull ? RestorationRules.NullRestrictions : RestorationRules.DuplicateRestrictions;
                    break;

                case "sessions":
                    var firstSession = state.Sessions.Single();
                    var duplicateType = scenario == "duplicateTypesBeforeOwner";

                    var secondType = scenario == "duplicateBeforeOwner"
                        ? RestrictionType.AllMute
                        : RestrictionType.ChatMute;

                    var secondSession = RestorationSession(
                        firstSession.ActiveRestrictionIds,
                        10,
                        duplicateType ? 2 : 1,
                        secondType,
                        owner
                    );
                    state.Sessions = withNull
                        ? new[] { firstSession, secondSession, null! }
                        : new[] { firstSession, secondSession };

                    rule = withNull
                        ? RestorationRules.NullSessions
                        : duplicateType ? RestorationRules.DuplicateSessionTypes : RestorationRules.DuplicateSessions;
                    break;

                case "tokens":
                    var firstToken = RestorationToken();
                    var duplicateValue = scenario == "duplicateValuesBeforeOwner";

                    var secondToken = RestorationToken(
                        duplicateValue ? 2 : 1,
                        scenario == "duplicateBeforeOwner" ? (byte)2 : (byte)1,
                        owner
                    );

                    state.Tokens = withNull
                        ? new[] { firstToken, secondToken, null! }
                        : new[] { firstToken, secondToken };
                    rule = withNull
                        ? RestorationRules.NullTokens
                        : duplicateValue ? RestorationRules.DuplicateTokenValues : RestorationRules.DuplicateTokenIds;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }

            var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

            RestorationAssertInvariant(
                exception,
                rule
            );
        }

        [Theory]
        [InlineData("restriction", "ownerBeforeStatus")]
        [InlineData("restriction", "statusBeforeTime")]
        [InlineData("session", "ownerBeforeTime")]
        [InlineData("token", "ownerBeforeStatus")]
        [InlineData("token", "statusBeforeTime")]
        [InlineData("action", "ownerBeforeStatus")]
        [InlineData("action", "statusBeforeTime")]
        [InlineData("deletion", "ownerBeforeTime")]
        [InlineData("deletion", "timeBeforeStatus")]
        public void Restore_RelatedEntityValidationOrder_ReportsFirstViolation(string kind, string scenario)
        {
            var state = new RestorationState { UpdatedAt = RestorationTime.AddHours(2) };

            var owner = scenario.StartsWith("owner", StringComparison.Ordinal)
                ? RestorationUserId(2)
                : state.UserId;

            var beforeUser = scenario.EndsWith("Time", StringComparison.Ordinal) || scenario == "timeBeforeStatus";
            var created = beforeUser ? RestorationTime.AddTicks(-1) : RestorationTime;
            string rule;

            switch (kind)
            {
                case "restriction":
                    var restriction = RestorationRestriction(
                        owner: owner,
                        createdAt: created,
                        status: RestrictionStatus.Completed
                    );
                    state.Restrictions = new[] { restriction };
                    state.Sessions = new[] { RestorationSession(new[] { restriction.Id }, 10) };
                    rule = scenario == "ownerBeforeStatus"
                        ? RestorationOwnerRule(kind)
                        : RestorationRules.ActiveRestrictions;
                    break;

                case "session":
                    var sessionRestriction = RestorationRestriction();
                    state.Restrictions = new[] { sessionRestriction };
                    state.Sessions = new[]
                    {
                        RestorationSession(new[] { sessionRestriction.Id }, 10, owner: owner, createdAt: created)
                    };
                    rule = RestorationOwnerRule(kind);
                    break;

                case "token":
                    state.Tokens = new[]
                    {
                        RestorationToken(
                            owner: owner,
                            createdAt: created,
                            status: TokenStatus.Expired
                        )
                    };
                    rule = scenario == "ownerBeforeStatus"
                        ? RestorationOwnerRule(kind)
                        : RestorationRules.ActiveTokens;
                    break;

                case "action":
                    state.Action = RestorationAction(owner, created, ActionStatus.Confirmed);
                    rule = scenario == "ownerBeforeStatus"
                        ? RestorationOwnerRule(kind)
                        : RestorationRules.ActiveAction;
                    break;

                case "deletion":
                    state.Deletion = RestorationDeletion(
                        owner,
                        created,
                        scenario == "timeBeforeStatus" ? DeletionStatus.Restored : DeletionStatus.Deleted
                    );
                    rule = RestorationOwnerRule(kind);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }

            if (scenario == "timeBeforeStatus")
            {
                var exception = Assert.Throws<DomainDataInconsistencyException>(() => state.Restore());

                RestorationAssertDateRange(
                    exception,
                    "CreatedAt",
                    "userCreatedAt",
                    state.CreatedAt
                );
            }
            else
            {
                var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

                RestorationAssertInvariant(
                    exception,
                    rule
                );
            }
        }

        [Theory]
        [InlineData("typesBeforeIds")]
        [InlineData("idsBeforeDuration")]
        [InlineData("durationBeforeStart")]
        [InlineData("startBeforeEnd")]
        public void Restore_RestrictionConsistencyValidationOrder_ReportsFirstViolation(string scenario)
        {
            var state = RestorationRestrictedState();
            string rule;

            switch (scenario)
            {
                case "typesBeforeIds":
                    state.Sessions = new[]
                    {
                        RestorationSession(new[] { RestorationRestrictionId(2) }, 20, type: RestrictionType.AllMute)
                    };
                    rule = RestorationRules.TypesMismatch;
                    break;

                case "idsBeforeDuration":
                    state.Sessions = new[] { RestorationSession(new[] { RestorationRestrictionId(2) }, 20) };
                    rule = RestorationRules.IdsMismatch;
                    break;

                case "durationBeforeStart":
                    state.Sessions = new[]
                    {
                        RestorationSession(
                            new[] { RestorationRestrictionId(1) },
                            20,
                            createdAt: RestorationTime.AddMinutes(1)
                        )
                    };
                    state.UpdatedAt = RestorationTime.AddMinutes(1);
                    rule = RestorationRules.DurationMismatch;
                    break;

                case "startBeforeEnd":
                    var start = RestorationTime.AddMinutes(1);
                    var end = start.AddMinutes(30);
                    var first = RestorationRestriction(1, 10, createdAt: RestorationTime);
                    var second = RestorationRestriction(2, 20, createdAt: end);
                    state.Restrictions = new[] { first, second };
                    state.Sessions = new[]
                    {
                        RestorationSession(new[] { first.Id, second.Id }, 30, createdAt: start)
                    };
                    state.UpdatedAt = end;
                    rule = RestorationRules.BeforeSession;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(scenario));
            }

            var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

            RestorationAssertInvariant(
                exception,
                rule
            );
        }

        [Theory]
        [InlineData("nullBeforeCore")]
        [InlineData("timeBeforeRestrictions")]
        [InlineData("restrictionsBeforeSessions")]
        [InlineData("sessionsBeforeConsistency")]
        [InlineData("consistencyBeforeTokens")]
        [InlineData("tokensBeforeAction")]
        [InlineData("actionBeforeDeletion")]
        [InlineData("deletionBeforeBan")]
        [InlineData("banBeforeUpdatedAt")]
        public void Restore_ValidationStages_ReportsFirstViolation(string scenario)
        {
            var state = new RestorationState { UpdatedAt = RestorationTime };
            string rule;

            switch (scenario)
            {
                case "nullBeforeCore":
                    state.Role = (Role)999;
                    var nullException = Assert.Throws<DomainDataInconsistencyException>(() =>
                        RestorationRestoreWithNulls(state, "userId"));
                    RestorationAssertEmpty(nullException, "userId");
                    return;

                case "timeBeforeRestrictions":
                    state.UpdatedAt = RestorationTime.AddTicks(-1);
                    state.Restrictions = new UserRestriction[] { null! };
                    rule = RestorationRules.UpdatedBeforeCreated;
                    break;

                case "restrictionsBeforeSessions":
                    state.Restrictions = new UserRestriction[] { null! };
                    state.Sessions = new UserRestrictionSession[] { null! };
                    rule = RestorationRules.NullRestrictions;
                    break;

                case "sessionsBeforeConsistency":
                    state = RestorationRestrictedState();
                    state.Sessions = new UserRestrictionSession[] { null! };
                    rule = RestorationRules.NullSessions;
                    break;

                case "consistencyBeforeTokens":
                    state = RestorationRestrictedState();
                    state.Sessions = Array.Empty<UserRestrictionSession>();
                    state.Tokens = new UserRefreshToken[] { null! };
                    rule = RestorationRules.TypesMismatch;
                    break;

                case "tokensBeforeAction":
                    state.Tokens = new UserRefreshToken[] { null! };
                    state.Action = RestorationAction(RestorationUserId(2));
                    rule = RestorationRules.NullTokens;
                    break;

                case "actionBeforeDeletion":
                    state.Action = RestorationAction(RestorationUserId(2));
                    state.Deletion = RestorationDeletion(status: DeletionStatus.Restored);
                    rule = RestorationOwnerRule("action");
                    break;

                case "deletionBeforeBan":
                    state = RestorationRestrictedState(RestrictionType.Ban);
                    state.Tokens = new[] { RestorationToken() };
                    state.Deletion = RestorationDeletion(status: DeletionStatus.Restored);
                    state.UpdatedAt = RestorationTime.AddHours(1);
                    rule = RestorationRules.RestoredDeletion;
                    break;

                case "banBeforeUpdatedAt":
                    state = RestorationRestrictedState(RestrictionType.Ban);
                    state.Tokens = new[] { RestorationToken() };
                    state.UpdatedAt = null;
                    rule = RestorationRules.TokenBeforeBanEnd;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(scenario));
            }

            var exception = Assert.Throws<DomainInvariantViolationException>(() => state.Restore());

            RestorationAssertInvariant(
                exception,
                rule
            );
        }
        #endregion
    }
}
