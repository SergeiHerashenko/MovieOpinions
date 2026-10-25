using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;

namespace Authorization.Domain.Common.Errors
{
    /// <summary>
    /// Описує очікувану помилку виконання доменної або прикладної операції.
    /// Використовується всередині Result і не є винятком.
    ///
    /// (Describes an expected failure of a domain or application operation.
    /// Used within Result and does not represent an exception.)
    /// </summary>
    public sealed class Error
    {
        /// <summary>
        /// Стабільний код помилки, який використовується для мапінгу,
        /// локалізації та формування відповіді.
        ///
        /// (Stable error code used for mapping, localization,
        /// and response construction.)
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Внутрішній нелокалізований опис помилки.
        /// Не повинен безпосередньо повертатися користувачеві.
        ///
        /// (Internal non-localized error description.
        /// Must not be returned directly to the client.)
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Семантична категорія очікуваної помилки.
        ///
        /// (Semantic category of the expected error.)
        /// </summary>
        public ErrorType ErrorType { get; }

        /// <summary>
        /// Створює опис очікуваної помилки з кодом, повідомленням і категорією.
        ///
        /// (Creates an expected error description with a code, message, and category.)
        /// </summary>
        /// <param name="code">
        /// Стабільний непорожній код помилки.
        /// 
        /// (A stable, non-blank error code.)
        /// </param>
        /// <param name="message">
        /// Непорожній внутрішній нелокалізований опис помилки.
        /// 
        /// (A non-blank internal, non-localized error description.)
        /// </param>
        /// <param name="errorType">
        /// Визначене значення категорії очікуваної помилки.
        /// 
        /// (A defined category of the expected error.)
        /// </param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Код або повідомлення відсутнє, є порожнім чи складається
        /// лише з пробільних символів, або категорія помилки
        /// не відповідає жодному оголошеному значенню ErrorType.
        ///
        /// (The code or message is null, empty, or whitespace-only,
        /// or the error category is not a defined ErrorType value.)
        /// </exception>
        public Error(
            string code,
            string message,
            ErrorType errorType)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw DomainDataInconsistencyException.Empty<Error>(
                    nameof(code),
                    OperationType.Create
                );
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw DomainDataInconsistencyException.Empty<Error>(
                    nameof(message),
                    OperationType.Create
                );
            }

            DomainGuard.AgainstUndefinedEnum<Error>(
                OperationType.Create,
                (errorType, nameof(errorType))
            );

            Code = code;
            Message = message;
            ErrorType = errorType;
        }
    }
}
