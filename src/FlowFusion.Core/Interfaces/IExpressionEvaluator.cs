namespace FlowFusion.Core.Interfaces;

public interface IExpressionEvaluator
{
    /// <summary>
    /// Evaluate a boolean expression in the context. Implementations must be thread-safe.
    /// Return true if expression evaluated to true, false otherwise.
    /// Throw only for infrastructure failure (parsing issues, etc.)
    /// </summary>
    ValueTask<bool> EvaluateAsync(string expression, FlowExecutionContext context, CancellationToken cancellation = default);

    /// <summary>
    /// Optional: pre-compile or warm-up expression into some cached representation.
    /// Returns the compiled expression delegate for reuse.
    /// </summary>
    ValueTask<Func<FlowExecutionContext, bool>> WarmupAsync(string expression, CancellationToken cancellation = default);
}
