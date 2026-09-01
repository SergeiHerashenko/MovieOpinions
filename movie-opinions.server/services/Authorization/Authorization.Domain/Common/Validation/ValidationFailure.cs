using Authorization.Domain.Common.Errors;

namespace Authorization.Domain.Common.Validation
{
    /// <summary>
    /// Базове представлення невдалого результату доменної валідації.
    /// 
    /// (Base representation of a failed domain validation result.)
    /// </summary>
    public class ValidationFailure
    {
        /// <summary>
        /// Доменна помилка, яка описує причину невдалої перевірки.
        /// 
        /// (Domain error describing the validation failure.)
        /// </summary>
        public required Error Error { get; init; }
    }
}
