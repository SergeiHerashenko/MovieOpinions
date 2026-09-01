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
        DataInconsistency = 1,

        InvalidOperation = 2,

        InvariantViolation = 3
    }
}
