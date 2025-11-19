using FlowFusion.Core.Interfaces;
using FlowFusion.Core.Models;
using FlowFusion.Runtime.Services;
using Microsoft.Extensions.Logging;
namespace FlowFusion.Runtime;

public sealed class WorkflowEngine(IExpressionEvaluator evaluator, ILogger<WorkflowEngine>? logger = null)
{

    /// <summary>
    /// Executes a workflow from its StartBlockId until completion or failure.
    /// The engine evaluates conditional transitions after a successful block execution.
    /// </summary>
    public async Task RunAsync(IWorkflow workflow, Core.Models.ExecutionContext context, CancellationToken cancellation = default)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(context);

        var current = workflow.GetBlockById(workflow.StartBlockId);
        while (current is not null)
        {
            cancellation.ThrowIfCancellationRequested();
            logger?.LogDebug("Executing block {BlockId}", current.Id);

            ExecutionResult result;
            try
            {
                result = await current.ExecuteAsync(context);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Block {BlockId} threw an exception", current.Id);
                result = ExecutionResult.Failure(next: null, ex);
            }

            if (!result.Succeeded)
            {
                logger?.LogInformation("Block {BlockId} failed; transitioning to {Next}", current.Id, result.NextBlockId ?? "(none)");
                current = workflow.GetBlockById(result.NextBlockId ?? string.Empty);
                if (current is null) break;
                continue;
            }

            // Evaluate conditional transitions for this source block in order.
            var transitions = workflow.ConditionalTransitions.Where(t => t.SourceBlockId == current.Id);
            string? selectedNext = null;
            foreach (var t in transitions)
            {
                bool ok;
                try
                {
                    ok = await evaluator.EvaluateAsync(t.Expression, context, cancellation);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Error evaluating expression for transition {Source}->{Target}", t.SourceBlockId, t.TargetBlockId);
                    // treat evaluation failure as not matching; or alternatively route to error -- design choice
                    ok = false;
                }

                if (ok)
                {
                    selectedNext = t.TargetBlockId;
                    logger?.LogDebug("Transition matched: {Source} -> {Target} (expr: {Expr})", t.SourceBlockId, t.TargetBlockId, t.Expression);
                    break;
                }
            }

            if (selectedNext is not null)
            {
                current = workflow.GetBlockById(selectedNext);
                continue;
            }

            // Fallback to ExecutionResult.NextBlockId (if provided)
            current = workflow.GetBlockById(result.NextBlockId);
        }

        logger?.LogInformation("Workflow {WorkflowId} execution finished.", workflow.Id);
    }
}