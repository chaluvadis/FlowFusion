namespace FlowFusion.Core.Models;
/// <summary>
/// FlowExecutionContext carries input variables and mutable state between blocks.
/// Keep allocations minimal — use small collections and value semantics where possible.
/// </summary>
public record FlowExecutionContext(IReadOnlyDictionary<string, object?> Variables)
{
    /// <summary>
    /// Immutable input variables available to all blocks (e.g., domain objects).
    /// </summary>
    public IReadOnlyDictionary<string, object?> Variables { get; }
        = Variables ?? new Dictionary<string, object?>();

    // Local mutable state used as the checkpoint state for the workflow; blocks can set/get values.
    private readonly Dictionary<string, object?> _state = new(StringComparer.Ordinal);

    public T? GetVariable<T>(string name) => Variables.TryGetValue(name, out var v) && v is T t ? t : default;

    public T? GetState<T>(string key) => _state.TryGetValue(key, out var v) && v is T t ? t : default;

    public void SetState(string key, object? value) => _state[key] = value;

    public IReadOnlyDictionary<string, object?> SnapshotState() => new Dictionary<string, object?>(_state);
}
