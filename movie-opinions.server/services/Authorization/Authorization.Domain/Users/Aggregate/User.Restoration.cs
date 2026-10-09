using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Users.Contracts;
using Authorization.Domain.Users.Entities.UsersDeletion;
using Authorization.Domain.Users.Entities.UsersPendingAction;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;
using Authorization.Domain.Users.Entities.UsersRefreshToken;
using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestriction.Enums;
using Authorization.Domain.Users.Entities.UsersRestrictionSession;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.Users.ValueObjects.PasswordUser;

namespace Authorization.Domain.Users
{
    public partial class User
    {
        private const int MAX_FAILED_ATTEMPTS = 3;

        #region Restoration
        /// <summary>
        /// Заповнює агрегат перевіреним станом зі сховища
        /// без створення доменних подій.
        ///
        /// (Populates the aggregate from validated stored state
        /// without recording domain events.)
        /// </summary>
        private User(
            UserId userId,
            DateTimeOffset createdAt,
            Login login,
            Password password,
            Role role,
            DateTimeOffset? updatedAt,
            DateTimeOffset? lastLoginAt,
            int failedPasswordAttempts,
            IEnumerable<UserRestriction> restrictions,
            IEnumerable<UserRestrictionSession> restrictionSessions,
            IEnumerable<UserRefreshToken> refreshTokens,
            UserPendingAction? pendingAction,
            UserDeletion? deletion)
            : base(userId, createdAt)
        {
            Login = login;
            Password = password;
            Role = role;
            UpdatedAt = updatedAt;
            LastLoginAt = lastLoginAt;
            FailedPasswordAttempts = failedPasswordAttempts;
            _restrictionSessions = restrictionSessions.ToList();
            _restrictions = restrictions.ToList();
            _refreshTokens = refreshTokens.ToList();
            _deletion = deletion;
            _action = pendingAction;
        }

        /// <summary>
        /// Перевіряє переданий стан і відновлює агрегат користувача
        /// зі збережених даних. Не створює доменних подій.
        ///
        /// (Validates the supplied state and restores the user aggregate
        /// from stored data. Does not record domain events.)
        /// </summary>
        /// <param name="userId">Збережений ідентифікатор користувача.</param>
        /// <param name="createdAt">Час створення користувача.</param>
        /// <param name="login">Збережений логін.</param>
        /// <param name="password">Збережений пароль у вигляді хешу.</param>
        /// <param name="role">Збережена роль.</param>
        /// <param name="updatedAt">Час останнього оновлення, якщо він є.</param>
        /// <param name="lastLoginAt">Час останнього входу, якщо він є.</param>
        /// <param name="failedPasswordAttempts">Кількість невдалих спроб введення пароля.</param>
        /// <param name="restrictions">Активні обмеження користувача.</param>
        /// <param name="restrictionSessions">Активні сесії обмежень.</param>
        /// <param name="refreshTokens">Активні токени оновлення.</param>
        /// <param name="pendingAction">Поточна незавершена дія, якщо вона є.</param>
        /// <param name="deletion">Дані про видалення, якщо вони є.</param>
        /// <returns>Відновлений агрегат користувача.</returns>
        public static User Restore(
            UserId userId,
            DateTimeOffset createdAt,
            Login login,
            Password password,
            Role role,
            DateTimeOffset? updatedAt,
            DateTimeOffset? lastLoginAt,
            int failedPasswordAttempts,
            IEnumerable<UserRestriction> restrictions,
            IEnumerable<UserRestrictionSession> restrictionSessions,
            IEnumerable<UserRefreshToken> refreshTokens,
            UserPendingAction? pendingAction,
            UserDeletion? deletion)
        {
            DomainGuard.AgainstNull<User>(
                OperationType.Restore,
                (userId, nameof(userId)),
                (login, nameof(login)),
                (password, nameof(password)),
                (restrictions, nameof(restrictions)),
                (restrictionSessions, nameof(restrictionSessions)),
                (refreshTokens, nameof(refreshTokens))
            );

            var restrictionData = restrictions.ToArray();
            var restrictionSessionData = restrictionSessions.ToArray();
            var refreshTokenData = refreshTokens.ToArray();

            ValidateCoreState(
                role,
                failedPasswordAttempts
            );

            ValidateUserTimestamps(
                createdAt,
                updatedAt,
                lastLoginAt
            );

            ValidateRestrictions(
                userId,
                createdAt,
                restrictionData
            );

            ValidateRestrictionSessions(
                userId,
                createdAt,
                restrictionSessionData
            );

            ValidateRestrictionConsistency(
                restrictionData,
                restrictionSessionData
            );

            ValidateRefreshTokens(
                userId,
                createdAt,
                refreshTokenData
            );

            ValidatePendingAction(
                userId,
                createdAt,
                pendingAction
            );

            ValidateDeletion(
                userId,
                createdAt,
                deletion
            );

            return new User(
                userId,
                createdAt,
                login,
                password,
                role,
                updatedAt,
                lastLoginAt,
                failedPasswordAttempts,
                restrictionData,
                restrictionSessionData,
                refreshTokenData,
                pendingAction,
                deletion
            );
        }
        #endregion

        #region Core State Validation
        /// <summary>
        /// Перевіряє базовий скалярний стан користувача,
        /// отриманий зі сховища.
        ///
        /// Переконується, що роль має підтримуване значення,
        /// а кількість невдалих спроб введення пароля перебуває
        /// в допустимому діапазоні.
        ///
        /// (Validates the user's core scalar state restored from storage.
        ///
        /// Ensures that the role has a supported value and that
        /// the failed password attempt count is within the allowed range.)
        /// </summary>
        /// <param name="role">Збережена роль користувача.</param>
        /// <param name="failedPasswordAttempts">Збережена кількість невдалих спроб введення пароля.</param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо роль не підтримується або кількість
        /// невдалих спроб виходить за допустимі межі.
        /// </exception>
        private static void ValidateCoreState(
            Role role,
            int failedPasswordAttempts)
        {
            DomainGuard.AgainstUndefinedEnum<User>(
                OperationType.Restore,
                (role, nameof(role))
            );

            if (failedPasswordAttempts is < 0 or >= MAX_FAILED_ATTEMPTS)
            {
                throw DomainDataInconsistencyException.ValueOutOfRange<User>(
                    nameof(failedPasswordAttempts),
                    failedPasswordAttempts,
                    OperationType.Restore,
                    context: new Dictionary<string, object>
                    {
                        ["FailedPasswordAttempts"] = failedPasswordAttempts,
                        ["MaximumFailedPasswordAttempts"] = MAX_FAILED_ATTEMPTS
                    }
                );
            }
        }

        /// <summary>
        /// Перевіряє узгодженість часових значень користувача.
        ///
        /// Час останнього оновлення та останнього входу,
        /// якщо вони задані, не можуть передувати часу
        /// створення користувача. Рівність часу дозволена.
        ///
        /// (Validates the consistency of the user's timestamps.
        ///
        /// The last update and last login timestamps, when present,
        /// cannot be earlier than the user's creation time.
        /// Equal timestamps are allowed.)
        /// </summary>
        /// <param name="createdAt">Час створення користувача.</param>
        /// <param name="updatedAt">Час останнього оновлення користувача, якщо він є.</param>
        /// <param name="lastLoginAt">Час останнього входу користувача, якщо він є.</param>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо одне з часових значень передує
        /// часу створення користувача.
        /// </exception>
        private static void ValidateUserTimestamps(
            DateTimeOffset createdAt,
            DateTimeOffset? updatedAt,
            DateTimeOffset? lastLoginAt)
        {
            if (updatedAt is not null && updatedAt < createdAt)
            {
                throw DomainInvariantViolationException.BrokenState<User>(
                    "UpdatedAt must not be earlier than CreatedAt.",
                    new Dictionary<string, object?>
                    {
                        ["UpdatedAt"] = updatedAt,
                        ["CreatedAt"] = createdAt
                    },
                    OperationType.Restore
                );
            }

            if (lastLoginAt is not null && lastLoginAt < createdAt)
            {
                throw DomainInvariantViolationException.BrokenState<User>(
                    "LastLoginAt must not be earlier than CreatedAt.",
                    new Dictionary<string, object?>
                    {
                        ["LastLoginAt"] = lastLoginAt,
                        ["CreatedAt"] = createdAt
                    },
                    OperationType.Restore
                );
            }
        }
        #endregion

        #region Restriction State Validation
        /// <summary>
        /// Перевіряє активні обмеження, передані для відновлення
        /// агрегату користувача.
        ///
        /// Колекція може бути порожньою, але не може містити
        /// null-елементи або повторювані ідентифікатори.
        /// Кожне обмеження повинно належати користувачу,
        /// мати активний статус і бути створеним не раніше
        /// самого користувача.
        ///
        /// (Validates the active restrictions supplied when restoring
        /// the user aggregate.
        ///
        /// The collection may be empty, but it cannot contain null elements
        /// or duplicate identifiers. Every restriction must belong to the user,
        /// have an active status, and must not have been created before the user.)
        /// </summary>
        /// <param name="userId">Ідентифікатор користувача, якому повинні належати обмеження.</param>
        /// <param name="userCreatedAt">Час створення користувача, що визначає нижню часову межу.</param>
        /// <param name="restrictions">Активні обмеження, отримані зі сховища.</param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо колекція містить null-елементи.
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо колекція містить null-елементи,
        /// повторювані ідентифікатори або обмеження не належить
        /// користувачу, не є активним чи було створене раніше за нього.
        /// </exception>
        private static void ValidateRestrictions(
            UserId userId,
            DateTimeOffset userCreatedAt,
            IReadOnlyCollection<UserRestriction> restrictions)
        {
            var nullRestrictionCount = restrictions.Count(x => x is null);

            if (nullRestrictionCount > 0)
            {
                throw DomainInvariantViolationException.BrokenState<User>(
                    "The active restriction collection contains null elements.",
                    new Dictionary<string, object?>
                    {
                        ["UserId"] = userId.Value,
                        ["RestrictionCount"] = restrictions.Count,
                        ["NullRestrictionCount"] = nullRestrictionCount
                    },
                    OperationType.Restore
                );
            }

            var duplicateRestrictionIds = restrictions
                .GroupBy(x => x.Id)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key.Value)
                .ToArray();

            if (duplicateRestrictionIds.Length > 0)
            {
                throw DomainInvariantViolationException.BrokenState<User>(
                    "The active restriction collection contains duplicate identifiers.",
                    new Dictionary<string, object?>
                    {
                        ["UserId"] = userId.Value,
                        ["RestrictionCount"] = restrictions.Count,
                        ["DuplicateRestrictionIds"] = duplicateRestrictionIds
                    },
                    OperationType.Restore
                );
            }

            foreach (var restriction in restrictions)
            {
                EnsureOwnedByUser(
                    userId,
                    restriction
                );

                if (restriction.Status != RestrictionStatus.Active)
                {
                    throw DomainInvariantViolationException.BrokenState<User>(
                        "Only active restrictions can be restored in the user's active restriction collection.",
                        new Dictionary<string, object?>
                        {
                            ["UserId"] = userId.Value,
                            ["RestrictionId"] = restriction.Id.Value,
                            ["RestrictionType"] = restriction.RestrictionType,
                            ["ExpectedRestrictionStatus"] = RestrictionStatus.Active,
                            ["ActualRestrictionStatus"] = restriction.Status
                        },
                        OperationType.Restore
                    );
                }

                DomainGuard.AgainstEarlierThan<User>(
                    OperationType.Restore,
                    (restriction.CreatedAt, nameof(restriction.CreatedAt)),
                    (userCreatedAt, nameof(userCreatedAt))
                );
            }
        }

        /// <summary>
        /// Перевіряє сесії обмежень, передані для відновлення
        /// агрегату користувача.
        ///
        /// Колекція може бути порожньою, але не може містити
        /// null-елементи, повторювані ідентифікатори або більше
        /// однієї сесії одного типу. Кожна сесія повинна належати
        /// користувачу та бути створена не раніше самого користувача.
        ///
        /// (Validates the restriction sessions supplied when restoring
        /// the user aggregate.
        ///
        /// The collection may be empty, but it cannot contain null elements,
        /// duplicate identifiers, or more than one session of the same type.
        /// Every session must belong to the user and must not have been
        /// created before the user.)
        /// </summary>
        /// <param name="userId">Ідентифікатор користувача, якому повинні належати сесії.</param>
        /// <param name="userCreatedAt">Час створення користувача, що визначає нижню часову межу.</param>
        /// <param name="restrictionSessions">Сесії обмежень, отримані зі сховища.</param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо колекція містить null-елементи.
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо колекція містить null-елементи,
        /// повторювані ідентифікатори або типи, чи сесія
        /// не належить користувачу або була створена раніше за нього.
        /// </exception>
        private static void ValidateRestrictionSessions(
            UserId userId,
            DateTimeOffset userCreatedAt,
            IReadOnlyCollection<UserRestrictionSession> restrictionSessions)
        {
            var nullSessionCount = restrictionSessions.Count(x => x is null);

            if (nullSessionCount > 0)
            {
                throw DomainInvariantViolationException.BrokenState<User>(
                    "The restriction session collection contains null elements.",
                    new Dictionary<string, object?>
                    {
                        ["UserId"] = userId.Value,
                        ["RestrictionSessionCount"] = restrictionSessions.Count,
                        ["NullRestrictionSessionCount"] = nullSessionCount
                    },
                    OperationType.Restore
                );
            }

            var duplicateSessionIds = restrictionSessions
                .GroupBy(x => x.Id)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key.Value)
                .ToArray();

            if (duplicateSessionIds.Length > 0)
            {
                throw DomainInvariantViolationException.BrokenState<User>(
                    "The restriction session collection contains duplicate identifiers.",
                    new Dictionary<string, object?>
                    {
                        ["UserId"] = userId.Value,
                        ["RestrictionSessionCount"] = restrictionSessions.Count,
                        ["DuplicateRestrictionSessionIds"] = duplicateSessionIds
                    },
                    OperationType.Restore
                );
            }

            var duplicateSessionTypes = restrictionSessions
                .GroupBy(x => x.RestrictionType)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToArray();

            if (duplicateSessionTypes.Length > 0)
            {
                throw DomainInvariantViolationException.BrokenState<User>(
                    "The user contains more than one restriction session of the same type.",
                    new Dictionary<string, object?>
                    {
                        ["UserId"] = userId.Value,
                        ["DuplicateRestrictionTypes"] = duplicateSessionTypes,
                        ["RestrictionSessionCount"] = restrictionSessions.Count
                    },
                    OperationType.Restore
                );
            }

            foreach (var session in restrictionSessions)
            {
                EnsureOwnedByUser(
                    userId,
                    session
                );

                DomainGuard.AgainstEarlierThan<User>(
                    OperationType.Restore,
                    (session.CreatedAt, nameof(session.CreatedAt)),
                    (userCreatedAt, nameof(userCreatedAt))
                );
            }
        }

        /// <summary>
        /// Перевіряє взаємну узгодженість активних обмежень
        /// і сесій обмежень під час відновлення користувача.
        ///
        /// Переконується, що набори типів обмежень і сесій точно збігаються,
        /// кожна сесія містить ідентифікатори всіх обмежень свого типу,
        /// її загальна тривалість відповідає сумі тривалостей обмежень,
        /// а жодне обмеження не було створене раніше за відповідну сесію.
        ///
        /// Порожні колекції є допустимими, якщо обмеження та сесії
        /// одночасно відсутні.
        ///
        /// (Validates consistency between active restrictions and restriction
        /// sessions while restoring the user.
        ///
        /// Ensures that restriction and session type sets match exactly,
        /// every session contains all restriction identifiers of its type,
        /// its total blocked duration equals the sum of restriction durations,
        /// and no restriction was created before its corresponding session.
        ///
        /// Empty collections are valid when both restrictions and sessions
        /// are absent.)
        /// </summary>
        /// <param name="restrictions">Активні обмеження, стан яких необхідно зіставити із сесіями.</param>
        /// <param name="restrictionSessions">Активні сесії, стан яких необхідно зіставити з обмеженнями.</param>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо типи, ідентифікатори, тривалість або час створення
        /// обмежень і сесій є неузгодженими.
        /// </exception>
        private static void ValidateRestrictionConsistency(
            IReadOnlyCollection<UserRestriction> restrictions,
            IReadOnlyCollection<UserRestrictionSession> restrictionSessions)
        {
            var restrictionsByType = restrictions
                .GroupBy(x => x.RestrictionType)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToArray()
                );

            var sessionsByType = restrictionSessions
                .ToDictionary(x => x.RestrictionType);

            var restrictionTypes = restrictionsByType.Keys.ToHashSet();
            var sessionTypes = sessionsByType.Keys.ToHashSet();

            if (!restrictionTypes.SetEquals(sessionTypes))
            {
                throw DomainInvariantViolationException.BrokenState<User>(
                    "The active restriction types do not exactly match " +
                    "the restriction session types.",
                    new Dictionary<string, object?>
                    {
                        ["RestrictionTypes"] = restrictionTypes.ToArray(),
                        ["SessionTypes"] = sessionTypes.ToArray(),
                        ["RestrictionTypesWithoutSession"] = restrictionTypes
                            .Except(sessionTypes)
                            .ToArray(),
                        ["SessionTypesWithoutRestrictions"] = sessionTypes
                            .Except(restrictionTypes)
                            .ToArray()
                    },
                    OperationType.Restore
                );
            }

            foreach (var (restrictionType, restrictionGroup) in restrictionsByType)
            {
                var session = sessionsByType[restrictionType];

                var restrictionIds = restrictionGroup
                    .Select(x => x.Id)
                    .ToArray();

                if (!session.ContainsExactly(restrictionIds))
                {
                    throw DomainInvariantViolationException.BrokenState<User>(
                        "The restriction session identifiers do not exactly match " +
                        "the active restrictions of its type.",
                        new Dictionary<string, object?>
                        {
                            ["SessionId"] = session.Id.Value,
                            ["RestrictionType"] = restrictionType,
                            ["AggregateRestrictionIds"] = restrictionIds
                                .Select(x => x.Value)
                                .ToArray(),
                            ["SessionRestrictionIds"] = session.ActiveRestrictionIds
                                .Select(x => x.Value)
                                .ToArray()
                        },
                        OperationType.Restore
                    );
                }

                var expectedTotalBlockedMinutes = restrictionGroup
                    .Sum(x => x.RestrictionRule.DurationMinutes);

                if (session.TotalBlockedMinutes != expectedTotalBlockedMinutes)
                {
                    throw DomainInvariantViolationException.BrokenState<User>(
                        "The restriction session total blocked duration does not match " +
                        "the duration of its active restrictions.",
                        new Dictionary<string, object?>
                        {
                            ["SessionId"] = session.Id.Value,
                            ["RestrictionType"] = restrictionType,
                            ["ExpectedTotalBlockedMinutes"] = expectedTotalBlockedMinutes,
                            ["ActualTotalBlockedMinutes"] = session.TotalBlockedMinutes,
                            ["RestrictionCount"] = restrictionGroup.Length
                        },
                        OperationType.Restore
                    );
                }

                var restrictionsCreatedBeforeSession = restrictionGroup
                    .Where(x => x.CreatedAt < session.CreatedAt)
                    .ToArray();

                if (restrictionsCreatedBeforeSession.Length > 0)
                {
                    throw DomainInvariantViolationException.BrokenState<User>(
                        "A restriction cannot be created before the session " +
                        "to which it belongs.",
                        new Dictionary<string, object?>
                        {
                            ["SessionId"] = session.Id.Value,
                            ["RestrictionType"] = restrictionType,
                            ["SessionCreatedAt"] = session.CreatedAt,
                            ["InvalidRestrictionIds"] = restrictionsCreatedBeforeSession
                                .Select(x => x.Id.Value)
                                .ToArray(),
                            ["InvalidRestrictionCreatedAtValues"] = restrictionsCreatedBeforeSession
                                .Select(x => x.CreatedAt)
                                .ToArray()
                        },
                        OperationType.Restore
                    );
                }
            }
        }
        #endregion

        #region Refresh Token Validation
        /// <summary>
        /// Перевіряє колекцію активних refresh-токенів
        /// під час відновлення користувача.
        ///
        /// Переконується, що колекція не містить null-елементів,
        /// ідентифікатори та значення токенів є унікальними,
        /// кожен токен належить відновлюваному користувачу,
        /// має активний статус і не був створений раніше за користувача.
        ///
        /// Порожня колекція є допустимою. Секретні значення токенів
        /// використовуються лише для перевірки унікальності
        /// та не додаються до контексту винятків.
        ///
        /// (Validates the active refresh-token collection while restoring
        /// the user.
        ///
        /// Ensures that the collection contains no null elements,
        /// token identifiers and values are unique, every token belongs
        /// to the restored user, has an active status, and was not created
        /// before the user.
        ///
        /// An empty collection is valid. Sensitive token values are used
        /// only for uniqueness validation and are not included
        /// in exception context.)
        /// </summary>
        /// <param name="userId">Ідентифікатор користувача, якому повинні належати всі токени.</param>
        /// <param name="userCreatedAt">
        /// Час створення користувача, відносно якого перевіряється час створення токенів.
        /// </param>
        /// <param name="refreshTokens">Активні refresh-токени, які необхідно перевірити.</param>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо колекція містить null-елементи, повторювані
        /// ідентифікатори або значення, чужий чи неактивний токен,
        /// або токен, створений раніше за користувача.
        /// </exception>
        private static void ValidateRefreshTokens(
            UserId userId,
            DateTimeOffset userCreatedAt,
            IReadOnlyCollection<UserRefreshToken> refreshTokens)
        {
            var nullRefreshTokens = refreshTokens.Count(x => x is null);

            if (nullRefreshTokens > 0)
            {
                throw DomainInvariantViolationException.BrokenState<User>(
                    "The refresh token collection contains null elements.",
                    new Dictionary<string, object?>
                    {
                        ["UserId"] = userId.Value,
                        ["RefreshTokenCount"] = refreshTokens.Count,
                        ["NullRefreshTokenCount"] = nullRefreshTokens
                    },
                    OperationType.Restore
                );
            }

            var duplicateRefreshTokenIds = refreshTokens
                .GroupBy(x => x.Id)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key.Value)
                .ToArray();

            if (duplicateRefreshTokenIds.Length > 0)
            {
                throw DomainInvariantViolationException.BrokenState<User>(
                    "The 'RefreshToken' collection contains duplicate identifiers.",
                    new Dictionary<string, object?>
                    {
                        ["UserId"] = userId.Value,
                        ["RefreshTokenCount"] = refreshTokens.Count,
                        ["DuplicateRefreshTokenIds"] = duplicateRefreshTokenIds
                    },
                    OperationType.Restore
                );
            }

            var duplicateRefreshTokenRecordIds = refreshTokens
                .GroupBy(x => x.RefreshToken)
                .Where(group => group.Skip(1).Any())
                .SelectMany(group => group.Select(x => x.Id.Value))
                .ToArray();

            if (duplicateRefreshTokenRecordIds.Length > 0)
            {
                throw DomainInvariantViolationException.BrokenState<User>(
                    "The refresh token collection contains duplicate token values.",
                    new Dictionary<string, object?>
                    {
                        ["UserId"] = userId.Value,
                        ["RefreshTokenCount"] = refreshTokens.Count,
                        ["AffectedRefreshTokenIds"] = duplicateRefreshTokenRecordIds
                    },
                    OperationType.Restore
                );
            }

            foreach (var refreshToken in refreshTokens)
            {
                EnsureOwnedByUser(
                    userId,
                    refreshToken
                );

                if (refreshToken.TokenStatus != TokenStatus.Active)
                {
                    throw DomainInvariantViolationException.BrokenState<User>(
                        "Only active refresh tokens can be restored as part of the user aggregate.",
                        new Dictionary<string, object?>
                        {
                            ["UserId"] = userId.Value,
                            ["RefreshTokenId"] = refreshToken.Id.Value,
                            ["RefreshTokenStatus"] = refreshToken.TokenStatus
                        },
                        OperationType.Restore
                    );
                }

                DomainGuard.AgainstEarlierThan<User>(
                    OperationType.Restore,
                    (refreshToken.CreatedAt, nameof(refreshToken.CreatedAt)),
                    (userCreatedAt, nameof(userCreatedAt))
                );
            }
        }
        #endregion

        #region Pending Action and Deletion Validation
        /// <summary>
        /// Перевіряє поточну незавершену дію користувача
        /// під час відновлення агрегату.
        ///
        /// Відсутність дії є допустимою. Якщо дію передано,
        /// вона повинна належати користувачу, мати активний статус
        /// і бути створена не раніше за самого користувача.
        ///
        /// (Validates the user's current pending action while restoring
        /// the aggregate.
        ///
        /// The absence of an action is valid. When supplied, the action
        /// must belong to the user, have an active status, and must not
        /// have been created before the user.)
        /// </summary>
        /// <param name="userId">Ідентифікатор користувача, якому повинна належати дія.</param>
        /// <param name="userCreatedAt">Час створення користувача, що визначає нижню часову межу.</param>
        /// <param name="pendingAction">Поточна незавершена дія або null, якщо вона відсутня.</param>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо дія належить іншому користувачу,
        /// не є активною або була створена раніше за користувача.
        /// </exception>
        private static void ValidatePendingAction(
            UserId userId,
            DateTimeOffset userCreatedAt,
            UserPendingAction? pendingAction)
        {
            if (pendingAction is not null)
            {
                EnsureOwnedByUser(
                    userId,
                    pendingAction
                );

                if (pendingAction.Status != ActionStatus.Active)
                {
                    throw DomainInvariantViolationException.BrokenState<User>(
                        "Only active pending action can be restored as part of the user aggregate.",
                        new Dictionary<string, object?>
                        {
                            ["UserId"] = userId.Value,
                            ["PendingActionId"] = pendingAction.Id.Value,
                            ["PendingActionStatus"] = pendingAction.Status
                        },
                        OperationType.Restore
                    );
                }

                DomainGuard.AgainstEarlierThan<User>(
                    OperationType.Restore,
                    (pendingAction.CreatedAt, nameof(pendingAction.CreatedAt)),
                    (userCreatedAt, nameof(userCreatedAt))
                );
            }
        }

        /// <summary>
        /// Перевіряє запис про виконане видалення користувача
        /// під час відновлення агрегату.
        ///
        /// Відсутність запису означає, що користувач не перебуває
        /// у видаленому стані. Якщо запис передано, він повинен
        /// належати користувачу та бути створений не раніше за нього.
        ///
        /// (Validates the completed user-deletion record while restoring
        /// the aggregate.
        ///
        /// The absence of a record means that the user is not in the
        /// deleted state. When supplied, the record must belong to the user
        /// and must not have been created before the user.)
        /// </summary>
        /// <param name="userId">Ідентифікатор користувача, якому повинен належати запис.</param>
        /// <param name="userCreatedAt">Час створення користувача, що визначає нижню часову межу.</param>
        /// <param name="userDeletion">Запис про виконане видалення або null, якщо користувача не видалено.</param>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо запис належить іншому користувачу
        /// або був створений раніше за користувача.
        /// </exception>
        private static void ValidateDeletion(
            UserId userId,
            DateTimeOffset userCreatedAt,
            UserDeletion? userDeletion)
        {
            if (userDeletion is not null)
            {
                EnsureOwnedByUser(
                    userId,
                    userDeletion
                );

                DomainGuard.AgainstEarlierThan<User>(
                    OperationType.Restore,
                    (userDeletion.CreatedAt, nameof(userDeletion.CreatedAt)),
                    (userCreatedAt, nameof(userCreatedAt))
                );
            }
        }
        #endregion

        #region Cross-State Validation
        // TODO : доробити метод , який перевіряє комплексно стан агрегату 
        private static void ValidateCrossState()
        {

        }
        #endregion

        #region Shared Validation Helpers
        /// <summary>
        /// Перевіряє, що доменний об’єкт належить користувачу,
        /// агрегат якого відновлюється.
        ///
        /// (Ensures that a domain object belongs to the user
        /// whose aggregate is being restored.)
        /// </summary>
        /// <typeparam name="TEntity">
        /// Тип доменного об’єкта, що визначає власника через
        /// <see cref="IUserOwned"/>.
        /// </typeparam>
        /// <param name="expectedUserId">Ідентифікатор очікуваного власника.</param>
        /// <param name="entity">Доменний об’єкт, власника якого необхідно перевірити.</param>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо об’єкт належить іншому користувачу.
        /// </exception>
        private static void EnsureOwnedByUser<TEntity>(
            UserId expectedUserId,
            TEntity entity)
            where TEntity : IUserOwned
        {
            if (entity.UserId == expectedUserId)
                return;

            throw DomainInvariantViolationException.BrokenState<User>(
                $"A '{typeof(TEntity).Name}' belonging to another user cannot " +
                "be restored as part of this aggregate.",
                new Dictionary<string, object?>
                {
                    ["EntityType"] = typeof(TEntity).Name,
                    ["ExpectedUserId"] = expectedUserId.Value,
                    ["ActualUserId"] = entity.UserId.Value
                },
                OperationType.Restore
            );
        }
        #endregion
    }
}
