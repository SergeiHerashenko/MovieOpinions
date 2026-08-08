using Authorization.Domain.Results;

namespace Authorization.Application.Abstractions.Orchestrator
{
    public interface IOrchestratorStep<TContext>
    {
        int Order { get; }

        Task<Result> ExecuteAsync(TContext context);

        Task RollbackAsync(TContext context);
    }
}
