using FlowFusion.Core;
using Xunit;

namespace FlowFusion.Core.Tests;

public class FlowEnvironmentTests
{
    #region isOwner Tests

    [Fact]
    public void EvaluateBoolean_IsOwner_True_WhenFunctionReturnsTrue()
    {
        // Arrange
        var env = new FlowEnvironment();
        env.RegisterFunction("isOwner", (ctx, args, ct) => Task.FromResult<object?>(true));
        var variables = new Dictionary<string, object?> { ["userId"] = "user123" };

        // Act
        bool result = env.EvaluateBoolean("isOwner(userId)", variables);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateBoolean_IsOwner_False_WhenFunctionReturnsFalse()
    {
        // Arrange
        var env = new FlowEnvironment();
        env.RegisterFunction("isOwner", (ctx, args, ct) => Task.FromResult<object?>(false));
        var variables = new Dictionary<string, object?> { ["userId"] = "user456" };

        // Act
        bool result = env.EvaluateBoolean("isOwner(userId)", variables);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Unregistered Function Tests

    [Fact]
    public void EvaluateBoolean_UnregisteredFunction_ThrowsException()
    {
        // Arrange
        var env = new FlowEnvironment();
        var variables = new Dictionary<string, object?>();

        // Act & Assert
        var exception = Assert.Throws<EvaluationException>(() =>
            env.EvaluateBoolean("unknownFunction()", variables));
        Assert.Contains("unknownFunction", exception.Message);
        Assert.Contains("not registered", exception.Message);
    }

    #endregion

    #region Literal Tests

    [Fact]
    public void EvaluateBoolean_TrueLiteral_ReturnsTrue()
    {
        // Arrange
        var env = new FlowEnvironment();
        var variables = new Dictionary<string, object?>();

        // Act
        bool result = env.EvaluateBoolean("true", variables);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateBoolean_FalseLiteral_ReturnsFalse()
    {
        // Arrange
        var env = new FlowEnvironment();
        var variables = new Dictionary<string, object?>();

        // Act
        bool result = env.EvaluateBoolean("false", variables);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EvaluateBoolean_NullLiteral_ReturnsFalse()
    {
        // Arrange
        var env = new FlowEnvironment();
        var variables = new Dictionary<string, object?>();

        // Act
        bool result = env.EvaluateBoolean("null", variables);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EvaluateBoolean_StringLiteral_NonEmpty_ReturnsTrue()
    {
        // Arrange
        var env = new FlowEnvironment();
        var variables = new Dictionary<string, object?>();

        // Act
        bool result = env.EvaluateBoolean("\"hello\"", variables);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateBoolean_StringLiteral_Empty_ReturnsFalse()
    {
        // Arrange
        var env = new FlowEnvironment();
        var variables = new Dictionary<string, object?>();

        // Act
        bool result = env.EvaluateBoolean("\"\"", variables);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EvaluateBoolean_NumberLiteral_NonZero_ReturnsTrue()
    {
        // Arrange
        var env = new FlowEnvironment();
        var variables = new Dictionary<string, object?>();

        // Act
        bool result = env.EvaluateBoolean("42", variables);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateBoolean_NumberLiteral_Zero_ReturnsFalse()
    {
        // Arrange
        var env = new FlowEnvironment();
        var variables = new Dictionary<string, object?>();

        // Act
        bool result = env.EvaluateBoolean("0", variables);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Identifier Tests

    [Fact]
    public void EvaluateBoolean_Identifier_ReturnsVariableValue()
    {
        // Arrange
        var env = new FlowEnvironment();
        var variables = new Dictionary<string, object?> { ["isEnabled"] = true };

        // Act
        bool result = env.EvaluateBoolean("isEnabled", variables);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EvaluateBoolean_UndefinedVariable_ThrowsException()
    {
        // Arrange
        var env = new FlowEnvironment();
        var variables = new Dictionary<string, object?>();

        // Act & Assert
        var exception = Assert.Throws<EvaluationException>(() =>
            env.EvaluateBoolean("undefinedVar", variables));
        Assert.Contains("undefinedVar", exception.Message);
        Assert.Contains("not defined", exception.Message);
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public void EvaluateBoolean_CancellationToken_ObservedByRegisteredFunction()
    {
        // Arrange
        var env = new FlowEnvironment();
        var cts = new CancellationTokenSource();
        CancellationToken observedToken = default;
        
        env.RegisterFunction("checkCancel", (ctx, args, ct) =>
        {
            observedToken = ct;
            return Task.FromResult<object?>(true);
        });
        var variables = new Dictionary<string, object?>();

        // Act
        cts.Cancel();
        env.EvaluateBoolean("checkCancel()", variables, cts.Token);

        // Assert
        Assert.True(observedToken.IsCancellationRequested);
    }

    [Fact]
    public async Task Compile_EvaluateAsync_CancellationToken_PropagatedToFunction()
    {
        // Arrange
        var env = new FlowEnvironment();
        var cts = new CancellationTokenSource();
        CancellationToken observedToken = default;
        
        env.RegisterFunction("asyncFunc", (ctx, args, ct) =>
        {
            observedToken = ctx.CancellationToken;
            return Task.FromResult<object?>(true);
        });
        var compiled = env.Compile("asyncFunc()");
        var variables = new Dictionary<string, object?>();

        // Act
        cts.Cancel();
        await compiled.EvaluateBooleanAsync(variables, cts.Token);

        // Assert
        Assert.True(observedToken.IsCancellationRequested);
    }

    #endregion

    #region Malformed Expression Tests

    [Fact]
    public void Compile_MalformedExpression_ThrowsParseException()
    {
        // Arrange
        var env = new FlowEnvironment();

        // Act & Assert
        Assert.Throws<ParseException>(() => env.Compile("func("));
    }

    [Fact]
    public void Compile_UnterminatedString_ThrowsParseException()
    {
        // Arrange
        var env = new FlowEnvironment();

        // Act & Assert
        Assert.Throws<ParseException>(() => env.Compile("\"unterminated"));
    }

    [Fact]
    public void Compile_UnexpectedCharacter_ThrowsParseException()
    {
        // Arrange
        var env = new FlowEnvironment();

        // Act & Assert
        Assert.Throws<ParseException>(() => env.Compile("@invalid"));
    }

    #endregion

    #region Function Registration Tests

    [Fact]
    public void RegisterFunction_DuplicateName_ReturnsFalse()
    {
        // Arrange
        var env = new FlowEnvironment();
        env.RegisterFunction("myFunc", (ctx, args, ct) => Task.FromResult<object?>(1));

        // Act
        bool result = env.RegisterFunction("myFunc", (ctx, args, ct) => Task.FromResult<object?>(2));

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RegisterFunction_UniqueName_ReturnsTrue()
    {
        // Arrange
        var env = new FlowEnvironment();

        // Act
        bool result = env.RegisterFunction("uniqueFunc", (ctx, args, ct) => Task.FromResult<object?>(1));

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Compile and Evaluate Tests

    [Fact]
    public async Task Compile_ThenEvaluateAsync_ReturnsCorrectResult()
    {
        // Arrange
        var env = new FlowEnvironment();
        env.RegisterFunction("add", (ctx, args, ct) =>
        {
            var a = Convert.ToInt32(args[0]);
            var b = Convert.ToInt32(args[1]);
            return Task.FromResult<object?>(a + b);
        });
        var compiled = env.Compile("add(1, 2)");
        var variables = new Dictionary<string, object?>();

        // Act
        var result = await compiled.EvaluateAsync(variables);

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public void EvaluateBoolean_FunctionWithArguments_PassesArgumentsCorrectly()
    {
        // Arrange
        var env = new FlowEnvironment();
        object?[]? receivedArgs = null;
        env.RegisterFunction("captureArgs", (ctx, args, ct) =>
        {
            receivedArgs = args;
            return Task.FromResult<object?>(true);
        });
        var variables = new Dictionary<string, object?> { ["x"] = 10, ["y"] = "test" };

        // Act
        env.EvaluateBoolean("captureArgs(x, y, 42, \"literal\")", variables);

        // Assert
        Assert.NotNull(receivedArgs);
        Assert.Equal(4, receivedArgs.Length);
        Assert.Equal(10, receivedArgs[0]);
        Assert.Equal("test", receivedArgs[1]);
        Assert.Equal(42, receivedArgs[2]);
        Assert.Equal("literal", receivedArgs[3]);
    }

    #endregion
}
