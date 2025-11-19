namespace FlowFusion.Core.Interfaces;

public interface IWorkflow
{
    string Id { get; }
    string StartBlockId { get; }
    IReadOnlyDictionary<string, IBlock> Blocks { get; }

    /// <summary>
    /// Optional conditional transitions evaluated after a block returns success.
    /// The engine evaluates these in order and takes the first match.
    /// </summary>
    IReadOnlyList<ConditionalTransition> ConditionalTransitions { get; }

    IBlock? GetBlockById(string? id) => id is null ? null : Blocks.TryGetValue(id, out var b) ? b : null;
}