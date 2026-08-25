using Authorization.Domain.Users;
using Authorization.Domain.Users.Entities.UsersPendingAction;

namespace Authorization.Application.Features.DeletingUser.StartDeletingUser.Models
{
    internal sealed class StartedDeletion
    {
        public User User { get; }

        public UserPendingAction Action { get; }

        public StartedDeletion(
            User user, 
            UserPendingAction action)
        {
            User = user;
            Action = action;
        }
    }
}
