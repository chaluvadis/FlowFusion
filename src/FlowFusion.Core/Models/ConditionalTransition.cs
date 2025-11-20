namespace FlowFusion.Core.Models;
/// <summary>
/// A conditional transition associated to a source block (engine may keep the source block id in the workflow metadata).
/// The Expression is evaluated against the current FlowExecutionContext. The engine chooses the first matching transition.
/// </summary>
public sealed record ConditionalTransition(
    string SourceBlockId,
    string TargetBlockId,
    string Expression
);
