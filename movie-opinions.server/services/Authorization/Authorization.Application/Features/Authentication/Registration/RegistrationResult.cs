using Authorization.Application.Features.Authentication.Registration.Enums;

namespace Authorization.Application.Features.Authentication.Registration
{
    public class RegistrationResult
    {
        public RegistrationNextStep NextStep { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
