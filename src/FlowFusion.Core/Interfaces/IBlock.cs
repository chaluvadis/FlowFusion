namespace FlowFusion.Core.Interfaces;

public interface IBlock
{
    string Id { get; }
    /// <summary>
    /// Execute the block. The block should return an ExecutionResult indicating success, failure, and optionally the next block id.
    /// </summary>
    /// <param name="context">Execution context</param>
    Task<ExecutionResult> ExecuteAsync(FlowExecutionContext context);
}
