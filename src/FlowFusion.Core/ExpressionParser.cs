namespace FlowFusion.Core;

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
            return ParseIdentifierExpression();

        if (Peek(TokenType.Number))
        {
            var token = Consume(TokenType.Number);
            if (double.TryParse(token.Value, out var value))
                return Expr.Constant((object?)value);
            throw new ArgumentException($"Invalid number: {token.Value}");
        }
        if (Peek(TokenType.String))
            return Expr.Constant((object?)Consume(TokenType.String).Value);

        throw new ArgumentException($"Unexpected token: {Current?.Type}");
    }
    private Expr ParseIdentifierExpression()
    {
        var identifier = Consume(TokenType.Identifier).Value;
        var expr = CreateBaseExpression(identifier);
        var lastIdentifier = identifier;

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
            else
            {
                break;
            }
        }

        return expr;
    }
    private Expr CreateBaseExpression(string identifier)
    {
        return identifier switch
        {
            "Variables" => Expr.Property(contextParam, "Variables"),
            "context" => contextParam,
            _ => Expr.Call(Expr.Property(contextParam, "Variables"), "get_Item", null, Expr.Constant(identifier))
        };
    }
    private MethodCallExpression ParsePropertyAccess(Expr expr, ref string lastIdentifier)
    {
        lastIdentifier = Consume(TokenType.Identifier).Value;
        return Expr.Call(typeof(ExpressionHelper), "GetProperty", null, expr, Expr.Constant(lastIdentifier));
    }
    private MethodCallExpression ParseIndexerAccess(Expr expr)
    {
        var indexExpr = ParseOrExpression();
        Expect(TokenType.RBracket);
        return Expr.Call(typeof(ExpressionHelper), "GetIndexer", null, expr, Expr.Convert(indexExpr, typeof(object)));
    }
    private MethodCallExpression ParseMethodCall(Expr expr, string methodName)
    {
        var args = ParseMethodArguments();
        var argsArray = Expr.NewArrayInit(typeof(object), args.Select(a => Expr.Convert(a, typeof(object))));
        return Expr.Call(typeof(ExpressionHelper), "CallMethod", null, expr, Expr.Constant(methodName), argsArray);
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
    private bool Match(TokenType type)
    {
        if (Peek(type)) { _position++; return true; }
        return false;
    }
    private bool Peek(TokenType type) => Current?.Type == type;
    private Token Consume(TokenType type)
    {
        if (!Peek(type)) throw new ArgumentException($"Expected {type}, got {Current?.Type}");
        return tokens[_position++];
    }
    private void Expect(TokenType type) => Consume(type);
    private Token? Current => _position < tokens.Count ? tokens[_position] : null;
}