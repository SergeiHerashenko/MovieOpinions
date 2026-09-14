using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
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
    public sealed class UserRefreshToken : Entity<UserRefreshTokenId>
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
        /// <summary>
        /// Створює активний refresh-токен для користувача, генерує його ID
        /// і секретне значення та встановлює строк дії.
        ///
        /// (Creates an active refresh token for a user, generates its ID
        /// and secret value, and establishes its lifetime.)
        /// </summary>
        /// <param name="userId">Ідентифікатор власника токена.</param>
        /// <param name="deviceInfo">Інформація про пристрій поточної сесії.</param>
        /// <param name="ipAddress">IP-адреса створення токена.</param>
        /// <param name="now">Поточний час доменної операції.</param>
        /// <param name="city">Визначене місто або null, якщо воно невідоме.</param>
        /// <returns>Створений активний refresh-токен.</returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо обов’язковий доменний об’єкт дорівнює null.
        /// </exception>
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
        /// <summary>
        /// Відновлює refresh-токен зі збереженого стану
        /// та перевіряє його часові й статусні інваріанти.
        ///
        /// (Restores a refresh token from persisted state
        /// and validates its temporal and status invariants.)
        /// </summary>
        /// <param name="userRefreshTokenId">Ідентифікатор сутності токена.</param>
        /// <param name="userId">Ідентифікатор власника токена.</param>
        /// <param name="refreshToken">Збережене значення refresh-токена.</param>
        /// <param name="deviceInfo">Збережена інформація про пристрій.</param>
        /// <param name="ipAddress">Збережена IP-адреса.</param>
        /// <param name="city">Збережене місто або null.</param>
        /// <param name="tokenStatus">Збережений статус токена.</param>
        /// <param name="expiresAt">Час завершення строку дії.</param>
        /// <param name="consumedAt">Час використання або null.</param>
        /// <param name="createdAt">Час створення токена.</param>
        /// <param name="revokedAt">Час відкликання або null.</param>
        /// <returns>Відновлений refresh-токен.</returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає через відсутні обов’язкові дані, невідомий статус
        /// або некоректні часові значення.
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо статус токена суперечить його часовим полям.
        /// </exception>
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
        /// Визначає, чи може refresh token бути використаний
        /// у вказаний момент часу.
        ///
        /// (Determines whether the refresh token can be used
        /// at the specified point in time.)
        /// </summary>
        /// <param name="now">Час, для якого виконується перевірка.</param>
        /// <returns>
        /// true, якщо токен має статус Active, уже створений
        /// і строк його дії ще не завершився; інакше false.
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
        /// (Determines whether the token expiration time
        /// has been reached or passed.)
        /// </summary>
        /// <param name="now">Час, для якого виконується перевірка.</param>
        /// <returns>true, якщо now дорівнює ExpiresAt або перевищує його.</returns>
        public bool HasExpirationElapsed(DateTimeOffset now)
        {
            return now >= ExpiresAt;
        }

        /// <summary>
        /// Повертає залишок часу, протягом якого токен може бути використаний.
        ///
        /// (Returns the remaining period during which the token can be used.)
        /// </summary>
        /// <param name="now">Час, відносно якого обчислюється залишок.</param>
        /// <returns>
        /// Залишок часу до завершення строку дії або TimeSpan.Zero,
        /// якщо токен зараз не може бути використаний.
        /// </returns>
        public TimeSpan GetRemainingUsableTime(DateTimeOffset now)
        {
            if (!CanBeUsed(now))
                return TimeSpan.Zero;

            return ExpiresAt - now;
        }
        #endregion

        #region Behavior
        /// <summary>
        /// Намагається одноразово використати активний refresh-токен.
        /// Якщо строк дії вже завершився, ліниво переводить токен у стан Expired.
        ///
        /// (Attempts to consume an active refresh token once.
        /// If its lifetime has elapsed, lazily transitions it to Expired.)
        /// </summary>
        /// <param name="now">Поточний час доменної операції.</param>
        /// <returns>
        /// Успішний результат переходу в Consumed або помилка,
        /// що пояснює причину відмови.
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо час операції передує часу створення токена.
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
        /// Намагається явно відкликати активний refresh-токен.
        /// Якщо строк дії вже завершився, ліниво переводить токен у стан Expired.
        ///
        /// (Attempts to explicitly revoke an active refresh token.
        /// If its lifetime has elapsed, lazily transitions it to Expired.)
        /// </summary>
        /// <param name="now">Поточний час доменної операції.</param>
        /// <returns>
        /// Успішний результат переходу в Revoked або помилка,
        /// що пояснює причину відмови.
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо час операції передує часу створення токена.
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
        /// Фіксує завершення строку дії активного токена.
        /// Не перезаписує термінальні стани Consumed, Revoked або Expired.
        ///
        /// (Records the expiration of an active token.
        /// Does not overwrite the Consumed, Revoked, or Expired terminal states.)
        /// </summary>
        /// <param name="now">Поточний час доменної операції.</param>
        /// <returns>true, якщо токен переведено в Expired; інакше false.</returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо час операції передує часу створення токена.
        /// </exception>
        internal bool MarkAsExpired(DateTimeOffset now)
        {
            DomainGuard.AgainstEarlierThan<UserRefreshToken>(
               OperationType.Update,
               (now, nameof(now)),
               (CreatedAt, nameof(CreatedAt))
            );

            return TryMarkAsExpired(now);
        }

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

            if (TryMarkAsExpired(now))
                return Result.Failure(TokenStatusErrors.ExpiredToken<UserRefreshToken>());

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

        private bool TryMarkAsExpired(DateTimeOffset now)
        {
            if (TokenStatus != TokenStatus.Active)
                return false;

            if (now < ExpiresAt)
                return false;

            TokenStatus = TokenStatus.Expired;

            return true;
        }
        #endregion
    }
}
