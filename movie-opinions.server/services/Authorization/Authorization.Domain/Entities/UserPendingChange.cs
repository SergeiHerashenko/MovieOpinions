using Authorization.Domain.Common;
using Authorization.Domain.DomainEvents.UserPendingChange;
using Authorization.Domain.Enums;
using Authorization.Domain.Errors;
using Authorization.Domain.Exceptions;
using Authorization.Domain.ValueObjects;
using Authorization.Domain.ValueObjects.Login;

namespace Authorization.Domain.Entities
{
    public class UserPendingChange : AggregateRoot
    {
        private static readonly TimeSpan ExpirationTime = TimeSpan.FromMinutes(30);

        public Guid UserId { get; private set; }

        public string ConfirmationToken { get; private set; }

        public UserChangeType UserChangeType { get; private set; }

        public Password? NewPassword { get; private set; }

        public Login? NewLogin { get; private set; }

        public DateTime ExpiresAt { get; private set; }

        public bool IsConfirmed { get; private set; }

        private UserPendingChange(Guid userId,
            string confirmationToken,
            UserChangeType userChangeType,
            Password? newPassword,
            Login? newLogin)
            : base()
        {
            if (userId == Guid.Empty)
                throw new ValidationDomainException(ErrorCodes.UserPendingChangeError.InvalidUserId, "Невалідний ідентифікатор користувача!");

            if (string.IsNullOrWhiteSpace(confirmationToken))
                throw new ValidationDomainException(ErrorCodes.UserPendingChangeError.InvalidConfirmToken, "Токен підтвердження не може бути порожнім!");

            UserId = userId;
            ConfirmationToken = confirmationToken;
            UserChangeType = userChangeType;
            NewPassword = newPassword;
            NewLogin = newLogin;
            ExpiresAt = CreatedAt.Add(ExpirationTime);
            IsConfirmed = false;
        }

        private UserPendingChange(Guid id,
            Guid userId,
            string confirmationToken,
            UserChangeType userChangeType,
            Password? newPassword,
            Login? newLogin,
            DateTime expiresAt,
            bool isConfirmed,
            DateTime createdAt)
            : base(id, createdAt)
        {
            UserId = userId;
            ConfirmationToken = confirmationToken;
            UserChangeType = userChangeType;
            NewPassword = newPassword;
            NewLogin = newLogin;
            ExpiresAt = expiresAt;
            IsConfirmed = isConfirmed;
        }

        public static UserPendingChange CreateLoginChange(Guid userId, string confirmationToken, Login newLogin)
        {
            var userPendingChange = new UserPendingChange(userId, confirmationToken, UserChangeType.LoginChange, null, newLogin);

            userPendingChange.AddDomainEvent(new UserLoginChangeRequestedEvent(userId, confirmationToken, userPendingChange.CreatedAt));

            return userPendingChange;
        }

        public static UserPendingChange CreatePasswordChange(Guid userId, string confirmationToken, Password newPassword)
        {
            var userPendingChange = new UserPendingChange(userId, confirmationToken, UserChangeType.PasswordChange, newPassword, null);

            userPendingChange.AddDomainEvent(new UserPasswordChangeRequestedEvent(userId, confirmationToken, userPendingChange.CreatedAt));

            return userPendingChange;
        }

        public static UserPendingChange Restore(Guid id,
            Guid userId,
            string confirmationToken,
            UserChangeType userChangeType,
            Password? newPassword,
            Login? newLogin,
            DateTime expiresAt,
            bool isConfirmed,
            DateTime createdAt)
        {
            return new UserPendingChange(id, userId, confirmationToken, userChangeType, newPassword, newLogin, expiresAt, isConfirmed, createdAt);
        }

        public bool CanBeConfirmed(string token, DateTime now)
        {
            return !IsConfirmed &&
                   ConfirmationToken == token &&
                   ExpiresAt > now;
        }

        public void Confirm(string token, DateTime now)
        {
            if (!CanBeConfirmed(token, now))
                throw new BusinessRuleViolationDomainException(ErrorCodes.TokenError.TokenExpired, "Токен недійсний, прострочений або вже використаний.");

            IsConfirmed = true;
        }
    }
}
