namespace FlowFusion.Tests.Expression;
[TestClass]
public class ExpressionEvaluatorTests
{
    private ExpressionEvaluator _evaluator;
    private static readonly FlowExecutionContext _context = new(new Dictionary<string, object?>
    {
        ["x"] = 10.0, // Use double for consistent comparisons
        ["y"] = 5.0,
        ["name"] = "test",
        ["flag"] = true,
        ["obj"] = new TestObject { Value = 42, Name = "testObj" },
        ["list"] = new List<int> { 1, 2, 3, 4, 5 }
    });
    [TestInitialize]
    public void Setup() => _evaluator = new ExpressionEvaluator();
    [TestMethod]
    public async Task EvaluateAsync_SimpleBooleanVariable_ReturnsValue()
    {
        // Arrange
        var context = new FlowExecutionContext(new Dictionary<string, object?> { ["isTrue"] = true });
        // Act
        var result = await _evaluator.EvaluateAsync("isTrue", context, TestContext.CancellationToken);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_NumericComparison_Equal_ReturnsTrue()
    {
        // Act
        var result = await _evaluator.EvaluateAsync("x == 10", _context, TestContext.CancellationToken);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_NumericComparison_NotEqual_ReturnsTrue()
    {
        // Act
        var result = await _evaluator.EvaluateAsync("x != 5", _context, TestContext.CancellationToken);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_NumericComparison_GreaterThan_ReturnsTrue()
    {
        // Act
        var result = await _evaluator.EvaluateAsync("x > 5", _context, TestContext.CancellationToken);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_NumericComparison_LessThan_ReturnsTrue()
    {
        // Act
        var result = await _evaluator.EvaluateAsync("y < 10", _context, TestContext.CancellationToken);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_NumericComparison_GreaterEqual_ReturnsTrue()
    {
        // Act
        var result = await _evaluator.EvaluateAsync("x >= 10", _context, TestContext.CancellationToken);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_NumericComparison_LessEqual_ReturnsTrue()
    {
        // Act
        var result = await _evaluator.EvaluateAsync("y <= 5", _context, TestContext.CancellationToken);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_BooleanOperations_And_ReturnsTrue()
    {
        // Act
        var result = await _evaluator.EvaluateAsync("x > 5 && y < 10", _context, TestContext.CancellationToken);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_BooleanOperations_Or_ReturnsTrue()
    {
        // Act
        var result = await _evaluator.EvaluateAsync("x < 5 || y > 0", _context, TestContext.CancellationToken);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_BooleanOperations_Not_ReturnsTrue()
    {
        // Arrange
        var context = new FlowExecutionContext(new Dictionary<string, object?> { ["flag"] = false });
        // Act
        var result = await _evaluator.EvaluateAsync("!flag", context, TestContext.CancellationToken);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_ArithmeticOperations_Addition_ReturnsTrue()
    {
        // Act
        var result = await _evaluator.EvaluateAsync("x + y == 15", _context, TestContext.CancellationToken);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_ArithmeticOperations_Subtraction_ReturnsTrue()
    {
        // Act
        var result = await _evaluator.EvaluateAsync("x - y == 5", _context, TestContext.CancellationToken);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_ArithmeticOperations_Multiplication_ReturnsTrue()
    {
        // Act
        var result = await _evaluator.EvaluateAsync("x * y == 50", _context, TestContext.CancellationToken);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_ArithmeticOperations_Division_ReturnsTrue()
    {
        // Act
        var result = await _evaluator.EvaluateAsync("x / y == 2", _context, TestContext.CancellationToken);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_ArithmeticOperations_Modulo_ReturnsTrue()
    {
        // Act
        var result = await _evaluator.EvaluateAsync("x % 3 == 1", _context, TestContext.CancellationToken);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_PropertyAccess_ReturnsTrue()
    {
        // Arrange
        var context = new FlowExecutionContext(new Dictionary<string, object?>
        {
            ["obj"] = new TestObject { Value = 42, Name = "testObj" }
        });
        // Act
        var result = await _evaluator.EvaluateAsync("obj.Value == 42", context, TestContext.CancellationToken);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_PropertyAccess_Deep_ReturnsTrue()
    {
        // Act
        var result = await _evaluator.EvaluateAsync("obj.Name == \"testObj\"", _context, TestContext.CancellationToken);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_IndexerAccess_ReturnsTrue()
    {
        // Arrange
        var context = new FlowExecutionContext(new Dictionary<string, object?>
        {
            ["list"] = new List<int> { 1, 2, 3, 4, 5 }
        });
        // Act
        var result = await _evaluator.EvaluateAsync("list[0] == 1", context, TestContext.CancellationToken);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_MethodCall_ReturnsTrue()
    {
        // Arrange
        var context = new FlowExecutionContext(new Dictionary<string, object?>
        {
            ["name"] = "test"
        });
        // Act
        var result = await _evaluator.EvaluateAsync("name.Length == 4", context, TestContext.CancellationToken);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_ComplexExpression_ReturnsTrue()
    {
        // Act
        var result = await _evaluator.EvaluateAsync("(x > 5 && y < 10) || (obj.Value == 42 && flag)", _context, TestContext.CancellationToken);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_VariablesPropertyAccess_ReturnsTrue()
    {
        // Arrange
        var context = new FlowExecutionContext(new Dictionary<string, object?> { ["x"] = 10.0 });
        // Act
        var result = await _evaluator.EvaluateAsync("Variables[\"x\"] == 10", context, TestContext.CancellationToken);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_ContextPropertyAccess_ReturnsTrue()
    {
        // Arrange
        var context = new FlowExecutionContext(new Dictionary<string, object?> { ["x"] = 10.0 });
        // Act
        var result = await _evaluator.EvaluateAsync("context.Variables[\"x\"] == 10", context, TestContext.CancellationToken);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_EmptyExpression_ReturnsFalse()
    {
        // Act
        var result = await _evaluator.EvaluateAsync("", _context, TestContext.CancellationToken);
        // Assert
        Assert.IsFalse(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_WhitespaceExpression_ReturnsFalse()
    {
        // Act
        var result = await _evaluator.EvaluateAsync("   ", _context, TestContext.CancellationToken);
        // Assert
        Assert.IsFalse(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_NullExpression_ReturnsFalse()
    {
        // Act
        var result = await _evaluator.EvaluateAsync(null, _context, TestContext.CancellationToken);
        // Assert
        Assert.IsFalse(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_InvalidExpression_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _evaluator.EvaluateAsync("invalid @ syntax", _context, TestContext.CancellationToken));
    }
    [TestMethod]
    public async Task WarmupAsync_ValidExpression_DoesNotThrow()
    {
        // Act & Assert - Should not throw
        await _evaluator.WarmupAsync("x > 5", TestContext.CancellationToken);
    }
    [TestMethod]
    public async Task WarmupAsync_EmptyExpression_DoesNotThrow()
    {
        // Act & Assert - Should not throw
        await _evaluator.WarmupAsync("", TestContext.CancellationToken);
    }
    [TestMethod]
    public async Task WarmupAsync_InvalidExpression_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _evaluator.WarmupAsync("invalid @ syntax", TestContext.CancellationToken));
    }
    [TestMethod]
    public async Task EvaluateAsync_WithCancellation_CanBeCancelled()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var context = new FlowExecutionContext(new Dictionary<string, object?> { ["flag"] = true });
        cts.Cancel();
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _evaluator.EvaluateAsync("flag", context, cts.Token));
    }
    [TestMethod]
    public async Task EvaluateAsync_FloatNumbers_ReturnsTrue()
    {
        // Arrange
        var context = new FlowExecutionContext(new Dictionary<string, object?> { ["pi"] = 3.14 });
        // Act
        var result = await _evaluator.EvaluateAsync("pi > 3.0", context, TestContext.CancellationToken);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_StringLiterals_ReturnsTrue()
    {
        // Act
        var result = await _evaluator.EvaluateAsync("name == \"test\"", _context, TestContext.CancellationToken);
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_Precedence_AdditionBeforeComparison_ReturnsTrue()
    {
        // Act
        var result = await _evaluator.EvaluateAsync("x + y > 10", _context, TestContext.CancellationToken); // 10 + 5 = 15 > 10
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_Precedence_MultiplicationBeforeAddition_ReturnsTrue()
    {
        // Act
        var result = await _evaluator.EvaluateAsync("x + y * 2 == 20", _context, TestContext.CancellationToken); // 10 + (5 * 2) = 20
        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task EvaluateAsync_Precedence_ParenthesesOverride_ReturnsTrue()
    {
        // Act
        var result = await _evaluator.EvaluateAsync("(x + y) * 2 == 30", _context, TestContext.CancellationToken); // (10 + 5) * 2 = 30
        // Assert
        Assert.IsTrue(result);
    }
    private class TestObject
    {
        public int Value { get; set; }
        public string Name { get; set; } = "";
    }
    public TestContext TestContext { get; set; }
}