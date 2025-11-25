namespace FlowFusion.Core;

/// <summary>
/// A small interpreter with parser, AST nodes (LiteralExpr, IdentifierExpr, CallExpr) and evaluation logic.
/// Supports literals (string/number/true/false/null), identifiers mapping to variables, and function calls name(args...).
/// </summary>
internal static class SimpleInterpreter
{
    /// <summary>
    /// Compiles an expression string into a compiled expression.
    /// </summary>
    /// <param name="expression">The expression to compile.</param>
    /// <param name="registry">The function registry for function lookups.</param>
    /// <returns>A compiled expression.</returns>
    public static ISimpleCompiledExpression Compile(string expression, FunctionRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(registry);
        
        var parser = new Parser(expression);
        var root = parser.Parse();
        return new SimpleCompiledExpression(root, registry);
    }

    #region AST Nodes

    /// <summary>
    /// Base class for expression AST nodes.
    /// </summary>
    internal abstract class Expr
    {
        public abstract Task<object?> EvaluateAsync(ExecutionContext context, FunctionRegistry registry);
    }

    /// <summary>
    /// Represents a literal value (string, number, bool, null).
    /// </summary>
    internal sealed class LiteralExpr : Expr
    {
        private readonly object? _value;

        public LiteralExpr(object? value) => _value = value;

        public override Task<object?> EvaluateAsync(ExecutionContext context, FunctionRegistry registry)
        {
            return Task.FromResult(_value);
        }
    }

    /// <summary>
    /// Represents an identifier that maps to a variable.
    /// </summary>
    internal sealed class IdentifierExpr : Expr
    {
        private readonly string _name;

        public IdentifierExpr(string name) => _name = name;

        public override Task<object?> EvaluateAsync(ExecutionContext context, FunctionRegistry registry)
        {
            if (context.Variables.TryGetValue(_name, out var value))
            {
                return Task.FromResult(value);
            }
            throw new EvaluationException($"Variable '{_name}' is not defined.");
        }
    }

    /// <summary>
    /// Represents a function call: name(args...).
    /// </summary>
    internal sealed class CallExpr : Expr
    {
        private readonly string _functionName;
        private readonly Expr[] _arguments;

        public CallExpr(string functionName, Expr[] arguments)
        {
            _functionName = functionName;
            _arguments = arguments;
        }

        public override async Task<object?> EvaluateAsync(ExecutionContext context, FunctionRegistry registry)
        {
            if (!registry.TryGet(_functionName, out var function) || function == null)
            {
                throw new EvaluationException($"Function '{_functionName}' is not registered.");
            }

            var args = new object?[_arguments.Length];
            for (int i = 0; i < _arguments.Length; i++)
            {
                args[i] = await _arguments[i].EvaluateAsync(context, registry).ConfigureAwait(false);
            }

            return await function(context, args, context.CancellationToken).ConfigureAwait(false);
        }
    }

    #endregion

    #region Parser

    /// <summary>
    /// Simple recursive descent parser for expressions.
    /// </summary>
    internal sealed class Parser
    {
        private readonly string _input;
        private int _pos;

        public Parser(string input)
        {
            _input = input;
            _pos = 0;
        }

        public Expr Parse()
        {
            SkipWhitespace();
            var expr = ParseExpression();
            SkipWhitespace();
            if (_pos < _input.Length)
            {
                throw new ParseException($"Unexpected character '{_input[_pos]}' at position {_pos}.");
            }
            return expr;
        }

        private Expr ParseExpression()
        {
            SkipWhitespace();

            if (_pos >= _input.Length)
            {
                throw new ParseException("Unexpected end of expression.");
            }

            char c = _input[_pos];

            // String literal
            if (c == '"' || c == '\'')
            {
                return ParseStringLiteral();
            }

            // Number literal
            if (char.IsDigit(c) || (c == '-' && _pos + 1 < _input.Length && char.IsDigit(_input[_pos + 1])))
            {
                return ParseNumberLiteral();
            }

            // Identifier or keyword (true/false/null) or function call
            if (char.IsLetter(c) || c == '_')
            {
                return ParseIdentifierOrCall();
            }

            throw new ParseException($"Unexpected character '{c}' at position {_pos}.");
        }

        /// <summary>
        /// Parses a string literal. Escape sequences are recognized here by skipping 2 characters
        /// for any backslash followed by another character. The actual interpretation of escape
        /// sequences is handled by <see cref="UnescapeString"/>, which must stay in sync with this method.
        /// </summary>
        private Expr ParseStringLiteral()
        {
            char quote = _input[_pos];
            _pos++; // skip opening quote
            int start = _pos;

            while (_pos < _input.Length && _input[_pos] != quote)
            {
                if (_input[_pos] == '\\' && _pos + 1 < _input.Length)
                {
                    _pos += 2; // skip escape sequence (see UnescapeString for interpretation)
                }
                else
                {
                    _pos++;
                }
            }

            if (_pos >= _input.Length)
            {
                throw new ParseException("Unterminated string literal.");
            }

            string value = UnescapeString(_input[start.._pos]);
            _pos++; // skip closing quote
            return new LiteralExpr(value);
        }

        private static string UnescapeString(string s)
        {
            if (!s.Contains('\\'))
                return s;

            var sb = new System.Text.StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '\\' && i + 1 < s.Length)
                {
                    char next = s[i + 1];
                    sb.Append(next switch
                    {
                        'n' => '\n',
                        'r' => '\r',
                        't' => '\t',
                        '\\' => '\\',
                        '"' => '"',
                        '\'' => '\'',
                        _ => next
                    });
                    i++;
                }
                else
                {
                    sb.Append(s[i]);
                }
            }
            return sb.ToString();
        }

        private Expr ParseNumberLiteral()
        {
            int start = _pos;
            if (_input[_pos] == '-')
            {
                _pos++;
            }

            while (_pos < _input.Length && char.IsDigit(_input[_pos]))
            {
                _pos++;
            }

            bool isDouble = false;
            if (_pos < _input.Length && _input[_pos] == '.')
            {
                isDouble = true;
                _pos++;
                while (_pos < _input.Length && char.IsDigit(_input[_pos]))
                {
                    _pos++;
                }
            }

            string numStr = _input[start.._pos];
            if (isDouble)
            {
                if (!double.TryParse(numStr, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double d))
                {
                    throw new ParseException($"Invalid number: {numStr}");
                }
                return new LiteralExpr(d);
            }
            else
            {
                if (!long.TryParse(numStr, NumberStyles.Integer | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long l))
                {
                    throw new ParseException($"Invalid number: {numStr}");
                }
                // Return int if in range, otherwise long
                if (l >= int.MinValue && l <= int.MaxValue)
                {
                    return new LiteralExpr((int)l);
                }
                return new LiteralExpr(l);
            }
        }

        private Expr ParseIdentifierOrCall()
        {
            int start = _pos;
            while (_pos < _input.Length && (char.IsLetterOrDigit(_input[_pos]) || _input[_pos] == '_'))
            {
                _pos++;
            }

            string identifier = _input[start.._pos];
            SkipWhitespace();

            // Check for function call
            if (_pos < _input.Length && _input[_pos] == '(')
            {
                return ParseFunctionCall(identifier);
            }

            // Check for keywords
            return identifier switch
            {
                "true" => new LiteralExpr(true),
                "false" => new LiteralExpr(false),
                "null" => new LiteralExpr(null),
                _ => new IdentifierExpr(identifier)
            };
        }

        private Expr ParseFunctionCall(string functionName)
        {
            _pos++; // skip '('
            var args = new List<Expr>();

            SkipWhitespace();
            while (_pos < _input.Length && _input[_pos] != ')')
            {
                args.Add(ParseExpression());
                SkipWhitespace();

                if (_pos < _input.Length && _input[_pos] == ',')
                {
                    _pos++; // skip ','
                    SkipWhitespace();
                }
            }

            if (_pos >= _input.Length || _input[_pos] != ')')
            {
                throw new ParseException("Expected ')' in function call.");
            }

            _pos++; // skip ')'
            return new CallExpr(functionName, args.ToArray());
        }

        private void SkipWhitespace()
        {
            while (_pos < _input.Length && char.IsWhiteSpace(_input[_pos]))
            {
                _pos++;
            }
        }
    }

    #endregion
}

/// <summary>
/// Exception thrown when parsing an expression fails.
/// </summary>
internal sealed class ParseException : Exception
{
    public ParseException(string message) : base(message) { }
}

/// <summary>
/// Exception thrown when evaluating an expression fails.
/// </summary>
internal sealed class EvaluationException : Exception
{
    public EvaluationException(string message) : base(message) { }
}
