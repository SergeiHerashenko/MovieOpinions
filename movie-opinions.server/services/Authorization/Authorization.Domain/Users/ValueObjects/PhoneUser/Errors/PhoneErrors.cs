using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Domain.Users.ValueObjects.PhoneUser.Errors
{
    /// <summary>
    /// Містить фабрики очікуваних доменних помилок,
    /// пов’язаних із повним телефонним номером.
    ///
    /// (Provides factories for expected domain errors
    /// related to a complete telephone number.)
    /// </summary>
    public static class PhoneErrors
    {
        public static Error EmptyPhone<TType>()
            => new(
                DomainErrorCodes.Phone.EmptyPhone,
                $"Phone number is empty or incomplete. " +
                $"Owner: '{typeof(TType).Name}'.",
                ErrorType.Validation
            );

        public static Error NotAllowedPhone<TType>()
            => new(
                DomainErrorCodes.Phone.NotAllowedPhone,
                $"Phone number is not allowed by the current account policy. " +
                $"Owner: '{typeof(TType).Name}'.",
                ErrorType.BusinessRule
            );
    }
}
