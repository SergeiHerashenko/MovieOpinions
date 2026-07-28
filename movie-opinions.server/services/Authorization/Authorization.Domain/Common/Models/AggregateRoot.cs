namespace Authorization.Domain.Common.Models
{
    public abstract class AggregateRoot<TId, TIdType> : Entity<TId>
        where TId : AggregateRootId<TIdType>
    {
        protected AggregateRoot(TId id, DateTimeOffset? createdAt = null) 
            : base(id, createdAt) { }
    }
}
