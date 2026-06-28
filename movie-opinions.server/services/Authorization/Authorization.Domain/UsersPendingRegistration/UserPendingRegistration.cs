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

        public static DomainResult<UserPendingRegistration> Create(Login login, Password password)
        {
            if(login is null)
                return DomainResult<UserPendingRegistration>.Failure(UserError.Empty(nameof(login), nameof(UserPendingRegistration)));

            if(password is null)
                return DomainResult<UserPendingRegistration>.Failure(UserError.Empty(nameof(password), nameof(UserPendingRegistration)));
            
            var userRendingRegistration = new UserPendingRegistration(UserPendingRegistrationId.CreateUnique(), login, password);

            userRendingRegistration.AddDomainEvent(
                new UserPendingRegistrationRequestedEvent(
                    userRendingRegistration.Id,
                    userRendingRegistration.Login,
                    userRendingRegistration.CreatedAt
                )
            );

            return DomainResult<UserPendingRegistration>.Success(userRendingRegistration);
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
                throw DomainDataInconsistencyException.EmptyOnRestore<UserPendingRegistration>(
                    nameof(login)
                );

            if(password is null)
                throw DomainDataInconsistencyException.EmptyOnRestore<UserPendingRegistration>(
                    nameof(password)
                );

            return new UserPendingRegistration(userPendingRegistrationId, login, password, createdAt, expiresAt);
        }
        #endregion

        #region Behavior
        public DomainResult Refresh(Password password, DateTimeOffset now)
        {
            if (password is null)
                return DomainResult.Failure(UserError.Empty(nameof(password), nameof(UserPendingRegistration)));

            Password = password;
            ExpiresAt = now.Add(ExpirationTime);

            AddDomainEvent(
                new UserPendingRegistrationRequestedEvent(
                    Id,
                    Login,
                    CreatedAt
                )
            );

            return DomainResult.Success();
        }

        public bool IsExpired(DateTimeOffset now)
            => now > ExpiresAt;
        #endregion
    }
}
