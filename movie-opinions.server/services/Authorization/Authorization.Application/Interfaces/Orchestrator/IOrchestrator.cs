using Authorization.Domain.Results;

namespace Authorization.Application.Interfaces.Orchestrator
{
    public interface IOrchestrator<TContext>
    {
        Task<Result> RunIntegrationsAsync(TContext context);
    }
}
