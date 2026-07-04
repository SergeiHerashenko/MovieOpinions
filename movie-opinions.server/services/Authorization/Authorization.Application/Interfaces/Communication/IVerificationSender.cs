using Authorization.Application.DTOs.Communication;
using Authorization.Domain.Results;

namespace Authorization.Application.Interfaces.Communication
{
    public interface IVerificationSender
    {
        Task<Result> VerifyCodeAsync(VerificationCommand verificationCommand);
    }
}
