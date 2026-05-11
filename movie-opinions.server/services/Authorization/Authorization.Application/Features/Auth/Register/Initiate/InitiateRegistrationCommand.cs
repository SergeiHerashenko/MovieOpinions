using Authorization.Application.Features.Auth.Register.Initiate.Result;
using MediatR;

namespace Authorization.Application.Features.Auth.Register.Initiate
{
    public class InitiateRegistrationCommand : IRequest<InitiateRegistrationResult>
    {
        public string Login { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public bool AcceptTerms {  get; set; }

        public InitiateRegistrationCommand(string login, string password, string confirmPassword, bool acceptTerms)
        {
            Login = login;
            Password = password;
            ConfirmPassword = confirmPassword;
            AcceptTerms = acceptTerms;
        }
    }
}
