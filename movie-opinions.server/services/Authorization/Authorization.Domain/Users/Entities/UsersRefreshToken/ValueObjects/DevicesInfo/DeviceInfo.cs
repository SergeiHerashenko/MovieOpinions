using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Entities.UsersRefreshToken.Enums;
using Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo.Validation;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo
{
    /// <summary>
    /// Представляє незмінний зліпок інформації про клієнтський пристрій,
    /// пов’язаний із сесією refresh token.
    /// Дані мають діагностичний характер і не є надійним
    /// ідентифікатором фізичного пристрою.
    ///
    /// (Represents an immutable snapshot of client-device information
    /// associated with a refresh-token session.
    /// This data is diagnostic and does not constitute a trusted
    /// physical-device identifier.)
    /// </summary>
    public sealed class DeviceInfo : ValueObject
    {
        /// <summary>
        /// Категорія клієнтського пристрою.
        ///
        /// (Client device category.)
        /// </summary>
        public DeviceType DeviceType { get; }

        /// <summary>
        /// Назва або опис операційної системи.
        ///
        /// (Operating-system name or description.)
        /// </summary>
        public string OperatingSystem { get; }

        /// <summary>
        /// Назва або опис браузера.
        ///
        /// (Browser name or description.)
        /// </summary>
        public string Browser { get; }

        /// <summary>
        /// Назва або опис моделі пристрою.
        ///
        /// (Device-model name or description.)
        /// </summary>
        public string DeviceModel { get; }

        private DeviceInfo(
            DeviceType deviceType,
            string operatingSystem,
            string browser,
            string deviceModel)
        {
            DeviceType = deviceType;
            OperatingSystem = operatingSystem;
            Browser = browser;
            DeviceModel = deviceModel;
        }

        #region Creation
        /// <summary>
        /// Перевіряє отриману інформацію про пристрій
        /// та створює її доменне представлення.
        ///
        /// (Validates the supplied device information
        /// and creates its domain representation.)
        /// </summary>
        /// <param name="deviceType">Категорія пристрою.</param>
        /// <param name="operatingSystem">Назва або опис операційної системи.</param>
        /// <param name="browser">Назва або опис браузера.</param>
        /// <param name="deviceModel">Назва або опис моделі пристрою.</param>
        /// <returns>
        /// Успішний результат із DeviceInfo або першу виявлену помилку валідації.
        /// </returns>
        public static Result<DeviceInfo> Create(
            DeviceType deviceType,
            string operatingSystem,
            string browser,
            string deviceModel)
        {
            var failure = DeviceInfoValidator.ValidateForError(
                deviceType,
                operatingSystem,
                browser,
                deviceModel
            );

            if (failure is not null)
                return Result<DeviceInfo>.Failure(failure.Value);

            return Result<DeviceInfo>.Success(
                new DeviceInfo(
                    deviceType,
                    operatingSystem,
                    browser,
                    deviceModel
                )
            );
        }
        #endregion

        #region Restoration
        /// <summary>
        /// Відновлює DeviceInfo зі збережених значень
        /// та перевіряє їх внутрішню консистентність.
        ///
        /// (Restores DeviceInfo from persisted values
        /// and validates their internal consistency.)
        /// </summary>
        /// <param name="deviceType">Збережена категорія пристрою.</param>
        /// <param name="operatingSystem">Збережений опис операційної системи.</param>
        /// <param name="browser">Збережений опис браузера.</param>
        /// <param name="deviceModel">Збережений опис моделі пристрою.</param>
        /// <returns>Відновлений DeviceInfo.</returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережені значення порушують правила DeviceInfo.
        /// </exception>
        public static DeviceInfo Restore(
            DeviceType deviceType,
            string operatingSystem,
            string browser,
            string deviceModel)
        {
            var failure = DeviceInfoValidator.ValidateForException(
                deviceType,
                operatingSystem,
                browser,
                deviceModel,
                OperationType.Restore
            );

            if (failure is not null)
                throw failure.Value;

            return new(deviceType, operatingSystem, browser, deviceModel);
        }
        #endregion

        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return DeviceType;
            yield return OperatingSystem;
            yield return Browser;
            yield return DeviceModel;
        }
    }
}
