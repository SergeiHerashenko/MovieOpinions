using Authorization.Domain.Common.Errors.Enums;
using Authorization.Domain.Users.Entities.UsersPendingAction.Enums;

namespace Authorization.Domain.Common.Errors.Common
{
    public static class CommonErrors
    {
        public static class Identifier
        {
            public static Error EmptyIdentifier<TValue>(string fieldName)
                => new(DomainErrorCodes.Identifier.Empty,
                       $"The identifier '{fieldName}' for '{typeof(TValue).Name}' is empty or missing!",
                       ErrorType.EmptyValue
                );

            public static Error IdentifierMismatch<TValue>(string fieldName)
                => new(DomainErrorCodes.Identifier.IdentifierMismatch,
                       $"The passed identifier '{fieldName}' does not match the current value in the system. Owner: {typeof(TValue).Name}",
                       ErrorType.Conflict
                );
        }

        public static class Unsupported
        {
            public static Error UnsupportedType<TValue>(string unsupportedValue)
                => new(DomainErrorCodes.General.UnsupportedType,
                       $"The type '{unsupportedValue}' is not supported for '{typeof(TValue).Name}'!",
                       ErrorType.UnsupportedType
                );
        }
    }
}
