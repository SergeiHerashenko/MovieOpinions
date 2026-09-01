using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;

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

        public Error(
            string code, 
            string message, 
            ErrorType errorType)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw DomainDataInconsistencyException.Empty<Error>(
                    nameof(code),
                    OperationType.Create
                );

            if (string.IsNullOrWhiteSpace(message))
                throw DomainDataInconsistencyException.Empty<Error>(
                    nameof(message),
                    OperationType.Create
                );

            if (!Enum.IsDefined(typeof(ErrorType), errorType))
                throw DomainDataInconsistencyException.UnsupportedDiscriminator<Error>(
                    nameof(errorType),
                    errorType,
                    OperationType.Create
                );

            Code = code;
            Message = message;
            ErrorType = errorType;
        }
    }
}
