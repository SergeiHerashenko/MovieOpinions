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
                $"This description of the reason for deletion is too long. Owner: {typeof(TType).Name}!",
                ErrorType.Validation
            );

        public static Error AlreadyRestored<TType>()
            => new(
                DomainErrorCodes.UserDeletion.UserAlreadyRestored,
                $"User is already restored. Owner: {typeof(TType).Name}!",
                ErrorType.Conflict
            );

        public static Error RestorationPeriodExpired<TType>()
            => new(
                DomainErrorCodes.UserDeletion.RestorationPeriodExpired,
                $"The time period allowed for account restoration has expired. Owner: {typeof(TType).Name}!",
                ErrorType.Forbidden
            );
    }
}
