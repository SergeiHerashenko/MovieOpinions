namespace Authorization.Domain.Common.Models
{
    public abstract class AggregateRoot<TId, TIdType> : Entity<TId>
        where TId : AggregateRootId<TIdType>
    {
        protected AggregateRoot(TId id)
            : base(id)
        {
            Id = id;
        }

        #pragma warning disable CS8618
        protected AggregateRoot() { }
        #pragma warning restore CS8618
    }
}
