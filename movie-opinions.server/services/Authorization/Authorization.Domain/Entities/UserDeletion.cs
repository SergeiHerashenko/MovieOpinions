using Authorization.Domain.Common;
using Authorization.Domain.DomainEvents.UserDeletion;
using Authorization.Domain.Errors;
using Authorization.Domain.Exceptions;

namespace Authorization.Domain.Entities
{
    public class UserDeletion : AggregateRoot
    {
        public Guid UserId { get; private set; }

        public string Login {  get; private set; }

        public string? Reason { get; private set; }

        private UserDeletion(Guid userId, string login, string? reason) : base()
        {
            if (userId == Guid.Empty)
                throw new ValidationDomainException(ErrorCodes.GeneralError.OperationNotAllowed, "Помилка отримання ідентифікатора користувача!");

            UserId = userId;
            Login = login;
            Reason = reason;
        }

        private UserDeletion(Guid id, Guid userId, string login, string? reason, DateTime createdAt)
            : base(id, createdAt)
        {
            if (userId == Guid.Empty || userId == Guid.Empty)
                throw new ValidationDomainException(ErrorCodes.GeneralError.OperationNotAllowed, "Помилка отримання ідентифікатора користувача!");

            UserId = userId;
            Login = login;
            Reason = reason;
        }

        public static UserDeletion CreateForDeletedUser(User user, string? reason, DateTime now)
        {
            if (user.IsDeleted)
                throw new InvalidStateDomainException(ErrorCodes.AccountStatusError.Deleted, "Користувач вже видалений.");

            var userDeletion =  new UserDeletion(user.Id, user.Login.Value, reason);

            userDeletion.AddDomainEvent(new UserAccountDeletionRequestedEvent(user.Id, user.Login.Value, reason, now));

            return userDeletion;
        }

        public static UserDeletion Restore(Guid id, Guid userId, string login, string? reason, DateTime createdAt)
        {
            if (id == Guid.Empty || userId == Guid.Empty)
                throw new ValidationDomainException(ErrorCodes.GeneralError.OperationNotAllowed, "Помилка отримання ідентифікатора користувача!");

            return new UserDeletion(id, userId, login, reason, createdAt);
        }
    }
}
