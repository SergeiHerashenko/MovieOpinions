using Authorization.Domain.Results;
using MediatR;

namespace Authorization.Application.Features.ChangingPassword.StartChangePassword
{
    public sealed class StartChangePasswordCommand 
        : IRequest<Result<StartChangePasswordResult>>
    {
        public string CurrentPassword { get; }

        public string NewPassword { get; }

        public StartChangePasswordCommand(
            string currentPassword, 
            string newPassword)
        {
            CurrentPassword = currentPassword;
            NewPassword = newPassword;
        }
    }
}
