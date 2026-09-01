namespace Authorization.Domain.Common.Validation
{
    /// <summary>
    /// Представляє невдалу перевірку під час відновлення доменного об'єкта.
    /// Окрім доменної помилки, містить фабрику відповідного технічного exception.
    ///
    /// (Represents a validation failure encountered while restoring a domain
    /// object. In addition to the domain error, contains a factory for the
    /// corresponding technical exception.)
    /// </summary>
    public sealed class ValidationRestoreFailure : ValidationFailure
    {
        /// <summary>
        /// Фабрика, яка відкладено створює exception для виявленої
        /// неузгодженості відновлених даних.
        ///
        /// (Factory that lazily creates an exception for the detected
        /// inconsistency in restored data.)
        /// </summary>
        public required Func<Exception> BuildException { get; init; }
    }
}
