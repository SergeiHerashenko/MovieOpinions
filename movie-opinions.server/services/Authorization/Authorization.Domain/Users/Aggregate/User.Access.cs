using Authorization.Domain.Results;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.Errors;

namespace Authorization.Domain.Users
{
    public partial class User
    {
        /// <summary>
        /// Перевіряє, чи може користувачу бути надано доступ
        /// у вказаний момент.
        ///
        /// Доступ забороняється видаленому користувачу або користувачу
        /// з активною сесією обмеження типу Ban. Наявність завершеної
        /// за часом сесії не блокує доступ.
        ///
        /// Метод не змінює стан агрегату.
        ///
        /// (Checks whether access can be granted to the user
        /// at the specified time.
        ///
        /// Access is denied when the user is deleted or has an active
        /// Ban restriction session. A session that has expired by time
        /// does not block access.
        ///
        /// The method does not modify aggregate state.)
        /// </summary>
        /// <param name="now">Час, відносно якого перевіряється активність бану.</param>
        /// <returns>
        /// Успіх, якщо доступ дозволений; інакше — відповідна доменна помилка.
        /// </returns>
        private Result ProvideAccess(DateTimeOffset now)
        {
            if (_deletion is not null)
                return Result.Failure(UserErrors.UserDeleted<User>());

            var banSession = _restrictionSessions
                .FirstOrDefault(x => x.RestrictionType == RestrictionType.Ban);

            if (banSession is null)
                return Result.Success();

            if (banSession.IsExpiredAt(now))
                return Result.Success();

            return Result.Failure(UserErrors.UserBlocked<User>());
        }
    }
}
