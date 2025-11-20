namespace FlowFusion.Expression;

internal record Token(TokenType Type, string Value);

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
