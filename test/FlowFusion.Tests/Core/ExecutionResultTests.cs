namespace FlowFusion.Tests.Core;
[TestClass]
public class ExecutionResultTests
{
    [TestMethod]
    public void Success_WithNextBlockId_ReturnsCorrectResult()
    {
        // Arrange & Act
        var result = ExecutionResult.Success("nextBlock");
        // Assert
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual("nextBlock", result.NextBlockId);
        Assert.IsNull(result.Error);
    }
    [TestMethod]
    public void Success_WithoutNextBlockId_ReturnsCorrectResult()
    {
        // Arrange & Act
        var result = ExecutionResult.Success(null);
        // Assert
        Assert.IsTrue(result.Succeeded);
        Assert.IsNull(result.NextBlockId);
        Assert.IsNull(result.Error);
    }
    [TestMethod]
    public void Failure_WithNextBlockIdAndException_ReturnsCorrectResult()
    {
        // Arrange
        var exception = new Exception("Test error");
        // Act
        var result = ExecutionResult.Failure("nextBlock", exception);
        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("nextBlock", result.NextBlockId);
        Assert.AreEqual(exception, result.Error);
    }
    [TestMethod]
    public void Failure_WithoutNextBlockIdAndException_ReturnsCorrectResult()
    {
        // Arrange & Act
        var result = ExecutionResult.Failure(null, null);
        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.IsNull(result.NextBlockId);
        Assert.IsNull(result.Error);
    }
    [TestMethod]
    public void Failure_WithExceptionOnly_ReturnsCorrectResult()
    {
        // Arrange
        var exception = new Exception("Test error");
        // Act
        var result = ExecutionResult.Failure(null, exception);
        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.IsNull(result.NextBlockId);
        Assert.AreEqual(exception, result.Error);
    }
    [TestMethod]
    public void ExecutionResult_IsValueType()
    {
        // Arrange & Act
        var result1 = ExecutionResult.Success("block1");
        var result2 = ExecutionResult.Success("block1");
        // Assert - Value types should be equal if values are equal
        Assert.AreEqual(result1, result2);
    }
}