namespace Authorization.Domain.Common.Errors.Enums
{
    /// <summary>
    /// Визначає категорію очікуваної помилки,
    /// представленої доменним об’єктом Error.
    ///
    /// Не використовується для класифікації доменних винятків,
    /// які представляють неочікувані збої або неузгодженість стану.
    ///
    /// (Defines the category of an expected failure represented
    /// by a domain Error object.
    ///
    /// It is not used to classify domain exceptions, which represent
    /// unexpected failures or state inconsistencies.)
    /// </summary>
    public enum ErrorType
    {
        /// <summary>
        /// Некоректні вхідні дані: відсутнє значення, неправильний формат,
        /// недопустима довжина або вихід за дозволений діапазон.
        ///
        /// (Invalid input data: a missing value, invalid format,
        /// invalid length, or a value outside the allowed range.)
        /// </summary>
        Validation = 0,

        /// <summary>
        /// Запитуваний ресурс або доменний об’єкт не знайдено.
        ///
        /// (The requested resource or domain object was not found.)
        /// </summary>
        NotFound = 1,

        /// <summary>
        /// Операція конфліктує з поточним станом ресурсу,
        /// наявними даними або паралельною зміною.
        ///
        /// (The operation conflicts with the current resource state,
        /// existing data, or a concurrent modification.)
        /// </summary>
        Conflict = 2,

        /// <summary>
        /// Виконання операції заборонене через відсутність дозволу
        /// або дію доменної політики доступу.
        ///
        /// (The operation is forbidden due to insufficient permission
        /// or an applicable domain access policy.)
        /// </summary>
        Forbidden = 3,

        /// <summary>
        /// Коректно сформований запит не може бути виконаний,
        /// оскільки порушує очікуване бізнес-правило або обмеження.
        ///
        /// (A well-formed request cannot be completed because it violates
        /// an expected business rule or constraint.)
        /// </summary>
        BusinessRule = 4,
    }
}
