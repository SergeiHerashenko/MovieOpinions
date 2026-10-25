using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Errors.Enums;

namespace Authorization.Domain.Users.Entities.UsersDeletion.Errors
{
    /// <summary>
    /// Містить очікувані помилки операцій, пов’язаних із видаленням користувача.
    ///
    /// (Contains expected failures related to user deletion operations.)
    /// </summary>
    public static class UserDeletionErrors
    {
        public static Error TooLongReason<TType>()
            => new(
                DomainErrorCodes.UserDeletion.TooLongReason,
                $"Deletion reason exceeds the maximum allowed length. " +
                $"Owner: '{typeof(TType).Name}'.",
                ErrorType.Validation
            );

        public static Error AlreadyRestored<TType>()
            => new(
                DomainErrorCodes.UserDeletion.UserAlreadyRestored,
                $"User account has already been restored. " +
                $"Owner: '{typeof(TType).Name}'.",
                ErrorType.Conflict
            );

        public static Error RestorationPeriodExpired<TType>()
            => new(
                DomainErrorCodes.UserDeletion.RestorationPeriodExpired,
                $"The account restoration period has expired. " +
                $"Owner: '{typeof(TType).Name}'.",
                ErrorType.Forbidden
            );
    }
}
