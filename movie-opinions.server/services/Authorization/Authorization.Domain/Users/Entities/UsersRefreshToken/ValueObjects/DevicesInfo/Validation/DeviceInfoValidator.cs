using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Users.Entities.UsersRefreshToken.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Errors;

namespace Authorization.Domain.Users.Entities.UsersRefreshToken.ValueObjects.DevicesInfo.Validation
{
    /// <summary>
    /// Координує повний набір правил валідації DeviceInfo
    /// та формує потрібне представлення виявленої помилки.
    ///
    /// (Coordinates the complete DeviceInfo validation rule set
    /// and produces the required representation of a detected failure.)
    /// </summary>
    internal static partial class DeviceInfoValidator
    {
        /// <summary>
        /// Містить упорядкований набір правил перевірки обов’язкових
        /// текстових полів, типу пристрою та довжини значень.
        ///
        /// (Contains the ordered rule set for validating required
        /// text fields, the device type, and value lengths.)
        /// </summary>
        private static readonly ValidationOrchestrator<
            DeviceInfoValidationData,
            ValidationFailure> _validation = new(
                [
                    new RequiredFieldsRule(),
                    new ValidDeviceTypeRule(),
                    new FieldLengthsRule()
                ]
            );

        /// <summary>
        /// Перевіряє дані DeviceInfo та повертає очікувану доменну помилку.
        /// Використовується під час обробки зовнішніх вхідних даних.
        ///
        /// (Validates DeviceInfo data and returns an expected domain error.
        /// Used when processing external input.)
        /// </summary>
        /// <param name="operationType">
        /// Операція, під час якої виконується валідація.
        ///
        /// (The operation during which validation is performed.)
        /// </param>
        /// <param name="deviceType">
        /// Визначений тип пристрою.
        ///
        /// (The detected device type.)
        /// </param>
        /// <param name="operatingSystem">
        /// Назва операційної системи.
        ///
        /// (The name of the operating system.)
        /// </param>
        /// <param name="browser">
        /// Назва браузера.
        ///
        /// (The browser name.)
        /// </param>
        /// <param name="deviceModel">
        /// Назва або модель пристрою.
        ///
        /// (The device name or model.)
        /// </param>
        /// <returns>
        /// Обгортка з першою виявленою доменною помилкою
        /// або null, якщо всі правила виконані.
        ///
        /// (A wrapper containing the first detected domain error,
        /// or null when all rules pass.)
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил валідації.
        ///
        /// (Thrown when an internal precondition or the required
        /// execution order of validation rules is violated.)
        /// </exception>
        internal static ValidationRulesFailure<Error>? ValidateForError(
            OperationType operationType,
            DeviceType deviceType,
            string operatingSystem,
            string browser,
            string deviceModel)
        {
            var data = BuildData(
                operationType,
                deviceType,
                operatingSystem,
                browser,
                deviceModel
            );

            return DomainValidationExecutor.ValidateForError(
                _validation,
                data
            );
        }

        /// <summary>
        /// Перевіряє дані DeviceInfo та для звичайного порушення створює
        /// відповідний об’єкт доменного винятку, не кидаючи його.
        ///
        /// Винятки, спричинені порушенням внутрішніх передумов правил,
        /// передаються безпосередньо виклику.
        ///
        /// (Validates DeviceInfo data and creates the corresponding domain
        /// exception object for a regular validation failure without throwing it.
        ///
        /// Exceptions caused by violated internal rule preconditions
        /// propagate directly to the caller.)
        /// </summary>
        /// <param name="operationType">
        /// Операція, під час якої виконується валідація.
        ///
        /// (The operation during which validation is performed.)
        /// </param>
        /// <param name="deviceType">
        /// Відновлений тип пристрою.
        ///
        /// (The persisted device type.)
        /// </param>
        /// <param name="operatingSystem">
        /// Відновлена назва операційної системи.
        ///
        /// (The persisted operating system name.)
        /// </param>
        /// <param name="browser">
        /// Відновлена назва браузера.
        ///
        /// (The persisted browser name.)
        /// </param>
        /// <param name="deviceModel">
        /// Відновлена назва або модель пристрою.
        ///
        /// (The persisted device name or model.)
        /// </param>
        /// <returns>
        /// Обгортка зі створеним винятком, якщо правило порушено;
        /// інакше null.
        ///
        /// (A wrapper containing the exception constructed for the
        /// first detected failure, or null when all rules pass.)
        /// </returns>
        /// <exception cref="DomainInvalidOperationException">
        /// Виникає, якщо порушено внутрішню передумову
        /// або порядок виконання правил валідації.
        ///
        /// (Thrown when an internal precondition or the required
        /// execution order of validation rules is violated.)
        /// </exception>
        internal static ValidationRulesFailure<Exception>? ValidateForException(
            OperationType operationType,
            DeviceType deviceType,
            string operatingSystem,
            string browser,
            string deviceModel)
        {
            var data = BuildData(
                operationType,
                deviceType,
                operatingSystem,
                browser,
                deviceModel
            );

            return DomainValidationExecutor.ValidateForException(
                _validation,
                data
            );
        }

        private static DeviceInfoValidationData BuildData(
            OperationType operationType,
            DeviceType deviceType,
            string operatingSystem,
            string browser,
            string deviceModel)
        {
            return new DeviceInfoValidationData()
            {
                DeviceType = deviceType,
                OperatingSystem = operatingSystem,
                Browser = browser,
                DeviceModel = deviceModel,
                OperationType = operationType
            };
        }
    }
}
