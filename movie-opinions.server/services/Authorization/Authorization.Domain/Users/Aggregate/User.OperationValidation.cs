using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Guard;

namespace Authorization.Domain.Users.Aggregate
{
    public partial class User
    {
        /// <summary>
        /// Перевіряє часову послідовність операції над агрегатом користувача.
        ///
        /// Час операції не може передувати часу створення користувача,
        /// його останнього оновлення або останнього успішного входу.
        /// Час оновлення та входу перевіряється лише за наявності відповідних значень.
        /// Рівність часових позначок допускається.
        ///
        /// Метод не змінює стан агрегату.
        ///
        /// (Validates the chronological consistency of a user aggregate operation.
        ///
        /// The operation timestamp cannot precede the user's creation,
        /// last update, or last successful sign-in timestamp.
        /// Update and sign-in timestamps are checked only when their values are present.
        /// Equal timestamps are allowed.
        ///
        /// The method does not modify aggregate state.)
        /// </summary>
        /// <param name="now">
        /// Час операції над агрегатом, який необхідно перевірити.
        /// 
        /// (User aggregate operation timestamp to validate.)
        /// </param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо час операції передує одній із часових позначок,
        /// що перевіряються.
        /// 
        /// (Thrown when the operation timestamp precedes
        /// any of the timestamps being checked.)
        /// </exception>
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
