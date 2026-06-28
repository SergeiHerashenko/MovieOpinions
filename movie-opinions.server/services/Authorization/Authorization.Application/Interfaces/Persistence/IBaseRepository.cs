namespace Authorization.Application.Interfaces.Persistence
{
    public interface IBaseRepository<T>
    {
        Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);

        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

        Task<T> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
