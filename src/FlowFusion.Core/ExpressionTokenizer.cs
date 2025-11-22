namespace FlowFusion.Core;

internal sealed class ExpressionTokenizer : ITokenizer
{
    public List<Token> Tokenize(string expression)
    {
        if (expression.Length > 10000)
            throw new ArgumentException("Expression is too long. Maximum allowed length is 10000 characters.", nameof(expression));
        return Tokenize(expression.AsSpan());
    }

    private static List<Token> Tokenize(ReadOnlySpan<char> expression)
    {
        var tokens = new List<Token>(Math.Max(1, expression.Length / 4)); // Estimate capacity
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

            var context = i > 10 ? expression[(i - 10)..i].ToString() + c : expression[0..(i + 1)].ToString();
            throw new ArgumentException($"Unexpected character '{c}' at position {i}. Context: '{context}'");
        }
        return tokens;
    }

    private static bool TryTokenizeIdentifier(ReadOnlySpan<char> expression, ref int i, List<Token> tokens)
    {
        if (!char.IsLetter(expression[i]) && expression[i] != '_')
            return false;

        var start = i;
        while (i < expression.Length && (char.IsLetterOrDigit(expression[i]) || expression[i] == '_')) i++;
        tokens.Add(new Token(TokenType.Identifier, expression[start..i].ToString()));
        return true;
    }

    private static bool TryTokenizeNumber(ReadOnlySpan<char> expression, ref int i, List<Token> tokens)
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
        tokens.Add(new Token(TokenType.Number, expression[start..i].ToString()));
        return true;
    }

    private static bool TryTokenizeString(ReadOnlySpan<char> expression, ref int i, List<Token> tokens)
    {
        if (expression[i] != '"')
            return false;

        i++; // skip opening "
        var start = i;
        while (i < expression.Length)
        {
            if (expression[i] == '"')
                break;
            if (expression[i] == '\\')
            {
                i++; // skip \
                if (i < expression.Length)
                    i++; // skip escaped char
                else
                    throw new ArgumentException($"Unterminated escape sequence at position {i - 1}");
            }
            else
            {
                i++;
            }
        }
        if (i >= expression.Length)
            throw new ArgumentException($"Unclosed string literal starting at position {start - 1}");
        tokens.Add(new Token(TokenType.String, expression[start..i].ToString()));
        i++; // skip closing "
        return true;
    }

    private static bool TryTokenizeOperator(ReadOnlySpan<char> expression, ref int i, List<Token> tokens)
    {
        // Check for double-character operators first
        if (i + 1 < expression.Length)
        {
            var doubleOp = expression[i..(i + 2)];
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

    private static Token? GetDoubleCharOperatorToken(ReadOnlySpan<char> op) => op switch
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

    private static bool TryTokenizeSymbol(ReadOnlySpan<char> expression, ref int i, List<Token> tokens)
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