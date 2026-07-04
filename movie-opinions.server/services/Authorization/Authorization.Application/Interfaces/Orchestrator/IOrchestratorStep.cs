using Authorization.Domain.Results;

namespace Authorization.Application.Interfaces.Orchestrator
{
    public interface IOrchestratorStep<TContext>
    {
        int Order { get; }

        Task<Result> ExecuteAsync(TContext context);

        Task RollbackAsync(TContext context);
    }
}
