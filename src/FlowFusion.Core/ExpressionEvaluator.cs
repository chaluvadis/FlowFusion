namespace FlowFusion.Core;

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
    /// Returns the compiled expression delegate for reuse.
    /// </summary>
    public Task<Func<FlowExecutionContext, bool>> WarmupAsync(string expression, CancellationToken cancellation = default)
    {
        if (!string.IsNullOrWhiteSpace(expression))
        {
            var compiled = _compiledExpressions.GetOrAdd(expression, expr => CompileExpression(expr).Compile());
            return Task.FromResult(compiled);
        }

        return Task.FromResult<Func<FlowExecutionContext, bool>>(null!);
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

            if (TryTokenizeIdentifier(expression, ref i, tokens) ||
                TryTokenizeNumber(expression, ref i, tokens) ||
                TryTokenizeString(expression, ref i, tokens) ||
                TryTokenizeOperator(expression, ref i, tokens) ||
                TryTokenizeSymbol(expression, ref i, tokens))
            {
                continue;
            }

            throw new ArgumentException($"Unexpected character '{c}' at position {i}");
        }
        return tokens;
    }

    private static bool TryTokenizeIdentifier(string expression, ref int i, List<Token> tokens)
    {
        if (!char.IsLetter(expression[i]) && expression[i] != '_')
            return false;

        var start = i;
        while (i < expression.Length && (char.IsLetterOrDigit(expression[i]) || expression[i] == '_')) i++;
        tokens.Add(new Token(TokenType.Identifier, expression[start..i]));
        return true;
    }

    private static bool TryTokenizeNumber(string expression, ref int i, List<Token> tokens)
    {
        if (!char.IsDigit(expression[i]))
            return false;

        var start = i;
        while (i < expression.Length && char.IsDigit(expression[i])) i++;
        if (i < expression.Length && expression[i] == '.')
        {
            i++;
            while (i < expression.Length && char.IsDigit(expression[i])) i++;
        }
        tokens.Add(new Token(TokenType.Number, expression[start..i]));
        return true;
    }

    private static bool TryTokenizeString(string expression, ref int i, List<Token> tokens)
    {
        if (expression[i] != '"')
            return false;

        i++; // skip opening "
        var start = i;
        while (i < expression.Length && expression[i] != '"')
            i += expression[i] == '\\' ? 2 : 1;
        tokens.Add(new Token(TokenType.String, expression[start..i]));
        i++; // skip closing "
        return true;
    }

    private static bool TryTokenizeOperator(string expression, ref int i, List<Token> tokens)
    {
        // Check for double-character operators first
        if (i + 1 < expression.Length)
        {
            var doubleOp = expression.Substring(i, 2);
            var token = GetDoubleCharOperatorToken(doubleOp);
            if (token is not null)
            {
                tokens.Add(token);
                i += 2;
                return true;
            }
        }

        // Single-character operators
        var singleOp = expression[i];
        var singleToken = GetSingleCharOperatorToken(singleOp);
        if (singleToken is not null)
        {
            tokens.Add(singleToken);
            i++;
            return true;
        }

        return false;
    }

    private static Token? GetDoubleCharOperatorToken(string op) => op switch
    {
        "==" => new Token(TokenType.Equal, "=="),
        "!=" => new Token(TokenType.NotEqual, "!="),
        "<=" => new Token(TokenType.LessEqual, "<="),
        ">=" => new Token(TokenType.GreaterEqual, ">="),
        "&&" => new Token(TokenType.And, "&&"),
        "||" => new Token(TokenType.Or, "||"),
        _ => null
    };

    private static Token? GetSingleCharOperatorToken(char op) => op switch
    {
        '<' => new Token(TokenType.Less, "<"),
        '>' => new Token(TokenType.Greater, ">"),
        '+' => new Token(TokenType.Plus, "+"),
        '-' => new Token(TokenType.Minus, "-"),
        '*' => new Token(TokenType.Multiply, "*"),
        '/' => new Token(TokenType.Divide, "/"),
        '%' => new Token(TokenType.Modulo, "%"),
        '!' => new Token(TokenType.Not, "!"),
        _ => null
    };

    private static bool TryTokenizeSymbol(string expression, ref int i, List<Token> tokens)
    {
        var token = expression[i] switch
        {
            '[' => new Token(TokenType.LBracket, "["),
            ']' => new Token(TokenType.RBracket, "]"),
            '.' => new Token(TokenType.Dot, "."),
            '(' => new Token(TokenType.LParen, "("),
            ')' => new Token(TokenType.RParen, ")"),
            ',' => new Token(TokenType.Comma, ","),
            _ => null
        };

        if (token is not null)
        {
            tokens.Add(token);
            i++;
            return true;
        }

        return false;
    }
}