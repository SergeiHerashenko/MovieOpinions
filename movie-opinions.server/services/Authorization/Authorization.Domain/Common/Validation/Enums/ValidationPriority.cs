namespace Authorization.Domain.Common.Validation.Enums
{
    /// <summary>
    /// Визначає порядок виконання доменних правил валідації.
    /// Правила з меншим числовим значенням виконуються раніше.
    ///
    /// (Defines the execution order of domain validation rules.
    /// Rules with lower numeric values are executed first.)
    /// </summary>
    public enum ValidationPriority
    {
        /// <summary>
        /// Перевірка наявності обов'язкового значення.
        /// 
        /// (Checks whether a required value is present.)
        /// </summary>
        Presence = 10,

        /// <summary>
        /// Перевірка доменних або бізнес-обмежень.
        /// 
        /// (Checks domain or business constraints.)
        /// </summary>
        BusinessRule = 20,

        /// <summary>
        /// Перевірка формату значення.
        /// 
        /// (Checks the value format.)
        /// </summary>
        Format = 30,

        /// <summary>
        /// Перевірка довжини або розміру значення.
        /// 
        /// (Checks the value length or size.)
        /// </summary>
        Length = 40,
    }
}
