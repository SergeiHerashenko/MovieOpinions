using Authorization.Application.Interfaces.Orchestrator;
using Authorization.Domain.Results;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Common.Orchestrator
{
    public sealed class Orchestrator<TContext> : IOrchestrator<TContext>
    {
        private readonly IEnumerable<IOrchestratorStep<TContext>> _step;
        private readonly ILogger<Orchestrator<TContext>> _logger;

        public Orchestrator(
            IEnumerable<IOrchestratorStep<TContext>> step,
            ILogger<Orchestrator<TContext>> logger)
        {
            _step = step.OrderBy(s => s.Order);
            _logger = logger;
        }

        public async Task<Result> RunIntegrationsAsync(TContext context)
        {
            var executeSteps = new Stack<IOrchestratorStep<TContext>>();

            foreach (var step in _step)
            {
                var result = await step.ExecuteAsync(context);

                if (result.IsFailure)
                {
                    await RollbackStepsAsync(executeSteps, context);

                    return result;
                }

                executeSteps.Push(step);
            }

            return Result.Success();
        }

        public async Task RollbackStepsAsync(Stack<IOrchestratorStep<TContext>> executedSteps, TContext context)
        {
            while (executedSteps.Count > 0)
            {
                var step = executedSteps.Pop();

                try
                {
                    await step.RollbackAsync(context);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(
                        ex,
                        "Saga rollback failed at step {StepName} for workflow context {ContextType}!",
                        step.GetType().Name,
                        typeof(TContext).Name
                    );
                }
            }
        }
    }
}
