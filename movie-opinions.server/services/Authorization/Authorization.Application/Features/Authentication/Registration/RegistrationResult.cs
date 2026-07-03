using Authorization.Application.Features.Authentication.Registration.Enums;

namespace Authorization.Application.Features.Authentication.Registration
{
    public class RegistrationResult
    {
        public RegistrationNextStep NextStep { get; private set; }

        public string RegistrationToken { get; private set; }

        public string Message { get; private set; }

        private RegistrationResult(RegistrationNextStep registrationNextStep, string registrationToken, string message)
        {
            NextStep = registrationNextStep;
            Message = message;
            RegistrationToken = registrationToken;
        }

        public static RegistrationResult Success(RegistrationNextStep registrationNextStep, string registrationToken, string message) 
            => new(registrationNextStep, registrationToken, message);
    }
}
