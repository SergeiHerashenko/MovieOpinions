using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Common.Validation.Interfaces;
using Authorization.Domain.Common.ValueObjects.PhoneUser.Errors;
using Authorization.Domain.Common.ValueObjects.PhoneUser.Validation.Phones.Catalog;

namespace Authorization.Domain.Common.ValueObjects.PhoneUser.Validation.Phones
{
    internal static partial class PhoneValidator
    {
        /// <summary>
        /// Перевіряє відповідність міжнародного коду країни
        /// та довжини національної частини записам каталогу
        /// <see cref="PhoneNumberingPlanCatalog"/>.
        ///
        /// Правило використовується під час створення та відновлення
        /// телефонного номера і вимагає успішного виконання
        /// <see cref="RequiredRule"/>.
        ///
        /// (Validates the country calling code and national number length
        /// against the entries in <see cref="PhoneNumberingPlanCatalog"/>.
        ///
        /// This rule is used during telephone-number creation and restoration
        /// and requires successful validation by <see cref="RequiredRule"/>.)
        /// </summary>
        private sealed class NumberingPlanPhoneRule : IValidationRule<PhoneValidationData, ValidationFailure>
        {
            /// <inheritdoc/>
            public ValidationPriority Priority => ValidationPriority.BusinessRule;

            /// <summary>
            /// Перевіряє поєднання коду країни та довжини національного номера.
            ///
            /// У разі невідповідності каталогу повертає опис порушення
            /// з очікуваною доменною помилкою та фабрикою відповідного винятку.
            /// Конкретна причина відмови включається у внутрішнє повідомлення
            /// помилки та діагностичний контекст винятку.
            ///
            /// (Validates the combination of a country calling code
            /// and national number length.
            ///
            /// If the combination does not match the catalog, returns a violation
            /// containing an expected domain error and a corresponding exception
            /// factory. The specific failure reason is included in the internal
            /// error message and the exception's diagnostic context.)
            /// </summary>
            /// <param name="data">
            /// Канонічні складові телефонного номера та контекст операції.
            ///
            /// (The canonical telephone-number components and operation context.)
            /// </param>
            /// <returns>
            /// Опис порушення або null, якщо поєднання відповідає каталогу.
            ///
            /// (A violation description, or null if the combination matches the catalog.)
            /// </returns>
            /// <exception cref="DomainInvalidOperationException">
            /// Код країни або національна частина відсутні, що порушує
            /// передумову успішного виконання RequiredRule.
            ///
            /// (The country calling code or national number is missing,
            /// violating the prerequisite established by RequiredRule.)
            /// </exception>
            /// <exception cref="DomainInvariantViolationException">
            /// Для переданого коду знайдено записи каталогу,
            /// але їхній об’єднаний набір допустимих довжин порожній.
            ///
            /// (Catalog entries exist for the supplied calling code,
            /// but their combined set of allowed lengths is empty.)
            /// </exception>
            public ValidationFailure? Validate(PhoneValidationData data)
            {
                if (string.IsNullOrWhiteSpace(data.NationalNumber) || string.IsNullOrWhiteSpace(data.CountryCode))
                {
                    throw DomainInvalidOperationException.PreconditionFailed<Phone>(
                        nameof(NumberingPlanPhoneRule),
                        nameof(RequiredRule),
                        data.OperationType,
                        context: new Dictionary<string, object>
                        {
                            ["FieldName"] = nameof(Phone)
                        }
                    );
                }

                var checkedResult = PhoneNumberingPlanCatalog.CheckNumberingPlan(
                    data.CountryCode,
                    data.NationalNumber.Length
                );

                if (checkedResult.IsSuccess)
                {
                    return null;
                }

                var reason = checkedResult.Status.ToString();

                var error = PhoneErrors.InvalidNumberingPlan<Phone>(reason);

                return new ValidationFailure
                {
                    Error = error,
                    BuildException = operationType =>
                        DomainDataInconsistencyException.InvalidFieldFormat<Phone>(
                            fieldName: nameof(Phone),
                            value: data.CountryCode +
                            data.NationalNumber,
                            operationType: operationType,
                            message: error.Message,
                            context: new Dictionary<string, object>
                            {
                                ["CountryCode"] = data.CountryCode,
                                ["NationalNumberLength"] = data.NationalNumber.Length,
                                ["NumberingPlanStatus"] = reason,
                                ["RuleName"] = nameof(NumberingPlanPhoneRule)
                            }
                        )
                };
            }
        }
    }
}
