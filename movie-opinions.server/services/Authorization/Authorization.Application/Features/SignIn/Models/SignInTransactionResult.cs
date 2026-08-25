using Authorization.Application.Common.Security.Models;
using Authorization.Domain.Users;

namespace Authorization.Application.Features.SignIn.Models
{
    public sealed class SignInTransactionResult
    {
        public User User { get; }

        public TokenResponse TokenResponse { get; }
        
        public SignInTransactionResult(
            User user, 
            TokenResponse tokenResponse)
        {
            User = user;
            TokenResponse = tokenResponse;
        }
    }
}
