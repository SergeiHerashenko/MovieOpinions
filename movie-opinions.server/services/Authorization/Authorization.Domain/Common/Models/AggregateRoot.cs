namespace Authorization.Domain.Common.Models
{
    public abstract class AggregateRoot<TId, TIdType> : Entity<TId>
        where TId : AggregateRootId<TIdType>
    {
        protected AggregateRoot(TId id)
            : base(id) { }

        protected AggregateRoot(TId id, DateTimeOffset createdAt)
            : base(id, createdAt) { }
    }
}
