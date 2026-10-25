namespace Authorization.Domain.Common.ValueObjects.PhoneUser.Validation.Phones.Catalog
{
    /// <summary>
    /// Представляє результат перевірки коду країни та довжини
    /// національного номера за каталогом.
    ///
    /// Ознака успішності обчислюється зі статусу перевірки.
    ///
    /// (Represents the result of checking a country calling code
    /// and national number length against the catalog.
    ///
    /// The success indicator is derived from the check status.)
    /// </summary>
    internal sealed class PhoneNumberingPlanCheckResult
    {
        /// <summary>
        /// Вказує, чи завершилася перевірка зі статусом
        /// <see cref="PhoneNumberingPlanCheckStatus.Valid"/>.
        ///
        /// (Indicates whether the check completed with the
        /// <see cref="PhoneNumberingPlanCheckStatus.Valid"/> status.)
        /// </summary>
        public bool IsSuccess =>
            Status == PhoneNumberingPlanCheckStatus.Valid;

        /// <summary>
        /// Конкретний підсумок перевірки.
        ///
        /// (The specific outcome of the check.)
        /// </summary>
        public PhoneNumberingPlanCheckStatus Status { get; }

        /// <summary>
        /// Ініціалізує результат перевірки переданим статусом.
        ///
        /// (Initializes the check result with the supplied status.)
        /// </summary>
        /// <param name="status">
        /// Підсумок перевірки коду країни та довжини номера.
        ///
        /// (The outcome of checking the calling code and number length.)
        /// </param>
        internal PhoneNumberingPlanCheckResult(PhoneNumberingPlanCheckStatus status)
        {
            Status = status;
        }
    }
}
