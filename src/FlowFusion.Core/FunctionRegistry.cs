namespace FlowFusion.Core;

/// <summary>
/// Internal registry for functions that can be called from expressions.
/// Thread-safe using ConcurrentDictionary.
/// </summary>
internal sealed class FunctionRegistry
{
    private readonly ConcurrentDictionary<string, Func<ExecutionContext, object?[], CancellationToken, Task<object?>>> _functions = new(StringComparer.Ordinal);

    /// <summary>
    /// Attempts to register a function with the specified name.
    /// </summary>
    /// <param name="name">The name of the function.</param>
    /// <param name="function">The function implementation.</param>
    /// <returns>True if the function was added; false if a function with that name already exists.</returns>
    public bool TryRegister(string name, Func<ExecutionContext, object?[], CancellationToken, Task<object?>> function)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(function);
        return _functions.TryAdd(name, function);
    }

    /// <summary>
    /// Attempts to get a function by name.
    /// </summary>
    /// <param name="name">The name of the function.</param>
    /// <param name="function">The function if found; null otherwise.</param>
    /// <returns>True if the function was found; false otherwise.</returns>
    public bool TryGet(string name, out Func<ExecutionContext, object?[], CancellationToken, Task<object?>>? function)
    {
        return _functions.TryGetValue(name, out function);
    }

    /// <summary>
    /// Clears all registered functions.
    /// </summary>
    public void Clear()
    {
        _functions.Clear();
    }
}
