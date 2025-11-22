namespace FlowFusion.Core;

internal sealed class ExpressionParser(
    IReadOnlyList<Token> tokens,
    ParameterExpression contextParam,
    string variablesPropertyName = "Variables",
    string contextIdentifier = "context")
{
    private readonly string _variablesPropertyName = variablesPropertyName ?? throw new ArgumentNullException(nameof(variablesPropertyName));
    private readonly string _contextIdentifier = contextIdentifier ?? throw new ArgumentNullException(nameof(contextIdentifier));

    private readonly IReadOnlyList<Token> _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
    private readonly ParameterExpression _contextParam = contextParam ?? throw new ArgumentNullException(nameof(contextParam));
    private int _position = 0;

    public Expression Parse() => ParseOrExpression();

    #region MethodInfo cache
    private static MethodInfo GetHelperMethod(string name, params Type[] paramTypes)
    {
        var mi = typeof(ExpressionHelper).GetMethod(
            name,
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: paramTypes,
            modifiers: null);

        if (mi is null)
            throw new MissingMethodException(typeof(ExpressionHelper).FullName, name);

        return mi;
    }

    private static readonly MethodInfo EqualMethod = GetHelperMethod(nameof(ExpressionHelper.Equal), typeof(object), typeof(object));
    private static readonly MethodInfo NotEqualMethod = GetHelperMethod(nameof(ExpressionHelper.NotEqual), typeof(object), typeof(object));
    private static readonly MethodInfo LessThanMethod = GetHelperMethod(nameof(ExpressionHelper.LessThan), typeof(object), typeof(object));
    private static readonly MethodInfo GreaterThanMethod = GetHelperMethod(nameof(ExpressionHelper.GreaterThan), typeof(object), typeof(object));
    private static readonly MethodInfo LessEqualMethod = GetHelperMethod(nameof(ExpressionHelper.LessEqual), typeof(object), typeof(object));
    private static readonly MethodInfo GreaterEqualMethod = GetHelperMethod(nameof(ExpressionHelper.GreaterEqual), typeof(object), typeof(object));
    private static readonly MethodInfo AndMethod = GetHelperMethod(nameof(ExpressionHelper.And), typeof(object), typeof(object));
    private static readonly MethodInfo OrMethod = GetHelperMethod(nameof(ExpressionHelper.Or), typeof(object), typeof(object));
    private static readonly MethodInfo NotMethod = GetHelperMethod(nameof(ExpressionHelper.Not), typeof(object));
    private static readonly MethodInfo AddMethod = GetHelperMethod(nameof(ExpressionHelper.Add), typeof(object), typeof(object));
    private static readonly MethodInfo SubtractMethod = GetHelperMethod(nameof(ExpressionHelper.Subtract), typeof(object), typeof(object));
    private static readonly MethodInfo MultiplyMethod = GetHelperMethod(nameof(ExpressionHelper.Multiply), typeof(object), typeof(object));
    private static readonly MethodInfo DivideMethod = GetHelperMethod(nameof(ExpressionHelper.Divide), typeof(object), typeof(object));
    private static readonly MethodInfo ModuloMethod = GetHelperMethod(nameof(ExpressionHelper.Modulo), typeof(object), typeof(object));
    private static readonly MethodInfo CallMethod = GetHelperMethod(nameof(ExpressionHelper.CallMethod), typeof(object), typeof(string), typeof(object[]));
    #endregion

    #region Parsing helpers
    private static Expression ConvertToObject(Expression e) => Expression.Convert(e, typeof(object));
    private Token? Current => _position < _tokens.Count ? _tokens[_position] : null;
    private bool Peek(TokenType type) => Current?.Type == type;
    private bool Match(TokenType type)
    {
        if (Peek(type)) { _position++; return true; }
        return false;
    }
    private Token Consume(TokenType type)
    {
        if (!Peek(type))
            throw new ParseException($"Expected token {type} at position {_position}, got {Current?.Type} (value: {Current?.Value})");
        return _tokens[_position++];
    }
    private void Expect(TokenType type) => Consume(type);
    #endregion

    private Expression ParseOrExpression()
    {
        var left = ParseAndExpression();
        while (Match(TokenType.Or))
        {
            var right = ParseAndExpression();
            left = Expression.Call(OrMethod, ConvertToObject(left), ConvertToObject(right));
        }
        return left;
    }

    private Expression ParseAndExpression()
    {
        var left = ParseEqualityExpression();
        while (Match(TokenType.And))
        {
            var right = ParseEqualityExpression();
            left = Expression.Call(AndMethod, ConvertToObject(left), ConvertToObject(right));
        }
        return left;
    }

    private Expression ParseEqualityExpression()
    {
        var left = ParseRelationalExpression();
        if (Match(TokenType.Equal))
        {
            var right = ParseRelationalExpression();
            left = Expression.Call(EqualMethod, ConvertToObject(left), ConvertToObject(right));
        }
        else if (Match(TokenType.NotEqual))
        {
            var right = ParseRelationalExpression();
            left = Expression.Call(NotEqualMethod, ConvertToObject(left), ConvertToObject(right));
        }
        return left;
    }

    private Expression ParseRelationalExpression()
    {
        var left = ParseAdditiveExpression();

        if (Match(TokenType.Less))
        {
            var right = ParseAdditiveExpression();
            left = Expression.Call(LessThanMethod, ConvertToObject(left), ConvertToObject(right));
        }
        else if (Match(TokenType.Greater))
        {
            var right = ParseAdditiveExpression();
            left = Expression.Call(GreaterThanMethod, ConvertToObject(left), ConvertToObject(right));
        }
        else if (Match(TokenType.LessEqual))
        {
            var right = ParseAdditiveExpression();
            left = Expression.Call(LessEqualMethod, ConvertToObject(left), ConvertToObject(right));
        }
        else if (Match(TokenType.GreaterEqual))
        {
            var right = ParseAdditiveExpression();
            left = Expression.Call(GreaterEqualMethod, ConvertToObject(left), ConvertToObject(right));
        }

        return left;
    }

    private Expression ParseAdditiveExpression()
    {
        var left = ParseMultiplicativeExpression();
        while (true)
        {
            if (Match(TokenType.Plus))
            {
                var right = ParseMultiplicativeExpression();
                left = Expression.Call(AddMethod, ConvertToObject(left), ConvertToObject(right));
            }
            else if (Match(TokenType.Minus))
            {
                var right = ParseMultiplicativeExpression();
                left = Expression.Call(SubtractMethod, ConvertToObject(left), ConvertToObject(right));
            }
            else break;
        }
        return left;
    }

    private Expression ParseMultiplicativeExpression()
    {
        var left = ParseUnaryExpression();
        while (true)
        {
            if (Match(TokenType.Multiply))
            {
                var right = ParseUnaryExpression();
                left = Expression.Call(MultiplyMethod, ConvertToObject(left), ConvertToObject(right));
            }
            else if (Match(TokenType.Divide))
            {
                var right = ParseUnaryExpression();
                left = Expression.Call(DivideMethod, ConvertToObject(left), ConvertToObject(right));
            }
            else if (Match(TokenType.Modulo))
            {
                var right = ParseUnaryExpression();
                left = Expression.Call(ModuloMethod, ConvertToObject(left), ConvertToObject(right));
            }
            else break;
        }
        return left;
    }

    private Expression ParseUnaryExpression()
    {
        if (Match(TokenType.Not))
        {
            var operand = ParseUnaryExpression();
            return Expression.Call(NotMethod, ConvertToObject(operand));
        }
        return ParsePrimaryExpression();
    }

    private Expression ParsePrimaryExpression()
    {
        if (Match(TokenType.LParen))
        {
            var expr = ParseOrExpression();
            Expect(TokenType.RParen);
            return expr;
        }

        if (Peek(TokenType.Identifier))
            return ParseIdentifierExpression();

        if (Peek(TokenType.Number))
        {
            var token = Consume(TokenType.Number);
            if (double.TryParse(token.Value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var value))
                return Expression.Constant((object?)value);
            throw new ParseException($"Invalid number '{token.Value}' at position {_position - 1}");
        }

        if (Peek(TokenType.String))
        {
            var token = Consume(TokenType.String);
            return Expression.Constant((object?)token.Value);
        }

        throw new ParseException($"Unexpected token {Current?.Type} (value: {Current?.Value}) at position {_position}");
    }

    private Expression ParseIdentifierExpression()
    {
        var identifier = Consume(TokenType.Identifier).Value;
        Expression expr = CreateBaseExpression(identifier);
        string? lastIdentifier = identifier;

        while (true)
        {
            if (Match(TokenType.Dot))
            {
                expr = ParsePropertyAccess(expr, ref lastIdentifier);
            }
            else if (Match(TokenType.LBracket))
            {
                expr = ParseIndexerAccess(expr);
                lastIdentifier = null;
            }
            else if (Match(TokenType.LParen))
            {
                expr = ParseMethodCall(expr, lastIdentifier ?? identifier);
                lastIdentifier = null;
            }
            else break;
        }

        return expr;
    }

    private Expression CreateBaseExpression(string identifier)
    {
        // "Variables" property on the context (e.g. context.Variables)
        if (string.Equals(identifier, _variablesPropertyName, StringComparison.Ordinal))
            return Expression.Property(_contextParam, _variablesPropertyName);

        if (string.Equals(identifier, _contextIdentifier, StringComparison.Ordinal))
            return _contextParam;

        // fallback: indexer access on context.Variables (calls get_Item)
        var variablesProp = Expression.Property(_contextParam, _variablesPropertyName);
        return Expression.Call(variablesProp, "get_Item", typeArguments: null, Expression.Constant(identifier));
    }

    private Expression ParsePropertyAccess(Expression expr, ref string? lastIdentifier)
    {
        lastIdentifier = Consume(TokenType.Identifier).Value;
        var nameExpr = Expression.Constant(lastIdentifier);
        return Expression.Call(typeof(ExpressionHelper), nameof(ExpressionHelper.GetProperty), typeArguments: null, expr, nameExpr);
    }

    private Expression ParseIndexerAccess(Expression expr)
    {
        var indexExpr = ParseOrExpression();
        Expect(TokenType.RBracket);
        return Expression.Call(typeof(ExpressionHelper), nameof(ExpressionHelper.GetIndexer), typeArguments: null, expr, ConvertToObject(indexExpr));
    }

    private Expression ParseMethodCall(Expression expr, string methodName)
    {
        var args = ParseMethodArguments();
        var argsArray = Expression.NewArrayInit(typeof(object), args.Select(a => ConvertToObject(a)));
        return Expression.Call(CallMethod, expr, Expression.Constant(methodName), argsArray);
    }

    private List<Expression> ParseMethodArguments()
    {
        var args = new List<Expression>();
        if (!Peek(TokenType.RParen))
        {
            do
            {
                args.Add(ParseOrExpression());
            }
            while (Match(TokenType.Comma));
        }
        Expect(TokenType.RParen);
        return args;
    }

    // Custom parse exception to provide context
    private sealed class ParseException(string message) : Exception(message)
    {
    }
}