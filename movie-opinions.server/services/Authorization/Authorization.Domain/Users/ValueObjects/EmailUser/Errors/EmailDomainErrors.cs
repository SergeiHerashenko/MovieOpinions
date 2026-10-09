using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Domain.Users.ValueObjects.EmailUser.Errors
{
    /// <summary>
    /// Містить очікувані доменні помилки, пов’язані
    /// з перевіркою доменної частини email-адреси.
    ///
    /// (Contains expected domain errors related
    /// to email domain-part validation.)
    /// </summary>
    public static class EmailDomainErrors
    {
        public static Error EmptyEmailDomain<TType>()
            => new(
                DomainErrorCodes.EmailDomain.EmptyDomainPart,
                $"Email domain is empty or missing. Owner: '{typeof(TType).Name}'!",
                ErrorType.Validation
            );

        public static Error NotAllowedEmailDomain<TType>()
            => new(
                DomainErrorCodes.EmailDomain.NotAllowedDomainPart,
                $"Email domain is not allowed. Owner: '{typeof(TType).Name}'!",
                ErrorType.BusinessRule
            );

        public static Error InvalidFormatEmailDomain<TType>()
            => new(
                DomainErrorCodes.EmailDomain.InvalidFormatEmailDomain,
                $"Email domain has an invalid format. Owner: '{typeof(TType).Name}'!",
                ErrorType.Validation
            );

        public static Error TooLongEmailDomain<TType>(
            int actualLength,
            int maximumLength)
            => new(
                DomainErrorCodes.EmailDomain.TooLongEmailDomain,
                $"Email domain length '{actualLength}' exceeds the maximum allowed " +
                $"length of '{maximumLength}' characters. Owner: '{typeof(TType).Name}'!",
                ErrorType.Validation
            );

        public static Error TooShortEmailDomain<TType>(
            int actualLength,
            int minimumLength)
            => new(
                DomainErrorCodes.EmailDomain.TooShortEmailDomain,
                $"Email domain length '{actualLength}' is below the minimum required " +
                $"length of '{minimumLength}' characters. Owner: '{typeof(TType).Name}'!",
                ErrorType.Validation
            );
    }
}
