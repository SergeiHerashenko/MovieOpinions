namespace Authorization.Domain.Common.ValueObjects.PhoneUser.Validation.Phones.Catalog
{
    /// <summary>
    /// Визначає підсумок перевірки міжнародного коду країни
    /// та довжини національного номера за каталогом.
    ///
    /// (Defines the outcome of checking a country calling code
    /// and national number length against the catalog.)
    /// </summary>
    internal enum PhoneNumberingPlanCheckStatus
    {
        /// <summary>
        /// Каталог містить запис, який підтримує переданий код
        /// і довжину національного номера.
        ///
        /// (The catalog contains an entry supporting the supplied
        /// calling code and national number length.)
        /// </summary>
        Valid = 0,

        /// <summary>
        /// Переданий міжнародний код відсутній у каталозі.
        ///
        /// (The supplied country calling code is absent from the catalog.)
        /// </summary>
        UnknownCountryCode = 1,

        /// <summary>
        /// Довжина національного номера менша за найменшу
        /// допустиму довжину для переданого коду.
        ///
        /// (The national number length is below the minimum
        /// allowed length for the supplied calling code.)
        /// </summary>
        NationalNumberTooShort = 2,

        /// <summary>
        /// Довжина національного номера більша за найбільшу
        /// допустиму довжину для переданого коду.
        ///
        /// (The national number length exceeds the maximum
        /// allowed length for the supplied calling code.)
        /// </summary>
        NationalNumberTooLong = 3,

        /// <summary>
        /// Довжина національного номера перебуває між мінімальною
        /// та максимальною допустимими довжинами,
        /// але не входить до набору дозволених значень.
        ///
        /// (The national number length falls between the minimum
        /// and maximum allowed lengths but is not included
        /// in the set of permitted values.)
        /// </summary>
        UnsupportedNationalNumberLength = 4,
    }
}
