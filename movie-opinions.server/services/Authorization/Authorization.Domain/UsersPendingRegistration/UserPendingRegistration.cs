using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions;
using Authorization.Domain.Common.Models;
using Authorization.Domain.DomainEvents.UserPendingRegistration;
using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.UsersPendingRegistration.ValueObjects;

namespace Authorization.Domain.UsersPendingRegistration
{
    public class UserPendingRegistration : AggregateRoot<UserPendingRegistrationId, Guid>
    {
        private static readonly TimeSpan ExpirationTime = TimeSpan.FromHours(12);

        public Login Login { get; private set; }

        public Password Password { get; private set; }

        public DateTimeOffset ExpiresAt { get; private set; }

        #region Creation
        private UserPendingRegistration(UserPendingRegistrationId userPendingRegistrationId, Login login, Password password)
            : base(userPendingRegistrationId)
        {
            Login = login;
            Password = password;
            ExpiresAt = CreatedAt.Add(ExpirationTime);
        }

        public static Result<UserPendingRegistration> Create(Login login, Password password)
        {
            if(login is null)
                return Result<UserPendingRegistration>.Failure(UserError.Empty(nameof(login)));

            if(password is null)
                return Result<UserPendingRegistration>.Failure(UserError.Empty(nameof(password)));
            
            var userRendingRegistration = new UserPendingRegistration(UserPendingRegistrationId.CreateUnique(), login, password);

            userRendingRegistration.AddDomainEvent(
                new UserPendingRegistrationRequestedEvent(
                    userRendingRegistration.Id,
                    userRendingRegistration.Login,
                    userRendingRegistration.CreatedAt
                )
            );

            return Result<UserPendingRegistration>.Success(userRendingRegistration);
        }
        #endregion

        #region Restore
        private UserPendingRegistration(
            UserPendingRegistrationId userPendingRegistrationId,
            Login login,
            Password password,
            DateTimeOffset createdAt,
            DateTimeOffset expiresAt)
            : base(userPendingRegistrationId, createdAt)
        {
            Login = login;
            Password = password;
            ExpiresAt = expiresAt;
        }

        public static UserPendingRegistration Restore(
            UserPendingRegistrationId userPendingRegistrationId,
            Login login,
            Password password,
            DateTimeOffset createdAt,
            DateTimeOffset expiresAt)
        {
            if (login is null)
                throw DomainDataInconsistencyException.EmptyOnRestore(
                    $"Missing required field {nameof(login)} during {nameof(UserPendingRegistration)} entity reconstruction!",
                    new Dictionary<string, object>
                    {
                        ["entity"] = nameof(UserPendingRegistration),
                        ["entityId"] = userPendingRegistrationId.Value,
                        ["field"] = nameof(login),
                        ["operation"] = "restore",
                    }
                );

            if(password is null)
                throw DomainDataInconsistencyException.EmptyOnRestore(
                    $"Missing required field {nameof(password)} during {nameof(UserPendingRegistration)} entity reconstruction!",
                    new Dictionary<string, object>
                    {
                        ["entity"] = nameof(UserPendingRegistration),
                        ["entityId"] = userPendingRegistrationId.Value,
                        ["field"] = nameof(password),
                        ["operation"] = "restore",
                    }
                );

            return new UserPendingRegistration(userPendingRegistrationId, login, password, createdAt, expiresAt);
        }
        #endregion

        #region Behavior
        public Result Refresh(Password password, DateTimeOffset now)
        {
            if (password is null)
                return Result.Failure(UserError.Empty(nameof(password)));

            Password = password;
            ExpiresAt = now.Add(ExpirationTime);

            return Result.Success();
        }

        public bool IsExpired(DateTimeOffset now)
            => now > ExpiresAt;
        #endregion
    }
}
