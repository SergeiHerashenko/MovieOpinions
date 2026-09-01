using Authorization.Domain.Common.Models.Interfaces;

namespace Authorization.Domain.Common.Models
{
    /// <summary>
    /// Базовий абстрактний клас для всіх доменних подій.
    /// Зберігає час настання події, переданий під час її створення.
    ///
    /// (Base abstract class for all domain events.
    /// Stores the occurrence time supplied when the event is created.)
    /// </summary>
    public abstract class DomainEvent : IDomainEvent
    {
        /// <summary>
        /// Дата та час настання доменної події.
        /// 
        /// (Date and time when the domain event occurred.)
        /// </summary>
        public DateTimeOffset OccurredOn { get; }

        /// <summary>
        /// Ініціалізує новий екземпляр доменної події.
        /// 
        /// (Initializes a new instance of a domain event.)
        /// </summary>
        /// <param name="occurredOn">Час настання події.</param>
        protected DomainEvent(DateTimeOffset occurredOn)
        {
            OccurredOn = occurredOn;
        }
    }
}
