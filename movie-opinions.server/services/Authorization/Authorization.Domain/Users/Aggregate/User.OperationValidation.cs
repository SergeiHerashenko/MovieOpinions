using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;

namespace Authorization.Domain.Users
{
    public partial class User
    {
        /// <summary>
        /// Перевіряє часову послідовність authentication-операції.
        ///
        /// Час операції не може передувати часу створення користувача,
        /// його останнього оновлення або останнього успішного входу.
        ///
        /// Метод не змінює стан агрегату.
        ///
        /// (Validates the chronological consistency of an authentication
        /// operation.
        ///
        /// The operation timestamp cannot precede the user's creation,
        /// last update, or last successful login timestamp.
        ///
        /// The method does not modify aggregate state.)
        /// </summary>
        /// <param name="now">Час операції автентифікації, який необхідно перевірити.</param>
        private void EnsureValidOperationTimestamp(DateTimeOffset now)
        {
            DomainGuard.AgainstEarlierThan<User>(
                OperationType.Update,
                (now, nameof(now)),
                (CreatedAt, nameof(CreatedAt))
            );

            if (UpdatedAt is not null)
            {
                DomainGuard.AgainstEarlierThan<User>(
                    OperationType.Update,
                    (now, nameof(now)),
                    (UpdatedAt.Value, nameof(UpdatedAt))
                );
            }

            if (LastLoginAt is not null)
            {
                DomainGuard.AgainstEarlierThan<User>(
                    OperationType.Update,
                    (now, nameof(now)),
                    (LastLoginAt.Value, nameof(LastLoginAt))
                );
            }
        }
    }
}
