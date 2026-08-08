using Authorization.Domain.Results;
using MediatR;

namespace Authorization.Application.Features.SignIn.Phones
{
    public class SignInWithPhoneCommand : IRequest<Result<SignInResult<Guid>>>
    {
        public string CountryCode { get; }

        public string PhoneNumber { get; }

        public string Password { get; }

        public SignInWithPhoneCommand(string countryCode, string phoneNumber, string password)
        {
            CountryCode = countryCode;
            PhoneNumber = phoneNumber;
            Password = password;
        }
    }
}
