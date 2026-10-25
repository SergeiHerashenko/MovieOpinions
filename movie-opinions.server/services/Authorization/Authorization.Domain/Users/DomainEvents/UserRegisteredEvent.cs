using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.Users.DomainEvents
{
    /// <summary>
    /// Доменна подія, що фіксує завершення реєстрації користувача.
    ///
    /// (Domain event indicating that user registration was completed.)
    /// </summary>
    public sealed class UserRegisteredEvent : DomainEvent
    {
        public UserId UserId { get; }

        public Login Login { get; }

        /// <summary>
        /// Створює подію завершення реєстрації користувача.
        ///
        /// (Creates an event representing completed user registration.)
        /// </summary>
        /// <param name="userId">
        /// Ідентифікатор зареєстрованого користувача.
        ///
        /// (The registered user identifier.)
        /// </param>
        /// <param name="login">
        /// Логін зареєстрованого користувача.
        ///
        /// (The registered user login.)
        /// </param>
        /// <param name="occurredOn">
        /// Час завершення реєстрації.
        ///
        /// (The registration completion time.)
        /// </param>
        public UserRegisteredEvent(
            UserId userId,
            Login login,
            DateTimeOffset occurredOn)
            : base(occurredOn)
        {
            UserId = userId;
            Login = login;
        }
    }
}
