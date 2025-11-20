namespace FlowFusion.Core.Models;
/// <summary>
/// A conditional transition associated to a source block (engine may keep the source block id in the workflow metadata).
/// The Expression is evaluated against the current FlowExecutionContext. The engine chooses the first matching transition.
/// </summary>
public sealed class ConditionalTransition(string sourceBlockId, string targetBlockId, string expression)
{
    public string SourceBlockId { get; init; } = sourceBlockId;
    public string TargetBlockId { get; init; } = targetBlockId;
    public string Expression { get; init; } = expression;
}
