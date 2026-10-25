using Authorization.Domain.Common.Errors;

namespace Authorization.Domain.Common.Validation.Interfaces
{
    /// <summary>
    /// Базовий контракт представлення порушення доменного правила,
    /// що містить очікувану доменну помилку.
    ///
    /// (Base contract for representing a domain-rule violation
    /// that contains an expected domain error.)
    /// </summary>
    internal interface IValidationFailure
    {
        /// <summary>
        /// Очікувана доменна помилка, що описує виявлене порушення.
        ///
        /// (The expected domain error describing the detected violation.)
        /// </summary>
        Error Error { get; }
    }
}
