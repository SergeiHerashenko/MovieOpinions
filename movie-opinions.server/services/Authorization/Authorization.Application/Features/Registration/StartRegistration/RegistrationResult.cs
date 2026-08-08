using Authorization.Application.Features.Registration.StartRegistration.Enums;

namespace Authorization.Application.Features.Registration.StartRegistration
{
    public class RegistrationResult
    {
        public RegistrationNextStep NextStep { get; private set; }

        public string RegistrationFlowToken { get; private set; }

        public string Message { get; private set; }

        private RegistrationResult(RegistrationNextStep registrationNextStep, string registrationFlowToken, string message)
        {
            NextStep = registrationNextStep;
            Message = message;
            RegistrationFlowToken = registrationFlowToken;
        }

        public static RegistrationResult Create(RegistrationNextStep registrationNextStep, string registrationFlowToken, string message)
            => new(registrationNextStep, registrationFlowToken, message);
    }
}
