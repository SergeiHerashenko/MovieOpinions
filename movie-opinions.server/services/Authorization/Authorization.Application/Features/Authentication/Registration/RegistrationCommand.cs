using Authorization.Application.Result;
using MediatR;

namespace Authorization.Application.Features.Authentication.Registration
{
    public class RegistrationCommand : IRequest<ApplicationResult<RegistrationResult>>
    {
        public string Login { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public bool AcceptTerms { get; set; }

        public RegistrationCommand(string login, string password, string confirmPassword, bool acceptTerms)
        {
            Login = login;
            Password = password;
            ConfirmPassword = confirmPassword;
            AcceptTerms = acceptTerms;
        }
    }
}
