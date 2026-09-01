namespace Authorization.Domain.Common.Models.Interfaces
{
    /// <summary>
    /// Представляє зафіксовану зміну стану агрегату,
    /// що очікує подальшої обробки або збереження.
    /// Не є доменною подією для публікації бізнес-обробникам.
    ///
    /// (Represents a recorded aggregate state change awaiting further
    /// processing or persistence. It is not a domain event intended for
    /// publication to business handlers.)
    /// </summary>
    public interface IAggregateChange
    {
        /// <summary>
        /// Дата та час настання зміни агрегату.
        /// 
        /// (Date and time when the aggregate change occurred.)
        /// </summary>
        DateTimeOffset OccurredOn { get; }
    }
}
