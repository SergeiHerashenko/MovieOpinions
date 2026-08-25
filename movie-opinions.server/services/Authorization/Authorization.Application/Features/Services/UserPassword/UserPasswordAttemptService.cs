using Authorization.Application.Abstractions.AggregateChanges;
using Authorization.Application.Abstractions.Clock;
using Authorization.Application.Abstractions.Events;
using Authorization.Application.Abstractions.Persistence;
using Authorization.Application.Abstractions.Security.Hashers;
using Authorization.Application.Abstractions.Services.UserPassword;
using Authorization.Application.Common.Errors.Users;
using Authorization.Domain.Results;
using Authorization.Domain.Users;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.Users.ValueObjects.PasswordUser;

namespace Authorization.Application.Features.Services.UserPassword
{
    public sealed class UserPasswordAttemptService : IUserPasswordAttemptService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAggregateChangesDispatcher _aggregateChangesDispatcher;
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IClock _clock;

        public UserPasswordAttemptService(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IAggregateChangesDispatcher aggregateChangesDispatcher,
            IDomainEventDispatcher domainEventDispatcher,
            IPasswordHasher passwordHasher,
            IClock clock)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _aggregateChangesDispatcher = aggregateChangesDispatcher;
            _domainEventDispatcher = domainEventDispatcher;
            _passwordHasher = passwordHasher;
            _clock = clock;
        }

        public Task<Result<User>> ProcessAsync(
            UserId userId, 
            string rawPassword, 
            CancellationToken cancellationToken = default)
        {
            return ProcessCoreAsync(
                ct => _userRepository.GetUserByIdAsync(userId, ct), 
                userId.Value.ToString(), 
                rawPassword,
                cancellationToken
            );
        }

        public Task<Result<User>> ProcessAsync(
            Login login, 
            string rawPassword, 
            CancellationToken cancellationToken = default)
        {
            return ProcessCoreAsync(
                ct => _userRepository.GetUserByLoginAsync(login, ct), 
                login.Value,
                rawPassword,
                cancellationToken
            );
        }

        private async Task<Result<User>> ProcessCoreAsync(
            Func<CancellationToken, Task<User?>> loadUser,
            string identifier,
            string rawPassword,
            CancellationToken cancellationToken = default)
        {
            var plainPasswordResult = PlainPassword.Create(rawPassword);

            if (plainPasswordResult.IsFailure)
                return Result<User>.Failure(plainPasswordResult.Errors);

            var user = await loadUser(cancellationToken);

            if(user is null)
            {
                _passwordHasher.FakeVerifyPassword(plainPasswordResult.Value);

                return Result<User>.Failure(UserErrors.NotFound<UserPasswordAttemptService>(identifier));
            }

            var passwordIsValid = _passwordHasher.VerifyPassword(plainPasswordResult.Value, user.Password.Value);

            if (passwordIsValid)
                return Result<User>.Success(user);

            var failedAttemptResult = await _unitOfWork.ExecuteAsync(async ct =>
            {
                var currentUser = await _userRepository.GetUserByIdForUpdateAsync(user.Id, ct);

                if (currentUser is null)
                    return Result<User>.Failure(UserErrors.NotFound<UserPasswordAttemptService>(identifier));

                var attemptResult = currentUser.RecordFailedPasswordAttempt(_clock.UtcNow);

                if (attemptResult.IsFailure)
                    return Result<User>.Failure(attemptResult.Errors);

                await _userRepository.UpdateUserAsync(currentUser, ct);

                await _aggregateChangesDispatcher.DispatchAsync(currentUser.AggregateChanges, ct);

                currentUser.ClearAggregateChanges();

                return Result<User>.Success(currentUser);
            }, cancellationToken);

            if (failedAttemptResult.IsFailure)
                return Result<User>.Failure(failedAttemptResult.Errors);

            var updatedUser = failedAttemptResult.Value;

            await _domainEventDispatcher.DispatchAsync(updatedUser.DomainEvents, cancellationToken);

            updatedUser.ClearDomainEvents();

            return Result<User>.Failure(UserErrors.InvalidPassword<UserPasswordAttemptService>(identifier));
        }
    }
}
