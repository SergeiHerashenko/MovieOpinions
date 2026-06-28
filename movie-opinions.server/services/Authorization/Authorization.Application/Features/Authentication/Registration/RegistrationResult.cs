using Authorization.Application.Features.Authentication.Registration.Enums;

namespace Authorization.Application.Features.Authentication.Registration
{
    public class RegistrationResult
    {
        public RegistrationNextStep NextStep { get; private set; }

        public string Message { get; private set; } = string.Empty;

        private RegistrationResult(RegistrationNextStep registrationNextStep, string message)
        {
            NextStep = registrationNextStep;
            Message = message;
        }

        public static RegistrationResult Success(RegistrationNextStep registrationNextStep, string message) 
            => new(registrationNextStep, message);
    }
}
