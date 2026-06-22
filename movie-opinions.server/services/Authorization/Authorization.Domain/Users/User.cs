using Authorization.Domain.Common.Errors.Users;
using Authorization.Domain.Common.Exceptions;
using Authorization.Domain.Common.Models;
using Authorization.Domain.DomainEvents.User;
using Authorization.Domain.Results;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects;
using Authorization.Domain.Users.ValueObjects.LoginUser;

namespace Authorization.Domain.Users
{
    public class User : AggregateRoot<UserId, Guid>
    {
        public Login Login { get; private set; }

        public Password Password { get; private set; }

        public Role Role { get; private set; }

        public DateTimeOffset? UpdatedAt { get; private set; }

        public DateTimeOffset? LastLoginAt { get; private set; }

        public string? LastLoginIp { get; private set; }

        public bool IsLoginConfirmed { get; private set; }

        public int FailedLoginAttempts { get; private set; }

        public bool IsBlocked { get; private set; }

        public bool IsDeleted { get; private set; }

        private const int MaxFailedAttempts = 3;

        #region Creation
        private User(UserId userId, Login login, Password password, Role role) : base(userId)
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

        public static Result<User> Create(Login login, Password password, Role role)
        {
            if (login is null)
                return Result<User>.Failure(UserError.Empty(nameof(Login), nameof(User)));

            if (password is null)
                return Result<User>.Failure(UserError.Empty(nameof(Password), nameof(User)));

            if (role != Role.User)
            {
                return Result<User>.Failure(UserError.OperationIsNotAllowed("User creation rejected due to invalid role!"));
            }

            var user = new User(UserId.CreateUnique(), login, password, role);

            user.AddDomainEvent(
                new UserRegisteredEvent(user.Id, user.Login, user.CreatedAt)
            );

            return Result<User>.Success(user);
        }
        #endregion

        #region Restore
        private User(
            UserId userId,
            DateTimeOffset createdAt,
            Login login,
            Password password,
            Role role,
            DateTimeOffset? updateAt,
            DateTimeOffset? lastLoginAt,
            string? lastLoginIp,
            bool isConfirmed,
            int failedLoginAttempts,
            bool isBlocked,
            bool isDeleted)
            : base(userId, createdAt)
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

        public static User Restore(
            UserId userId,
            DateTimeOffset createdAt, 
            Login login,
            Password password,
            Role role,
            DateTimeOffset? updateAt,
            DateTimeOffset? lastLoginAt,
            string? lastLoginIp,
            bool isConfirmed,
            int failedLoginAttempts,
            bool isBlocked,
            bool isDeleted)
        {
            if (userId is null)
                throw DomainDataInconsistencyException.EmptyOnRestore<User>(nameof(userId));

            if (login is null)
                throw DomainDataInconsistencyException.EmptyOnRestore<User>(nameof(login));

            if(password is null)
                throw DomainDataInconsistencyException.EmptyOnRestore<User>(nameof(password));

            if (!Enum.IsDefined(typeof(Role), role))
            {
                throw DomainDataInconsistencyException.UnsupportedDiscriminator<User>(
                    role.ToString()
                );
            }

            return new User(
                userId, 
                createdAt, 
                login, 
                password, 
                role, 
                updateAt, 
                lastLoginAt, 
                lastLoginIp, 
                isConfirmed, 
                failedLoginAttempts, 
                isBlocked, 
                isDeleted
            );
        }
        #endregion

        #region Behavior
        public Result ChangeLogin(Login newLogin, DateTimeOffset updateTime)
        {
            var access = ProvideAccess();

            if (!access.IsSuccess)
                return access;

            if (newLogin is null)
                return Result.Failure(UserError.Empty(nameof(newLogin), nameof(User)));

            if (Login == newLogin)
                return Result.Failure(UserError.NoChangesDetected($"The {newLogin} cannot be the same as the old login!"));

            var oldLogin = Login.Value;

            Login = newLogin;
            IsLoginConfirmed = false;
            UpdatedAt = updateTime;

            AddDomainEvent(
                new UserLoginChangedEvent(Id, oldLogin, Login, updateTime)
            );

            return Result.Success();
        }

        public Result ChangePassword(Password newPassword, DateTimeOffset updateTime)
        {
            var access = ProvideAccess();

            if (!access.IsSuccess)
                return access;

            if(newPassword is null)
                return Result.Failure(UserError.Empty(nameof(newPassword), nameof(User)));

            if(newPassword == Password)
                return Result.Failure(UserError.NoChangesDetected($"The {newPassword} cannot be the same as the old login!"));

            Password = newPassword;
            UpdatedAt = updateTime;

            AddDomainEvent(
                new UserPasswordChangedEvent(Id, Login, updateTime)
            );

            return Result.Success();
        }

        public Result ConfirmLogin(DateTimeOffset updateTime)
        {
            var access = ProvideAccess();

            if (!access.IsSuccess)
                return access;

            if (IsLoginConfirmed)
                return Result.Failure(UserError.NoChangesDetected("Login already confirmed!"));

            IsLoginConfirmed = true;
            UpdatedAt = updateTime;

            return Result.Success();
        }

        public Result Block(DateTimeOffset updateTime, string reason)
        {
            var access = ProvideAccess();

            if (!access.IsSuccess)
                return access;

            IsBlocked = true;
            UpdatedAt = updateTime;

            AddDomainEvent(
                new UserBlockedEvent(Id, Login, reason, updateTime)
            );

            return Result.Success();
        }

        public Result RemoveBlock(DateTimeOffset updateTime)
        {
            var access = ProvideAccess();

            if (access.IsSuccess)
                return access;

            IsBlocked = false;
            UpdatedAt = updateTime;

            AddDomainEvent(
                new UserRemoveBlockedEvent(Id, Login, updateTime)
            );

            return Result.Success();
        }

        public Result RecordFailedLoginAttempt(DateTimeOffset updateTime)
        {
            var access = ProvideAccess();

            if (!access.IsSuccess)
                return access;

            FailedLoginAttempts++;
            UpdatedAt = updateTime;

            if (FailedLoginAttempts >= MaxFailedAttempts)
            {
                Block(updateTime, "Max failed login attempts exceeded!");
            }

            return Result.Success();
        }

        public Result LoginSuccess(string ip, DateTimeOffset loginTime)
        {
            var access = ProvideAccess();

            if (!access.IsSuccess)
                return access;

            FailedLoginAttempts = 0;

            LastLoginAt = loginTime;
            LastLoginIp = ip;
            UpdatedAt = loginTime;

            AddDomainEvent(
                new UserLoggedInEvent(Id, Login, ip, loginTime)
            );

            return Result.Success();
        }

        public Result Delete(DateTimeOffset updateTime)
        {
            var access = ProvideAccess();

            if (!access.IsSuccess)
                return access;

            IsDeleted = true;
            UpdatedAt = updateTime;

            AddDomainEvent(
                new UserDeletedEvent(Id, Login, updateTime)
            );

            return Result.Success();
        }

        public Result Undelete(DateTimeOffset restoreUntil, DateTimeOffset now)
        {
            if(!IsDeleted)
                return Result.Success();

            if (restoreUntil < now)
                return Result.Failure(UserError.RestoreIsNotAllowed(restoreUntil));

            IsDeleted = false;
            UpdatedAt = now;

            AddDomainEvent(
                new UserUndeleteEvent(Id, Login, now)
            );

            return Result.Success();
        }
        #endregion

        #region Guards
        private Result ProvideAccess()
        {
            if (IsBlocked)
                return Result.Failure(UserError.UserIsBlocked);

            if (IsDeleted)
                return Result.Failure(UserError.UserIsDeleted);

            return Result.Success();
        }
        #endregion
    }
}