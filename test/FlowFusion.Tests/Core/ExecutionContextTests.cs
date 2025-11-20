namespace FlowFusion.Tests.Core;
[TestClass]
public class ExecutionContextTests
{
    [TestMethod]
    public void Constructor_WithVariables_SetsVariablesProperty()
    {
        // Arrange
        var variables = new Dictionary<string, object?> { ["key1"] = "value1", ["key2"] = 42 };
        // Act
        var context = new FlowExecutionContext(variables);
        // Assert
        Assert.AreEqual(variables, context.Variables);
        Assert.AreEqual("value1", context.Variables["key1"]);
        Assert.AreEqual(42, context.Variables["key2"]);
    }
    [TestMethod]
    public void Constructor_WithNullVariables_SetsEmptyDictionary()
    {
        // Arrange & Act
        var context = new FlowExecutionContext(null!);
        // Assert
        Assert.IsNotNull(context.Variables);
        Assert.IsEmpty(context.Variables);
    }
    [TestMethod]
    public void GetVariable_Generic_WithCorrectType_ReturnsValue()
    {
        // Arrange
        var variables = new Dictionary<string, object?> { ["name"] = "John", ["age"] = 30 };
        var context = new FlowExecutionContext(variables);
        // Act
        var name = context.GetVariable<string>("name");
        var age = context.GetVariable<int>("age");
        // Assert
        Assert.AreEqual("John", name);
        Assert.AreEqual(30, age);
    }
    [TestMethod]
    public void GetVariable_Generic_WithWrongType_ReturnsDefault()
    {
        // Arrange
        var variables = new Dictionary<string, object?> { ["name"] = "John" };
        var context = new FlowExecutionContext(variables);
        // Act
        var age = context.GetVariable<int>("name");
        // Assert
        Assert.AreEqual(0, age); // default for int
    }
    [TestMethod]
    public void GetVariable_Generic_WithNonExistentKey_ReturnsDefault()
    {
        // Arrange
        var variables = new Dictionary<string, object?> { ["name"] = "John" };
        var context = new FlowExecutionContext(variables);
        // Act
        var age = context.GetVariable<int>("age");
        // Assert
        Assert.AreEqual(0, age);
    }
    [TestMethod]
    public void GetVariable_Generic_WithNullValue_ReturnsDefault()
    {
        // Arrange
        var variables = new Dictionary<string, object?> { ["name"] = null };
        var context = new FlowExecutionContext(variables);
        // Act
        var name = context.GetVariable<string>("name");
        // Assert
        Assert.IsNull(name);
    }
    [TestMethod]
    public void SetState_SetsValueInStateDictionary()
    {
        // Arrange
        var context = new FlowExecutionContext(new Dictionary<string, object?>());
        // Act
        context.SetState("key1", "value1");
        context.SetState("key2", 42);
        // Assert
        Assert.AreEqual("value1", context.GetState<string>("key1"));
        Assert.AreEqual(42, context.GetState<int>("key2"));
    }
    [TestMethod]
    public void SetState_OverwritesExistingValue()
    {
        // Arrange
        var context = new FlowExecutionContext(new Dictionary<string, object?>());
        context.SetState("key", "original");
        // Act
        context.SetState("key", "updated");
        // Assert
        Assert.AreEqual("updated", context.GetState<string>("key"));
    }
    [TestMethod]
    public void SetState_WithNullValue_SetsNull()
    {
        // Arrange
        var context = new FlowExecutionContext(new Dictionary<string, object?>());
        // Act
        context.SetState("key", null);
        // Assert
        Assert.IsNull(context.GetState<string>("key"));
    }
    [TestMethod]
    public void GetState_Generic_WithCorrectType_ReturnsValue()
    {
        // Arrange
        var context = new FlowExecutionContext(new Dictionary<string, object?>());
        context.SetState("count", 10);
        context.SetState("message", "hello");
        // Act
        var count = context.GetState<int>("count");
        var message = context.GetState<string>("message");
        // Assert
        Assert.AreEqual(10, count);
        Assert.AreEqual("hello", message);
    }
    [TestMethod]
    public void GetState_Generic_WithWrongType_ReturnsDefault()
    {
        // Arrange
        var context = new FlowExecutionContext(new Dictionary<string, object?>());
        context.SetState("value", "string");
        // Act
        var result = context.GetState<int>("value");
        // Assert
        Assert.AreEqual(0, result);
    }
    [TestMethod]
    public void GetState_Generic_WithNonExistentKey_ReturnsDefault()
    {
        // Arrange
        var context = new FlowExecutionContext(new Dictionary<string, object?>());
        // Act
        var result = context.GetState<string>("nonexistent");
        // Assert
        Assert.IsNull(result);
    }
    [TestMethod]
    public void SnapshotState_ReturnsCopyOfState()
    {
        // Arrange
        var context = new FlowExecutionContext(new Dictionary<string, object?>());
        context.SetState("key1", "value1");
        context.SetState("key2", 42);
        // Act
        var snapshot = context.SnapshotState();
        // Assert
        Assert.HasCount(2, snapshot);
        Assert.AreEqual("value1", snapshot["key1"]);
        Assert.AreEqual(42, snapshot["key2"]);
        // Verify it's a copy
        context.SetState("key1", "modified");
        Assert.AreEqual("value1", snapshot["key1"]); // Original snapshot unchanged
    }
    [TestMethod]
    public void SnapshotState_WhenStateIsEmpty_ReturnsEmptyDictionary()
    {
        // Arrange
        var context = new FlowExecutionContext(new Dictionary<string, object?>());
        // Act
        var snapshot = context.SnapshotState();
        // Assert
        Assert.IsEmpty(snapshot);
    }
}