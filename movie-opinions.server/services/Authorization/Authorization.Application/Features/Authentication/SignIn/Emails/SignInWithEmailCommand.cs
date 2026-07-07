using Authorization.Domain.Results;
using MediatR;

namespace Authorization.Application.Features.Authentication.SignIn.Emails
{
    public class SignInWithEmailCommand : IRequest<Result<SignInResult<Guid>>>
    {
        public string Email { get; }

        public string Password { get; }

        public SignInWithEmailCommand(string email, string password)
        {
            Email = email;
            Password = password;
        }
    }
}
