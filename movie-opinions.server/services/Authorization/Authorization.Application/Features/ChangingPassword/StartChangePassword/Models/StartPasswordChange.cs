using Authorization.Domain.Users;
using Authorization.Domain.Users.Entities.UsersPendingAction;

namespace Authorization.Application.Features.ChangingPassword.StartChangePassword.Models
{
    internal sealed class StartPasswordChange
    {
        public User User { get; }

        public UserPendingAction Action { get; }

        public StartPasswordChange(
            User user, 
            UserPendingAction action)
        {
            User = user;
            Action = action;
        }
    }
}
