using Authorization.Domain.Common.Exceptions.Enums;

namespace Authorization.Domain.Common.Validation.Interfaces
{
    /// <summary>
    /// Розширений контракт представлення порушення,
    /// що додатково підтримує побудову доменного винятку
    /// з урахуванням типу виконуваної операції.
    ///
    /// (Extended violation contract that additionally supports
    /// constructing a domain exception using the current operation type.)
    /// </summary>
    internal interface IExceptionValidationFailure : IValidationFailure
    {
        Func<OperationType, Exception> BuildException { get; }
    }
}
