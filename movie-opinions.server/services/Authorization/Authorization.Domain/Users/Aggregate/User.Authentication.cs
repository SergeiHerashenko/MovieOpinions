using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Contracts;
using Authorization.Domain.Users.DomainEvents;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.IpAddresses;
using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.Policies;

namespace Authorization.Domain.Users.Aggregate
{
    public partial class User
    {
        /// <summary>
        /// Фіксує невдалу спробу перевірки пароля,
        /// якщо користувачу дозволений доступ.
        ///
        /// Якщо наступна спроба досягає встановленого порогу,
        /// застосовує системний бан і після його успішного застосування
        /// скидає лічильник невдалих спроб. Інакше збільшує лічильник.
        ///
        /// Після успішної операції оновлює час зміни користувача.
        ///
        /// (Records a failed password verification attempt
        /// if the user is allowed access.
        ///
        /// If the next attempt reaches the configured threshold,
        /// applies a system ban and resets the failed-attempt counter
        /// after the ban is successfully applied. Otherwise, increments the counter.
        ///
        /// Updates the user modification time after a successful operation.)
        /// </summary>
        /// <param name="now">
        /// Час невдалої спроби перевірки пароля.
        /// (Time of the failed password verification attempt.)
        /// </param>
        /// <returns>
        /// Успіх після оновлення лічильника або застосування бану;
        /// інакше — помилка доступу чи застосування обмеження.
        /// (Success after updating the counter or applying the ban;
        /// otherwise, an access or restriction application error.)
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
        /// Фіксує успішний вхід користувача після перевірки доступу.
        ///
        /// Якщо поєднання інформації про пристрій та IP-адреси
        /// відсутнє серед refresh-токенів агрегату, додає подію
        /// входу з пристрою, який система визначила як новий.
        ///
        /// Скидає лічильник невдалих спроб перевірки пароля
        /// та оновлює час останнього входу й зміни користувача.
        ///
        /// (Records a successful user sign-in after checking access.
        ///
        /// If the device-information and IP-address combination
        /// is absent from the aggregate's refresh tokens, records
        /// a sign-in event for a device identified by the system as new.
        ///
        /// Resets the failed password verification counter
        /// and updates the last sign-in and user modification times.)
        /// </summary>
        /// <remarks>
        /// Метод викликається після успішної перевірки облікових даних
        /// і до додавання refresh-токена поточного входу до агрегату.
        ///
        /// (Call this method after successfully verifying the credentials
        /// and before adding the refresh token for the current sign-in
        /// to the aggregate.)
        /// </remarks>
        /// <param name="deviceInfo">
        /// Інформація про пристрій, з якого виконано вхід.
        /// (Information about the device used to sign in.)
        /// </param>
        /// <param name="ipAddress">
        /// IP-адреса клієнта під час входу.
        /// (Client IP address at sign-in.)
        /// </param>
        /// <param name="now">
        /// Час успішного входу користувача.
        /// (Time of the successful user sign-in.)
        /// </param>
        /// <returns>
        /// Успіх після фіксації входу або помилка доступу.
        /// (Success after recording the sign-in, or an access error.)
        /// </returns>
        public Result LoginSuccess(
            DeviceInfo deviceInfo,
            IpAddress ipAddress,
            DateTimeOffset now)
        {
            DomainGuard.AgainstNull<User>(
                OperationType.Update,
                (deviceInfo, nameof(deviceInfo)),
                (ipAddress, nameof(ipAddress))
            );

            EnsureValidOperationTimestamp(now);

            var access = ProvideAccess(now);

            if (access.IsFailure)
                return access;

            var isNewDevice = IsNewDevice(
                deviceInfo,
                ipAddress
            );

            if (isNewDevice)
            {
                AddDomainEvent(new UserSignedInFromNewDeviceEvent(
                    Id,
                    Login,
                    deviceInfo,
                    ipAddress,
                    now)
                );
            }

            FailedPasswordAttempts = 0;
            LastLoginAt = now;
            UpdatedAt = now;

            return Result.Success();
        }

        /// <summary>
        /// Формує дані системного бану за надмірну кількість
        /// невдалих спроб перевірки пароля.
        ///
        /// Назва, тривалість, причина та ініціатор обмеження
        /// визначаються політикою <see cref="RestrictionPolicy"/>.
        ///
        /// (Builds system-ban data for excessive failed
        /// password verification attempts.
        ///
        /// The restriction name, duration, reason, and issuer
        /// are defined by <see cref="RestrictionPolicy"/>.)
        /// </summary>
        /// <returns>
        /// Успішний результат із даними бану
        /// або помилка валідації правила обмеження.
        /// (Success containing the ban data,
        /// or a restriction-rule validation error.)
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
        /// Формує дані системного бану та додає відповідне
        /// обмеження до агрегату користувача.
        ///
        /// Після успішного додавання обмеження скидає лічильник
        /// невдалих спроб перевірки пароля та оновлює час зміни користувача.
        ///
        /// (Builds system-ban data and adds the corresponding
        /// restriction to the user aggregate.
        ///
        /// After successfully adding the restriction, resets the failed
        /// password verification counter and updates the user modification time.)
        /// </summary>
        /// <param name="now">
        /// Час застосування системного бану.
        /// (Time the system ban is applied.)
        /// </param>
        /// <returns>
        /// Успіх після застосування бану
        /// або помилка формування чи додавання обмеження.
        /// (Success after applying the ban,
        /// or an error while building or adding the restriction.)
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

        /// <summary>
        /// Визначає, чи є поєднання інформації про пристрій
        /// та IP-адреси новим для refresh-токенів, наявних в агрегаті.
        ///
        /// (Determines whether the device-information and IP-address
        /// combination is new relative to the refresh tokens held by the aggregate.)
        /// </summary>
        /// <param name="deviceInfo">
        /// Інформація про пристрій, яку потрібно порівняти.
        /// (Device information to compare.)
        /// </param>
        /// <param name="ipAddress">
        /// IP-адреса, яку потрібно порівняти.
        /// (IP address to compare.)
        /// </param>
        /// <returns>
        /// <c>true</c>, якщо жоден refresh-токен агрегату не містить
        /// такого самого поєднання інформації про пристрій та IP-адреси;
        /// інакше — <c>false</c>.
        /// (<c>true</c> if no refresh token in the aggregate contains
        /// the same device-information and IP-address combination;
        /// otherwise, <c>false</c>.)
        /// </returns>
        private bool IsNewDevice(
            DeviceInfo deviceInfo,
            IpAddress ipAddress)
        {
            return !_refreshTokens.Any(x =>
                x.DeviceInfo == deviceInfo &&
                x.IpAddress == ipAddress
            );
        }
    }
}
