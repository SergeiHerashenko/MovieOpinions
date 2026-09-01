using Authorization.Domain.Common.Models.Interfaces;

namespace Authorization.Domain.Common.Models
{
    /// <summary>
    /// Базовий абстрактний клас для зафіксованих змін стану агрегату,
    /// які очікують подальшого застосування або збереження.
    /// На відміну від доменної події, Aggregate Change описує зміну стану,
    /// а не бізнес-факт для публікації іншим обробникам.
    /// Зберігає час зміни, переданий під час її створення.
    ///
    /// (Base abstract class for recorded aggregate state changes awaiting
    /// further processing or persistence. Unlike a domain event, an aggregate
    /// change describes a state mutation rather than a business fact intended
    /// for publication. Stores the occurrence time supplied at creation.)
    /// </summary>
    public abstract class AggregateChange : IAggregateChange
    {
        /// <summary>
        /// Дата та час, коли відбулася зміна агрегату.
        /// Значення передається під час створення зміни.
        ///
        /// (Date and time when the aggregate change occurred.
        /// The value is supplied when the change is created.)
        /// </summary>
        public DateTimeOffset OccurredOn { get; }

        /// <summary>
        /// Ініціалізує базовий стан зміни агрегату.
        /// (Initializes the base state of an aggregate change.)
        /// </summary>
        /// <param name="occurredOn">Час настання зміни.</param>
        protected AggregateChange(DateTimeOffset occurredOn)
        {
            OccurredOn = occurredOn;
        }
    }
}
