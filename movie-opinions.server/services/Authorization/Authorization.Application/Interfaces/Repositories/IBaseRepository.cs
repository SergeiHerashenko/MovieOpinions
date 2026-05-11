namespace Authorization.Application.Interfaces.Repositories
{
    public interface IBaseRepository<T>
    {
        Task<T> CreateAsync(T entity, CancellationToken cancellationToken);

        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken);

        Task<T> DeleteAsync(Guid id, CancellationToken cancellationToken);
    }
}
