using Authorization.Domain.Users.AggregateChanges.Restriction;
using Authorization.Domain.Users.Entities.UsersRestriction;
using Authorization.Domain.Users.Entities.UsersRestrictionSession;

namespace Authorization.Domain.Users
{
    public partial class User
    {
        /// <summary>
        /// Переводить попередньо перевірене обмеження
        /// у завершений стан і видаляє його з активної
        /// колекції агрегату.
        ///
        /// (Transitions a pre-validated restriction to the completed state
        /// and removes it from the aggregate’s active collection.)
        /// </summary>
        /// <param name="restriction">Обмеження для завершення.</param>
        /// <param name="now">Час завершення обмеження.</param>
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
        /// (Transitions a pre-validated restriction to the revoked state
        /// and removes it from the aggregate’s active collection.)
        /// </summary>
        /// <param name="restriction">Обмеження для відкликання.</param>
        /// <param name="now">Час відкликання обмеження.</param>
        private void RevokeAndRemoveRestriction(
            UserRestriction restriction,
            DateTimeOffset now)
        {
            restriction.RevokeRestriction(now);
            _restrictions.Remove(restriction);
        }

        /// <summary>
        /// Видаляє оброблену сесію з активної колекції агрегату.
        ///
        /// Метод викликається після переходу всіх її обмежень
        /// у відповідні кінцеві стани.
        ///
        /// (Removes a processed session from the aggregate’s active collection.
        ///
        /// The method is called after all of its restrictions have transitioned
        /// to their corresponding terminal states.)
        /// </summary>
        /// <param name="session">Сесія, яку необхідно видалити.</param>
        private void RemoveRestrictionSession(UserRestrictionSession session)
        {
            _restrictionSessions.Remove(session);
        }

        /// <summary>
        /// Записує зміну агрегату для кожного зміненого обмеження.
        ///
        /// Метод не змінює доменний стан самих обмежень.
        ///
        /// (Records an aggregate change for every modified restriction.
        ///
        /// The method does not modify the domain state of the restrictions.)
        /// </summary>
        /// <param name="restrictions">Змінені обмеження, які необхідно зберегти.</param>
        /// <param name="now">Час виникнення змін агрегату.</param>
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
