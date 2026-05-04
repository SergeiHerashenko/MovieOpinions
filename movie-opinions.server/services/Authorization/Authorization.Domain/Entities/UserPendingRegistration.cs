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

        private UserPendingRegistration(Login login, Password passwordhash)
            : base()
        {
            if (login == null)
                throw new ValidationDomainException(ErrorCodes.LoginError.Empty, "Логін є обов'язковим!");

            if (passwordhash == null)
                throw new ValidationDomainException(ErrorCodes.PasswordError.Empty, "Пароль є обов'язковим!");

            Login = login;
            PasswordHash = passwordhash;
            ExpiresAt = CreatedAt.Add(ExpirationTime);
        }

        private UserPendingRegistration(Guid id, Login login, Password passwordhash, DateTime createdAt, DateTime expiresAt)
            : base(id, createdAt)
        {
            Login = login;
            PasswordHash = passwordhash;
            ExpiresAt = expiresAt;
        }

        public static UserPendingRegistration Create(Login login, Password passwordHash)
        {
            var userRendingRegistration = new UserPendingRegistration(login, passwordHash);

            if(userRendingRegistration.Login.Type == LoginType.Email)
            {
                userRendingRegistration.AddDomainEvent(new UserPendingRegistrationRequestedEvent(
                    userRendingRegistration.Id, 
                    userRendingRegistration.Login.Value,
                    userRendingRegistration.CreatedAt));
            }

            userRendingRegistration.AddDomainEvent(new UserPendingRegistrationRequestedEvent(userRendingRegistration.Id, null, userRendingRegistration.CreatedAt));

            return userRendingRegistration;
        }

        public static UserPendingRegistration Restore(Guid id, Login login, Password passwordhash, DateTime createdAt, DateTime expiresAt)
        {
            return new UserPendingRegistration(id, login, passwordhash, createdAt, expiresAt);
        }

        public void Refresh(Password passwordHash, DateTime now)
        {
            PasswordHash = passwordHash;
            ExpiresAt = now.Add(ExpirationTime);
        }

        public bool IsExpired(DateTime now)
            => now > ExpiresAt;
    }
}
