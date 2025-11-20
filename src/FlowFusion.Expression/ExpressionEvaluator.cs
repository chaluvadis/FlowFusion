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
    public async Task WarmupAsync(string expression, CancellationToken cancellation = default)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return;
        // Compile if not already cached
        _compiledExpressions.GetOrAdd(expression, expr => CompileExpression(expr).Compile());
    }
    /// <summary>
    /// Evaluates a boolean expression in the given context.
    /// </summary>
    public async Task<bool> EvaluateAsync(string expression, FlowExecutionContext context, CancellationToken cancellation = default)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return false;
        var compiled = _compiledExpressions.GetOrAdd(expression, expr => CompileExpression(expr).Compile());
        return compiled(context);
    }
    private static Expression<Func<FlowExecutionContext, bool>> CompileExpression(string expression)
    {
        var parameter = Expr.Parameter(typeof(FlowExecutionContext), "context");
        var body = ParseExpression(expression, parameter);
        return Expr.Lambda<Func<FlowExecutionContext, bool>>(body, parameter);
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
        var i = 0;
        while (i < expression.Length)
        {
            var c = expression[i];
            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }
            if (char.IsLetter(c) || c == '_')
            {
                var start = i;
                while (i < expression.Length && (char.IsLetterOrDigit(expression[i]) || expression[i] == '_'))
                    i++;
                var value = expression[start..i];
                tokens.Add(new Token(TokenType.Identifier, value));
                continue;
            }
            if (char.IsDigit(c))
            {
                var start = i;
                while (i < expression.Length && char.IsDigit(expression[i]))
                    i++;
                if (i < expression.Length && expression[i] == '.')
                {
                    i++;
                    while (i < expression.Length && char.IsDigit(expression[i]))
                        i++;
                }
                var value = expression[start..i];
                tokens.Add(new Token(TokenType.Number, value));
                continue;
            }
            if (c == '"')
            {
                i++;
                var start = i;
                while (i < expression.Length && expression[i] != '"')
                {
                    if (expression[i] == '\\') i += 2;
                    else i++;
                }
                var value = expression[start..i];
                i++;
                tokens.Add(new Token(TokenType.String, value));
                continue;
            }
            if (c == '[')
            {
                tokens.Add(new Token(TokenType.LBracket, "["));
                i++;
                continue;
            }
            if (c == ']')
            {
                tokens.Add(new Token(TokenType.RBracket, "]"));
                i++;
                continue;
            }
            if (c == '.')
            {
                tokens.Add(new Token(TokenType.Dot, "."));
                i++;
                continue;
            }
            if (c == '(')
            {
                tokens.Add(new Token(TokenType.LParen, "("));
                i++;
                continue;
            }
            if (c == ')')
            {
                tokens.Add(new Token(TokenType.RParen, ")"));
                i++;
                continue;
            }
            if (c == ',')
            {
                tokens.Add(new Token(TokenType.Comma, ","));
                i++;
                continue;
            }
            if (i + 1 < expression.Length)
            {
                var next = expression[i + 1];
                if (c == '=' && next == '=')
                {
                    tokens.Add(new Token(TokenType.Equal, "=="));
                    i += 2;
                    continue;
                }
                if (c == '!' && next == '=')
                {
                    tokens.Add(new Token(TokenType.NotEqual, "!="));
                    i += 2;
                    continue;
                }
                if (c == '<' && next == '=')
                {
                    tokens.Add(new Token(TokenType.LessEqual, "<="));
                    i += 2;
                    continue;
                }
                if (c == '>' && next == '=')
                {
                    tokens.Add(new Token(TokenType.GreaterEqual, ">="));
                    i += 2;
                    continue;
                }
                if (c == '&' && next == '&')
                {
                    tokens.Add(new Token(TokenType.And, "&&"));
                    i += 2;
                    continue;
                }
                if (c == '|' && next == '|')
                {
                    tokens.Add(new Token(TokenType.Or, "||"));
                    i += 2;
                    continue;
                }
            }
            if (c == '<')
            {
                tokens.Add(new Token(TokenType.Less, "<"));
                i++;
                continue;
            }
            if (c == '>')
            {
                tokens.Add(new Token(TokenType.Greater, ">"));
                i++;
                continue;
            }
            if (c == '+')
            {
                tokens.Add(new Token(TokenType.Plus, "+"));
                i++;
                continue;
            }
            if (c == '-')
            {
                tokens.Add(new Token(TokenType.Minus, "-"));
                i++;
                continue;
            }
            if (c == '*')
            {
                tokens.Add(new Token(TokenType.Multiply, "*"));
                i++;
                continue;
            }
            if (c == '/')
            {
                tokens.Add(new Token(TokenType.Divide, "/"));
                i++;
                continue;
            }
            if (c == '%')
            {
                tokens.Add(new Token(TokenType.Modulo, "%"));
                i++;
                continue;
            }
            if (c == '!')
            {
                tokens.Add(new Token(TokenType.Not, "!"));
                i++;
                continue;
            }
            throw new ArgumentException($"Unexpected character '{c}' at position {i}");
        }
        return tokens;
    }
}
internal enum TokenType
{
    Identifier,
    Number,
    String,
    LBracket,
    RBracket,
    Dot,
    LParen,
    RParen,
    Comma,
    Equal,
    NotEqual,
    Less,
    Greater,
    LessEqual,
    GreaterEqual,
    And,
    Or,
    Plus,
    Minus,
    Multiply,
    Divide,
    Modulo,
    Not
}
internal record Token(TokenType Type, string Value);
public static class ExpressionHelper
{
    public static object? GetProperty(object? obj, string propertyName)
    {
        if (obj is null) return null;
        var prop = obj.GetType().GetProperty(propertyName);
        return prop?.GetValue(obj);
    }
    public static object? GetIndexer(object? obj, object? index)
    {
        if (obj is null) return null;
        var method = obj.GetType().GetMethod("get_Item", [index?.GetType() ?? typeof(object)]);
        return method?.Invoke(obj, [index]);
    }
    public static object? CallMethod(object? obj, string methodName, params object?[] args)
    {
        if (obj is null) return null;
        var types = args.Select(a => a?.GetType() ?? typeof(object)).ToArray();
        var method = obj.GetType().GetMethod(methodName, types);
        return method?.Invoke(obj, args);
    }
    public static bool Equal(object? left, object? right) => Equals(left, right);
    public static bool NotEqual(object? left, object? right) => !Equals(left, right);
    public static bool LessThan(object? left, object? right) => Compare(left, right) < 0;
    public static bool GreaterThan(object? left, object? right) => Compare(left, right) > 0;
    public static bool LessEqual(object? left, object? right) => Compare(left, right) <= 0;
    public static bool GreaterEqual(object? left, object? right) => Compare(left, right) >= 0;
    public static bool And(object? left, object? right) => Convert.ToBoolean(left) && Convert.ToBoolean(right);
    public static bool Or(object? left, object? right) => Convert.ToBoolean(left) || Convert.ToBoolean(right);
    public static bool Not(object? value) => !Convert.ToBoolean(value);
    public static object? Add(object? left, object? right) => Convert.ToDouble(left) + Convert.ToDouble(right);
    public static object? Subtract(object? left, object? right) => Convert.ToDouble(left) - Convert.ToDouble(right);
    public static object? Multiply(object? left, object? right) => Convert.ToDouble(left) * Convert.ToDouble(right);
    public static object? Divide(object? left, object? right) => Convert.ToDouble(left) / Convert.ToDouble(right);
    public static object? Modulo(object? left, object? right) => Convert.ToDouble(left) % Convert.ToDouble(right);
    private static int Compare(object? left, object? right)
    {
        if (left is IComparable l && right is IComparable r)
            return l.CompareTo(r);
        return 0;
    }
}
internal class ExpressionParser(List<Token> tokens, ParameterExpression contextParam)
{
    private int _position = 0;

    public Expr Parse() => ParseOrExpression();
    private Expr ParseOrExpression()
    {
        var left = ParseAndExpression();
        while (Match(TokenType.Or))
        {
            var right = ParseAndExpression();
            left = Expr.Call(OrMethod, Expr.Convert(left, typeof(object)), Expr.Convert(right, typeof(object)));
        }
        return left;
    }
    private Expr ParseAndExpression()
    {
        var left = ParseEqualityExpression();
        while (Match(TokenType.And))
        {
            var right = ParseEqualityExpression();
            left = Expr.Call(AndMethod, Expr.Convert(left, typeof(object)), Expr.Convert(right, typeof(object)));
        }
        return left;
    }
    private static readonly System.Reflection.MethodInfo EqualMethod = typeof(ExpressionHelper).GetMethod("Equal", [typeof(object), typeof(object)])!;
    private static readonly System.Reflection.MethodInfo NotEqualMethod = typeof(ExpressionHelper).GetMethod("NotEqual", [typeof(object), typeof(object)])!;
    private static readonly System.Reflection.MethodInfo LessThanMethod = typeof(ExpressionHelper).GetMethod("LessThan", [typeof(object), typeof(object)])!;
    private static readonly System.Reflection.MethodInfo GreaterThanMethod = typeof(ExpressionHelper).GetMethod("GreaterThan", [typeof(object), typeof(object)])!;
    private static readonly System.Reflection.MethodInfo LessEqualMethod = typeof(ExpressionHelper).GetMethod("LessEqual", [typeof(object), typeof(object)])!;
    private static readonly System.Reflection.MethodInfo GreaterEqualMethod = typeof(ExpressionHelper).GetMethod("GreaterEqual", [typeof(object), typeof(object)])!;
    private static readonly System.Reflection.MethodInfo AndMethod = typeof(ExpressionHelper).GetMethod("And", [typeof(object), typeof(object)])!;
    private static readonly System.Reflection.MethodInfo OrMethod = typeof(ExpressionHelper).GetMethod("Or", [typeof(object), typeof(object)])!;
    private static readonly System.Reflection.MethodInfo NotMethod = typeof(ExpressionHelper).GetMethod("Not", [typeof(object)])!;
    private static readonly System.Reflection.MethodInfo AddMethod = typeof(ExpressionHelper).GetMethod("Add", [typeof(object), typeof(object)])!;
    private static readonly System.Reflection.MethodInfo SubtractMethod = typeof(ExpressionHelper).GetMethod("Subtract", [typeof(object), typeof(object)])!;
    private static readonly System.Reflection.MethodInfo MultiplyMethod = typeof(ExpressionHelper).GetMethod("Multiply", [typeof(object), typeof(object)])!;
    private static readonly System.Reflection.MethodInfo DivideMethod = typeof(ExpressionHelper).GetMethod("Divide", [typeof(object), typeof(object)])!;
    private static readonly System.Reflection.MethodInfo ModuloMethod = typeof(ExpressionHelper).GetMethod("Modulo", [typeof(object), typeof(object)])!;
    private Expr ParseEqualityExpression()
    {
        var left = ParseRelationalExpression();
        if (Match(TokenType.Equal))
        {
            var right = ParseRelationalExpression();
            left = Expr.Call(EqualMethod, Expr.Convert(left, typeof(object)), Expr.Convert(right, typeof(object)));
        }
        else if (Match(TokenType.NotEqual))
        {
            var right = ParseRelationalExpression();
            left = Expr.Call(NotEqualMethod, Expr.Convert(left, typeof(object)), Expr.Convert(right, typeof(object)));
        }
        return left;
    }
    private Expr ParseRelationalExpression()
    {
        var left = ParseAdditiveExpression();
        if (Match(TokenType.Less))
        {
            var right = ParseAdditiveExpression();
            left = Expr.Call(LessThanMethod, Expr.Convert(left, typeof(object)), Expr.Convert(right, typeof(object)));
        }
        else if (Match(TokenType.Greater))
        {
            var right = ParseAdditiveExpression();
            left = Expr.Call(GreaterThanMethod, Expr.Convert(left, typeof(object)), Expr.Convert(right, typeof(object)));
        }
        else if (Match(TokenType.LessEqual))
        {
            var right = ParseAdditiveExpression();
            left = Expr.Call(LessEqualMethod, Expr.Convert(left, typeof(object)), Expr.Convert(right, typeof(object)));
        }
        else if (Match(TokenType.GreaterEqual))
        {
            var right = ParseAdditiveExpression();
            left = Expr.Call(GreaterEqualMethod, Expr.Convert(left, typeof(object)), Expr.Convert(right, typeof(object)));
        }
        return left;
    }
    private Expr ParseAdditiveExpression()
    {
        var left = ParseMultiplicativeExpression();
        while (true)
        {
            if (Match(TokenType.Plus))
            {
                var right = ParseMultiplicativeExpression();
                left = Expr.Call(AddMethod, Expr.Convert(left, typeof(object)), Expr.Convert(right, typeof(object)));
            }
            else if (Match(TokenType.Minus))
            {
                var right = ParseMultiplicativeExpression();
                left = Expr.Call(SubtractMethod, Expr.Convert(left, typeof(object)), Expr.Convert(right, typeof(object)));
            }
            else
            {
                break;
            }
        }
        return left;
    }
    private Expr ParseMultiplicativeExpression()
    {
        var left = ParseUnaryExpression();
        while (true)
        {
            if (Match(TokenType.Multiply))
            {
                var right = ParseUnaryExpression();
                left = Expr.Call(MultiplyMethod, Expr.Convert(left, typeof(object)), Expr.Convert(right, typeof(object)));
            }
            else if (Match(TokenType.Divide))
            {
                var right = ParseUnaryExpression();
                left = Expr.Call(DivideMethod, Expr.Convert(left, typeof(object)), Expr.Convert(right, typeof(object)));
            }
            else if (Match(TokenType.Modulo))
            {
                var right = ParseUnaryExpression();
                left = Expr.Call(ModuloMethod, Expr.Convert(left, typeof(object)), Expr.Convert(right, typeof(object)));
            }
            else
            {
                break;
            }
        }
        return left;
    }
    private Expr ParseUnaryExpression()
    {
        if (Match(TokenType.Not))
        {
            var operand = ParseUnaryExpression();
            return Expr.Call(NotMethod, Expr.Convert(operand, typeof(object)));
        }
        return ParsePrimaryExpression();
    }
    private Expr ParsePrimaryExpression()
    {
        if (Match(TokenType.LParen))
        {
            var expr = ParseOrExpression();
            Expect(TokenType.RParen);
            return expr;
        }
        if (Peek(TokenType.Identifier))
        {
            return ParseIdentifierExpression();
        }
        if (Peek(TokenType.Number))
        {
            var token = Consume(TokenType.Number);
            if (double.TryParse(token.Value, out var value))
            {
                return Expr.Constant((object?)value);
            }
            throw new ArgumentException($"Invalid number: {token.Value}");
        }
        if (Peek(TokenType.String))
        {
            var token = Consume(TokenType.String);
            return Expr.Constant((object?)token.Value);
        }
        throw new ArgumentException($"Unexpected token: {Current?.Type}");
    }
    private Expr ParseIdentifierExpression()
    {
        var identifier = Consume(TokenType.Identifier).Value;
        Expr expr;
        string? lastIdentifier = identifier;
        if (identifier == "Variables")
        {
            expr = Expr.Property(contextParam, "Variables");
        }
        else if (identifier == "context")
        {
            expr = contextParam;
        }
        else
        {
            // Direct variable name: assume Variables["name"]
            var variablesProp = Expr.Property(contextParam, "Variables");
            expr = Expr.Call(variablesProp, "get_Item", null, Expr.Constant(identifier));
        }
        while (true)
        {
            if (Match(TokenType.Dot))
            {
                lastIdentifier = Consume(TokenType.Identifier).Value;
                expr = Expr.Call(typeof(ExpressionHelper), "GetProperty", null, expr, Expr.Constant(lastIdentifier));
            }
            else if (Match(TokenType.LBracket))
            {
                var indexExpr = ParseOrExpression();
                Expect(TokenType.RBracket);
                expr = Expr.Call(typeof(ExpressionHelper), "GetIndexer", null, expr, Expr.Convert(indexExpr, typeof(object)));
                lastIdentifier = null;
            }
            else if (Match(TokenType.LParen))
            {
                // Method call
                var args = new List<Expr>();
                if (!Peek(TokenType.RParen))
                {
                    do
                    {
                        args.Add(ParseOrExpression());
                    } while (Match(TokenType.Comma));
                }
                Expect(TokenType.RParen);
                var methodName = lastIdentifier ?? identifier;
                var argsArray = Expr.NewArrayInit(typeof(object), args.Select(a => Expr.Convert(a, typeof(object))));
                expr = Expr.Call(typeof(ExpressionHelper), "CallMethod", null, expr, Expr.Constant(methodName), argsArray);
                lastIdentifier = null;
            }
            else
            {
                break;
            }
        }
        return expr;
    }
    private bool Match(TokenType type)
    {
        if (Peek(type))
        {
            _position++;
            return true;
        }
        return false;
    }
    private bool Peek(TokenType type) => Current?.Type == type;
    private Token Consume(TokenType type)
    {
        if (!Peek(type))
            throw new ArgumentException($"Expected {type}, got {Current?.Type}");
        return tokens[_position++];
    }
    private void Expect(TokenType type) => Consume(type);
    private Token? Current => _position < tokens.Count ? tokens[_position] : null;
}