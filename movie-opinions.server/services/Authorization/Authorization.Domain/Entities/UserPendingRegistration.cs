using Authorization.Domain.Common;
using Authorization.Domain.DomainEvents.UserPendingRegistration;
using Authorization.Domain.Enums;
using Authorization.Domain.Errors;
using Authorization.Domain.Exceptions;
using Authorization.Domain.ValueObjects;
using Authorization.Domain.ValueObjects.Login;

namespace Authorization.Domain.Entities
{
    public class UserPendingRegistration : AggregateRoot
    {
        private static readonly TimeSpan ExpirationTime = TimeSpan.FromHours(12);

        public Login Login { get; private set; }

        public Password PasswordHash { get; private set; }

        public DateTime ExpiresAt { get; private set; }

        #region Creation
        private UserPendingRegistration(Login login, Password passwordhash)
           : base()
        {
            if (login is null)
                throw new ValidationDomainException(
                    ErrorCodes.LoginError.Empty,
                    $"{nameof(login)} validation failed: value is null. Entity {nameof(UserPendingRegistration)}!"
                );

            if (passwordhash is null)
                throw new ValidationDomainException(
                    ErrorCodes.PasswordError.Empty,
                    $"{nameof(passwordhash)} validation failed: value is null. Entity {nameof(UserPendingRegistration)}!"
                );

            Login = login;
            PasswordHash = passwordhash;
            ExpiresAt = CreatedAt.Add(ExpirationTime);
        }

        public static UserPendingRegistration Create(Login login, Password passwordHash)
        {
            var userRendingRegistration = new UserPendingRegistration(login, passwordHash);

            if (userRendingRegistration.Login.Type == LoginType.Email)
            {
                userRendingRegistration.AddDomainEvent(
                    new UserPendingRegistrationRequestedEvent(
                        userRendingRegistration.Id,
                        userRendingRegistration.Login.Value,
                        userRendingRegistration.CreatedAt
                    )
                );
            }

            userRendingRegistration.AddDomainEvent(
                new UserPendingRegistrationRequestedEvent(
                    userRendingRegistration.Id, 
                    null, 
                    userRendingRegistration.CreatedAt
                )
            );

            return userRendingRegistration;
        }
        #endregion

        #region Restore
        private UserPendingRegistration(Guid id, Login login, Password passwordhash, DateTime createdAt, DateTime expiresAt)
            : base(id, createdAt)
        {
            if (login is null)
                throw new DataInconsistencyDomainException(
                    ErrorCodes.RestoreError.NullReference,
                    $"Missing required field {nameof(login.Value)} during {nameof(UserPendingRegistration)} entity reconstruction!"
                );

            if (passwordhash is null)
                throw new DataInconsistencyDomainException(
                    ErrorCodes.RestoreError.NullReference,
                    $"Missing required field {nameof(passwordhash)} during {nameof(UserPendingRegistration)} entity reconstruction!"
                );

            Login = login;
            PasswordHash = passwordhash;
            ExpiresAt = expiresAt;
        }

        public static UserPendingRegistration Restore(Guid id, Login login, Password passwordhash, DateTime createdAt, DateTime expiresAt)
        {
            return new UserPendingRegistration(id, login, passwordhash, createdAt, expiresAt);
        }
        #endregion

        #region Behavior
        public void Refresh(Password passwordHash, DateTime now)
        {
            PasswordHash = passwordHash;
            ExpiresAt = now.Add(ExpirationTime);
        }

        public bool IsExpired(DateTime now)
            => now > ExpiresAt;
        #endregion
    }
}