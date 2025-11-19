namespace FlowFusion.Core.Models;
/// <summary>
/// Lightweight execution result as a readonly value type.
/// </summary>
public readonly record struct ExecutionResult(bool Succeeded, string? NextBlockId = null, Exception? Error = null)
{
    public static ExecutionResult Success(string? next = null) => new(true, next, null);
    public static ExecutionResult Failure(string? next = null, Exception? ex = null) => new(false, next, ex);
}