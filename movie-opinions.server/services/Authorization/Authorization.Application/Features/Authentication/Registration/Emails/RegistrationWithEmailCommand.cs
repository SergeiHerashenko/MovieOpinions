using Authorization.Application.Result;
using MediatR;

namespace Authorization.Application.Features.Authentication.Registration.Emails
{
    public class RegistrationWithEmailCommand : IRequest<ApplicationResult<RegistrationResult>>
    {
        public string Email { get; }

        public string Password { get; }

        public string ConfirmPassword { get; }

        public bool AcceptTerms { get; }

        public RegistrationWithEmailCommand(string email, string password, string confirmPassword, bool acceptTerms)
        {
            Email = email;
            Password = password;
            ConfirmPassword = confirmPassword;
            AcceptTerms = acceptTerms;
        }
    }
}
