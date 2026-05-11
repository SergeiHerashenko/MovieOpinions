using Authorization.Application.Features.Auth.Register.Initiate.Enum;

namespace Authorization.Application.Features.Auth.Register.Initiate.Result
{
    public class InitiateRegistrationResult
    {
        public required InitiateRegistrationNextStep NestStep { get; set; }

        public required string Message { get; set; }
    }
}
