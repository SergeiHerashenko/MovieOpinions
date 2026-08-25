using Authorization.Application.Abstractions.Mapping;
using Authorization.Application.Common.Enums;
using Authorization.Application.Common.Exceptions;
using Authorization.Application.DTOs.Communication.Notifications.Enums;
using Authorization.Application.DTOs.Communication.Verification.Enums;
using Authorization.Application.Features.Services.UserActionConfirmation.Mappers.Models;
using Authorization.Domain.Users.Entities.UsersPendingAction.Action;

namespace Authorization.Application.Features.Services.UserActionConfirmation.Mappers
{
    public sealed class UserActionConfirmationMapping : IUserActionConfirmationMapping
    {
        public ActionNotificationConfig GetNotificationConfiguration<TAction>()
            where TAction : UserAction
        {
            return typeof(TAction) switch
            {
                var t when t == typeof(PasswordChangeAction) 
                    => new(RateLimitAction.PasswordChange, NotificationType.PendingChangePassword),

                var t when t == typeof(LoginChangeAction) 
                    => new(RateLimitAction.LoginChange, NotificationType.PendingChangeLogin),

                var t when t == typeof(DeleteAccountAction) 
                    => new(RateLimitAction.SendChangeDeletionConfirmation, NotificationType.PendingDeletingUser),

                _ => throw ApplicationInvalidOperationException.UnsupportedValue<UserActionConfirmationMapping>(typeof(TAction).Name)
            };
        }

        public ActionVerificationConfig GetVerificationConfiguration<TAction>()
            where TAction : UserAction
        {
            return typeof(TAction) switch
            {
                var t when t == typeof(PasswordChangeAction) 
                    => new(RateLimitAction.PasswordChange, VerificationType.ChangePassword),

                var t when t == typeof(LoginChangeAction) 
                    => new(RateLimitAction.LoginChange, VerificationType.ChangeLogin),

                var t when t == typeof(DeleteAccountAction)
                    => new(RateLimitAction.ConfirmationDeletingUser, VerificationType.DeletionUser),

                _ => throw ApplicationInvalidOperationException.UnsupportedValue<UserActionConfirmationMapping>(typeof(TAction).Name)
            };
        }
    }
}
