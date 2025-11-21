namespace FlowFusion.Workflow;

public sealed class WorkflowEngine(
    IExpressionEvaluator evaluator,
    ILogger<WorkflowEngine>? logger = null) : IWorkflowEngine
{
    /// <summary>
    /// Executes a workflow from its StartBlockId until completion or failure.
    /// The engine evaluates conditional transitions after a successful block execution.
    /// </summary>
    public async Task RunAsync(
        IWorkflow workflow,
        FlowExecutionContext context,
        CancellationToken cancellation = default
    )
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(context);

        var current = workflow.GetBlockById(workflow.StartBlockId);
        while (current is not null)
        {
            CheckCancellation(cancellation);

            var result = await ExecuteBlockAsync(current, context, cancellation);
            CheckCancellation(cancellation); // Check again after block execution

            if (!result.Succeeded)
            {
                current = HandleBlockFailure(workflow, current, result);
                continue;
            }

            current = await EvaluateTransitionsAsync(workflow, current, context, result, cancellation);
        }

        logger?.LogInformation("Workflow {WorkflowId} execution finished.", workflow.Id);
    }

    private void CheckCancellation(CancellationToken cancellation)
    {
        if (cancellation.IsCancellationRequested)
            throw new TaskCanceledException();
    }

    private async Task<ExecutionResult> ExecuteBlockAsync(IBlock block, FlowExecutionContext context, CancellationToken cancellation)
    {
        logger?.LogDebug("Executing block {BlockId}", block.Id);

        try
        {
            var result = await block.ExecuteAsync(context);
            CheckCancellation(cancellation);
            return result;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Block {BlockId} threw an exception", block.Id);
            return ExecutionResult.Failure(next: null, ex);
        }
    }

    private IBlock? HandleBlockFailure(IWorkflow workflow, IBlock current, ExecutionResult result)
    {
        logger?.LogInformation("Block {BlockId} failed; transitioning to {Next}",
            current.Id, result.NextBlockId ?? "(none)");

        var nextBlock = workflow.GetBlockById(result.NextBlockId ?? string.Empty);
        return nextBlock; // Returns null if no next block, which will exit the loop
    }

    private async Task<IBlock?> EvaluateTransitionsAsync(
        IWorkflow workflow,
        IBlock current,
        FlowExecutionContext context,
        ExecutionResult result,
        CancellationToken cancellation)
    {
        var transitions = workflow.ConditionalTransitions.Where(t => t.SourceBlockId == current.Id);

        foreach (var transition in transitions)
        {
            if (await EvaluateTransitionAsync(transition, context, cancellation))
            {
                return workflow.GetBlockById(transition.TargetBlockId);
            }
        }

        // Fallback to ExecutionResult.NextBlockId (if provided)
        return workflow.GetBlockById(result.NextBlockId);
    }

    private async Task<bool> EvaluateTransitionAsync(ConditionalTransition transition, FlowExecutionContext context, CancellationToken cancellation)
    {
        try
        {
            var result = await evaluator.EvaluateAsync(transition.Expression, context, cancellation);
            if (result)
            {
                logger?.LogDebug("Transition matched: {Source} -> {Target} (expr: {Expr})",
                    transition.SourceBlockId, transition.TargetBlockId, transition.Expression);
            }
            return result;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error evaluating expression for transition {Source}->{Target}",
                transition.SourceBlockId, transition.TargetBlockId);
            // treat evaluation failure as not matching; or alternatively route to error -- design choice
            return false;
        }
    }
}