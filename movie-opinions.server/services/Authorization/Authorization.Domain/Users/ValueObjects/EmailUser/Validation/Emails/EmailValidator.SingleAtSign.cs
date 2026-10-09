using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Validation;
using Authorization.Domain.Common.Validation.Enums;
using Authorization.Domain.Users.ValueObjects.EmailUser.Errors;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Validation.Emails
{
    internal static partial class EmailValidator
    {
        /// <summary>
        /// Перевіряє, що email-адреса містить рівно один символ '@',
        /// а локальна та доменна частини не є порожніми.
        ///
        /// Успішне виконання правила встановлює передумову
        /// для безпечного розділення адреси методом Split.
        ///
        /// Правило вимагає, щоб перевірка наявності значення
        /// правилом <see cref="RequiredRule"/> уже була успішною.
        ///
        /// (Validates that an email address contains exactly one '@'
        /// character and that both local and domain parts are non-empty.
        ///
        /// Successful validation establishes the prerequisite
        /// for safely splitting the address with the Split method.
        ///
        /// This rule requires successful presence validation by
        /// <see cref="RequiredRule"/>.)
        /// </summary>
        private sealed class SingleAtSignEmailRule : IValidationRule<EmailValidationData, ValidationFailure>
        {
            public ValidationPriority Priority => ValidationPriority.Format;

            public ValidationFailure? Validate(EmailValidationData data)
            {
                if (string.IsNullOrWhiteSpace(data.Value))
                {
                    throw DomainInvalidOperationException.PreconditionFailed<Email>(
                        nameof(SingleAtSignEmailRule),
                        nameof(RequiredRule),
                        data.OperationType,
                        context: new Dictionary<string, object>
                        {
                            ["FieldName"] = nameof(Email)
                        }
                    );
                }

                var first = data.Value.IndexOf('@');
                var last = data.Value.LastIndexOf('@');

                if (first <= 0 || first != last || first == data.Value.Length - 1)
                {
                    return new ValidationFailure()
                    {
                        Error = EmailErrors.InvalidFormatEmail<Email>(),
                        BuildException = operationType =>
                            DomainDataInconsistencyException.InvalidFieldFormat<Email>(
                                nameof(Email),
                                data.Value,
                                operationType
                            )
                    };
                }

                return null;
            }
        }
    }
}
