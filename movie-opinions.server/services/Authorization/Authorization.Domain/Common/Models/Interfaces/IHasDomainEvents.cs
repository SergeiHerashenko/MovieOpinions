namespace Authorization.Domain.Common.Models.Interfaces
{
    /// <summary>
    /// Контракт для доменних сутностей та агрегатів,
    /// які накопичують доменні події до моменту їх публікації.
    ///
    /// (Contract for domain entities and aggregates that accumulate
    /// domain events until they are dispatched.)
    /// </summary>
    public interface IHasDomainEvents
    {
        /// <summary>
        /// Накопичені доменні події, що очікують публікації.
        /// 
        /// (Accumulated domain events awaiting dispatch.)
        /// </summary>
        IReadOnlyList<IDomainEvent> DomainEvents { get; }

        /// <summary>
        /// Очищає накопичені доменні події.
        /// Повинен викликатися після їх успішної публікації.
        ///
        /// (Clears accumulated domain events.
        /// Should be called after they have been successfully dispatched.)
        /// </summary>
        void ClearDomainEvents();
    }
}
