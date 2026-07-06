using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.Validations;
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

        public UserId UserId { get; private set; }

        public Login Login { get; private set; }

        public Password Password { get; private set; }

        public RegistrationToken RegistrationToken { get; private set; }

        public DateTimeOffset ExpiresAt { get; private set; }

        #region Creation
        private UserPendingRegistration(
            UserPendingRegistrationId userPendingRegistrationId,
            UserId userId,
            Login login, 
            Password password,
            RegistrationToken registrationToken)
            : base(userPendingRegistrationId)
        {
            UserId = userId;
            Login = login;
            Password = password;
            RegistrationToken = registrationToken;
            ExpiresAt = CreatedAt.Add(ExpirationTime);
        }

        public static Result<UserPendingRegistration> Create(Login login, Password password)
        {
            if(login is null)
                return Result<UserPendingRegistration>.Failure(UserErrors.LoginError.EmptyLogin<UserPendingRegistration>());

            if(password is null)
                return Result<UserPendingRegistration>.Failure(UserErrors.PasswordError.EmptyPassword<UserPendingRegistration>());
            
            var userRendingRegistration = new UserPendingRegistration(
                UserPendingRegistrationId.CreateUnique(),
                UserId.CreateUnique(),
                login, 
                password, 
                RegistrationToken.CreateUnique()
            );

            userRendingRegistration.AddDomainEvent(
                new UserPendingRegistrationRequestedEvent(
                    userRendingRegistration.UserId,
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
            UserId userId,
            Login login,
            Password password,
            RegistrationToken registrationToken,
            DateTimeOffset createdAt,
            DateTimeOffset expiresAt)
            : base(userPendingRegistrationId, createdAt)
        {
            UserId = userId;
            Login = login;
            Password = password;
            RegistrationToken = registrationToken;
            ExpiresAt = expiresAt;
        }

        public static UserPendingRegistration Restore(
            UserPendingRegistrationId userPendingRegistrationId,
            UserId userId,
            Login login,
            Password password,
            RegistrationToken registrationToken,
            DateTimeOffset createdAt,
            DateTimeOffset expiresAt)
        {
            DomainGuard.AgainstNull<UserPendingRegistration>(
                (login, nameof(login)),
                (password, nameof(password)),
                (registrationToken, nameof(registrationToken))
            );

            return new UserPendingRegistration(userPendingRegistrationId, userId, login, password, registrationToken, createdAt, expiresAt);
        }
        #endregion

        #region Behavior
        public Result Refresh(Password password, DateTimeOffset now)
        {
            if (password is null)
                return Result.Failure(UserErrors.PasswordError.EmptyPassword<UserPendingRegistration>());

            Password = password;
            RegistrationToken = RegistrationToken.CreateUnique();
            ExpiresAt = now.Add(ExpirationTime);

            AddDomainEvent(
                new UserPendingRegistrationRequestedEvent(
                    UserId,
                    Login,
                    CreatedAt
                )
            );

            return Result.Success();
        }

        public bool IsExpired(DateTimeOffset now)
            => now > ExpiresAt;
        #endregion
    }
}
