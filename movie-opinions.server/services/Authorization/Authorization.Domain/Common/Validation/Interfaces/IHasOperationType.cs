using Authorization.Domain.Common.Exceptions.Enums;

namespace Authorization.Domain.Common.Validation.Interfaces
{
    /// <summary>
    /// Позначає дані валідації, що містять контекст
    /// доменної операції, під час якої виконується перевірка.
    ///
    /// (Marks validation data containing the domain-operation context
    /// in which validation is performed.)
    /// </summary>
    internal interface IHasOperationType : IValidationData
    {
        /// <summary>
        /// Операція, у контексті якої виконується валідація.
        ///
        /// (The operation in whose context validation is performed.)
        /// </summary>
        OperationType OperationType { get; }
    }
}
