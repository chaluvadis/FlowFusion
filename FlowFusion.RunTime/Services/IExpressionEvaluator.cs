namespace FlowFusion.Runtime.Services;

public interface IExpressionEvaluator
{
    /// <summary>
    /// Evaluate a boolean expression in the context. Implementations must be thread-safe.
    /// Return true if expression evaluated to true, false otherwise.
    /// Throw only for infrastructure failure (parsing issues, etc.)
    /// </summary>
    Task<bool> EvaluateAsync(string expression, Core.Models.ExecutionContext context, CancellationToken cancellation = default);

    /// <summary>
    /// Optional: pre-compile or warm-up expression into some cached representation.
    /// </summary>
    Task WarmupAsync(string expression, CancellationToken cancellation = default);
}