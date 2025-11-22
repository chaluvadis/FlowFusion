namespace FlowFusion.Core;



internal sealed class ExpressionParser : IExpressionParser
{
    private IReadOnlyList<Token> _tokens;
    private ParameterExpression _contextParam;
    private string _variablesPropertyName;
    private string _contextIdentifier;
    private bool _autoVariablesMode;
    private int _position;

    public Expr Parse(IReadOnlyList<Token> tokens, ParameterExpression contextParam, string variablesPropertyName = "Variables", string contextIdentifier = "context", bool autoVariablesMode = true)
    {
        _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
        _contextParam = contextParam ?? throw new ArgumentNullException(nameof(contextParam));
        _variablesPropertyName = variablesPropertyName ?? throw new ArgumentNullException(nameof(variablesPropertyName));
        _contextIdentifier = contextIdentifier ?? throw new ArgumentNullException(nameof(contextIdentifier));
        _autoVariablesMode = autoVariablesMode;
        _position = 0;
        return ParseOrExpression();
    }

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

    private static readonly FrozenDictionary<string, MethodInfo> _methodCache = new Dictionary<string, MethodInfo>
    {
        { "Equal", GetHelperMethod(nameof(ExpressionHelper.Equal), typeof(object), typeof(object)) },
        { "NotEqual", GetHelperMethod(nameof(ExpressionHelper.NotEqual), typeof(object), typeof(object)) },
        { "LessThan", GetHelperMethod(nameof(ExpressionHelper.LessThan), typeof(object), typeof(object)) },
        { "GreaterThan", GetHelperMethod(nameof(ExpressionHelper.GreaterThan), typeof(object), typeof(object)) },
        { "LessEqual", GetHelperMethod(nameof(ExpressionHelper.LessEqual), typeof(object), typeof(object)) },
        { "GreaterEqual", GetHelperMethod(nameof(ExpressionHelper.GreaterEqual), typeof(object), typeof(object)) },
        { "And", GetHelperMethod(nameof(ExpressionHelper.And), typeof(object), typeof(object)) },
        { "Or", GetHelperMethod(nameof(ExpressionHelper.Or), typeof(object), typeof(object)) },
        { "Not", GetHelperMethod(nameof(ExpressionHelper.Not), typeof(object)) },
        { "Add", GetHelperMethod(nameof(ExpressionHelper.Add), typeof(object), typeof(object)) },
        { "Subtract", GetHelperMethod(nameof(ExpressionHelper.Subtract), typeof(object), typeof(object)) },
        { "Multiply", GetHelperMethod(nameof(ExpressionHelper.Multiply), typeof(object), typeof(object)) },
        { "Divide", GetHelperMethod(nameof(ExpressionHelper.Divide), typeof(object), typeof(object)) },
        { "Modulo", GetHelperMethod(nameof(ExpressionHelper.Modulo), typeof(object), typeof(object)) },
        { "CallMethod", GetHelperMethod(nameof(ExpressionHelper.CallMethod), typeof(object), typeof(string), typeof(object[])) },
    }.ToFrozenDictionary();
    #endregion

    #region Parsing helpers
    private static Expr ConvertToObject(Expr e) => Expr.Convert(e, typeof(object));
    private Token? Current => _position < _tokens.Count ? _tokens[_position] : null;
    private bool Peek(TokenType type) => Current is not null && Current.Type == type;
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

    private Expr ParseOrExpression()
    {
        var left = ParseAndExpression();
        while (Match(TokenType.Or))
        {
            var right = ParseAndExpression();
            left = Expr.Call(_methodCache["Or"], ConvertToObject(left), ConvertToObject(right));
        }
        return left;
    }

    private Expr ParseAndExpression()
    {
        var left = ParseEqualityExpression();
        while (Match(TokenType.And))
        {
            var right = ParseEqualityExpression();
            left = Expr.Call(_methodCache["And"], ConvertToObject(left), ConvertToObject(right));
        }
        return left;
    }

    private Expr ParseEqualityExpression()
    {
        var left = ParseRelationalExpression();
        if (Match(TokenType.Equal))
        {
            var right = ParseRelationalExpression();
            left = Expr.Call(_methodCache["Equal"], ConvertToObject(left), ConvertToObject(right));
        }
        else if (Match(TokenType.NotEqual))
        {
            var right = ParseRelationalExpression();
            left = Expr.Call(_methodCache["NotEqual"], ConvertToObject(left), ConvertToObject(right));
        }
        return left;
    }

    private Expr ParseRelationalExpression()
    {
        var left = ParseAdditiveExpression();

        if (Match(TokenType.Less))
        {
            var right = ParseAdditiveExpression();
            left = Expr.Call(_methodCache["LessThan"], ConvertToObject(left), ConvertToObject(right));
        }
        else if (Match(TokenType.Greater))
        {
            var right = ParseAdditiveExpression();
            left = Expr.Call(_methodCache["GreaterThan"], ConvertToObject(left), ConvertToObject(right));
        }
        else if (Match(TokenType.LessEqual))
        {
            var right = ParseAdditiveExpression();
            left = Expr.Call(_methodCache["LessEqual"], ConvertToObject(left), ConvertToObject(right));
        }
        else if (Match(TokenType.GreaterEqual))
        {
            var right = ParseAdditiveExpression();
            left = Expr.Call(_methodCache["GreaterEqual"], ConvertToObject(left), ConvertToObject(right));
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
                left = Expr.Call(_methodCache["Add"], ConvertToObject(left), ConvertToObject(right));
            }
            else if (Match(TokenType.Minus))
            {
                var right = ParseMultiplicativeExpression();
                left = Expr.Call(_methodCache["Subtract"], ConvertToObject(left), ConvertToObject(right));
            }
            else break;
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
                left = Expr.Call(_methodCache["Multiply"], ConvertToObject(left), ConvertToObject(right));
            }
            else if (Match(TokenType.Divide))
            {
                var right = ParseUnaryExpression();
                left = Expr.Call(_methodCache["Divide"], ConvertToObject(left), ConvertToObject(right));
            }
            else if (Match(TokenType.Modulo))
            {
                var right = ParseUnaryExpression();
                left = Expr.Call(_methodCache["Modulo"], ConvertToObject(left), ConvertToObject(right));
            }
            else break;
        }
        return left;
    }

    private Expr ParseUnaryExpression()
    {
        if (Match(TokenType.Not))
        {
            var operand = ParseUnaryExpression();
            return Expr.Call(_methodCache["Not"], ConvertToObject(operand));
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
            return ParseIdentifierExpression();

        if (Peek(TokenType.Number))
        {
            var token = Consume(TokenType.Number);
            if (double.TryParse(token.Value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var value))
                return Expr.Constant((object?)value);
            throw new ParseException($"Invalid number '{token.Value}' at position {_position - 1}");
        }

        if (Peek(TokenType.String))
        {
            var token = Consume(TokenType.String);
            return Expr.Constant((object?)token.Value);
        }

        throw new ParseException($"Unexpected token {Current?.Type} (value: {Current?.Value}) at position {_position}");
    }

    private Expr ParseIdentifierExpression()
    {
        var identifier = Consume(TokenType.Identifier).Value;
        Expr expr = CreateBaseExpression(identifier);
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

    private Expr CreateBaseExpression(string identifier)
    {
        // "Variables" property on the context (e.g. context.Variables)
        if (string.Equals(identifier, _variablesPropertyName, StringComparison.Ordinal))
            return Expr.Property(_contextParam, _variablesPropertyName);

        if (string.Equals(identifier, _contextIdentifier, StringComparison.Ordinal))
            return _contextParam;

        // In auto-variables mode, treat bare identifiers as Variables["identifier"]
        if (_autoVariablesMode)
        {
            var variablesProp = Expr.Property(_contextParam, _variablesPropertyName);
            return Expr.Call(variablesProp, "get_Item", typeArguments: null, Expr.Constant(identifier));
        }

        // fallback: indexer access on context.Variables (calls get_Item)
        var variablesPropFallback = Expr.Property(_contextParam, _variablesPropertyName);
        return Expr.Call(variablesPropFallback, "get_Item", typeArguments: null, Expr.Constant(identifier));
    }

    private Expr ParsePropertyAccess(Expr expr, ref string? lastIdentifier)
    {
        lastIdentifier = Consume(TokenType.Identifier).Value;
        var nameExpr = Expr.Constant(lastIdentifier);
        return Expr.Call(typeof(ExpressionHelper), nameof(ExpressionHelper.GetProperty), typeArguments: null, expr, nameExpr);
    }

    private Expr ParseIndexerAccess(Expr expr)
    {
        var indexExpr = ParseOrExpression();
        Expect(TokenType.RBracket);
        return Expr.Call(typeof(ExpressionHelper), nameof(ExpressionHelper.GetIndexer), typeArguments: null, expr, ConvertToObject(indexExpr));
    }

    private Expr ParseMethodCall(Expr expr, string methodName)
    {
        var args = ParseMethodArguments();
        var argsArray = Expr.NewArrayInit(typeof(object), args.Select(a => ConvertToObject(a)));
        return Expr.Call(_methodCache["CallMethod"], expr, Expr.Constant(methodName), argsArray);
    }

    private List<Expr> ParseMethodArguments()
    {
        var args = new List<Expr>();
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