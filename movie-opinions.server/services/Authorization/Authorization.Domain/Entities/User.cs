using Authorization.Domain.Common;
using Authorization.Domain.DomainEvents.User;
using Authorization.Domain.Enums;
using Authorization.Domain.Errors;
using Authorization.Domain.Exceptions;
using Authorization.Domain.ValueObjects;
using Authorization.Domain.ValueObjects.Login;

namespace Authorization.Domain.Entities
{
    public class User : AggregateRoot
    {
        public Login Login { get; private set; }

        public Password Password { get; private set; }

        public Role Role { get; private set; }

        public DateTime? UpdatedAt { get; private set; }

        public DateTime? LastLoginAt { get; private set; }

        public string? LastLoginIp { get; private set; }

        public bool IsLoginConfirmed { get; private set; }

        public int FailedLoginAttempts { get; private set; }

        public bool IsBlocked { get; private set; }

        public bool IsDeleted { get; private set; }

        private const int MaxFailedAttempts = 3;

        private User(Login login, Password password, Role role) : base()
        {
            Login = login;
            Password = password;
            Role = role;
            UpdatedAt = null;
            LastLoginAt = null;
            LastLoginIp = null;
            IsLoginConfirmed = false;
            FailedLoginAttempts = 0;
            IsBlocked = false;
            IsDeleted = false;
        }

        private User(Guid id,
            DateTime createdAt,
            Login login,
            Password password,
            Role role,
            DateTime? updateAt,
            DateTime? lastLoginAt,
            string? lastLoginIp,
            bool isConfirmed,
            int failedLoginAttempts,
            bool isBlocked,
            bool isDeleted)
            : base(id, createdAt)
        {
            Login = login;
            Password = password;
            Role = role;
            UpdatedAt = updateAt;
            LastLoginAt = lastLoginAt;
            LastLoginIp = lastLoginIp;
            IsLoginConfirmed = isConfirmed;
            FailedLoginAttempts = failedLoginAttempts;
            IsBlocked = isBlocked;
            IsDeleted = isDeleted;
        }

        public static User CreateNewUser(Login login, Password password, Role role)
        {
            if (login == null)
                throw new ValidationDomainException(ErrorCodes.LoginError.Empty, "Логін є обов'язковим!");

            if (password == null)
                throw new ValidationDomainException(ErrorCodes.PasswordError.Empty, "Пароль є обов'язковим!");

            if (role != Role.User)
                throw new ValidationDomainException(ErrorCodes.GeneralError.OperationNotAllowed, "Обліковий запис можна створити тільки з роллю User!");

            var user = new User(login, password, role);

            user.AddDomainEvent(new UserRegisteredEvent(user.Id, user.Login.Value, user.Login.Type, user.CreatedAt));

            return user;
        }

        public static User Restore(Guid id, 
            DateTime createdAt, 
            Login login,
            Password password, 
            Role role, 
            DateTime? updateAt,
            DateTime? lastLoginAt,
            string? lastLoginIp,
            bool isConfirmed,
            int failedLoginAttempts,
            bool isBlocked,
            bool isDeleted)
        {
            if (login == null)
                throw new ValidationDomainException(ErrorCodes.LoginError.Empty, "Логін є обов'язковим для відновлення.");
              
            if (password == null)
                throw new ValidationDomainException(ErrorCodes.PasswordError.Empty, "Пароль є обов'язковим для відновлення.");

            return new User(id, createdAt, login, password, role, updateAt, lastLoginAt, lastLoginIp, isConfirmed, failedLoginAttempts, isBlocked, isDeleted);
        }

        public void ChangeLogin(Login newLogin, DateTime updateTime)
        {
            if (newLogin == null)
                throw new ValidationDomainException(ErrorCodes.LoginError.Empty, "Логін є обов'язковим!");

            if (IsDeleted)
                throw new InvalidStateDomainException(ErrorCodes.AccountStatusError.Deleted, "Не можна змінювати логін видаленого користувача!");

            if (IsBlocked)
                throw new InvalidStateDomainException(ErrorCodes.AccountStatusError.Blocked, "Не можна змінювати логін заблокованого користувача!");

            if (Login == newLogin) return;

            var oldLogin = Login.Value;

            Login = newLogin;
            IsLoginConfirmed = false;
            UpdatedAt = updateTime;

            AddDomainEvent(new UserLoginChangedEvent(Id, oldLogin, newLogin.Value, newLogin.Type, updateTime)); 
        }

        public void ChangePassword(Password newPassword, DateTime updateTime)
        {
            if (newPassword == null)
                throw new ValidationDomainException(ErrorCodes.PasswordError.Empty, "Пароль є обов'язковим!");

            if (IsDeleted)
                throw new InvalidStateDomainException(ErrorCodes.AccountStatusError.Deleted, "Не можна змінювати пароль видаленого користувача!");

            if (IsBlocked)
                throw new InvalidStateDomainException(ErrorCodes.AccountStatusError.Blocked, "Не можна змінювати пароль заблокованого користувача!");

            if (Password == newPassword) return;

            Password = newPassword;
            UpdatedAt = updateTime;

            AddDomainEvent(new UserPasswordChangedEvent(Id, updateTime));
        }

        public void ConfirmLogin(DateTime updateTime)
        {
            if (IsDeleted)
                throw new InvalidStateDomainException(ErrorCodes.AccountStatusError.Deleted, "Неможливо підтвердити логін видаленого користувача.");

            if (IsLoginConfirmed) return;

            IsLoginConfirmed = true;
            UpdatedAt = updateTime;
        }

        public void Block(DateTime updateTime, string reason)
        {
            if (IsDeleted)
                throw new InvalidStateDomainException(ErrorCodes.AccountStatusError.Deleted, "Неможливо заблокувати видаленого користувача.");

            if (IsBlocked) return;

            IsBlocked = true;
            UpdatedAt = updateTime;

            AddDomainEvent(new UserBlockedEvent(Id, reason, updateTime));
        }

        public void RecordFailedLoginAttempt(DateTime updateTime)
        {
            if (IsBlocked || IsDeleted) return;

            FailedLoginAttempts++;
            UpdatedAt = updateTime;

            if (FailedLoginAttempts >= MaxFailedAttempts)
            {
                Block(updateTime, "Автоматичне блокування після 3 невдалих спроб входу");
            }
        }

        public void LoginSuccess(string ip, DateTime loginTime)
        {
            if (IsDeleted || IsBlocked)
                throw new InvalidStateDomainException(ErrorCodes.GeneralError.OperationNotAllowed, "Неможливо виконати вхід для заблокованого або видаленого користувача.");

            FailedLoginAttempts = 0;

            LastLoginAt = loginTime;
            LastLoginIp = ip;
            UpdatedAt = loginTime;

            AddDomainEvent(new UserLoggedInEvent(Id, ip, loginTime));
        }

        public void Delete(DateTime updateTime)
        {
            if (IsDeleted) return;

            IsDeleted = true;
            UpdatedAt = updateTime;

            AddDomainEvent(new UserDeletedEvent(Id, updateTime));
        }
    }
}
