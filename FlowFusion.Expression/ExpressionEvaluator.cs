using FlowFusion.Expression;
namespace FlowFusion.Expressions;
/// <summary>
/// A tiny, demo expression evaluator. NOT production-grade.
/// This example supports simple "variable == value" comparisons for demo and tests.
/// Implement real expression engines (CEL, Roslyn sandbox, etc.) in dedicated packages.
/// </summary>
public sealed class ExpressionEvaluator : IExpressionEvaluator
{
    // Very naive evaluator for demo only
    public Task WarmupAsync(string expression, CancellationToken cancellation = default)
    {
        // no-op for this simple evaluator
        return Task.CompletedTask;
    }

    public static Task<bool> EvaluateAsync(string expression, Core.Models.ExecutionContext context, CancellationToken cancellation = default)
    {
        // Example supported format: "variables.order.total > 100"
        // Very simplistic parsing for demo purposes only
        if (string.IsNullOrWhiteSpace(expression)) return Task.FromResult(false);

        // Demo: support "var == literal" or "var > literal" for numeric
        // e.g., "order.total > 100"
        try
        {
            if (expression.Contains("=="))
            {
                var parts = expression.Split("==", 2);
                var left = parts[0].Trim();
                var right = parts[1].Trim().Trim('"', '\'');
                var value = ResolveVariable(left, context);
                return Task.FromResult(value?.ToString() == right);
            }
            if (expression.Contains('>'))
            {
                var parts = expression.Split(">", 2);
                var left = parts[0].Trim();
                var right = parts[1].Trim();
                var value = ResolveVariable(left, context);
                if (double.TryParse(value?.ToString(), out var lv) && double.TryParse(right, out var rv))
                    return Task.FromResult(lv > rv);
            }
        }
        catch { /* swallow for demo */ }

        return Task.FromResult(false);
    }

    private static object? ResolveVariable(string path, Core.Models.ExecutionContext context)
    {
        // very simple dot-notation resolver: "order.total" -> Variables["order"] then property "total" via reflection
        var parts = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return null;
        if (!context.Variables.TryGetValue(parts[0], out var obj) || obj is null) return null;
        object? cur = obj;
        for (int i = 1; i < parts.Length && cur is not null; i++)
        {
            var prop = cur.GetType().GetProperty(parts[i]);
            cur = prop?.GetValue(cur);
        }
        return cur;
    }
}