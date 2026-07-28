using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Results;
using Authorization.Domain.Users.ValueObjects.LoginUser;
using Authorization.Domain.Users.ValueObjects.PasswordUser;
using Authorization.Domain.UsersPendingRegistration.DomainEvents;
using Authorization.Domain.UsersPendingRegistration.ValueObjects;

namespace Authorization.Domain.UsersPendingRegistration
{
    public class UserPendingRegistration : AggregateRoot<UserPendingRegistrationId, Guid>
    {
        private static readonly TimeSpan ExpirationTime = TimeSpan.FromHours(1);

        public Login Login { get; private set; }

        public Password Password { get; private set; }

        public RegistrationFlowToken RegistrationFlowToken { get; private set; }

        public DateTimeOffset ExpiresAt { get; private set; }

        #region Creation
        private UserPendingRegistration(
            UserPendingRegistrationId userPendingRegistrationId,
            Login login,
            Password password,
            RegistrationFlowToken registrationFlowToken,
            DateTimeOffset now)
            : base(userPendingRegistrationId, now)
        {
            Login = login;
            Password = password;
            RegistrationFlowToken = registrationFlowToken;
            ExpiresAt = now.Add(ExpirationTime);
        }

        public static Result<UserPendingRegistration> Create(Login login, Password password, DateTimeOffset now)
        {
            if (login is null)
                return Result<UserPendingRegistration>.Failure(LoginErrors.EmptyLogin<UserPendingRegistration>());

            if (password is null)
                return Result<UserPendingRegistration>.Failure(PasswordErrors.EmptyHashPassword<UserPendingRegistration>());

            var userPendingRegistration = new UserPendingRegistration(
                UserPendingRegistrationId.Create(),
                login,
                password,
                RegistrationFlowToken.Create(),
                now
            );

            userPendingRegistration.AddDomainEvent(
                new UserPendingRegistrationEvent(
                    userPendingRegistration.Login,
                    userPendingRegistration.CreatedAt
                )
            );

            return Result<UserPendingRegistration>.Success(userPendingRegistration);
        }
        #endregion

        #region Restoration
        private UserPendingRegistration(
            UserPendingRegistrationId userPendingRegistrationId,
            Login login,
            Password password,
            RegistrationFlowToken registrationFlowToken,
            DateTimeOffset createdAt,
            DateTimeOffset expiresAt)
            : base(userPendingRegistrationId, createdAt)
        {
            Login = login;
            Password = password;
            RegistrationFlowToken = registrationFlowToken;
            ExpiresAt = expiresAt;
        }

        public static UserPendingRegistration Restore(
            UserPendingRegistrationId userPendingRegistrationId,
            Login login,
            Password password,
            RegistrationFlowToken registrationFlowToken,
            DateTimeOffset createdAt,
            DateTimeOffset expiresAt)
        {
            DomainGuard.AgainstNull<UserPendingRegistration>(
                (userPendingRegistrationId, nameof(userPendingRegistrationId)),
                (login, nameof(login)),
                (password, nameof(password)),
                (registrationFlowToken, nameof(registrationFlowToken))
            );

            if(expiresAt <= createdAt)
                throw DomainDataInconsistencyException.ValueOutOfRange<UserPendingRegistration>(nameof(expiresAt), expiresAt);

            return new UserPendingRegistration(userPendingRegistrationId, login, password, registrationFlowToken, createdAt, expiresAt);
        }
        #endregion

        #region Behavior
        public Result Refresh(Password password, DateTimeOffset now)
        {
            if (password is null)
                return Result<UserPendingRegistration>.Failure(PasswordErrors.EmptyHashPassword<UserPendingRegistration>());

            Password = password;
            RegistrationFlowToken = RegistrationFlowToken.Create();
            ExpiresAt = now.Add(ExpirationTime);

            AddDomainEvent(new UserPendingRegistrationEvent(Login, now));

            return Result.Success();
        }

        public bool IsExpired(DateTimeOffset now)
            => now > ExpiresAt;
        #endregion
    }
}
