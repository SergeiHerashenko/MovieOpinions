namespace Authorization.Domain.Common.Exceptions.Enums
{
    /// <summary>
    /// Категорія внутрішнього винятку, яка визначає його семантику
    /// та спосіб централізованої обробки.
    ///
    /// (Category of an internal exception that defines its semantics
    /// and centralized handling strategy.)
    /// </summary>
    public enum ExceptionType
    {
        /// <summary>
        /// Некоректні, неузгоджені або непідтримувані доменні дані.
        ///
        /// (Invalid, inconsistent, or unsupported domain data.)
        /// </summary>
        DataInconsistency = 0,

        /// <summary>
        /// Некоректне використання операції або порушення її передумов.
        ///
        /// (Invalid use of an operation or violation of its preconditions.)
        /// </summary>
        InvalidOperation = 1,

        /// <summary>
        /// Порушення інваріантів доменного об’єкта.
        ///
        /// (Violation of a domain object's invariants.)
        /// </summary>
        InvariantViolation = 2
    }
}
