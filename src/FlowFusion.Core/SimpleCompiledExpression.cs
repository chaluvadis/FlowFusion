namespace FlowFusion.Core;

/// <summary>
/// Implementation of ISimpleCompiledExpression that holds an expression root and function registry.
/// </summary>
internal sealed class SimpleCompiledExpression : ISimpleCompiledExpression
{
    private readonly SimpleInterpreter.Expr _root;
    private readonly FunctionRegistry _registry;

    internal SimpleCompiledExpression(SimpleInterpreter.Expr root, FunctionRegistry registry)
    {
        _root = root;
        _registry = registry;
    }

    /// <inheritdoc />
    public async Task<object?> EvaluateAsync(IReadOnlyDictionary<string, object?> variables, CancellationToken cancellationToken = default)
    {
        var context = new ExecutionContext(variables, cancellationToken);
        return await _root.EvaluateAsync(context, _registry).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> EvaluateBooleanAsync(IReadOnlyDictionary<string, object?> variables, CancellationToken cancellationToken = default)
    {
        var result = await EvaluateAsync(variables, cancellationToken).ConfigureAwait(false);
        return ConvertToBoolean(result);
    }

    private static bool ConvertToBoolean(object? value)
    {
        return value switch
        {
            null => false,
            bool b => b,
            int i => i != 0,
            long l => l != 0,
            double d => d != 0,
            string s => !string.IsNullOrEmpty(s),
            _ => true
        };
    }
}
