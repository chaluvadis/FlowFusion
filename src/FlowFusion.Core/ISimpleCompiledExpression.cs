namespace FlowFusion.Core;

/// <summary>
/// Represents a compiled expression that can be evaluated.
/// </summary>
public interface ISimpleCompiledExpression
{
    /// <summary>
    /// Evaluates the expression asynchronously and returns the result.
    /// </summary>
    /// <param name="variables">Variables available during evaluation.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The result of the expression evaluation.</returns>
    Task<object?> EvaluateAsync(IReadOnlyDictionary<string, object?> variables, CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates the expression asynchronously and returns a boolean result.
    /// </summary>
    /// <param name="variables">Variables available during evaluation.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The boolean result of the expression evaluation.</returns>
    Task<bool> EvaluateBooleanAsync(IReadOnlyDictionary<string, object?> variables, CancellationToken cancellationToken = default);
}
