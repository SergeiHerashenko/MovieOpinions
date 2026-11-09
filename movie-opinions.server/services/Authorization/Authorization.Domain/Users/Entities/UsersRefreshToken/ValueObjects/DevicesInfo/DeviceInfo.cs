using Authorization.Domain.Common.Exceptions.DomainException;
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
        #region Properties
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
        #endregion

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
        /// <param name="deviceType">
        /// Категорія пристрою.
        ///
        /// (The device category.)
        /// </param>
        /// <param name="operatingSystem">
        /// Назва або опис операційної системи.
        ///
        /// (The operating system name or description.)
        /// </param>
        /// <param name="browser">
        /// Назва або опис браузера.
        ///
        /// (The browser name or description.)
        /// </param>
        /// <param name="deviceModel">
        /// Назва або опис моделі пристрою.
        ///
        /// (The device model name or description.)
        /// </param>
        /// <returns>
        /// Успішний результат із DeviceInfo або першу виявлену
        /// помилку валідації.
        ///
        /// (A successful result containing DeviceInfo, or a failed result
        /// containing the first detected validation error.)
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил валідації.
        ///
        /// (Thrown when an internal precondition or the required
        /// execution order of validation rules is violated.)
        /// </exception>
        public static Result<DeviceInfo> Create(
            DeviceType deviceType,
            string operatingSystem,
            string browser,
            string deviceModel)
        {
            var failure = DeviceInfoValidator.ValidateForError(
                OperationType.Create,
                deviceType,
                operatingSystem,
                browser,
                deviceModel
            );

            if (failure is not null)
            {
                return Result<DeviceInfo>.Failure(failure.Value);
            }

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
        /// <param name="deviceType">
        /// Збережена категорія пристрою.
        ///
        /// (The persisted device category.)
        /// </param>
        /// <param name="operatingSystem">
        /// Збережений опис операційної системи.
        ///
        /// (The persisted operating system description.)
        /// </param>
        /// <param name="browser">
        /// Збережений опис браузера.
        ///
        /// (The persisted browser description.)
        /// </param>
        /// <param name="deviceModel">
        /// Збережений опис моделі пристрою.
        ///
        /// (The persisted device model description.)
        /// </param>
        /// <returns>
        /// Відновлений DeviceInfo.
        ///
        /// (The restored DeviceInfo.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо збережені значення порушують правила DeviceInfo.
        ///
        /// (Thrown when the persisted values violate the DeviceInfo validation rules.)
        /// </exception>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил валідації.
        ///
        /// (Thrown when an internal precondition or the required
        /// execution order of validation rules is violated.)
        /// </exception>
        public static DeviceInfo Restore(
            DeviceType deviceType,
            string operatingSystem,
            string browser,
            string deviceModel)
        {
            var failure = DeviceInfoValidator.ValidateForException(
                OperationType.Restore,
                deviceType,
                operatingSystem,
                browser,
                deviceModel
            );

            if (failure is not null)
            {
                throw failure.Value;
            }

            return new(
                deviceType,
                operatingSystem,
                browser,
                deviceModel
            );
        }
        #endregion

        /// <inheritdoc/>
        public override IEnumerable<object?> GetEqualityComponents()
        {
            yield return DeviceType;
            yield return OperatingSystem;
            yield return Browser;
            yield return DeviceModel;
        }
    }
}
