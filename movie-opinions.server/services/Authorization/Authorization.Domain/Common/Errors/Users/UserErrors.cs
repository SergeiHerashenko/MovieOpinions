namespace Authorization.Domain.Common.Errors.Users
{
    public static class UserErrors
    {
        public static class IdentifierError
        {
            public static Error EmptyIdentifier<TValue>()
                => new(DomainErrorCodes.EmailErrorCode.EmptyEmail,
                       $"Identifier cannot be empty. Owner: {typeof(TValue).Name}!",
                       ErrorType.EmptyValue
                );
        }

        public static class EmailError
        {
            public static Error EmptyEmail<TValue>()
                => new(DomainErrorCodes.EmailErrorCode.EmptyEmail,
                       $"Email cannot be empty. Owner: {typeof(TValue).Name}!",
                       ErrorType.EmptyValue
                );

            public static Error InvalidFormat<TValue>()
                => new(DomainErrorCodes.EmailErrorCode.InvalidFormatEmail,
                       $"Invalid email format. Owner: {typeof(TValue).Name}!",
                       ErrorType.Validation
                );

            public static Error TooLong<TValue>()
                => new(DomainErrorCodes.EmailErrorCode.TooLongEmail,
                       $"Email is too long. Owner: {typeof(TValue).Name}!",
                       ErrorType.Validation
                );

            public static Error NotAllowed<TValue>()
                => new(DomainErrorCodes.EmailErrorCode.NotAllowedEmail,
                       $"This email is not allowed. Owner: {typeof(TValue).Name}!",
                       ErrorType.PolicyViolation
                );
        }

        public static class PhoneError
        {
            public static Error EmptyPhone<TValue>()
                => new(DomainErrorCodes.PhoneErrorCode.EmptyPhone,
                       $"Phone cannot be empty. Owner: {typeof(TValue).Name}!",
                       ErrorType.EmptyValue
                );

            public static Error EmptyCountryCode<TValue>()
                => new(DomainErrorCodes.PhoneErrorCode.EmptyContryCode,
                       $"Country code cannot be empty. Owner: {typeof(TValue).Name}!",
                       ErrorType.EmptyValue
                );

            public static Error PhoneInvalidFormat<TValue>()
                => new(DomainErrorCodes.PhoneErrorCode.InvalidFormatPhone,
                       $"Invalid phone format. Owner: {typeof(TValue).Name}!",
                       ErrorType.Validation
                );

            public static Error CountryCodeInvalidFormat<TValue>()
                => new(DomainErrorCodes.PhoneErrorCode.InvalidFormatCountryCode,
                       $"Invalid country code format. Owner: {typeof(TValue).Name}!",
                       ErrorType.Validation
                );

            public static Error TooLong<TValue>()
                => new(DomainErrorCodes.PhoneErrorCode.TooLongPhone,
                       $"Phone is too long. Owner: {typeof(TValue).Name}!",
                       ErrorType.Validation
                );

            public static Error TooShort<TValue>()
                => new(DomainErrorCodes.PhoneErrorCode.TooShortPhone,
                       $"Phone is too short. Owner: {typeof(TValue).Name}!",
                       ErrorType.Validation
                );
        }

        public static class PasswordError
        {
            public static Error EmptyPassword<TValue>()
                => new(DomainErrorCodes.PasswordErrorCode.EmptyPassword,
                       $"Password cannot be empty. Owner: {typeof(TValue).Name}!",
                       ErrorType.EmptyValue
                );
        }

        public static class LoginError
        {
            public static Error EmptyLogin<TValue>()
                => new(DomainErrorCodes.LoginErrorCode.EmptyLogin,
                       $"Login cannot be empty. Owner: {typeof(TValue).Name}!",
                       ErrorType.EmptyValue
                );
        }

        public static class AccessError
        {
            public static Error UserIsBlocked<TValue>()
                => new(DomainErrorCodes.AccessErrorCode.AccountBlocked,
                       $"Operation not allowed for blocked user. Owner: {typeof(TValue).Name}!",
                       ErrorType.Forbidden
                );

            public static Error UserIsDeleted<TValue>()
                => new(DomainErrorCodes.AccessErrorCode.AccountDeleted,
                       $"Operation not allowed for deleted user. Owner: {typeof(TValue).Name}!",
                       ErrorType.Forbidden
                );

            public static Error RestoreIsNotAllowed<TValue>(DateTimeOffset dateDeleted)
                => new(DomainErrorCodes.AccessErrorCode.RestoreIsNotAllowed,
                       $"Account deleted permanently. Date deleted: {dateDeleted}. Owner: {typeof(TValue).Name}",
                       ErrorType.Forbidden
                );
        }

        public static class GeneralError
        {
            public static Error OperationIsNotAllowed<TValue>(string message)
                => new(DomainErrorCodes.GeneralErrorCode.OperationIsNotAllowed,
                       $"{message}. Owner: {typeof(TValue).Name}!",
                       ErrorType.Forbidden
                );

            public static Error NoChangesDetected<TValue>(string newValue, string nameField)
                => new(DomainErrorCodes.GeneralErrorCode.NoChangesDetected,
                       $"'{typeof(TValue).Name}': The new value '{newValue}' for field '{nameField}' must be different from the current one!",
                       ErrorType.Forbidden
                );

            public static Error AlreadyConfirmed<TValue>()
                => new(DomainErrorCodes.GeneralErrorCode.AlreadyConfirmed,
                       $"'{typeof(TValue).Name}': This confirmation has already been applied!",
                       ErrorType.Conflict
                );

            public static Error AlreadyRestored<TValue>(string stateName)
                => new(DomainErrorCodes.GeneralErrorCode.AlreadyRestored,
                       $"'{typeof(TValue).Name}': The value is already in state '{stateName}'",
                       ErrorType.Conflict
                );
        }
    }
}
