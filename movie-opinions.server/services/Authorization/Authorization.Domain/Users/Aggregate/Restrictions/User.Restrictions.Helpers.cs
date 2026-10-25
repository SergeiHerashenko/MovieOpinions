using Authorization.Domain.Users.AggregateChanges.Restriction;
using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestrictionSession;

namespace Authorization.Domain.Users.Aggregate
{
    public partial class User
    {
        /// <summary>
        /// Переводить попередньо перевірене обмеження
        /// у завершений стан і видаляє його з активної
        /// колекції агрегату.
        ///
        /// Запис відповідної зміни агрегату виконується окремо.
        ///
        /// (Transitions a pre-validated restriction to the completed state
        /// and removes it from the aggregate's active collection.
        ///
        /// The corresponding aggregate change is recorded separately.)
        /// </summary>
        /// <param name="restriction">
        /// Попередньо перевірене обмеження, яке необхідно завершити.
        ///
        /// (Pre-validated restriction to complete.)
        /// </param>
        /// <param name="now">
        /// Час, яким фіксується завершення обмеження.
        ///
        /// (Time recorded for the restriction's completion.)
        /// </param>
        private void CompleteAndRemoveRestriction(
            UserRestriction restriction,
            DateTimeOffset now)
        {
            restriction.MarkAsCompleted(now);
            _restrictions.Remove(restriction);
        }

        /// <summary>
        /// Переводить попередньо перевірене обмеження
        /// у відкликаний стан і видаляє його з активної
        /// колекції агрегату.
        ///
        /// Запис відповідної зміни агрегату виконується окремо.
        ///
        /// (Transitions a pre-validated restriction to the revoked state
        /// and removes it from the aggregate's active collection.
        ///
        /// The corresponding aggregate change is recorded separately.)
        /// </summary>
        /// <param name="restriction">
        /// Попередньо перевірене обмеження, яке необхідно відкликати.
        ///
        /// (Pre-validated restriction to revoke.)
        /// </param>
        /// <param name="now">
        /// Час, яким фіксується відкликання обмеження.
        ///
        /// (Time recorded for the restriction's revocation.)
        /// </param>
        private void RevokeAndRemoveRestriction(
            UserRestriction restriction,
            DateTimeOffset now)
        {
            restriction.RevokeRestriction(now);
            _restrictions.Remove(restriction);
        }

        /// <summary>
        /// Видаляє оброблену сесію обмежень
        /// з активної колекції агрегату.
        ///
        /// Обмеження сесії повинні бути переведені
        /// у відповідні кінцеві стани до виклику методу.
        ///
        /// (Removes a processed restriction session
        /// from the aggregate's active collection.
        ///
        /// The session's restrictions must have transitioned
        /// to their corresponding terminal states before this method is called.)
        /// </summary>
        /// <param name="session">
        /// Оброблена сесія обмежень, яку необхідно видалити з колекції.
        ///
        /// (Processed restriction session to remove from the collection.)
        /// </param>
        private void RemoveRestrictionSession(UserRestrictionSession session)
        {
            _restrictionSessions.Remove(session);
        }

        /// <summary>
        /// Записує зміну агрегату про оновлення
        /// для кожного переданого обмеження.
        ///
        /// Метод викликається після застосування змін
        /// і не змінює доменний стан самих обмежень.
        ///
        /// (Records an aggregate update change
        /// for each supplied restriction.
        ///
        /// The method is called after applying the changes
        /// and does not modify the restrictions' domain state.)
        /// </summary>
        /// <param name="restrictions">
        /// Змінені обмеження, для яких необхідно записати оновлення.
        ///
        /// (Modified restrictions whose updates must be recorded.)
        /// </param>
        /// <param name="now">
        /// Час операції, що передається до записів змін агрегату.
        ///
        /// (Operation time passed to the aggregate-change records.)
        /// </param>
        private void AddRestrictionUpdatedChanges(
            IReadOnlyCollection<UserRestriction> restrictions,
            DateTimeOffset now)
        {
            foreach (var restriction in restrictions)
            {
                AddAggregateChange(new UserRestrictionUpdated(
                    restriction,
                    now)
                );
            }
        }
    }
}
