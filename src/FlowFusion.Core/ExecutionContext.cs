namespace FlowFusion.Core;

/// <summary>
/// Holds variables and cancellation token for expression evaluation.
/// </summary>
public sealed class ExecutionContext
{
    /// <summary>
    /// Gets the variables available during expression evaluation.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Variables { get; }

    /// <summary>
    /// Gets the cancellation token for the current evaluation.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Creates a new execution context with the specified variables and cancellation token.
    /// </summary>
    /// <param name="variables">The variables available during evaluation.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    public ExecutionContext(IReadOnlyDictionary<string, object?>? variables, CancellationToken cancellationToken = default)
    {
        Variables = variables ?? new Dictionary<string, object?>();
        CancellationToken = cancellationToken;
    }
}
