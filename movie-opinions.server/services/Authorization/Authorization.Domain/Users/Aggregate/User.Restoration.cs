using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Common.ValueObjects.PasswordUser;
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

namespace Authorization.Domain.Users.Aggregate
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
        /// зі збережених даних.
        ///
        /// Перевіряє базові значення, часові межі, належність
        /// пов’язаних об’єктів користувачу та узгодженість
        /// обмежень із відповідними сесіями.
        ///
        /// Не створює доменних подій.
        ///
        /// (Validates the supplied state and restores the user aggregate
        /// from stored data.
        ///
        /// Checks core values, timestamp boundaries, ownership
        /// of related objects, and consistency between restrictions
        /// and their corresponding sessions.
        ///
        /// Does not record domain events.)
        /// </summary>
        /// <param name="userId">
        /// Збережений ідентифікатор користувача.
        ///
        /// (Stored user identifier.)
        /// </param>
        /// <param name="createdAt">
        /// Час створення користувача.
        ///
        /// (User creation time.)
        /// </param>
        /// <param name="login">
        /// Збережений логін користувача.
        ///
        /// (Stored user login.)
        /// </param>
        /// <param name="password">
        /// Збережений пароль у вигляді хешу.
        ///
        /// (Stored password represented by its hash.)
        /// </param>
        /// <param name="role">
        /// Збережена роль користувача.
        ///
        /// (Stored user role.)
        /// </param>
        /// <param name="updatedAt">
        /// Час останнього оновлення користувача, якщо він є.
        ///
        /// (Last user update time, if present.)
        /// </param>
        /// <param name="lastLoginAt">
        /// Час останнього входу користувача, якщо він є.
        ///
        /// (Last user login time, if present.)
        /// </param>
        /// <param name="failedPasswordAttempts">
        /// Збережена кількість невдалих спроб введення пароля.
        ///
        /// (Stored number of failed password attempts.)
        /// </param>
        /// <param name="restrictions">
        /// Активні обмеження користувача.
        ///
        /// (Active user restrictions.)
        /// </param>
        /// <param name="restrictionSessions">
        /// Сесії обмежень користувача.
        ///
        /// (User restriction sessions.)
        /// </param>
        /// <param name="refreshTokens">
        /// Refresh-токени користувача з активним статусом.
        ///
        /// (User refresh tokens with an active status.)
        /// </param>
        /// <param name="pendingAction">
        /// Поточна незавершена дія або null, якщо вона відсутня.
        ///
        /// (Current pending action, or null when no action is present.)
        /// </param>
        /// <param name="deletion">
        /// Запис про виконане видалення або null, якщо він відсутній.
        ///
        /// (Completed deletion record, or null when no record is present.)
        /// </param>
        /// <returns>
        /// Відновлений агрегат користувача.
        ///
        /// (Restored user aggregate.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо обов’язкове збережене значення відсутнє,
        /// роль не підтримується, кількість невдалих спроб виходить
        /// за допустимі межі або пов’язаний об’єкт був створений
        /// раніше за користувача.
        ///
        /// (Thrown when a required stored value is missing,
        /// the role is unsupported, the failed password attempt count
        /// is outside the allowed range, or a related object was created
        /// before the user.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо час останнього оновлення або входу передує
        /// створенню користувача чи пов’язані об’єкти порушують вимоги
        /// до складу колекцій, належності користувачу, статусів
        /// або взаємної узгодженості.
        ///
        /// (Thrown when the last update or login time precedes
        /// user creation, or related objects violate requirements
        /// for collection contents, ownership, statuses,
        /// or mutual consistency.)
        /// </exception>
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

            ValidateRefreshTokensAgainstBan(
                userId,
                restrictionSessionData,
                refreshTokenData
            );

            ValidateAggregateUpdateConsistency(
                userId,
                updatedAt,
                failedPasswordAttempts,
                restrictionData,
                restrictionSessionData,
                refreshTokenData,
                pendingAction,
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
        /// <param name="role">
        /// Збережена роль користувача.
        ///
        /// (Stored user role.)
        /// </param>
        /// <param name="failedPasswordAttempts">
        /// Збережена кількість невдалих спроб введення пароля.
        ///
        /// (Stored number of failed password attempts.)
        /// </param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо роль не підтримується або кількість
        /// невдалих спроб виходить за допустимі межі.
        ///
        /// (Thrown when the role is unsupported or the failed
        /// password attempt count is outside the allowed range.)
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
        /// Якщо час останнього входу заданий, час останнього оновлення
        /// також повинен бути заданий і не може передувати часу входу.
        ///
        /// (Validates the consistency of the user's timestamps.
        ///
        /// The last update and last login timestamps, when present,
        /// cannot be earlier than the user's creation time.
        /// Equal timestamps are allowed.
        ///
        /// When the last login timestamp is present, the last update
        /// timestamp must also be present and cannot precede the login.)
        /// </summary>
        /// <param name="createdAt">
        /// Час створення користувача.
        ///
        /// (User creation time.)
        /// </param>
        /// <param name="updatedAt">
        /// Час останнього оновлення користувача, якщо він є.
        ///
        /// (Last user update time, if present.)
        /// </param>
        /// <param name="lastLoginAt">
        /// Час останнього входу користувача, якщо він є.
        ///
        /// (Last user login time, if present.)
        /// </param>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо час оновлення або останнього входу передує
        /// створенню користувача, час оновлення відсутній за наявності
        /// часу останнього входу або передує цьому входу.
        ///
        /// (Thrown when the update or last sign-in time precedes
        /// user creation, or when the update time is missing despite
        /// a recorded last sign-in or precedes that sign-in.)
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

            if (lastLoginAt is not null && updatedAt is null)
            {
                throw DomainInvariantViolationException.BrokenState<User>(
                    "UpdatedAt must be set when LastLoginAt is set.",
                    new Dictionary<string, object?>
                    {
                        ["CreatedAt"] = createdAt,
                        ["UpdatedAt"] = updatedAt,
                        ["LastLoginAt"] = lastLoginAt
                    },
                    OperationType.Restore
                );
            }

            if (lastLoginAt is not null && updatedAt < lastLoginAt)
            {
                throw DomainInvariantViolationException.BrokenState<User>(
                    "UpdatedAt must not be earlier than LastLoginAt.",
                    new Dictionary<string, object?>
                    {
                        ["UpdatedAt"] = updatedAt,
                        ["LastLoginAt"] = lastLoginAt
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
        /// <param name="userId">
        /// Ідентифікатор користувача, якому повинні належати обмеження.
        ///
        /// (Identifier of the user who must own the restrictions.)
        /// </param>
        /// <param name="userCreatedAt">
        /// Час створення користувача, що визначає нижню часову межу.
        ///
        /// (User creation time defining the earliest allowed
        /// restriction creation time.)
        /// </param>
        /// <param name="restrictions">
        /// Активні обмеження, отримані зі сховища.
        ///
        /// (Active restrictions retrieved from storage.)
        /// </param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо обмеження було створене раніше за користувача.
        ///
        /// (Thrown when a restriction was created before the user.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо колекція містить null-елементи,
        /// повторювані ідентифікатори або обмеження
        /// не належить користувачу чи не має активного статусу.
        ///
        /// (Thrown when the collection contains null elements
        /// or duplicate identifiers, or a restriction belongs
        /// to another user or does not have an active status.)
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
        /// <param name="userId">
        /// Ідентифікатор користувача, якому повинні належати сесії.
        ///
        /// (Identifier of the user who must own the sessions.)
        /// </param>
        /// <param name="userCreatedAt">
        /// Час створення користувача, що визначає нижню часову межу.
        ///
        /// (User creation time defining the earliest allowed
        /// session creation time.)
        /// </param>
        /// <param name="restrictionSessions">
        /// Сесії обмежень, отримані зі сховища.
        ///
        /// (Restriction sessions retrieved from storage.)
        /// </param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо сесія була створена раніше за користувача.
        ///
        /// (Thrown when a session was created before the user.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо колекція містить null-елементи,
        /// повторювані ідентифікатори, більше однієї сесії
        /// одного типу або сесію, що належить іншому користувачу.
        ///
        /// (Thrown when the collection contains null elements,
        /// duplicate identifiers, more than one session of the same type,
        /// or a session belonging to another user.)
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
        /// набір ідентифікаторів у кожній сесії точно відповідає набору
        /// активних обмежень її типу, загальна тривалість сесії дорівнює
        /// сумі тривалостей цих обмежень, а жодне обмеження не було
        /// створене раніше за відповідну сесію.
        ///
        /// Порожні колекції є допустимими, якщо обмеження та сесії
        /// одночасно відсутні.
        ///
        /// (Validates consistency between active restrictions and restriction
        /// sessions while restoring the user.
        ///
        /// Ensures that restriction and session type sets match exactly,
        /// each session's restriction identifier set exactly matches
        /// the active restrictions of its type, its total blocked duration
        /// equals the sum of their durations, and no restriction was created
        /// before its corresponding session.
        ///
        /// Empty collections are valid when both restrictions and sessions
        /// are absent.)
        /// </summary>
        /// <param name="restrictions">
        /// Активні обмеження, стан яких необхідно зіставити із сесіями.
        ///
        /// (Active restrictions whose state must be compared
        /// with the restriction sessions.)
        /// </param>
        /// <param name="restrictionSessions">
        /// Сесії обмежень, стан яких необхідно зіставити з обмеженнями.
        ///
        /// (Restriction sessions whose state must be compared
        /// with the active restrictions.)
        /// </param>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо типи, набори ідентифікаторів, тривалість
        /// або час створення обмежень і сесій є неузгодженими.
        ///
        /// (Thrown when restriction and session types, identifier sets,
        /// durations, or creation timestamps are inconsistent.)
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

                var sessionExpiresAt = session.GetExpirationDate();

                var restrictionsCreatedAtOrAfterSessionEnd = restrictionGroup
                    .Where(restriction => restriction.CreatedAt >= sessionExpiresAt)
                    .ToArray();

                if (restrictionsCreatedAtOrAfterSessionEnd.Length > 0)
                {
                    throw DomainInvariantViolationException.BrokenState<User>(
                        "A current restriction cannot be created at or after " +
                        "its session's expiration time.",
                        new Dictionary<string, object?>
                        {
                            ["SessionId"] = session.Id.Value,
                            ["RestrictionType"] = session.RestrictionType,
                            ["SessionCreatedAt"] = session.CreatedAt,
                            ["SessionExpiresAt"] = sessionExpiresAt,
                            ["InvalidRestrictionIds"] =
                                restrictionsCreatedAtOrAfterSessionEnd
                                    .Select(restriction => restriction.Id.Value)
                                    .ToArray(),
                            ["InvalidRestrictionCreatedAtValues"] =
                                restrictionsCreatedAtOrAfterSessionEnd
                                    .Select(restriction => restriction.CreatedAt)
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
        /// Перевіряє колекцію поточних refresh-токенів
        /// під час відновлення агрегату користувача.
        ///
        /// Переконується, що колекція не містить null-елементів,
        /// ідентифікатори та значення токенів є унікальними,
        /// кожен токен належить відновлюваному користувачу,
        /// має статус Active і не був створений раніше за користувача.
        ///
        /// Порожня колекція є допустимою. Секретні значення токенів
        /// використовуються лише для перевірки унікальності
        /// та не додаються до контексту винятків.
        ///
        /// (Validates the current refresh-token collection
        /// while restoring the user aggregate.
        ///
        /// Ensures that the collection contains no null elements,
        /// token identifiers and values are unique, and every token
        /// belongs to the restored user, has Active status,
        /// and was not created before the user.
        ///
        /// An empty collection is valid. Sensitive token values
        /// are used only for uniqueness validation and are not
        /// included in exception context.)
        /// </summary>
        /// <param name="userId">
        /// Ідентифікатор користувача, якому повинні належати всі токени.
        ///
        /// (Identifier of the user who must own all tokens.)
        /// </param>
        /// <param name="userCreatedAt">
        /// Час створення користувача, раніше якого
        /// не може бути створений жоден його токен.
        ///
        /// (User creation time, before which none of the user's
        /// tokens may have been created.)
        /// </param>
        /// <param name="refreshTokens">
        /// Поточні refresh-токени зі статусом Active,
        /// які необхідно перевірити.
        ///
        /// (Current refresh tokens with Active status to validate.)
        /// </param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо токен створений раніше за користувача.
        ///
        /// (Thrown when a token was created before the user.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо колекція містить null-елементи,
        /// повторювані ідентифікатори або значення токенів,
        /// токен іншого користувача чи токен без статусу Active.
        ///
        /// (Thrown when the collection contains null elements,
        /// duplicate token identifiers or values, a token belonging
        /// to another user, or a token without Active status.)
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
        /// <param name="userId">
        /// Ідентифікатор користувача, якому повинна належати дія.
        ///
        /// (Identifier of the user who must own the action.)
        /// </param>
        /// <param name="userCreatedAt">
        /// Час створення користувача, що визначає нижню часову межу.
        ///
        /// (User creation time defining the earliest allowed
        /// action creation time.)
        /// </param>
        /// <param name="pendingAction">
        /// Поточна незавершена дія або null, якщо вона відсутня.
        ///
        /// (Current pending action, or null when no action is present.)
        /// </param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо дія була створена раніше за користувача.
        ///
        /// (Thrown when the action was created before the user.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо дія належить іншому користувачу
        /// або не має активного статусу.
        ///
        /// (Thrown when the action belongs to another user
        /// or does not have an active status.)
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
        /// Запис зі статусом Restored не може бути поточним видаленням.
        /// Після відновлення облікового запису поточне видалення відсутнє;
        /// історичний запис може зберігатися окремо.
        ///
        /// (Validates the completed user-deletion record while restoring
        /// the aggregate.
        ///
        /// The absence of a record means that the user is not in the
        /// deleted state. When supplied, the record must belong to the user
        /// and must not have been created before the user.
        ///
        /// A record with Restored status cannot be the current deletion.
        /// After account restoration, no current deletion is present;
        /// the historical record may be retained separately.)
        /// </summary>
        /// <param name="userId">
        /// Ідентифікатор користувача, якому повинен належати запис.
        ///
        /// (Identifier of the user who must own the record.)
        /// </param>
        /// <param name="userCreatedAt">
        /// Час створення користувача, що визначає нижню часову межу.
        ///
        /// (User creation time defining the earliest allowed
        /// deletion record creation time.)
        /// </param>
        /// <param name="userDeletion">
        /// Запис про виконане видалення або null, якщо користувача не видалено.
        ///
        /// (Completed deletion record, or null when the user is not deleted.)
        /// </param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо запис був створений раніше за користувача.
        ///
        /// (Thrown when the record was created before the user.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо запис про видалення належить іншому користувачу
        /// або має статус Restored і тому не може бути поточним
        /// записом видалення агрегату.
        ///
        /// (Thrown when the deletion record belongs to another user
        /// or has Restored status and therefore cannot be the aggregate's
        /// current deletion record.)
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

                if (userDeletion.Status == DeletionStatus.Restored)
                {
                    throw DomainInvariantViolationException.BrokenState<User>(
                        "A restored deletion record cannot be attached " +
                        "as the user's current deletion.",
                        new Dictionary<string, object?>
                        {
                            ["UserId"] = userId.Value,
                            ["DeletionId"] = userDeletion.Id.Value,
                            ["DeletionStatus"] = userDeletion.Status
                        },
                        OperationType.Restore
                    );
                }
            }
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
        ///
        /// (Type of domain object that identifies its owner through
        /// <see cref="IUserOwned"/>.)
        /// </typeparam>
        /// <param name="expectedUserId">
        /// Ідентифікатор очікуваного власника.
        ///
        /// (Expected owner's identifier.)
        /// </param>
        /// <param name="entity">
        /// Доменний об’єкт, власника якого необхідно перевірити.
        ///
        /// (Domain object whose ownership must be checked.)
        /// </param>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо об’єкт належить іншому користувачу.
        ///
        /// (Thrown when the object belongs to another user.)
        /// </exception>
        private static void EnsureOwnedByUser<TEntity>(
            UserId expectedUserId,
            TEntity entity)
            where TEntity : IUserOwned
        {
            if (entity.UserId == expectedUserId)
            {
                return;
            }

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

        #region Validate RefreshTokens Against Ban
        /// <summary>
        /// Перевіряє узгодженість поточних refresh-токенів
        /// із поточною сесією бану під час відновлення агрегату.
        ///
        /// Якщо сесія бану присутня, кожен поточний токен
        /// повинен бути створений у момент завершення бану
        /// або пізніше.
        ///
        /// (Validates the consistency of current refresh tokens
        /// with the current ban session while restoring the aggregate.
        ///
        /// When a ban session is present, every current token
        /// must have been created at or after the ban expiration time.)
        /// </summary>
        /// <remarks>
        /// Виконується після перевірки складу колекцій,
        /// належності сутностей користувачу та допустимості
        /// їхніх станів.
        ///
        /// (Runs after collection contents, entity ownership,
        /// and permitted entity states have been validated.)
        /// </remarks>
        /// <param name="userId">
        /// Ідентифікатор користувача, агрегат якого відновлюється.
        ///
        /// (Identifier of the user whose aggregate is being restored.)
        /// </param>
        /// <param name="restrictionSessions">
        /// Попередньо перевірені поточні сесії обмежень.
        ///
        /// (Previously validated current restriction sessions.)
        /// </param>
        /// <param name="refreshTokens">
        /// Попередньо перевірені поточні refresh-токени
        /// зі статусом Active.
        ///
        /// (Previously validated current refresh tokens
        /// with Active status.)
        /// </param>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо поточний refresh-токен створений
        /// раніше завершення переданої сесії бану.
        ///
        /// (Thrown when a current refresh token was created
        /// before the supplied ban session's expiration time.)
        /// </exception>
        private static void ValidateRefreshTokensAgainstBan(
            UserId userId,
            IReadOnlyCollection<UserRestrictionSession> restrictionSessions,
            IReadOnlyCollection<UserRefreshToken> refreshTokens)
        {
            var banSession = restrictionSessions
                .FirstOrDefault(session => session.RestrictionType == RestrictionType.Ban);

            if (banSession is null)
            {
                return;
            }

            var banExpiresAt = banSession.GetExpirationDate();

            foreach (var refreshToken in refreshTokens)
            {
                if (refreshToken.CreatedAt >= banExpiresAt)
                    continue;

                throw DomainInvariantViolationException.BrokenState<User>(
                    "A current refresh token must have been created " +
                    "at or after the attached ban session's expiration time.",
                    new Dictionary<string, object?>
                    {
                        ["UserId"] = userId.Value,
                        ["BanSessionId"] = banSession.Id.Value,
                        ["BanSessionCreatedAt"] = banSession.CreatedAt,
                        ["BanSessionExpiresAt"] = banExpiresAt,
                        ["RefreshTokenId"] = refreshToken.Id.Value,
                        ["RefreshTokenCreatedAt"] = refreshToken.CreatedAt
                    },
                    OperationType.Restore
                );
            }
        }
        #endregion

        #region Validate Aggregate Update Consistency
        /// <summary>
        /// Перевіряє узгодженість загального часу оновлення User
        /// зі станом лічильника невдалих спроб та відомими датами
        /// пов’язаних сутностей.
        ///
        /// За наявності поточних дочірніх сутностей або ненульового
        /// лічильника загальний час оновлення повинен бути заданий.
        /// Він не може передувати датам створення дітей
        /// та власному часу оновлення поточного запису видалення.
        ///
        /// (Validates the aggregate-wide User update timestamp against
        /// the failed password attempt count and known timestamps
        /// of related entities.
        ///
        /// When current child entities are present or the attempt count
        /// is nonzero, the aggregate-wide update timestamp must be set.
        /// It must not precede child creation timestamps or the current
        /// deletion record's own update timestamp.)
        /// </summary>
        /// <remarks>
        /// Виконується після перевірки базового стану користувача,
        /// складу колекцій, належності та допустимих станів дітей.
        ///
        /// Строки дії та інші майбутні дедлайни не порівнюються
        /// із загальним часом оновлення.
        ///
        /// (Runs after the user's core state, collection contents,
        /// entity ownership, and permitted child states are validated.
        ///
        /// Expiration times and other future deadlines are not compared
        /// against the aggregate-wide update timestamp.)
        /// </remarks>
        /// <param name="userId">
        /// Ідентифікатор відновлюваного користувача.
        ///
        /// (Identifier of the user being restored.)
        /// </param>
        /// <param name="userUpdatedAt">
        /// Час останньої фактичної зміни всього агрегату User.
        ///
        /// (Time of the last actual change to the entire User aggregate.)
        /// </param>
        /// <param name="failedPasswordAttempts">
        /// Попередньо перевірений лічильник невдалих спроб введення пароля.
        ///
        /// (Previously validated failed password attempt count.)
        /// </param>
        /// <param name="restrictions">
        /// Попередньо перевірені поточні обмеження.
        ///
        /// (Previously validated current restrictions.)
        /// </param>
        /// <param name="restrictionSessions">
        /// Попередньо перевірені поточні сесії обмежень.
        ///
        /// (Previously validated current restriction sessions.)
        /// </param>
        /// <param name="refreshTokens">
        /// Попередньо перевірені поточні refresh-токени.
        ///
        /// (Previously validated current refresh tokens.)
        /// </param>
        /// <param name="pendingAction">
        /// Попередньо перевірена поточна незавершена дія або null.
        ///
        /// (Previously validated current pending action, or null.)
        /// </param>
        /// <param name="deletion">
        /// Попередньо перевірений поточний запис видалення або null.
        ///
        /// (Previously validated current deletion record, or null.)
        /// </param>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо стан агрегату свідчить про виконану зміну,
        /// але загальний час оновлення відсутній.
        ///
        /// (Thrown when the aggregate state indicates a completed change,
        /// but the aggregate-wide update timestamp is missing.)
        /// </exception>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо загальний час оновлення передує відомій
        /// даті створення або оновлення пов’язаної сутності.
        ///
        /// (Thrown when the aggregate-wide update timestamp precedes
        /// a known creation or update timestamp of a related entity.)
        /// </exception>
        private static void ValidateAggregateUpdateConsistency(
            UserId userId,
            DateTimeOffset? userUpdatedAt,
            int failedPasswordAttempts,
            IReadOnlyCollection<UserRestriction> restrictions,
            IReadOnlyCollection<UserRestrictionSession> restrictionSessions,
            IReadOnlyCollection<UserRefreshToken> refreshTokens,
            UserPendingAction? pendingAction,
            UserDeletion? deletion)
        {
            var requiresUpdateAt =
                failedPasswordAttempts > 0 ||
                restrictions.Count > 0 ||
                restrictionSessions.Count > 0 ||
                refreshTokens.Count > 0 ||
                pendingAction is not null ||
                deletion is not null;

            if (!requiresUpdateAt)
            {
                return;
            }

            if (userUpdatedAt is null)
            {
                throw DomainInvariantViolationException.BrokenState<User>(
                    "User.UpdatedAt must be set when the aggregate contains " +
                    "related state or recorded failed password attempts.",
                    new Dictionary<string, object?>
                    {
                        ["UserId"] = userId.Value,
                        ["UserUpdatedAt"] = userUpdatedAt,
                        ["FailedPasswordAttempts"] = failedPasswordAttempts,
                        ["RestrictionCount"] = restrictions.Count,
                        ["RestrictionSessionCount"] = restrictionSessions.Count,
                        ["RefreshTokenCount"] = refreshTokens.Count,
                        ["PendingActionId"] = pendingAction?.Id.Value,
                        ["DeletionId"] = deletion?.Id.Value
                    },
                    OperationType.Restore
                );
            }

            var aggregateUpdatedAt = userUpdatedAt.Value;

            foreach (var restriction in restrictions)
            {
                DomainGuard.AgainstEarlierThan<User>(
                    OperationType.Restore,
                    (aggregateUpdatedAt, nameof(aggregateUpdatedAt)),
                    (restriction.CreatedAt, nameof(restriction.CreatedAt))
                );
            }

            foreach (var session in restrictionSessions)
            {
                DomainGuard.AgainstEarlierThan<User>(
                    OperationType.Restore,
                    (aggregateUpdatedAt, nameof(aggregateUpdatedAt)),
                    (session.CreatedAt, nameof(session.CreatedAt))
                );
            }

            foreach (var refreshToken in refreshTokens)
            {
                DomainGuard.AgainstEarlierThan<User>(
                    OperationType.Restore,
                    (aggregateUpdatedAt, nameof(aggregateUpdatedAt)),
                    (refreshToken.CreatedAt, nameof(refreshToken.CreatedAt))
                );
            }

            if (pendingAction is not null)
            {
                DomainGuard.AgainstEarlierThan<User>(
                    OperationType.Restore,
                    (aggregateUpdatedAt, nameof(aggregateUpdatedAt)),
                    (pendingAction.CreatedAt, nameof(pendingAction.CreatedAt))
                );
            }

            if (deletion is not null)
            {
                DomainGuard.AgainstEarlierThan<User>(
                    OperationType.Restore,
                    (aggregateUpdatedAt, nameof(aggregateUpdatedAt)),
                    (deletion.CreatedAt, nameof(deletion.CreatedAt))
                );

                if (deletion.UpdatedAt is not null)
                {
                    DomainGuard.AgainstEarlierThan<User>(
                        OperationType.Restore,
                        (aggregateUpdatedAt, nameof(aggregateUpdatedAt)),
                        (deletion.UpdatedAt.Value, nameof(deletion.UpdatedAt))
                    );
                }
            }
        }
        #endregion
    }
}
