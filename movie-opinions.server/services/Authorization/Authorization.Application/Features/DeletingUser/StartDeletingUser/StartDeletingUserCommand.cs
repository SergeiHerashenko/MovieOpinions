using Authorization.Domain.Results;
using MediatR;

namespace Authorization.Application.Features.DeletingUser.StartDeletingUser
{
    public class StartDeletingUserCommand : IRequest<Result<StartDeletingUserResult>>
    {
        public string Password { get; } 

        public string? Reason { get; }

        public StartDeletingUserCommand(string password, string? reason)
        {
            Password = password;
            Reason = reason;
        }
    }
}
