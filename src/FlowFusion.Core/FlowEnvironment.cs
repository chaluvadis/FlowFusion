namespace FlowFusion.Core;

/// <summary>
/// Public implementation of IFlowEnvironment that provides function registration, expression compilation, and evaluation.
/// </summary>
public sealed class FlowEnvironment : IFlowEnvironment
{
    private readonly FunctionRegistry _registry = new();

    /// <inheritdoc />
    public bool RegisterFunction(string name, Func<ExecutionContext, object?[], CancellationToken, Task<object?>> function)
    {
        return _registry.TryRegister(name, function);
    }

    /// <inheritdoc />
    public ISimpleCompiledExpression Compile(string expression)
    {
        return SimpleInterpreter.Compile(expression, _registry);
    }

    /// <inheritdoc />
    /// <remarks>
    /// This method uses GetAwaiter().GetResult() to provide a synchronous API as required by the interface contract.
    /// For async usage, prefer using Compile() followed by EvaluateBooleanAsync() to avoid blocking.
    /// Note: Registered functions should avoid blocking calls to prevent deadlocks in this synchronous context.
    /// </remarks>
    public bool EvaluateBoolean(string expression, IReadOnlyDictionary<string, object?> variables, CancellationToken cancellationToken = default)
    {
        var compiled = Compile(expression);
        return compiled.EvaluateBooleanAsync(variables, cancellationToken).GetAwaiter().GetResult();
    }
}
