using Authorization.Application.Features.Auth.Register.Initiate.Enum;
using Authorization.Domain.ValueObjects.Login;

namespace Authorization.Application.Interfaces.Services
{
    public interface IRegistrationNextStepResolver
    {
        InitiateRegistrationNextStep Resolve(Login login);
    }
}
