namespace Authorization.Application.Abstractions.Persistence
{
    public interface IUnitOfWork
    {
        Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);

        Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default);
    }
}
