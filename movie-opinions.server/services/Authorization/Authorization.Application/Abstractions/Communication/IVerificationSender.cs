using Authorization.Application.DTOs.Communication;
using Authorization.Domain.Results;

namespace Authorization.Application.Abstractions.Communication
{
    public interface IVerificationSender
    {
        Task<Result> VerifyCodeAsync<TId>(VerificationRequest<TId> verificationCommand);
    }
}
