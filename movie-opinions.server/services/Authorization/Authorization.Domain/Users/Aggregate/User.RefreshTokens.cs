using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Results;
using Authorization.Domain.Users.AggregateChanges.Tokens;
using Authorization.Domain.Users.Entities.UsersRefreshToken;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.Errors;

namespace Authorization.Domain.Users.Aggregate
{
    public partial class User
    {
        /// <summary>
        /// Створює refresh-токен користувача після перевірки
        /// доступу до облікового запису.
        ///
        /// Додає створений токен до колекції агрегату та записує
        /// зміну агрегату для його збереження.
        ///
        /// (Creates a user refresh token after checking account access.
        ///
        /// Adds the created token to the aggregate's collection
        /// and records an aggregate change for its persistence.)
        /// </summary>
        /// <param name="deviceInfo">
        /// Інформація про клієнтський пристрій.
        ///
        /// (Client-device information.)
        /// </param>
        /// <param name="ipAddress">
        /// IP-адреса клієнта.
        ///
        /// (Client IP address.)
        /// </param>
        /// <param name="now">
        /// Час створення refresh-токена.
        ///
        /// (Refresh-token creation time.)
        /// </param>
        /// <param name="city">
        /// Необов’язкова назва міста, пов’язана з токеном.
        ///
        /// (Optional city name associated with the token.)
        /// </param>
        /// <returns>
        /// Успішний результат зі створеним refresh-токеном
        /// або помилка доступу до облікового запису.
        ///
        /// (Success containing the created refresh token,
        /// or an account access error.)
        /// </returns>
        public Result<UserRefreshToken> CreateRefreshToken(
            DeviceInfo deviceInfo,
            IpAddress ipAddress,
            DateTimeOffset now,
            string? city = null)
        {
            DomainGuard.AgainstNull<User>(
                OperationType.Update,
                (deviceInfo, nameof(deviceInfo)),
                (ipAddress, nameof(ipAddress))
            );

            EnsureValidOperationTimestamp(now);

            var access = ProvideAccess(now);

            if (access.IsFailure)
            {
                return Result<UserRefreshToken>.Failure(access.Errors);
            }

            var refreshToken = UserRefreshToken.Create(
                Id,
                deviceInfo,
                ipAddress,
                now,
                city
            );

            _refreshTokens.Add(refreshToken);

            AddAggregateChange(new UserRefreshTokenCreated(
                refreshToken,
                now)
            );

            UpdatedAt = now;

            return Result<UserRefreshToken>.Success(refreshToken);
        }

        /// <summary>
        /// Позначає refresh-токен як використаний після перевірки
        /// доступу до облікового запису.
        ///
        /// Після успішної операції вилучає токен із поточної колекції
        /// агрегату та записує зміну для збереження його оновленого стану.
        ///
        /// (Marks a refresh token as consumed after checking account access.
        ///
        /// After a successful operation, removes the token from the aggregate's
        /// current collection and records a change to persist its updated state.)
        /// </summary>
        /// <remarks>
        /// Якщо операція переводить токен зі стану Active у Expired,
        /// токен вилучається з поточної колекції, а його оновлення
        /// записується навіть за невдалого результату операції.
        /// Початкова помилка прострочення повертається виклику.
        ///
        /// (If the operation transitions the token from Active to Expired,
        /// the token is removed from the current collection and its update
        /// is recorded even when the operation returns a failure.
        /// The original expiration error is returned to the caller.)
        /// </remarks>
        /// <param name="userRefreshTokenId">
        /// Ідентифікатор refresh-токена, який потрібно використати.
        ///
        /// (Identifier of the refresh token to consume.)
        /// </param>
        /// <param name="now">
        /// Час використання refresh-токена.
        ///
        /// (Refresh-token consumption time.)
        /// </param>
        /// <returns>
        /// Успіх після використання токена або помилка, якщо доступ
        /// заборонений, токен відсутній чи його неможливо використати.
        ///
        /// (Success after consuming the token, or an error if access
        /// is denied, the token is missing, or it cannot be consumed.)
        /// </returns>
        public Result ConsumeRefreshToken(
            UserRefreshTokenId userRefreshTokenId,
            DateTimeOffset now)
        {
            DomainGuard.AgainstNull<User>(
                OperationType.Update,
                (userRefreshTokenId, nameof(userRefreshTokenId))
            );

            EnsureValidOperationTimestamp(now);

            var access = ProvideAccess(now);

            if (access.IsFailure)
            {
                return Result.Failure(access.Errors);
            }

            return ProcessRefreshToken(
                userRefreshTokenId,
                now,
                (token, time) => token.Consume(time)
            );
        }

        /// <summary>
        /// Відкликає refresh-токен користувача.
        ///
        /// Відкликання допускається також для видаленого
        /// або заблокованого облікового запису.
        ///
        /// Після успішної операції вилучає токен із поточної колекції
        /// агрегату та записує зміну для збереження його оновленого стану.
        ///
        /// (Revokes a user refresh token.
        ///
        /// Revocation is also allowed for a deleted or blocked account.
        ///
        /// After a successful operation, removes the token from the aggregate's
        /// current collection and records a change to persist its updated state.)
        /// </summary>
        /// <remarks>
        /// Якщо операція переводить токен зі стану Active у Expired,
        /// токен вилучається з поточної колекції, а його оновлення
        /// записується навіть за невдалого результату операції.
        /// Початкова помилка прострочення повертається виклику.
        ///
        /// (If the operation transitions the token from Active to Expired,
        /// the token is removed from the current collection and its update
        /// is recorded even when the operation returns a failure.
        /// The original expiration error is returned to the caller.)
        /// </remarks>
        /// <param name="userRefreshTokenId">
        /// Ідентифікатор refresh-токена, який потрібно відкликати.
        ///
        /// (Identifier of the refresh token to revoke.)
        /// </param>
        /// <param name="now">
        /// Час відкликання refresh-токена.
        ///
        /// (Refresh-token revocation time.)
        /// </param>
        /// <returns>
        /// Успіх після відкликання токена або помилка,
        /// якщо токен відсутній чи його неможливо відкликати.
        ///
        /// (Success after revoking the token, or an error
        /// if the token is missing or cannot be revoked.)
        /// </returns>
        public Result RevokeRefreshToken(
            UserRefreshTokenId userRefreshTokenId,
            DateTimeOffset now)
        {
            DomainGuard.AgainstNull<User>(
                OperationType.Update,
                (userRefreshTokenId, nameof(userRefreshTokenId))
            );

            EnsureValidOperationTimestamp(now);

            return ProcessRefreshToken(
                userRefreshTokenId,
                now,
                (token, time) => token.Revoke(time)
            );
        }

        /// <summary>
        /// Завершує дію всіх refresh-токенів із поточної колекції агрегату.
        ///
        /// Для кожного токена визначається кінцевий статус: Expired,
        /// якщо строк дії вже минув, або Revoked в іншому випадку.
        /// Після переходу токен вилучається з поточної колекції,
        /// а його оновлення записується до змін агрегату.
        ///
        /// (Terminates all refresh tokens in the aggregate's current collection.
        ///
        /// Each token receives its final status: Expired if its lifetime
        /// has elapsed, or Revoked otherwise. After the transition, the token
        /// is removed from the current collection and its update is recorded
        /// among the aggregate changes.)
        /// </summary>
        /// <remarks>
        /// Поточна колекція повинна містити лише токени зі збереженим
        /// статусом Active, зокрема ті, строк дії яких уже минув.
        ///
        /// UpdatedAt оновлюється, якщо було завершено хоча б один токен.
        /// Порожня колекція не спричиняє зміни дати.
        ///
        /// Метод змінює стан агрегату в пам’яті та реєструє зміни
        /// для подальшого збереження. У разі винятку вже виконані
        /// зміни в пам’яті автоматично не відкочуються.
        ///
        /// (The current collection must contain only tokens whose stored
        /// status is Active, including tokens whose lifetime has elapsed.
        ///
        /// UpdatedAt is updated if at least one token is terminated.
        /// An empty collection does not change the timestamp.
        ///
        /// The method changes the aggregate's in-memory state and records
        /// changes for subsequent persistence. If an exception occurs,
        /// in-memory changes already applied are not automatically reverted.)
        /// </remarks>
        /// <param name="now">
        /// Єдиний час операції для завершення всіх токенів.
        ///
        /// (The operation timestamp used to terminate every token.)
        /// </param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо час операції порушує часові вимоги
        /// агрегату або передує часу створення одного з токенів.
        ///
        /// (Thrown when the operation time violates the aggregate's timestamp
        /// requirements or precedes the creation time of a token.)
        /// </exception>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо токен із поточної колекції
        /// не має статусу Active.
        ///
        /// (Thrown when a token in the current collection
        /// does not have Active status.)
        /// </exception>
        public void TerminateAllRefreshToken(DateTimeOffset now)
        {
            EnsureValidOperationTimestamp(now);

            var refreshTokens = _refreshTokens.ToArray();

            if (refreshTokens.Length == 0)
            {
                return;
            }

            foreach (var refreshToken in refreshTokens)
            {
                refreshToken.Terminate(now);

                _refreshTokens.Remove(refreshToken);

                AddAggregateChange(new UserRefreshTokenUpdated(
                    refreshToken.Id,
                    Id,
                    refreshToken.TokenStatus,
                    refreshToken.ConsumedAt,
                    refreshToken.RevokedAt,
                    now)
                );
            }

            UpdatedAt = now;
        }

        /// <summary>
        /// Знаходить токен і виконує передану операцію. У разі успіху
        /// або переходу Active у Expired вилучає токен з поточної
        /// колекції та записує оновлення. Повертає результат операції.
        ///
        /// (Finds the token and executes the supplied operation. On success
        /// or an Active-to-Expired transition, removes the token from the
        /// current collection and records its update. Returns the operation result.)
        /// </summary>
        /// <remarks>
        /// Зміни сутності, внесені переданою операцією, залишаються
        /// в пам’яті агрегату навіть у разі повернення помилки.
        ///
        /// Успішна операція або перехід зі стану Active у Expired
        /// спричиняють вилучення токена з поточної колекції
        /// та реєстрацію його оновлення. Початковий результат операції
        /// повертається виклику, зокрема помилка прострочення.
        ///
        /// Відмова без переходу Active у Expired не спричиняє
        /// вилучення токена чи реєстрації його оновлення.
        ///
        /// (Entity mutations made by the supplied operation remain
        /// in the aggregate's memory even when a failure is returned.
        ///
        /// A successful operation or an Active-to-Expired transition
        /// causes the token to be removed from the current collection
        /// and its update to be recorded. The original operation result
        /// is returned to the caller, including an expiration failure.
        ///
        /// A failure without an Active-to-Expired transition does not
        /// remove the token or record its update.)
        /// </remarks>
        /// <param name="userRefreshTokenId">
        /// Ідентифікатор refresh-токена для обробки.
        ///
        /// (Identifier of the refresh token to process.)
        /// </param>
        /// <param name="now">
        /// Час операції, попередньо перевірений на рівні агрегату.
        ///
        /// (Operation timestamp already validated at the aggregate level.)
        /// </param>
        /// <param name="tokenOperation">
        /// Операція над refresh-токеном, що повертає результат виконання.
        ///
        /// (Refresh-token operation returning its execution result.)
        /// </param>
        /// <returns>
        /// Успіх після виконання операції та реєстрації зміни агрегату;
        /// інакше — помилка відсутності токена або результат невдалої операції.
        ///
        /// (Success after executing the operation and recording the aggregate change;
        /// otherwise, a missing-token error or the failed operation result.)
        /// </returns>
        private Result ProcessRefreshToken(
            UserRefreshTokenId userRefreshTokenId,
            DateTimeOffset now,
            Func<UserRefreshToken, DateTimeOffset, Result> tokenOperation)
        {
            DomainGuard.AgainstNull<User>(
                OperationType.Update,
                (tokenOperation, nameof(tokenOperation)),
                (userRefreshTokenId, nameof(userRefreshTokenId))
            );

            var refreshToken = _refreshTokens
                .FirstOrDefault(x => x.Id == userRefreshTokenId);

            if (refreshToken is null)
            {
                return Result.Failure(UserErrors.NotFoundToken<User>());
            }

            var statusBeforeOperation = refreshToken.TokenStatus;

            var operationResult = tokenOperation(
                refreshToken,
                now
            );

            var becameExpired =
                statusBeforeOperation == TokenStatus.Active &&
                refreshToken.TokenStatus == TokenStatus.Expired;

            if (operationResult.IsSuccess || becameExpired)
            {
                _refreshTokens.Remove(refreshToken);

                AddAggregateChange(new UserRefreshTokenUpdated(
                    refreshToken.Id,
                    Id,
                    refreshToken.TokenStatus,
                    refreshToken.ConsumedAt,
                    refreshToken.RevokedAt,
                    now)
                );

                UpdatedAt = now;
            }

            return operationResult;
        }
    }
}
