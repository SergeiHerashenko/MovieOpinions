using Authorization.Domain.Users.Entities.UsersRestriction.ValueObjects.Restriction;
using Authorization.Domain.Users.Enums;

namespace Authorization.Domain.Users.Contracts
{
    /// <summary>
    /// Містить дані обмеження користувача: тип, правило,
    /// ініціатора накладення та необов’язкову причину.
    ///
    /// (Contains user restriction data: its type, rule,
    /// issuer, and optional reason.)
    /// </summary>
    public class RestrictionData
    {
        /// <summary>
        /// Тип обмеження користувача.
        ///
        /// (The user restriction type.)
        /// </summary>
        public required RestrictionType RestrictionType { get; set; }

        /// <summary>
        /// Правило, що визначає назву та тривалість обмеження у хвилинах.
        ///
        /// (The rule defining the restriction name and duration in minutes.)
        /// </summary>
        public required RestrictionRule RestrictionRule { get; set; }

        /// <summary>
        /// Ім’я або позначення ініціатора накладення обмеження.
        ///
        /// (The name or designation of the restriction issuer.)
        /// </summary>
        public required string ImposedBy { get; set; }

        /// <summary>
        /// Необов’язкова причина накладення обмеження.
        ///
        /// (The optional reason for imposing the restriction.)
        /// </summary>
        public string? Reason { get; set; }
    }
}
