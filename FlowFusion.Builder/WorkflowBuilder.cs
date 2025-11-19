using FlowFusion.Core.Interfaces;
using FlowFusion.Core.Models;

namespace FlowFusion.Builder;

public sealed class WorkflowBuilder(string id)
{
    private readonly Dictionary<string, IBlock> _blocks = new(StringComparer.Ordinal);
    private readonly List<ConditionalTransition> _transitions = new();

    private string? _startId;

    public WorkflowBuilder StartWith(IBlock block)
    {
        _blocks[block.Id] = block;
        _startId = block.Id;
        return this;
    }

    public WorkflowBuilder Add(IBlock block)
    {
        _blocks[block.Id] = block;
        return this;
    }

    public WorkflowBuilder AddConditionalTransition(string sourceBlockId, string targetBlockId, string expression)
    {
        _transitions.Add(new ConditionalTransition(sourceBlockId, targetBlockId, expression));
        return this;
    }

    public IWorkflow Build()
    {
        if (string.IsNullOrEmpty(_startId)) throw new InvalidOperationException("Start block not specified.");
        return new InMemoryWorkflow(id, _startId, _blocks, _transitions);
    }

    // Simple in-memory workflow implementation
    private sealed class InMemoryWorkflow(string id, string startBlockId, IDictionary<string, IBlock> blocks, IEnumerable<ConditionalTransition> transitions) : IWorkflow
    {
        public string Id { get; } = id;
        public string StartBlockId { get; } = startBlockId;
        public IReadOnlyDictionary<string, IBlock> Blocks { get; } = new Dictionary<string, IBlock>(blocks, StringComparer.Ordinal);
        public IReadOnlyList<ConditionalTransition> ConditionalTransitions { get; } = transitions.ToList().AsReadOnly();
    }
}