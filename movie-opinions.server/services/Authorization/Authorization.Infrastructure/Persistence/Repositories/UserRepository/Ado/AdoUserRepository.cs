using Authorization.Application.Abstractions.Persistence;
using Authorization.Domain.Users;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Infrastructure.Persistence.Repositories.UserRepository.Ado
{
    internal sealed class AdoUserRepository : IUserRepository
    {
        private readonly AdoUserQueryRepository _adoUserQueryRepository;
        private readonly AdoUserCommandRepository _adoUserCommandRepository;

        public AdoUserRepository(
            AdoUserQueryRepository adoUserQueryRepository,
            AdoUserCommandRepository adoUserCommandRepository)
        {
            _adoUserQueryRepository = adoUserQueryRepository;
            _adoUserCommandRepository = adoUserCommandRepository;
        }

        public Task CreateUserAsync(User entity, CancellationToken cancellationToken = default)
            => _adoUserCommandRepository.CreateUserAsync(entity, cancellationToken);

        public Task UpdateUserAsync(User entity, CancellationToken cancellationToken = default)
            => _adoUserCommandRepository.UpdateUserAsync(entity, cancellationToken);

        public Task<User?> GetUserByIdAsync(UserId userId, CancellationToken cancellationToken = default)
            => _adoUserQueryRepository.GetUserByIdAsync(userId, cancellationToken);

        public Task<User?> GetUserByLoginAsync(Login login, CancellationToken cancellationToken = default)
            => _adoUserQueryRepository.GetUserByLoginAsync(login, cancellationToken);

        public Task<bool> ExistsUserByLoginAsync(Login login, CancellationToken cancellationToken = default)
            => _adoUserQueryRepository.ExistsUserByLoginAsync(login, cancellationToken);
    }
}
