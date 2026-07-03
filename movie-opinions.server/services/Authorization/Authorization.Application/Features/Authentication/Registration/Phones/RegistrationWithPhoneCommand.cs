using Authorization.Domain.Results;
using MediatR;

namespace Authorization.Application.Features.Authentication.Registration.Phones
{
    public class RegistrationWithPhoneCommand : IRequest<Result<RegistrationResult>>
    {
        public string CountryCode { get; }

        public string PhoneNumber { get; }

        public string Password { get; }

        public string ConfirmPassword { get; }

        public bool AcceptTerms { get; }

        public RegistrationWithPhoneCommand(string countryCode, string phoneNumber, string password, string confirmPassword, bool acceptTerms)
        {
            CountryCode = countryCode;
            PhoneNumber = phoneNumber;
            Password = password;
            ConfirmPassword = confirmPassword;
            AcceptTerms = acceptTerms;
        }
    }
}
