using Authorization.Domain.Users.Entities.UsersPendingAction;

namespace Authorization.Domain.Users
{
    public partial class User
    {
        private void SetPendingAction(UserPendingAction pendingAction)
        {
            _action = pendingAction;
        }
    }
}
