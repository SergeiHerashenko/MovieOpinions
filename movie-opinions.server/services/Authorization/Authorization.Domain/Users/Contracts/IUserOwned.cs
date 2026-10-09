using Authorization.Domain.Users.ValueObjects;

namespace Authorization.Domain.Users.Contracts
{
    /// <summary>
    /// Визначає контракт доменного об’єкта,
    /// який належить конкретному користувачу.
    ///
    /// (Defines a contract for a domain object
    /// owned by a specific user.)
    /// </summary>
    internal interface IUserOwned
    {
        /// <summary>
        /// Ідентифікатор користувача, якому належить об’єкт.
        ///
        /// (Identifier of the user who owns the object.)
        /// </summary>
        UserId UserId { get; }
    }
}
