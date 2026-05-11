using Authorization.Domain.Errors;
using Authorization.Domain.Exceptions;

namespace Authorization.Domain.Common
{
    public class BaseEntity : IBaseEntity
    {
        public Guid Id { get; protected set; }

        public DateTime CreatedAt { get; protected set; }

        // 1. Конструктор для створення сутності (Народження)
        protected BaseEntity()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }

        // 2. Конструктор для читання (Відродження)
        protected BaseEntity(Guid id, DateTime createdAt)
        {
            if(id == Guid.Empty) 
                throw new DataInconsistencyDomainException(ErrorCodes.RestoreError.NullReference, "Invalid entity identifier!");

            Id = id;
            CreatedAt = createdAt;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not BaseEntity other) return false;

            if (ReferenceEquals(this, other)) return true;

            if (Id == Guid.Empty || other.Id == Guid.Empty) return false;

            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(BaseEntity? a, BaseEntity? b) => Equals(a, b);

        public static bool operator !=(BaseEntity? a, BaseEntity? b) => !Equals(a, b);
    }
}
