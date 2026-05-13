using Authorization.Application.Errors;
using Authorization.Application.Exceptions;
using Authorization.Application.Features.Auth.Register.Initiate.Enum;
using Authorization.Application.Interfaces.Services;
using Authorization.Domain.Enums;
using Authorization.Domain.ValueObjects.Login;

namespace Authorization.Application.Features.Auth.Register.Initiate
{
    public class RegistrationNextStepResolver : IRegistrationNextStepResolver
    {
        public InitiateRegistrationNextStep Resolve(Login login)
        {
            return login.Type switch
            {
                LoginType.Email => InitiateRegistrationNextStep.EmailConfirmation,
                LoginType.Phone => InitiateRegistrationNextStep.SmsConfirmation,
                _ => throw new UnknowOperationException(
                    ApplicationErrorCodes.UnknowType.UnknownTypeStep,
                    "Unknown next registration step type"
                )
            };
        }
    }
}
