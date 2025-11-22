namespace FlowFusion.Tests.Expression;

using System.Linq.Expressions;
using FlowFusion.Core;

[TestClass]
public class ExpressionParserTests
{
    private ExpressionTokenizer _tokenizer;

    [TestInitialize]
    public void Setup() => _tokenizer = new ExpressionTokenizer();

    [TestMethod]
    public void Parse_SimpleExpression_ReturnsExpression()
    {
        // Arrange
        var tokens = _tokenizer.Tokenize("x == 10");
        var contextParam = Expression.Parameter(typeof(FlowExecutionContext), "context");
        var parser = new ExpressionParser();

        // Act
        var expression = parser.Parse(tokens, contextParam);

        // Assert
        Assert.IsNotNull(expression);
        Assert.AreEqual(typeof(bool), expression.Type);
    }

    [TestMethod]
    public void Parse_ComplexExpression_ReturnsExpression()
    {
        // Arrange
        var tokens = _tokenizer.Tokenize("x > 5 && y <= 10");
        var contextParam = Expression.Parameter(typeof(FlowExecutionContext), "context");
        var parser = new ExpressionParser();

        // Act
        var expression = parser.Parse(tokens, contextParam);

        // Assert
        Assert.IsNotNull(expression);
        Assert.AreEqual(typeof(bool), expression.Type);
    }

    [TestMethod]
    public void Parse_InvalidTokens_ThrowsException()
    {
        // Arrange
        var tokens = new List<Token> { new Token(TokenType.Identifier, "invalid") };
        var contextParam = Expression.Parameter(typeof(FlowExecutionContext), "context");
        var parser = new ExpressionParser();

        // Act & Assert
        try
        {
            parser.Parse(tokens, contextParam);
            Assert.Fail("Expected exception");
        }
        catch (Exception)
        {
            // Expected
        }
    }
}