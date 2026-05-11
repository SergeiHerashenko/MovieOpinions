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

        #region Creation
        private User(Login login, Password password, Role role) : base()
        {
            if (login is null)
                throw new ValidationDomainException(
                    ErrorCodes.LoginError.Empty, 
                    $"{nameof(login)} validation failed: value is null. Entity {nameof(User)}!"
                );

            if (password is null)
                throw new ValidationDomainException(
                    ErrorCodes.PasswordError.Empty, 
                    $"{nameof(password)} validation failed: value is null. Entity {nameof(User)}!"
                );

            if (role != Role.User)
                throw new ValidationDomainException(
                    ErrorCodes.GeneralError.OperationNotAllowed, 
                    $"User creation rejected due to invalid role. Entity {nameof(User)}!"
                );

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

        public static User CreateNewUser(Login login, Password password, Role role)
        {
            var user = new User(login, password, role);

            user.AddDomainEvent(
                new UserRegisteredEvent(user.Id, user.Login.Value, user.Login.Type, user.CreatedAt)
            );

            return user;
        }
        #endregion

        #region Restore
        private User(
            Guid id,
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
            if (login is null)
                throw new DataInconsistencyDomainException(
                    ErrorCodes.RestoreError.NullReference,
                    $"Missing required field {nameof(login.Value)} during {nameof(User)} entity reconstruction!"
                );

            if (password is null)
                throw new DataInconsistencyDomainException(
                    ErrorCodes.RestoreError.NullReference,
                    $"Missing required field {nameof(password)} during {nameof(User)} entity reconstruction!"
                );

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
            return new User(id, createdAt, login, password, role, updateAt, lastLoginAt, lastLoginIp, isConfirmed, failedLoginAttempts, isBlocked, isDeleted);
        }
        #endregion

        #region Behavior
        public void ChangeLogin(Login newLogin, DateTime updateTime)
        {
            if (newLogin is null)
                throw new ValidationDomainException(
                    ErrorCodes.LoginError.Empty,
                    $"The {nameof(newLogin)} is required!"
                );

            EnsureActive();

            if (Login == newLogin)
                return;

            var oldLogin = Login.Value;

            Login = newLogin;
            IsLoginConfirmed = false;
            UpdatedAt = updateTime;

            AddDomainEvent(
                new UserLoginChangedEvent(Id, oldLogin, newLogin.Value, newLogin.Type, updateTime)
            );
        }

        public void ChangePassword(Password newPassword, DateTime updateTime)
        {
            if (newPassword is null)
                throw new ValidationDomainException(
                    ErrorCodes.PasswordError.Empty,
                    $"The {nameof(newPassword)} is required!"
                );

            EnsureActive();

            if (Password == newPassword) 
                return;

            Password = newPassword;
            UpdatedAt = updateTime;

            AddDomainEvent(
                new UserPasswordChangedEvent(Id, updateTime)
            );
        }

        public void ConfirmLogin(DateTime updateTime)
        {
            EnsureNotDeleted();

            if (IsLoginConfirmed) 
                return;

            IsLoginConfirmed = true;
            UpdatedAt = updateTime;
        }

        public void Block(DateTime updateTime, string reason)
        {
            EnsureNotDeleted();

            if (IsBlocked) 
                return;

            IsBlocked = true;
            UpdatedAt = updateTime;

            AddDomainEvent(
                new UserBlockedEvent(Id, reason, updateTime)
            );
        }

        public void RecordFailedLoginAttempt(DateTime updateTime)
        {
            if (IsBlocked || IsDeleted)
                return;

            FailedLoginAttempts++;
            UpdatedAt = updateTime;

            if (FailedLoginAttempts >= MaxFailedAttempts)
            {
                Block(updateTime, "Max failed login attempts exceeded!");
            }
        }

        public void LoginSuccess(string ip, DateTime loginTime)
        {
            if (IsDeleted || IsBlocked)
                throw new BusinessRuleViolationDomainException(
                    ErrorCodes.GeneralError.OperationNotAllowed,
                    $"Login operation not allowed for blocked or deleted user. Entity {nameof(User)}!"
                );

            FailedLoginAttempts = 0;

            LastLoginAt = loginTime;
            LastLoginIp = ip;
            UpdatedAt = loginTime;

            AddDomainEvent(
                new UserLoggedInEvent(Id, ip, loginTime)
            );
        }

        public void Delete(DateTime updateTime)
        {
            if (IsDeleted)
                return;

            IsDeleted = true;
            UpdatedAt = updateTime;

            AddDomainEvent(
                new UserDeletedEvent(Id, updateTime)
            );
        }
        #endregion

        #region Guards
        private void EnsureActive()
        {
            EnsureNotDeleted();

            if (IsBlocked)
                throw new BusinessRuleViolationDomainException(
                    ErrorCodes.AccountStatusError.Blocked,
                    $"Operation not allowed for blocked user. Entity {nameof(User)}!"
                );
        }

        private void EnsureNotDeleted()
        {
            if (IsDeleted)
                throw new BusinessRuleViolationDomainException(
                    ErrorCodes.AccountStatusError.Deleted,
                    $"Operation not allowed for deleted user. Entity {nameof(User)}!"
                );
        }
        #endregion
    }
}