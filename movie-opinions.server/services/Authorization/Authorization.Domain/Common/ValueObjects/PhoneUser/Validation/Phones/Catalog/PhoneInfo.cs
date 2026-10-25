namespace Authorization.Domain.Common.ValueObjects.PhoneUser.Validation.Phones.Catalog
{
    /// <summary>
    /// Описує міжнародні телефонні коди та допустимі довжини
    /// національної частини для одного запису каталогу.
    ///
    /// (Describes the international calling codes and allowed
    /// national number lengths for a single catalog entry.)
    /// </summary>
    internal class PhoneInfo
    {
        /// <summary>
        /// Міжнародні телефонні коди, пов’язані із записом.
        /// Коди зберігаються зі знаком «+».
        ///
        /// (The international calling codes associated with the entry.
        /// Codes are stored with the "+" sign.)
        /// </summary>
        public HashSet<string> CountryCodes { get; set; } =
            new HashSet<string>();

        /// <summary>
        /// Допустимі довжини національної частини в цифрах.
        /// Міжнародний код країни до цієї довжини не входить.
        ///
        /// (The allowed national number lengths in digits.
        /// The country calling code is excluded from this length.)
        /// </summary>
        public HashSet<int> NationalNumberLengths { get; set; } =
            new HashSet<int>();
    }
}
