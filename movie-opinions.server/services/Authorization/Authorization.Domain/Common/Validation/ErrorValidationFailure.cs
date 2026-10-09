using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Validation.Interfaces;

namespace Authorization.Domain.Common.Validation
{
    /// <summary>
    /// Представляє очікуване порушення доменного правила,
    /// яке може бути повернене як доменна помилка
    /// та не потребує побудови винятку.
    ///
    /// (Represents an expected domain-rule violation
    /// that can be returned as a domain error
    /// and does not require exception construction.)
    /// </summary>
    internal sealed class ErrorValidationFailure : IValidationFailure
    {
        public required Error Error { get; init; }
    }
}
