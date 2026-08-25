using Authorization.Application.Features.Services.UserActionConfirmation.Mappers.Models;
using Authorization.Domain.Users.Entities.UsersPendingAction.Action;

namespace Authorization.Application.Abstractions.Mapping
{
    public interface IUserActionConfirmationMapping
    {
        ActionNotificationConfig GetNotificationConfiguration<TAction>()
            where TAction : UserAction;

        ActionVerificationConfig GetVerificationConfiguration<TAction>()
            where TAction : UserAction;
    }
}
