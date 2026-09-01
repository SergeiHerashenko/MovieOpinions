namespace Authorization.Domain.Common.Models.Interfaces
{
    /// <summary>
    /// Контракт для доменних сутностей та агрегатів,
    /// які накопичують зміни стану до моменту їх обробки.
    ///
    /// (Contract for domain entities and aggregates that accumulate
    /// state changes until they are processed.)
    /// </summary>
    public interface IHasAggregateChanges
    {
        /// <summary>
        /// Накопичені зміни агрегату, що очікують обробки або збереження.
        /// 
        /// (Accumulated aggregate changes awaiting processing or persistence.)
        /// </summary>
        IReadOnlyList<IAggregateChange> AggregateChanges { get; }

        /// <summary>
        /// Очищає накопичені зміни агрегату.
        /// Повинен викликатися після їх успішної обробки.
        ///
        /// (Clears accumulated aggregate changes.
        /// Should be called after they have been successfully processed.)
        /// </summary>
        void ClearAggregateChanges();
    }
}
