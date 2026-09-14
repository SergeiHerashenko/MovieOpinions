using Authorization.Domain.Common.Errors;
using Authorization.Domain.Common.Exceptions.Enums;

namespace Authorization.Domain.Common.Validation
{
    /// <summary>
    /// Містить обидва можливі представлення порушеного правила:
    /// очікувану помилку та фабрику доменного винятку.
    ///
    /// (Contains both possible representations of a violated rule:
    /// an expected error and a domain-exception factory.)
    /// </summary>
    internal class ValidationFailure
    {
        /// <summary>
        /// Доменна помилка, яка описує причину невдалої перевірки.
        /// 
        /// (Domain error describing the validation failure.)
        /// </summary>
        public required Error Error { get; init; }

        /// <summary>
        /// Фабрика, яка відкладено створює exception для виявленої
        /// неузгодженості відновлених даних.
        ///
        /// (Factory that lazily creates an exception for the detected
        /// inconsistency in restored data.)
        /// </summary>
        public required Func<OperationType, Exception> BuildException { get; init; }
    }
}
