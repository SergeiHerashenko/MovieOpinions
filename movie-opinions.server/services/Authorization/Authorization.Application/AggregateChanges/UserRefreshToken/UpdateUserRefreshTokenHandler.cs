using Authorization.Application.Abstractions.Persistence;
using Authorization.Application.Common.AggregateChanges;
using Authorization.Domain.Users.AggregateChanges.Tokens;
using MediatR;

namespace Authorization.Application.AggregateChanges.UserRefreshToken
{
    public sealed class UpdateUserRefreshTokenHandler : IRequestHandler<AggregateChangeRequest<UserRefreshTokenUpdated>>
    {
        private readonly IUserRefreshTokenRepository _userRefreshTokenRepository;

        public UpdateUserRefreshTokenHandler(IUserRefreshTokenRepository userRefreshTokenRepository)
        {
            _userRefreshTokenRepository = userRefreshTokenRepository;
        }

        public async Task Handle(
            AggregateChangeRequest<UserRefreshTokenUpdated> change, 
            CancellationToken cancellationToken = default)
        {
            await _userRefreshTokenRepository.UpdateStatusRefreshTokenAsync(change.AggregateChange, cancellationToken);
        }
    }
}
