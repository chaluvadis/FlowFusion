namespace FlowFusion.Runtime;
public interface IWorkflowEngine
{
    Task RunAsync(IWorkflow workflow, FlowExecutionContext context, CancellationToken cancellation = default);
}