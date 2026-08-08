using Authorization.Domain.Results;

namespace Authorization.Application.Abstractions.Orchestrator
{
    public interface IOrchestrator<TContext>
    {
        Task<Result> RunIntegrationsAsync(TContext context);
    }
}
