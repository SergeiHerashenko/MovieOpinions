namespace Authorization.Domain.Common.Exceptions.Enums
{
    /// <summary>
    /// Операція, під час якої виник внутрішній виняток.
    /// Використовується як діагностичний контекст.
    ///
    /// (Operation during which an internal exception occurred.
    /// Used as diagnostic context.)
    /// </summary>
    public enum OperationType
    {
        /// <summary>
        /// Відновлення доменного об’єкта зі збереженого стану.
        ///
        /// (Restoring a domain object from persisted state.)
        /// </summary>
        Restore = 0,

        /// <summary>
        /// Створення доменного об’єкта.
        ///
        /// (Creating a domain object.)
        /// </summary>
        Create = 1,

        /// <summary>
        /// Зміна стану доменного об’єкта.
        ///
        /// (Changing the state of a domain object.)
        /// </summary>
        Update = 2,

        /// <summary>
        /// Видалення доменного об’єкта.
        ///
        /// (Deleting a domain object.)
        /// </summary>
        Delete = 3,

        /// <summary>
        /// Читання даних або доступ до значення.
        ///
        /// (Reading data or accessing a value.)
        /// </summary>
        Read = 4,

        /// <summary>
        /// Порівняння доменних об’єктів або їхніх значень.
        ///
        /// (Comparing domain objects or their values.)
        /// </summary>
        Compare = 5
    }
}
