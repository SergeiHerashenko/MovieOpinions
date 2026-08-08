using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Domain.Common.Errors.Users
{
    public static class EmailErrors
    {
        public static Error EmptyEmail<TValue>()
            => new(DomainErrorCodes.Email.EmptyEmail,
                   $"The email address for '{typeof(TValue).Name}' is empty or missing!",
                   ErrorType.EmptyValue
            );

        public static Error InvalidFormatEmail<TValue>(string email)
            => new(DomainErrorCodes.Email.InvalidFormatEmail,
                   $"The email address '{email}' is not valid. Owner {typeof(TValue).Name}!",
                   ErrorType.Validation
            );

        public static Error EmptyDomainPart<TValue>()
            => new(DomainErrorCodes.Email.EmptyEmailDomain,
                   $"The email domain for '{typeof(TValue).Name}' is empty or missing!",
                   ErrorType.EmptyValue
            );

        public static Error NotAllowedDomainPart<TValue>(string emailDomain)
            => new(DomainErrorCodes.Email.NotAllowedEmailDomain,
                   $"Email domain {emailDomain} is not allowed. Owner {typeof(TValue).Name}!",
                   ErrorType.PolicyViolation
            );

        public static Error InvalidFormatDomainPart<TValue>(string emailDomain)
            => new(DomainErrorCodes.Email.InvalidFormatEmailDomainPart,
                   $"The domain of the email address '{emailDomain}' is not valid. Owner {typeof(TValue).Name}!",
                   ErrorType.Validation
            );

        public static Error TooLongDomainPart<TValue>(string emailDomain)
            => new(DomainErrorCodes.Email.TooLongEmailDomainPart,
                   $"The domain part length of the email '{emailDomain}' is too long. Owner {typeof(TValue).Name}!",
                   ErrorType.OutOfRange
            );

        public static Error TooShortDomainPart<TValue>(string emailDomain)
            => new(DomainErrorCodes.Email.TooShortEmailDomainPart,
                   $"The domain part length of the email '{emailDomain}' is too short. Owner {typeof(TValue).Name}!",
                   ErrorType.OutOfRange
            );

        public static Error EmptyLocalPart<TValue>()
            => new(DomainErrorCodes.Email.EmptyEmailLocalPart,
                   $"The email local part for '{typeof(TValue).Name}' is empty or missing!",
                   ErrorType.EmptyValue
            );

        public static Error InvalidFormatLocalPart<TValue>(string emailLocalPart)
            => new(DomainErrorCodes.Email.InvalidFormatEmailLocalPart,
                   $"The local part of the email address '{emailLocalPart}' is not valid. Owner {typeof(TValue).Name}!",
                   ErrorType.Validation
            );

        public static Error TooLongLocalPart<TValue>(string emailLocalPart)
            => new(DomainErrorCodes.Email.TooLongEmailLocalPart,
                   $"The local part length of the email '{emailLocalPart}' is too long. Owner {typeof(TValue).Name}!",
                   ErrorType.OutOfRange
            );

        public static Error TooShortLocalPart<TValue>(string emailLocalPart)
            => new(DomainErrorCodes.Email.TooShortEmailLocalPart,
                   $"The local part length of the email '{emailLocalPart}' is too short. Owner {typeof(TValue).Name}!",
                   ErrorType.OutOfRange
            );
    }
}
