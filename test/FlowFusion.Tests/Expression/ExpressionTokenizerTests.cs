namespace FlowFusion.Tests.Expression;

[TestClass]
public class ExpressionTokenizerTests
{
    private ExpressionTokenizer _tokenizer;

    [TestInitialize]
    public void Setup() => _tokenizer = new ExpressionTokenizer();

    [TestMethod]
    public void Tokenize_ValidExpression_ReturnsTokens()
    {
        // Act
        var tokens = _tokenizer.Tokenize("x == 10");

        // Assert
        Assert.AreEqual(3, tokens.Count);
        Assert.AreEqual(TokenType.Identifier, tokens[0].Type);
        Assert.AreEqual("x", tokens[0].Value);
        Assert.AreEqual(TokenType.Equal, tokens[1].Type);
        Assert.AreEqual(TokenType.Number, tokens[2].Type);
    }

    [TestMethod]
    public void Tokenize_StringLiteral_ReturnsStringToken()
    {
        // Act
        var tokens = _tokenizer.Tokenize("\"hello\"");

        // Assert
        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(TokenType.String, tokens[0].Type);
        Assert.AreEqual("hello", tokens[0].Value);
    }

    [TestMethod]
    public void Tokenize_EscapedString_ReturnsCorrectValue()
    {
        // Act
        var tokens = _tokenizer.Tokenize("\"hel\\\"lo\"");

        // Assert
        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual("hel\\\"lo", tokens[0].Value);
    }

    [TestMethod]
    public void Tokenize_UnclosedString_ThrowsArgumentException()
    {
        // Act & Assert
        try
        {
            _tokenizer.Tokenize("\"unclosed");
            Assert.Fail("Expected ArgumentException");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void Tokenize_UnterminatedEscape_ThrowsArgumentException()
    {
        // Act & Assert
        try
        {
            _tokenizer.Tokenize("\"test\\");
            Assert.Fail("Expected ArgumentException");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void Tokenize_TooLongExpression_ThrowsArgumentException()
    {
        // Arrange
        var longExpr = new string('x', 10001);

        // Act & Assert
        try
        {
            _tokenizer.Tokenize(longExpr);
            Assert.Fail("Expected ArgumentException");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void Tokenize_InvalidCharacter_ThrowsArgumentException()
    {
        // Act & Assert
        try
        {
            _tokenizer.Tokenize("x @ y");
            Assert.Fail("Expected ArgumentException");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void Tokenize_ComplexExpression_ReturnsAllTokens()
    {
        // Act
        var tokens = _tokenizer.Tokenize("x > 5 && y <= 10");

        // Assert
        Assert.AreEqual(7, tokens.Count);
    }
}