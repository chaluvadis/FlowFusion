namespace FlowFusion.Expression;

/// <summary>
/// High-performance expression evaluator supporting C# syntax subset.
/// Compiles expressions to delegates for maximum runtime performance.
/// Supports multiple variable access patterns and complex expressions.
/// </summary>
public sealed class ExpressionEvaluator : IExpressionEvaluator
{
    private readonly ConcurrentDictionary<string, Func<FlowExecutionContext, bool>> _compiledExpressions = new();

    /// <summary>
    /// Warms up an expression by parsing and compiling it to a delegate.
    /// </summary>
    public Task WarmupAsync(string expression, CancellationToken cancellation = default)
    {
        if (!string.IsNullOrWhiteSpace(expression))
            _compiledExpressions.GetOrAdd(expression, expr => CompileExpression(expr).Compile());

        return Task.CompletedTask;
    }

    /// <summary>
    /// Evaluates a boolean expression in the given context.
    /// </summary>
    public Task<bool> EvaluateAsync(string expression, FlowExecutionContext context, CancellationToken cancellation = default)
    {
        cancellation.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(expression))
            return Task.FromResult(false);
        var compiled = _compiledExpressions.GetOrAdd(expression, expr => CompileExpression(expr).Compile());
        cancellation.ThrowIfCancellationRequested();
        return Task.FromResult(compiled(context));
    }

    private static Expression<Func<FlowExecutionContext, bool>> CompileExpression(string expression)
    {
        var parameter = Expr.Parameter(typeof(FlowExecutionContext), "context");
        var body = ParseExpression(expression, parameter);
        return Expr.Lambda<Func<FlowExecutionContext, bool>>(Expr.Convert(body, typeof(bool)), parameter);
    }

    private static Expr ParseExpression(string expression, ParameterExpression contextParam)
    {
        var tokens = Tokenize(expression);
        var parser = new ExpressionParser(tokens, contextParam);
        return parser.Parse();
    }

    private static List<Token> Tokenize(string expression)
    {
        var tokens = new List<Token>();
        int i = 0;
        while (i < expression.Length)
        {
            var c = expression[i];
            if (char.IsWhiteSpace(c)) { i++; continue; }

            switch (c)
            {
                case var _ when char.IsLetter(c) || c == '_':
                    var start = i;
                    while (i < expression.Length && (char.IsLetterOrDigit(expression[i]) || expression[i] == '_')) i++;
                    tokens.Add(new Token(TokenType.Identifier, expression[start..i]));
                    continue;
                case var _ when char.IsDigit(c):
                    start = i;
                    while (i < expression.Length && char.IsDigit(expression[i])) i++;
                    if (i < expression.Length && expression[i] == '.')
                    {
                        i++;
                        while (i < expression.Length && char.IsDigit(expression[i])) i++;
                    }
                    tokens.Add(new Token(TokenType.Number, expression[start..i]));
                    continue;
                case '"':
                    i++; // skip "
                    start = i;
                    while (i < expression.Length && expression[i] != '"')
                        i += expression[i] == '\\' ? 2 : 1;
                    tokens.Add(new Token(TokenType.String, expression[start..i]));
                    i++; // skip closing "
                    continue;
                case '[':
                    tokens.Add(new Token(TokenType.LBracket, "[")); i++; continue;
                case ']':
                    tokens.Add(new Token(TokenType.RBracket, "]")); i++; continue;
                case '.':
                    tokens.Add(new Token(TokenType.Dot, ".")); i++; continue;
                case '(':
                    tokens.Add(new Token(TokenType.LParen, "(")); i++; continue;
                case ')':
                    tokens.Add(new Token(TokenType.RParen, ")")); i++; continue;
                case ',':
                    tokens.Add(new Token(TokenType.Comma, ",")); i++; continue;
                default:
                    {
                        // Handle double-char ops with pattern matching
                        if (i + 1 < expression.Length)
                        {
                            var pattern = $"{c}{expression[i + 1]}";
                            var token = pattern switch
                            {
                                "==" => new Token(TokenType.Equal, "=="),
                                "!=" => new Token(TokenType.NotEqual, "!="),
                                "<=" => new Token(TokenType.LessEqual, "<="),
                                ">=" => new Token(TokenType.GreaterEqual, ">="),
                                "&&" => new Token(TokenType.And, "&&"),
                                "||" => new Token(TokenType.Or, "||"),
                                _ => null
                            };
                            if (token is not null)
                            {
                                tokens.Add(token);
                                i += 2;
                                continue;
                            }
                        }

                        // Single-char ops
                        tokens.Add(c switch
                        {
                            '<' => new Token(TokenType.Less, "<"),
                            '>' => new Token(TokenType.Greater, ">"),
                            '+' => new Token(TokenType.Plus, "+"),
                            '-' => new Token(TokenType.Minus, "-"),
                            '*' => new Token(TokenType.Multiply, "*"),
                            '/' => new Token(TokenType.Divide, "/"),
                            '%' => new Token(TokenType.Modulo, "%"),
                            '!' => new Token(TokenType.Not, "!"),
                            _ => throw new ArgumentException($"Unexpected character '{c}' at position {i}")
                        });
                        i++;
                        continue;
                    }
            }
        }
        return tokens;
    }
}