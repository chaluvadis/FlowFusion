namespace FlowFusion.Core;

/// <summary>
/// Represents a flow environment that can register functions, compile expressions, and evaluate boolean expressions.
/// </summary>
public interface IFlowEnvironment
{
    /// <summary>
    /// Registers a function with the specified name that can be called from expressions.
    /// </summary>
    /// <param name="name">The name of the function.</param>
    /// <param name="function">The function implementation that takes an ExecutionContext, arguments, and CancellationToken.</param>
    /// <returns>True if the function was registered successfully; false if a function with that name already exists.</returns>
    bool RegisterFunction(string name, Func<ExecutionContext, object?[], CancellationToken, Task<object?>> function);

    /// <summary>
    /// Compiles an expression string into a compiled expression that can be evaluated later.
    /// </summary>
    /// <param name="expression">The expression string to compile.</param>
    /// <returns>A compiled expression that can be evaluated.</returns>
    ISimpleCompiledExpression Compile(string expression);

    /// <summary>
    /// Evaluates an expression and returns a boolean result.
    /// </summary>
    /// <param name="expression">The expression string to evaluate.</param>
    /// <param name="variables">Variables available during evaluation.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The boolean result of the expression evaluation.</returns>
    bool EvaluateBoolean(string expression, IReadOnlyDictionary<string, object?> variables, CancellationToken cancellationToken = default);
}
