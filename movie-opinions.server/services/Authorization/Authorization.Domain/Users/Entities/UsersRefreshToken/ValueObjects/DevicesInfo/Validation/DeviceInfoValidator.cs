using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Users.Entities.UsersRefreshToken.Enums;

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
        /// <param name="deviceType">Визначений тип пристрою.</param>
        /// <param name="operatingSystem">Назва операційної системи.</param>
        /// <param name="browser">Назва браузера.</param>
        /// <param name="deviceModel">Назва або модель пристрою.</param>
        /// <returns>
        /// Обгортка з доменною помилкою, якщо правило порушено;
        /// інакше null.
        /// </returns>
        internal static ValidationRulesFailure<Error>? ValidateForError(
            DeviceType deviceType,
            string? operatingSystem,
            string? browser,
            string? deviceModel)
        {
            var failure = Validate(
                deviceType,
                operatingSystem,
                browser,
                deviceModel
            );

            if (failure is null)
                return null;

            return new ValidationRulesFailure<Error>(failure.Error);
        }

        /// <summary>
        /// Перевіряє дані DeviceInfo та створює відповідний доменний виняток.
        /// Метод лише повертає об’єкт винятку й самостійно його не кидає.
        ///
        /// (Validates DeviceInfo data and creates the corresponding domain
        /// exception. The method only returns the exception object and does
        /// not throw it.)
        /// </summary>
        /// <param name="deviceType">Відновлений тип пристрою.</param>
        /// <param name="operatingSystem">Відновлена назва операційної системи.</param>
        /// <param name="browser">Відновлена назва браузера.</param>
        /// <param name="deviceModel">Відновлена назва або модель пристрою.</param>
        /// <param name="operationType">
        /// Операція, для якої створюється діагностичний виняток.
        /// </param>
        /// <returns>
        /// Обгортка зі створеним винятком, якщо правило порушено;
        /// інакше null.
        /// </returns>
        internal static ValidationRulesFailure<Exception>? ValidateForException(
            DeviceType deviceType,
            string? operatingSystem,
            string? browser,
            string? deviceModel,
            OperationType operationType)
        {
            var failure = Validate(
                deviceType,
                operatingSystem,
                browser,
                deviceModel
            );

            if (failure is null)
                return null;

            var exception = failure.BuildException(operationType);

            return new ValidationRulesFailure<Exception>(exception);
        }

        private static ValidationFailure? Validate(
            DeviceType deviceType,
            string? operatingSystem,
            string? browser,
            string? deviceModel)
        {
            var data = new DeviceInfoValidationData()
            {
                DeviceType = deviceType,
                OperatingSystem = operatingSystem,
                Browser = browser,
                DeviceModel = deviceModel
            };

            return _validation.Validate(data);
        }
    }
}
