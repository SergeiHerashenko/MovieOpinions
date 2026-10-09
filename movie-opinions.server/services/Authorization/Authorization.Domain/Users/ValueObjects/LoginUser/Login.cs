using Authorization.Domain.Common.Exceptions.Enums;
using Authorization.Domain.Common.Guard;
using Authorization.Domain.Common.Models;
using Authorization.Domain.Users.Enums;
using Authorization.Domain.Users.ValueObjects.EmailUser;
using Authorization.Domain.Users.ValueObjects.PhoneUser;
using Authorization.Domain.Common.Exceptions.DomainException;

namespace Authorization.Domain.Users.ValueObjects.LoginUser
{
    /// <summary>
    /// Визначає базове доменне представлення логіна користувача.
    ///
    /// Конкретний тип логіна інкапсулює валідну email-адресу
    /// або телефонний номер і надає їх канонічне рядкове значення.
    ///
    /// (Defines the base domain representation of a user login.
    ///
    /// A concrete login type encapsulates either a valid email address
    /// or telephone number and exposes its canonical string value.)
    /// </summary>
    public abstract class Login : ValueObject
    {
        /// <summary>
        /// Канонічне рядкове значення логіна.
        ///
        /// (Canonical string value of the login.)
        /// </summary>
        public abstract string Value { get; }

        /// <summary>
        /// Тип ідентифікатора, використаного як логін.
        ///
        /// (Type of identifier used as the login.)
        /// </summary>
        public abstract LoginType Type { get; }

        /// <summary>
        /// Створює логін на основі попередньо перевіреної email-адреси.
        ///
        /// (Creates a login from a previously validated email address.)
        /// </summary>
        /// <param name="email">Валідна email-адреса користувача.</param>
        /// <returns>Логін типу Email.</returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо email-адреса відсутня.
        /// </exception>
        public static Login From(Email email)
        {
            DomainGuard.AgainstNull<Login>(
                OperationType.Create,
                (email, nameof(email))
            );

            return new EmailLogin(email);
        }

        /// <summary>
        /// Створює логін на основі попередньо перевіреного
        /// телефонного номера.
        ///
        /// (Creates a login from a previously validated telephone number.)
        /// </summary>
        /// <param name="phone">Валідний телефонний номер користувача.</param>
        /// <returns>Логін типу Phone.</returns>
        /// <exception cref="DomainDataInconsistencyException">
        /// Виникає, якщо телефонний номер відсутній.
        /// </exception>
        public static Login From(Phone phone)
        {
            DomainGuard.AgainstNull<Login>(
                OperationType.Create,
                (phone, nameof(phone))
            );

            return new PhoneLogin(phone);
        }
    }
}
