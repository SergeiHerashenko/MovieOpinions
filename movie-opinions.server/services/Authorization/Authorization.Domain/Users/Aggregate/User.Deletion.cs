using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Results;
using Authorization.Domain.Users.AggregateChanges.Deletion;
using Authorization.Domain.Users.DomainEvents.Deletion;
using Authorization.Domain.Users.Entities.UsersDeletion;
using Authorization.Domain.Users.Entities.UsersDeletion.ValueObjects;
using Authorization.Domain.Users.Errors;

namespace Authorization.Domain.Users.Aggregate
{
    public partial class User
    {
        /// <summary>
        /// Переводить обліковий запис у стан видалення.
        ///
        /// Перевіряє вхідні дані та часову послідовність операції.
        /// Якщо обліковий запис уже перебуває у стані видалення,
        /// повертає доменну помилку без повторної зміни стану.
        ///
        /// Після успішного створення сутності видалення зберігає її
        /// в агрегаті та записує відповідну зміну агрегату.
        /// Доменну подію про підтверджене видалення записує зовнішній
        /// сценарій підтвердження відкладеної дії.
        ///
        /// (Transitions the account into the deleted state.
        ///
        /// Validates the supplied data and chronological consistency
        /// of the operation. If the account is already in the deleted state,
        /// returns a domain error without changing the state again.
        ///
        /// After successfully creating the deletion entity, stores it
        /// in the aggregate and records the corresponding aggregate change.
        /// The confirmed-deletion domain event is recorded by the enclosing
        /// pending-action confirmation flow.)
        /// </summary>
        /// <param name="now">
        /// Час видалення облікового запису.
        ///
        /// (Time the account is marked as deleted.)
        /// </param>
        /// <param name="reason">
        /// Причина видалення облікового запису.
        ///
        /// (Account deletion reason.)
        /// </param>
        /// <returns>
        /// Успіх після створення стану видалення або помилка,
        /// якщо обліковий запис уже видалений.
        ///
        /// (Success after establishing the deletion state,
        /// or an error if the account is already deleted.)
        /// </returns>
        public Result Delete(
            DateTimeOffset now,
            DeletionReason reason)
        {
            DomainGuard.AgainstNull<User>(
                OperationType.Update,
                (reason, nameof(reason))
            );

            EnsureValidOperationTimestamp(now);

            if (IsDeleted)
            {
                return Result.Failure(UserErrors.UserAlreadyDeleted<User>());
            }

            var deletion = UserDeletion.Create(
                Id,
                Login,
                now,
                reason
            );

            _deletion = deletion;

            AddAggregateChange(new UserDeletionCreated(
                deletion,
                now)
            );

            UpdatedAt = now;

            return Result.Success();
        }

        /// <summary>
        /// Відновлює видалений обліковий запис протягом дозволеного
        /// періоду відновлення.
        ///
        /// Делегує перевірку та перехід стану сутності видалення.
        /// Після успішного відновлення очищає поточний стан видалення,
        /// оновлює час зміни користувача та записує доменну подію
        /// і відповідну зміну агрегату.
        ///
        /// Якщо обліковий запис не видалений або більше не може бути
        /// відновлений, повертає доменну помилку без подальшої зміни стану.
        ///
        /// (Restores a deleted account within the allowed restoration period.
        ///
        /// Delegates validation and the state transition to the deletion entity.
        /// After successful restoration, clears the current deletion state,
        /// updates the user's modification timestamp, and records the
        /// corresponding domain event and aggregate change.
        ///
        /// If the account is not deleted or can no longer be restored,
        /// returns a domain error without any subsequent state mutation.)
        /// </summary>
        /// <param name="now">
        /// Час відновлення облікового запису.
        ///
        /// (Time the account is restored.)
        /// </param>
        /// <returns>
        /// Успіх після відновлення або відповідна доменна помилка.
        ///
        /// (Success after restoration, or the corresponding domain error.)
        /// </returns>
        public Result Undelete(DateTimeOffset now)
        {
            EnsureValidOperationTimestamp(now);

            var deletion = _deletion;

            if (deletion is null)
            {
                return Result.Failure(UserErrors.UserNotDeleted<User>());
            }

            var result = deletion.Undelete(now);

            if (result.IsFailure)
            {
                return result;
            }

            _deletion = null;
            UpdatedAt = now;

            AddDomainEvent(new UserAccountRestoredEvent(
                Id,
                deletion.Id,
                deletion.Login,
                now)
            );

            AddAggregateChange(new UserDeletionUpdated(
                deletion,
                now)
            );

            return Result.Success();
        }

        /// <summary>
        /// Намагається перевести поточний запис про видалення користувача
        /// в остаточний стан.
        ///
        /// Делегує сутності видалення перевірку завершення періоду
        /// відновлення та можливості переходу з її поточного стану.
        ///
        /// Після успішного переходу записує доменну подію завершення
        /// видалення та відповідну зміну агрегату.
        /// Якщо запис відсутній або перехід не відбувся, повертає false
        /// без зміни стану та реєстрації подій чи змін агрегату.
        ///
        /// (Attempts to transition the current user deletion record
        /// to its final state.
        ///
        /// Delegates checks of the restoration deadline and transition
        /// eligibility to the deletion entity.
        ///
        /// After a successful transition, records the deletion-finalized
        /// domain event and the corresponding aggregate change.
        /// If the record is absent or no transition occurs, returns false
        /// without modifying state or recording events or aggregate changes.)
        /// </summary>
        /// <param name="now">
        /// Час перевірки та можливої фіналізації стану видалення.
        ///
        /// (Time of the check and possible finalization of the deletion state.)
        /// </param>
        /// <returns>
        /// <see langword="true"/>, якщо сутність видалення переведено
        /// в остаточний стан; інакше — <see langword="false"/>.
        ///
        /// (<see langword="true"/> if the deletion entity transitions
        /// to its final state; otherwise, <see langword="false"/>.)
        /// </returns>
        public bool TryFinalizeDeletion(DateTimeOffset now)
        {
            EnsureValidOperationTimestamp(now);

            var deletion = _deletion;

            if (deletion is null)
            {
                return false;
            }

            if (!deletion.TryMarkAsPermanentlyDeleted(now))
            {
                return false;
            }

            AddDomainEvent(new UserAccountDeletionFinalizedEvent(
                Id,
                deletion.Id,
                deletion.Login,
                now)
            );

            AddAggregateChange(new UserDeletionUpdated(
                deletion,
                now)
            );

            UpdatedAt = now;

            return true;
        }

        /// <summary>
        /// Повертає поточну сутність видалення облікового запису.
        ///
        /// Метод не змінює стан агрегату.
        ///
        /// (Returns the current account-deletion entity.
        ///
        /// The method does not modify aggregate state.)
        /// </summary>
        /// <returns>
        /// Сутність видалення або помилка, якщо обліковий запис
        /// не перебуває у стані видалення.
        /// (The deletion entity, or an error if the account
        /// is not in the deleted state.)
        /// </returns>
        public Result<UserDeletion> GetDeletion()
        {
            if (_deletion is null)
            {
                return Result<UserDeletion>.Failure(UserErrors.UserNotDeleted<User>());
            }

            return Result<UserDeletion>.Success(_deletion);
        }
    }
}
