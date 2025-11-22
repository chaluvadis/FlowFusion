namespace FlowFusion.Core;

/// <summary>
/// High-performance expression evaluator supporting C# syntax subset.
/// Compiles expressions to delegates for maximum runtime performance.
/// Supports multiple variable access patterns and complex expressions.
/// </summary>
/// <remarks>
/// Initializes a new instance with auto-variables mode.
/// </remarks>
/// <param name="autoVariablesMode">If true, bare identifiers are automatically mapped to Variables["identifier"].</param>
public sealed class ExpressionEvaluator(bool autoVariablesMode) : IExpressionEvaluator
{
    private readonly ConcurrentDictionary<string, Func<FlowExecutionContext, bool>> _compiledExpressions = new();
    private readonly ITokenizer _tokenizer = new ExpressionTokenizer();

    /// <summary>
    /// Initializes a new instance with default settings (Variables access required).
    /// </summary>
    public ExpressionEvaluator() : this(false) { }

    /// <summary>
    /// Warms up an expression by parsing and compiling it to a delegate.
    /// Returns the compiled expression delegate for reuse.
    /// </summary>
    public ValueTask<Func<FlowExecutionContext, bool>> WarmupAsync(
        string expression,
        CancellationToken cancellation = default)
    {
        if (!string.IsNullOrWhiteSpace(expression))
        {
            // Input validation: prevent extremely long expressions
            if (expression.Length > 10000)
                throw new ArgumentException("Expression is too long. Maximum allowed length is 10000 characters.", nameof(expression));

            var compiled = _compiledExpressions.GetOrAdd(expression, expr => CompileExpression(expr).Compile());
            return ValueTask.FromResult(compiled);
        }

        return ValueTask.FromResult<Func<FlowExecutionContext, bool>>(null!);
    }

    /// <summary>
    /// Evaluates a boolean expression in the given context.
    /// </summary>
    public ValueTask<bool> EvaluateAsync(
        string expression,
        FlowExecutionContext context,
        CancellationToken cancellation = default)
    {
        cancellation.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(expression))
            return ValueTask.FromResult(false);

        // Input validation: prevent extremely long expressions that could cause stack overflow or performance issues
        if (expression.Length > 10000)
            throw new ArgumentException("Expression is too long. Maximum allowed length is 10000 characters.", nameof(expression));

        var compiled = _compiledExpressions.GetOrAdd(expression, expr => CompileExpression(expr).Compile());
        cancellation.ThrowIfCancellationRequested();
        return ValueTask.FromResult(compiled(context));
    }

    private Expression<Func<FlowExecutionContext, bool>> CompileExpression(string expression)
    {
        var parameter = Expr.Parameter(typeof(FlowExecutionContext), "context");
        var body = ParseExpression(expression, parameter);
        return Expr.Lambda<Func<FlowExecutionContext, bool>>(Expr.Convert(body, typeof(bool)), parameter);
    }

    private Expr ParseExpression(string expression, ParameterExpression contextParam)
    {
        var tokens = _tokenizer.Tokenize(expression);
        var parser = new ExpressionParser(tokens, contextParam, "Variables", "context", autoVariablesMode);
        return parser.Parse();
    }
}