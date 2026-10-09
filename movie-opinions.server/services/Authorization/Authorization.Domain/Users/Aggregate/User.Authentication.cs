using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Contracts;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.Policies;

namespace Authorization.Domain.Users
{
    public partial class User
    {
        /// <summary>
        /// Фіксує невдалу спробу введення пароля користувачем.
        ///
        /// Перед зміною стану перевіряє допустимість операції
        /// та обчислює майбутню кількість невдалих спроб.
        /// Якщо встановлений поріг досягнуто, додає користувачу
        /// обмеження типу Ban і після успішного додавання
        /// скидає лічильник спроб.
        ///
        /// Якщо підготовка або додавання обмеження завершується
        /// помилкою, стан користувача залишається незмінним.
        ///
        /// (Records a failed password attempt for the user.
        ///
        /// Before changing state, validates that the operation is allowed
        /// and calculates the resulting failed-attempt count.
        /// When the configured threshold is reached, adds a Ban restriction
        /// and resets the counter after the restriction is successfully added.
        ///
        /// If restriction preparation or addition fails,
        /// the user's state remains unchanged.)
        /// </summary>
        /// <param name="now">Час, у який зафіксовано невдалу спробу введення пароля.</param>
        /// <returns>
        /// Успіх після запису спроби або створення бану;
        /// помилка, якщо доступ до операції заборонений
        /// чи обмеження не вдалося підготувати або додати.
        /// </returns>
        public Result RecordFailedPasswordAttempt(DateTimeOffset now)
        {
            EnsureValidOperationTimestamp(now);

            var access = ProvideAccess(now);

            if (access.IsFailure)
                return access;

            var nextFailedPasswordAttempts = checked(FailedPasswordAttempts + 1);

            if (nextFailedPasswordAttempts >= MAX_FAILED_ATTEMPTS)
            {
                return ApplyFailedPasswordAttemptBan(now);
            }

            FailedPasswordAttempts = nextFailedPasswordAttempts;
            UpdatedAt = now;

            return Result.Success();
        }

        /// <summary>
        /// Фіксує успішний вхід користувача.
        ///
        /// Перед зміною стану перевіряє часову послідовність
        /// і можливість надання доступу. Після успішної перевірки
        /// скидає лічильник невдалих спроб введення пароля,
        /// записує час останнього входу та оновлення користувача.
        ///
        /// Якщо доступ заборонено, стан користувача
        /// залишається незмінним.
        ///
        /// (Records a successful user login.
        ///
        /// Before changing state, validates chronological consistency
        /// and verifies that access can be granted. After successful
        /// validation, resets the failed-password-attempt counter
        /// and records the user's last login and update timestamps.
        ///
        /// If access is denied, the user's state remains unchanged.)
        /// </summary>
        /// <param name="now">Час успішного входу користувача.</param>
        /// <returns>
        /// Успіх після запису входу або помилка,
        /// якщо користувачу не може бути надано доступ.
        /// </returns>
        public Result LoginSuccess(DateTimeOffset now)
        {
            EnsureValidOperationTimestamp(now);

            var access = ProvideAccess(now);

            if (access.IsFailure)
                return access;

            FailedPasswordAttempts = 0;
            LastLoginAt = now;
            UpdatedAt = now;

            return Result.Success();
        }

        /// <summary>
        /// Формує дані автоматичного бану, який застосовується
        /// після досягнення граничної кількості невдалих
        /// спроб введення пароля.
        ///
        /// Створює правило обмеження на основі доменної політики
        /// без зміни стану користувача.
        ///
        /// (Builds the automatic ban data applied after reaching
        /// the failed-password-attempt threshold.
        ///
        /// Creates the restriction rule from the domain policy
        /// without modifying the user's state.)
        /// </summary>
        /// <returns>
        /// Дані для створення обмеження типу Ban або помилка,
        /// якщо правило обмеження не вдалося створити.
        /// </returns>
        private static Result<RestrictionData> CreateFailedPasswordAttemptBanData()
        {
            var ruleResult = RestrictionRule.Create(
                RestrictionPolicy.FailedLoginBanName,
                RestrictionPolicy.FailedLoginBanDurationMinutes
            );

            if (ruleResult.IsFailure)
                return Result<RestrictionData>.Failure(ruleResult.Errors);

            return Result<RestrictionData>.Success(new RestrictionData
            {
                RestrictionType = RestrictionType.Ban,
                RestrictionRule = ruleResult.Value,
                Reason = RestrictionPolicy.FailedLoginBanReason,
                ImposedBy = RestrictionPolicy.FailedLoginBanRestrictedBy
            });
        }

        /// <summary>
        /// Перевіряє часову послідовність authentication-операції.
        ///
        /// Час операції не може передувати часу створення користувача,
        /// його останнього оновлення або останнього успішного входу.
        ///
        /// Метод не змінює стан агрегату.
        ///
        /// (Validates the chronological consistency of an authentication
        /// operation.
        ///
        /// The operation timestamp cannot precede the user's creation,
        /// last update, or last successful login timestamp.
        ///
        /// The method does not modify aggregate state.)
        /// </summary>
        /// <param name="now">Час операції автентифікації, який необхідно перевірити.</param>
        private void EnsureValidOperationTimestamp(DateTimeOffset now)
        {
            DomainGuard.AgainstEarlierThan<User>(
                OperationType.Update,
                (now, nameof(now)),
                (CreatedAt, nameof(CreatedAt))
            );

            if (UpdatedAt is not null)
            {
                DomainGuard.AgainstEarlierThan<User>(
                    OperationType.Update,
                    (now, nameof(now)),
                    (UpdatedAt.Value, nameof(UpdatedAt))
                );
            }

            if (LastLoginAt is not null)
            {
                DomainGuard.AgainstEarlierThan<User>(
                    OperationType.Update,
                    (now, nameof(now)),
                    (LastLoginAt.Value, nameof(LastLoginAt))
                );
            }
        }

        /// <summary>
        /// Готує та застосовує автоматичний бан після досягнення
        /// граничної кількості невдалих спроб введення пароля.
        ///
        /// Після успішного додавання обмеження скидає лічильник
        /// невдалих спроб і оновлює час зміни користувача.
        ///
        /// Якщо правило або обмеження не вдалося створити,
        /// стан користувача залишається незмінним.
        ///
        /// (Prepares and applies the automatic ban after reaching
        /// the failed-password-attempt threshold.
        ///
        /// After successfully adding the restriction, resets the failed-attempt
        /// counter and updates the user's modification timestamp.
        ///
        /// If the rule or restriction cannot be created,
        /// the user's state remains unchanged.)
        /// </summary>
        /// <param name="now">Час застосування автоматичного бану.</param>
        /// <returns>
        /// Успіх після додавання бану або помилка,
        /// отримана під час підготовки чи додавання обмеження.
        /// </returns>
        private Result ApplyFailedPasswordAttemptBan(DateTimeOffset now)
        {
            var restrictionData = CreateFailedPasswordAttemptBanData();

            if (restrictionData.IsFailure)
                return restrictionData;

            var addResult = AddRestrictions(
                new[] { restrictionData.Value },
                now
            );

            if (addResult.IsFailure)
                return addResult;

            FailedPasswordAttempts = 0;
            UpdatedAt = now;

            return Result.Success();
        }
    }
}
