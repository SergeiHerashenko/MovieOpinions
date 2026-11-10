using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Contracts;
using Authorization.Domain.Users.Entities.UsersRefreshToken.Errors;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken
{
    /// <summary>
    /// Представляє refresh-токен користувача та керує його життєвим циклом.
    /// Зберігає контекст створення токена і підтримує переходи між станами
    /// Active, Consumed, Revoked та Expired.
    ///
    /// (Represents a user's refresh token and manages its lifecycle.
    /// Stores the token creation context and supports transitions between
    /// Active, Consumed, Revoked, and Expired states.)
    /// </summary>
    public sealed class UserRefreshToken : Entity<UserRefreshTokenId>, IUserOwned
    {
        private static readonly TimeSpan ExpirationTime = TimeSpan.FromDays(7);

        #region Properties
        /// <summary>
        /// Ідентифікатор користувача, якому належить токен.
        ///
        /// (Identifier of the user who owns the token.)
        /// </summary>
        public UserId UserId { get; }

        /// <summary>
        /// Згенерований refresh-токен.
        /// Його сире значення є секретним і не повинно потрапляти в логи.
        ///
        /// (Generated refresh token.
        /// Its raw value is sensitive and must not be logged.)
        /// </summary>
        public RefreshToken RefreshToken { get; }

        /// <summary>
        /// Зліпок інформації про пристрій, для якого створено токен.
        ///
        /// (Snapshot of the device information for which the token was created.)
        /// </summary>
        public DeviceInfo DeviceInfo { get; }

        /// <summary>
        /// IP-адреса, з якої було створено токен.
        ///
        /// (IP address from which the token was created.)
        /// </summary>
        public IpAddress IpAddress { get; }

        /// <summary>
        /// Визначене місто або null, якщо географічне розташування невідоме.
        ///
        /// (Resolved city, or null when the geographic location is unknown.)
        /// </summary>
        public string? City { get; }

        /// <summary>
        /// Поточний збережений стан життєвого циклу токена.
        ///
        /// (Current persisted lifecycle state of the token.)
        /// </summary>
        public TokenStatus TokenStatus { get; private set; }

        /// <summary>
        /// Дата й час завершення строку дії токена.
        ///
        /// (Date and time when the token expires.)
        /// </summary>
        public DateTimeOffset ExpiresAt { get; }

        /// <summary>
        /// Дата й час успішного одноразового використання токена.
        ///
        /// (Date and time when the token was successfully consumed.)
        /// </summary>
        public DateTimeOffset? ConsumedAt { get; private set; }

        /// <summary>
        /// Дата й час явного відкликання токена.
        ///
        /// (Date and time when the token was explicitly revoked.)
        /// </summary>
        public DateTimeOffset? RevokedAt { get; private set; }
        #endregion

        #region Creation
        private UserRefreshToken(
            UserRefreshTokenId userRefreshTokenId,
            UserId userId,
            RefreshToken refreshToken,
            DeviceInfo deviceInfo,
            IpAddress ipAddress,
            DateTimeOffset now,
            string? city = null)
            : base(userRefreshTokenId, now)
        {
            UserId = userId;
            RefreshToken = refreshToken;
            DeviceInfo = deviceInfo;
            IpAddress = ipAddress;
            City = city;
            TokenStatus = TokenStatus.Active;
            ExpiresAt = now.Add(ExpirationTime);
            ConsumedAt = null;
            RevokedAt = null;
        }

        /// <summary>
        /// Створює активний refresh-токен для користувача, генерує його
        /// ідентифікатор і секретне значення та встановлює строк дії.
        ///
        /// (Creates an active refresh token for a user, generates its
        /// identifier and secret value, and establishes its lifetime.)
        /// </summary>
        /// <param name="userId">
        /// Ідентифікатор власника токена.
        ///
        /// (Identifier of the token owner.)
        /// </param>
        /// <param name="deviceInfo">
        /// Інформація про пристрій поточної сесії.
        ///
        /// (Device information for the current session.)
        /// </param>
        /// <param name="ipAddress">
        /// IP-адреса, з якої створюється токен.
        ///
        /// (The IP address from which the token is created.)
        /// </param>
        /// <param name="now">
        /// Поточний час доменної операції.
        ///
        /// (The current domain-operation time.)
        /// </param>
        /// <param name="city">
        /// Визначене місто або null, якщо воно невідоме.
        ///
        /// (The resolved city, or null when it is unknown.)
        /// </param>
        /// <returns>
        /// Створений активний refresh-токен.
        ///
        /// (The newly created active refresh token.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо обов’язковий доменний об’єкт дорівнює null.
        ///
        /// (Thrown when a required domain object is null.)
        /// </exception>
        internal static UserRefreshToken Create(
            UserId userId,
            DeviceInfo deviceInfo,
            IpAddress ipAddress,
            DateTimeOffset now,
            string? city = null)
        {
            DomainGuard.AgainstNull<UserRefreshToken>(
                OperationType.Create,
                (userId, nameof(userId)),
                (deviceInfo, nameof(deviceInfo)),
                (ipAddress, nameof(ipAddress))
            );

            return new UserRefreshToken(
                UserRefreshTokenId.Create(),
                userId,
                RefreshToken.Create(),
                deviceInfo,
                ipAddress,
                now,
                city
            );
        }
        #endregion

        #region Restoration
        private UserRefreshToken(
            UserRefreshTokenId userRefreshTokenId,
            UserId userId,
            RefreshToken refreshToken,
            DeviceInfo deviceInfo,
            IpAddress ipAddress,
            string? city,
            TokenStatus tokenStatus,
            DateTimeOffset expiresAt,
            DateTimeOffset? consumedAt,
            DateTimeOffset createdAt,
            DateTimeOffset? revokedAt)
            : base(userRefreshTokenId, createdAt)
        {
            UserId = userId;
            RefreshToken = refreshToken;
            DeviceInfo = deviceInfo;
            IpAddress = ipAddress;
            City = city;
            TokenStatus = tokenStatus;
            ExpiresAt = expiresAt;
            ConsumedAt = consumedAt;
            RevokedAt = revokedAt;
        }

        /// <summary>
        /// Відновлює refresh-токен зі збереженого стану
        /// та перевіряє його часові й статусні інваріанти.
        ///
        /// (Restores a refresh token from its persisted state
        /// and validates its temporal and status invariants.)
        /// </summary>
        /// <param name="userRefreshTokenId">
        /// Збережений ідентифікатор сутності токена.
        ///
        /// (The persisted token entity identifier.)
        /// </param>
        /// <param name="userId">
        /// Ідентифікатор власника токена.
        ///
        /// (Identifier of the token owner.)
        /// </param>
        /// <param name="refreshToken">
        /// Відновлене доменне значення refresh-токена.
        ///
        /// (The restored domain refresh token value.)
        /// </param>
        /// <param name="deviceInfo">
        /// Відновлена інформація про пристрій.
        ///
        /// (The restored device information.)
        /// </param>
        /// <param name="ipAddress">
        /// Відновлена IP-адреса.
        ///
        /// (The restored IP address.)
        /// </param>
        /// <param name="city">
        /// Збережене місто або null, якщо воно невідоме.
        ///
        /// (The persisted city, or null when it is unknown.)
        /// </param>
        /// <param name="tokenStatus">
        /// Збережений статус токена.
        ///
        /// (The persisted token status.)
        /// </param>
        /// <param name="expiresAt">
        /// Збережений час завершення строку дії токена.
        ///
        /// (The persisted token expiration time.)
        /// </param>
        /// <param name="consumedAt">
        /// Час одноразового використання або null, якщо токен не використано.
        ///
        /// (The consumption time, or null when the token has not been consumed.)
        /// </param>
        /// <param name="createdAt">
        /// Збережений час створення токена.
        ///
        /// (The persisted token creation time.)
        /// </param>
        /// <param name="revokedAt">
        /// Час відкликання або null, якщо токен не відкликано.
        ///
        /// (The revocation time, or null when the token has not been revoked.)
        /// </param>
        /// <returns>
        /// Відновлений refresh-токен.
        ///
        /// (The restored refresh token.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо обов’язкові доменні об’єкти відсутні,
        /// статус не підтримується або час використання чи відкликання
        /// передує часу створення токена.
        ///
        /// (Thrown when required domain objects are missing,
        /// the status is unsupported, or the consumption or revocation time
        /// precedes the token creation time.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо час завершення строку дії не пізніший за час створення
        /// або статус токена не узгоджується з його часовими полями.
        ///
        /// (Thrown when the expiration time is not later than the creation time
        /// or the token status is inconsistent with its lifecycle timestamps.)
        /// </exception>
        public static UserRefreshToken Restore(
            UserRefreshTokenId userRefreshTokenId,
            UserId userId,
            RefreshToken refreshToken,
            DeviceInfo deviceInfo,
            IpAddress ipAddress,
            string? city,
            TokenStatus tokenStatus,
            DateTimeOffset expiresAt,
            DateTimeOffset? consumedAt,
            DateTimeOffset createdAt,
            DateTimeOffset? revokedAt)
        {
            DomainGuard.AgainstNull<UserRefreshToken>(
                OperationType.Restore,
                (userRefreshTokenId, nameof(userRefreshTokenId)),
                (userId, nameof(userId)),
                (refreshToken, nameof(refreshToken)),
                (deviceInfo, nameof(deviceInfo)),
                (ipAddress, nameof(ipAddress))
            );

            ValidateState(
                OperationType.Restore,
                tokenStatus,
                expiresAt,
                consumedAt,
                createdAt,
                revokedAt
            );

            return new UserRefreshToken(
                userRefreshTokenId,
                userId,
                refreshToken,
                deviceInfo,
                ipAddress,
                city,
                tokenStatus,
                expiresAt,
                consumedAt,
                createdAt,
                revokedAt
            );
        }
        #endregion

        #region Validation
        /// <summary>
        /// Перевіряє строк дії токена, хронологію його використання
        /// й відкликання та узгодженість статусу з часовими полями.
        ///
        /// (Validates the token lifetime, the chronology of its consumption
        /// and revocation, and the consistency of its status with its timestamps.)
        /// </summary>
        private static void ValidateState(
            OperationType operationType,
            TokenStatus tokenStatus,
            DateTimeOffset expiresAt,
            DateTimeOffset? consumedAt,
            DateTimeOffset createdAt,
            DateTimeOffset? revokedAt)
        {
            DomainGuard.AgainstUndefinedEnum<UserRefreshToken>(
                operationType,
                (tokenStatus, nameof(tokenStatus))
            );

            if (expiresAt <= createdAt)
            {
                throw DomainInvariantViolationException.BrokenState<UserRefreshToken>(
                    "Expiration time must be later than creation time.",
                    new Dictionary<string, object?>
                    {
                        ["CreatedAt"] = createdAt,
                        ["ExpiresAt"] = expiresAt
                    },
                    operationType
                );
            }

            ValidateLifecycleTimestamps(
                operationType,
                createdAt,
                (consumedAt, nameof(consumedAt)),
                (revokedAt, nameof(revokedAt))
            );

            bool isValidStatus = tokenStatus switch
            {
                TokenStatus.Active =>
                    consumedAt is null
                    && revokedAt is null,

                TokenStatus.Expired =>
                    consumedAt is null
                    && revokedAt is null,

                TokenStatus.Consumed =>
                    consumedAt is not null
                    && consumedAt.Value < expiresAt
                    && revokedAt is null,

                TokenStatus.Revoked =>
                    revokedAt is not null
                    && consumedAt is null,

                _ => false
            };

            if (!isValidStatus)
            {
                throw DomainInvariantViolationException.BrokenState<UserRefreshToken>(
                    $"Inconsistent state: fields '{nameof(tokenStatus)}' and '{nameof(consumedAt)}' " +
                    $"and '{nameof(revokedAt)}' are not consistent!",
                    new Dictionary<string, object?>
                    {
                        ["TokenStatus"] = tokenStatus,
                        ["ExpiresAt"] = expiresAt,
                        ["ConsumedAt"] = consumedAt,
                        ["RevokedAt"] = revokedAt,
                    },
                    operationType
                );
            }
        }

        /// <summary>
        /// Перевіряє, що кожне задане часове значення не передує
        /// часу створення токена. Значення null пропускаються.
        ///
        /// (Validates that each supplied timestamp does not precede
        /// the token creation time. Null values are skipped.)
        /// </summary>
        /// <param name="operationType">
        /// Операція, під час якої виконується перевірка.
        ///
        /// (The operation during which validation is performed.)
        /// </param>
        /// <param name="createdAt">
        /// Час створення токена.
        ///
        /// (The token creation time.)
        /// </param>
        /// <param name="currentsTime">
        /// Необов’язкові часові значення з назвами відповідних полів.
        ///
        /// (Optional timestamps paired with their corresponding field names.)
        /// </param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо задане часове значення передує часу створення токена.
        ///
        /// (Thrown when a supplied timestamp precedes the token creation time.)
        /// </exception>
        private static void ValidateLifecycleTimestamps(
            OperationType operationType,
            DateTimeOffset createdAt,
            params (DateTimeOffset? currentTime, string nameCurrentTime)[] currentsTime)
        {
            foreach (var (time, name) in currentsTime)
            {
                if (time.HasValue)
                {
                    DomainGuard.AgainstEarlierThan<UserRefreshToken>(
                        operationType,
                        (time.Value, name),
                        (createdAt, nameof(createdAt))
                    );
                }
            }
        }
        #endregion

        #region Queries
        /// <summary>
        /// Визначає, чи може refresh-токен бути використаний
        /// у вказаний момент часу.
        ///
        /// (Determines whether the refresh token can be used
        /// at the specified point in time.)
        /// </summary>
        /// <param name="now">
        /// Час, для якого виконується перевірка.
        ///
        /// (The point in time for which the check is performed.)
        /// </param>
        /// <returns>
        /// true, якщо токен має статус Active, уже створений
        /// і строк його дії ще не завершився; інакше false.
        ///
        /// (true when the token is Active, has already been created,
        /// and has not yet expired; otherwise, false.)
        /// </returns>
        public bool CanBeUsed(DateTimeOffset now)
        {
            return TokenStatus == TokenStatus.Active
                && now >= CreatedAt
                && !HasExpirationElapsed(now);
        }

        /// <summary>
        /// Визначає, чи настав або минув час завершення строку дії токена.
        ///
        /// Перевірка не залежить від поточного статусу токена.
        ///
        /// (Determines whether the token expiration time has been reached or passed.
        ///
        /// The check is independent of the current token status.)
        /// </summary>
        /// <param name="now">
        /// Час, для якого виконується перевірка.
        ///
        /// (The point in time for which the check is performed.)
        /// </param>
        /// <returns>
        /// true, якщо now дорівнює ExpiresAt або перевищує його; інакше false.
        ///
        /// (true when now is equal to or later than ExpiresAt; otherwise, false.)
        /// </returns>
        public bool HasExpirationElapsed(DateTimeOffset now)
        {
            return now >= ExpiresAt;
        }

        /// <summary>
        /// Повертає залишок часу, протягом якого токен може бути використаний.
        ///
        /// (Returns the remaining period during which the token can be used.)
        /// </summary>
        /// <param name="now">
        /// Час, відносно якого обчислюється залишок.
        ///
        /// (The point in time from which the remaining period is calculated.)
        /// </param>
        /// <returns>
        /// Залишок часу до завершення строку дії або TimeSpan.Zero,
        /// якщо токен не може бути використаний у вказаний момент.
        ///
        /// (The remaining time until expiration, or TimeSpan.Zero
        /// when the token cannot be used at the specified point in time.)
        /// </returns>
        public TimeSpan GetRemainingUsableTime(DateTimeOffset now)
        {
            if (!CanBeUsed(now))
            {
                return TimeSpan.Zero;
            }

            return ExpiresAt - now;
        }
        #endregion

        #region Behavior
        /// <summary>
        /// Намагається одноразово використати активний refresh-токен
        /// та зафіксувати час його використання.
        ///
        /// Якщо активний токен прострочений, переводить його у стан Expired
        /// і повертає помилку завершення строку дії.
        ///
        /// (Attempts to consume an active refresh token once
        /// and record its consumption time.
        ///
        /// If the active token has expired, transitions it to Expired
        /// and returns an expiration error.)
        /// </summary>
        /// <param name="now">
        /// Поточний час доменної операції.
        ///
        /// (The current domain-operation time.)
        /// </param>
        /// <returns>
        /// Успішний результат переходу в Consumed або невдалий результат
        /// із помилкою, що пояснює причину відмови.
        ///
        /// (A successful result for the transition to Consumed,
        /// or a failed result containing an error explaining the rejection.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо час операції передує часу створення токена.
        ///
        /// (Thrown when the operation time precedes the token creation time.)
        /// </exception>
        internal Result Consume(DateTimeOffset now)
        {
            return ChangeTokenStatus(
                now,
                TokenStatus.Consumed,
                () => ConsumedAt = now
            );
        }

        /// <summary>
        /// Намагається явно відкликати активний refresh-токен
        /// та зафіксувати час його відкликання.
        ///
        /// Якщо активний токен прострочений, переводить його у стан Expired
        /// і повертає помилку завершення строку дії.
        ///
        /// (Attempts to explicitly revoke an active refresh token
        /// and record its revocation time.
        ///
        /// If the active token has expired, transitions it to Expired
        /// and returns an expiration error.)
        /// </summary>
        /// <param name="now">
        /// Поточний час доменної операції.
        ///
        /// (The current domain-operation time.)
        /// </param>
        /// <returns>
        /// Успішний результат переходу в Revoked або невдалий результат
        /// із помилкою, що пояснює причину відмови.
        ///
        /// (A successful result for the transition to Revoked,
        /// or a failed result containing an error explaining the rejection.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо час операції передує часу створення токена.
        ///
        /// (Thrown when the operation time precedes the token creation time.)
        /// </exception>
        internal Result Revoke(DateTimeOffset now)
        {
            return ChangeTokenStatus(
                now,
                TokenStatus.Revoked,
                () => RevokedAt = now
            );
        }

        /// <summary>
        /// Завершує дію активного refresh-токена.
        /// Якщо строк дії вже минув, переводить токен у стан Expired.
        /// Інакше переводить токен у стан Revoked та встановлює RevokedAt.
        ///
        /// (Terminates an active refresh token.
        /// If its lifetime has elapsed, transitions it to Expired.
        /// Otherwise, transitions it to Revoked and assigns RevokedAt.)
        /// </summary>
        /// <remarks>
        /// Метод призначений для токенів зі збереженим статусом Active.
        /// Завершення строку дії такого токена є звичайним сценарієм
        /// і не спричиняє відмови.
        ///
        /// Метод змінює лише стан самої сутності. Вилучення токена
        /// з поточної колекції та реєстрацію зміни виконує агрегат.
        ///
        /// (The method is intended for tokens whose stored status is Active.
        /// An elapsed lifetime is a normal scenario and does not cause
        /// the operation to fail.
        ///
        /// The method changes only the entity's own state. Removing the token
        /// from the current collection and recording its change are handled
        /// by the aggregate.)
        /// </remarks>
        /// <param name="now">
        /// Час завершення дії токена.
        ///
        /// (The token termination time.)
        /// </param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо час операції передує часу створення токена.
        ///
        /// (Thrown when the operation time precedes the token creation time.)
        /// </exception>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо токен не має статусу Active.
        ///
        /// (Thrown when the token does not have Active status.)
        /// </exception>
        internal void Terminate(DateTimeOffset now)
        {
            DomainGuard.AgainstEarlierThan<UserRefreshToken>(
                OperationType.Update,
                (now, nameof(now)),
                (CreatedAt, nameof(CreatedAt))
            );

            if (TokenStatus != TokenStatus.Active)
            {
                throw DomainInvalidOperationException.PreconditionFailed<UserRefreshToken>(
                    nameof(Terminate),
                    "The refresh token must have Active status before termination.",
                    OperationType.Update,
                    context: new Dictionary<string, object>
                    {
                        ["RefreshTokenId"] = Id.Value,
                        ["TokenStatus"] = TokenStatus,
                        ["OperationTime"] = now
                    }
                );
            }

            if (TryMarkAsExpired(now))
            {
                return;
            }

            RevokedAt = now;
            TokenStatus = TokenStatus.Revoked;
        }

        /// <summary>
        /// Перевіряє час операції та обробляє прострочення активного токена.
        /// Якщо токен залишається активним, виконує передану дію
        /// та встановлює цільовий статус.
        ///
        /// Перехід у Expired може відбутися навіть при поверненні
        /// невдалого результату. Винятки переданої дії передаються виклику.
        ///
        /// (Validates the operation time and handles expiration of an active token.
        /// If the token remains active, executes the supplied action
        /// and assigns the target status.
        ///
        /// A transition to Expired may occur even when a failed result
        /// is returned. Exceptions from the supplied action propagate to the caller.)
        /// </summary>
        /// <param name="now">
        /// Поточний час доменної операції.
        ///
        /// (The current domain-operation time.)
        /// </param>
        /// <param name="targetStatus">
        /// Цільовий статус токена.
        ///
        /// (The target token status.)
        /// </param>
        /// <param name="applyTransitionData">
        /// Дія, що встановлює дані переходу перед зміною статусу.
        ///
        /// (The action that assigns transition data before the status is changed.)
        /// </param>
        /// <returns>
        /// Успішний результат переходу або невдалий результат
        /// через прострочення чи неактивний статус токена.
        ///
        /// (A successful transition result, or a failed result
        /// caused by expiration or a non-active token status.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо час операції передує створенню токена або цільовий статус не підтримується.
        ///
        /// (Thrown when the operation time precedes token creation or the target status is unsupported.)
        /// </exception>
        private Result ChangeTokenStatus(
            DateTimeOffset now,
            TokenStatus targetStatus,
            Action applyTransitionData)
        {
            DomainGuard.AgainstEarlierThan<UserRefreshToken>(
                OperationType.Update,
                (now, nameof(now)),
                (CreatedAt, nameof(CreatedAt))
            );

            DomainGuard.AgainstUndefinedEnum<UserRefreshToken>(
                OperationType.Update,
                (targetStatus, nameof(targetStatus))
            );

            if (TryMarkAsExpired(now))
            {
                return Result.Failure(TokenStatusErrors.ExpiredToken<UserRefreshToken>());
            }

            if (TokenStatus != TokenStatus.Active)
            {
                return Result.Failure(TokenStatusErrors.InvalidStatusTransition<UserRefreshToken>(
                    TokenStatus,
                    targetStatus)
                );
            }

            applyTransitionData();
            TokenStatus = targetStatus;

            return Result.Success();
        }

        /// <summary>
        /// Переводить токен у стан Expired, лише якщо він має статус Active
        /// і час завершення строку дії вже настав.
        ///
        /// (Transitions the token to Expired only when its status is Active
        /// and its expiration time has been reached.)
        /// </summary>
        /// <param name="now">
        /// Час, для якого перевіряється завершення строку дії.
        ///
        /// (The point in time used to check expiration.)
        /// </param>
        /// <returns>
        /// true, якщо статус змінено на Expired; інакше false.
        ///
        /// (true when the status changes to Expired; otherwise, false.)
        /// </returns>
        private bool TryMarkAsExpired(DateTimeOffset now)
        {
            if (TokenStatus != TokenStatus.Active)
            {
                return false;
            }

            if (now < ExpiresAt)
            {
                return false;
            }

            TokenStatus = TokenStatus.Expired;

            return true;
        }
        #endregion
    }
}
