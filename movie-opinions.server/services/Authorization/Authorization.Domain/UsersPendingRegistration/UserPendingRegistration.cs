using Authorization.Domain.Common.Exceptions.DomainException;
using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Common.ValueObjects.LoginUser;
using Authorization.Domain.Common.ValueObjects.PasswordUser;
using Authorization.Domain.UsersPendingRegistration.DomainEvents;
using Authorization.Domain.UsersPendingRegistration.ValueObjects;

namespace Authorization.Domain.UsersPendingRegistration
{
    /// <summary>
    /// Тимчасовий агрегат незавершеної реєстрації користувача.
    /// Зберігає підготовлені облікові дані, ідентифікатор поточного
    /// реєстраційного потоку та строк його дії.
    ///
    /// (Temporary aggregate representing an incomplete user registration.
    /// Stores prepared credentials, the current registration-flow identifier,
    /// and its expiration time.)
    /// </summary>
    public sealed class UserPendingRegistration : AggregateRoot<UserPendingRegistrationId, Guid>
    {
        private static readonly TimeSpan RegistrationLifetime = TimeSpan.FromHours(1);

        #region Properties
        /// <summary>
        /// Логін, для якого виконується реєстрація.
        ///
        /// (Login for which registration is being performed.)
        /// </summary>
        public Login Login { get; }

        /// <summary>
        /// Пароль у доменному представленні, який буде використано
        /// після успішного підтвердження реєстрації.
        ///
        /// (Password in its domain representation to be used
        /// after successful registration confirmation.)
        /// </summary>
        public Password Password { get; private set; }

        /// <summary>
        /// Непрозорий ідентифікатор поточного реєстраційного потоку.
        ///
        /// (Opaque identifier of the current registration flow.)
        /// </summary>
        public RegistrationFlowToken RegistrationFlowToken { get; private set; }

        /// <summary>
        /// Дата й час завершення строку дії поточного реєстраційного потоку.
        ///
        /// (Date and time when the current registration flow expires.)
        /// </summary>
        public DateTimeOffset ExpiresAt { get; private set; }
        #endregion

        #region Creation
        private UserPendingRegistration(
            UserPendingRegistrationId userPendingRegistrationId,
            Login login,
            Password password,
            RegistrationFlowToken registrationFlowToken,
            DateTimeOffset now)
            : base(userPendingRegistrationId, now)
        {
            Login = login;
            Password = password;
            RegistrationFlowToken = registrationFlowToken;
            ExpiresAt = now.Add(RegistrationLifetime);
        }

        /// <summary>
        /// Створює нову незавершену реєстрацію, генерує її ID і flow token,
        /// встановлює строк дії та реєструє доменну подію.
        ///
        /// (Creates a new pending registration, generates its ID and flow token,
        /// sets its lifetime, and records a domain event.)
        /// </summary>
        /// <param name="login">
        /// Валідний логін майбутнього користувача.
        ///
        /// (The valid login of the prospective user.)
        /// </param>
        /// <param name="password">
        /// Валідний пароль у доменному представленні.
        ///
        /// (The valid password in its domain representation.)
        /// </param>
        /// <param name="now">
        /// Поточний час доменної операції.
        ///
        /// (The current domain operation timestamp.)
        /// </param>
        /// <returns>
        /// Створена незавершена реєстрація.
        ///
        /// (The created pending registration.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо обов’язковий доменний об’єкт дорівнює null.
        ///
        /// (Thrown when a required domain object is null.)
        /// </exception>
        public static UserPendingRegistration Create(
            Login login,
            Password password,
            DateTimeOffset now)
        {
            DomainGuard.AgainstNull<UserPendingRegistration>(
                OperationType.Create,
                (login, nameof(login)),
                (password, nameof(password))
            );

            var userPendingRegistration = new UserPendingRegistration(
                UserPendingRegistrationId.Create(),
                login,
                password,
                RegistrationFlowToken.Create(),
                now
            );

            userPendingRegistration.AddDomainEvent(
                new UserRegistrationRequestedEvent(
                    userPendingRegistration.Id,
                    userPendingRegistration.Login,
                    userPendingRegistration.CreatedAt
                )
            );

            return userPendingRegistration;
        }
        #endregion

        #region Restoration
        private UserPendingRegistration(
            UserPendingRegistrationId userPendingRegistrationId,
            Login login,
            Password password,
            RegistrationFlowToken registrationFlowToken,
            DateTimeOffset createdAt,
            DateTimeOffset expiresAt)
            : base(userPendingRegistrationId, createdAt)
        {
            Login = login;
            Password = password;
            RegistrationFlowToken = registrationFlowToken;
            ExpiresAt = expiresAt;
        }

        /// <summary>
        /// Відновлює агрегат зі збереженого стану без створення доменних подій.
        ///
        /// (Restores the aggregate from persisted state without recording
        /// domain events.)
        /// </summary>
        /// <param name="userPendingRegistrationId">
        /// Ідентифікатор агрегату.
        ///
        /// (The aggregate identifier.)
        /// </param>
        /// <param name="login">
        /// Збережений логін.
        ///
        /// (The stored login.)
        /// </param>
        /// <param name="password">
        /// Збережений пароль у доменному представленні.
        ///
        /// (The stored password in its domain representation.)
        /// </param>
        /// <param name="registrationFlowToken">
        /// Збережений flow token.
        ///
        /// (The stored flow token.)
        /// </param>
        /// <param name="createdAt">
        /// Час створення агрегату.
        ///
        /// (The aggregate creation timestamp.)
        /// </param>
        /// <param name="expiresAt">
        /// Час завершення строку дії.
        ///
        /// (The expiration timestamp.)
        /// </param>
        /// <returns>
        /// Відновлений агрегат.
        ///
        /// (The restored aggregate.)
        /// </returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо обов’язковий доменний об’єкт дорівнює null.
        ///
        /// (Thrown when a required domain object is null.)
        /// </exception>
        /// <exception cref="DomainInvariantViolationException">
        /// Виникає, якщо строк дії завершується не пізніше часу створення.
        ///
        /// (Thrown when the expiration time precedes or equals the creation time.)
        /// </exception>
        public static UserPendingRegistration Restore(
            UserPendingRegistrationId userPendingRegistrationId,
            Login login,
            Password password,
            RegistrationFlowToken registrationFlowToken,
            DateTimeOffset createdAt,
            DateTimeOffset expiresAt)
        {
            DomainGuard.AgainstNull<UserPendingRegistration>(
                OperationType.Restore,
                (userPendingRegistrationId, nameof(userPendingRegistrationId)),
                (login, nameof(login)),
                (password, nameof(password)),
                (registrationFlowToken, nameof(registrationFlowToken))
            );

            if (expiresAt <= createdAt)
            {
                throw DomainInvariantViolationException.BrokenState<UserPendingRegistration>(
                    "Expiration time must be later than creation time.",
                    new Dictionary<string, object?>
                    {
                        ["CreatedAt"] = createdAt,
                        ["ExpiresAt"] = expiresAt
                    },
                    OperationType.Restore
                );
            }

            return new UserPendingRegistration(
                userPendingRegistrationId,
                login,
                password,
                registrationFlowToken,
                createdAt,
                expiresAt
            );
        }
        #endregion

        #region Behavior
        /// <summary>
        /// Поновлює реєстраційний потік: замінює пароль і flow token,
        /// повторно встановлює строк дії та реєструє доменну подію.
        ///
        /// (Renews the registration flow by replacing the password and flow token,
        /// resetting its lifetime, and recording a domain event.)
        /// </summary>
        /// <param name="password">
        /// Новий валідний пароль у доменному представленні.
        ///
        /// (The new valid password in its domain representation.)
        /// </param>
        /// <param name="now">
        /// Поточний час доменної операції.
        ///
        /// (The current time of the domain operation.)
        /// </param>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо пароль дорівнює null або час операції передує створенню агрегату.
        ///
        /// (Thrown when the password is null or the operation time precedes the aggregate creation.)
        /// </exception>
        public void Refresh(
            Password password,
            DateTimeOffset now)
        {
            DomainGuard.AgainstNull<UserPendingRegistration>(
                OperationType.Update,
                (password, nameof(password))
            );

            DomainGuard.AgainstEarlierThan<UserPendingRegistration>(
                OperationType.Update,
                (now, nameof(now)),
                (CreatedAt, nameof(CreatedAt))
            );

            Password = password;
            RegistrationFlowToken = RegistrationFlowToken.Create();
            ExpiresAt = now.Add(RegistrationLifetime);

            AddDomainEvent(new UserRegistrationRequestedEvent(
                Id,
                Login,
                now)
            );
        }

        /// <summary>
        /// Визначає, чи завершився строк дії реєстраційного потоку
        /// на вказаний момент часу.
        ///
        /// (Determines whether the registration flow has expired
        /// at the specified point in time.)
        /// </summary>
        /// <param name="pointInTime">
        /// Час, відносно якого виконується перевірка.
        ///
        /// (The time against which the check is performed.)
        /// </param>
        /// <returns>
        /// true, якщо pointInTime дорівнює ExpiresAt або перевищує його; інакше false.
        ///
        /// (true if pointInTime is greater than or equal to ExpiresAt; otherwise, false.)
        /// </returns>
        public bool IsExpired(DateTimeOffset pointInTime)
            => pointInTime >= ExpiresAt;
        #endregion
    }
}
