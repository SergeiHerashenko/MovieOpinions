namespace Authorization.Domain.Common.Models.Interfaces
{
    /// <summary>
    /// Представляє бізнес-факт, який уже відбувся в доменній моделі
    /// та може бути опублікований одному або декільком обробникам.
    ///
    /// (Represents a business fact that has already occurred in the domain
    /// model and may be dispatched to one or more handlers.)
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>
        /// Дата та час настання доменної події.
        /// 
        /// (Date and time when the domain event occurred.)
        /// </summary>
        DateTimeOffset OccurredOn { get; }
    }
}
