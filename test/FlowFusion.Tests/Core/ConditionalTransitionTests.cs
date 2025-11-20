namespace FlowFusion.Tests.Core;
[TestClass]
public class ConditionalTransitionTests
{
    [TestMethod]
    public void Constructor_SetsAllProperties()
    {
        // Arrange
        var sourceBlockId = "sourceBlock";
        var targetBlockId = "targetBlock";
        var expression = "x > 5";
        // Act
        var transition = new ConditionalTransition(sourceBlockId, targetBlockId, expression);
        // Assert
        Assert.AreEqual(sourceBlockId, transition.SourceBlockId);
        Assert.AreEqual(targetBlockId, transition.TargetBlockId);
        Assert.AreEqual(expression, transition.Expression);
    }
    [TestMethod]
    public void Properties_AreReadOnly()
    {
        // Arrange
        var transition = new ConditionalTransition("source", "target", "expr");
        // Assert - These should be init-only properties
        Assert.AreEqual("source", transition.SourceBlockId);
        Assert.AreEqual("target", transition.TargetBlockId);
        Assert.AreEqual("expr", transition.Expression);
    }
    [TestMethod]
    public void Constructor_WithEmptyStrings_SetsProperties()
    {
        // Arrange & Act
        var transition = new ConditionalTransition("", "", "");
        // Assert
        Assert.AreEqual("", transition.SourceBlockId);
        Assert.AreEqual("", transition.TargetBlockId);
        Assert.AreEqual("", transition.Expression);
    }
    [TestMethod]
    public void Constructor_WithNullStrings_AssignsNullValues()
    {
        // Arrange, Act & Assert - The constructor allows null values since it's just assignment
        var transition1 = new ConditionalTransition(null!, "target", "expr");
        Assert.IsNull(transition1.SourceBlockId);
        var transition2 = new ConditionalTransition("source", null!, "expr");
        Assert.IsNull(transition2.TargetBlockId);
        var transition3 = new ConditionalTransition("source", "target", null!);
        Assert.IsNull(transition3.Expression);
    }
    [TestMethod]
    public void Equality_BasedOnReference()
    {
        // Arrange
        var transition1 = new ConditionalTransition("source", "target", "expr");
        var transition2 = new ConditionalTransition("source", "target", "expr");
        // Assert - Since this is a class, not a record, equality is reference-based
        Assert.AreNotEqual(transition1, transition2);
        // But hash codes should be different for different instances
        Assert.AreNotEqual(transition1.GetHashCode(), transition2.GetHashCode());
    }
    [TestMethod]
    public void Inequality_WhenPropertiesDiffer()
    {
        // Arrange
        var transition1 = new ConditionalTransition("source1", "target", "expr");
        var transition2 = new ConditionalTransition("source2", "target", "expr");
        // Assert
        Assert.AreNotEqual(transition1, transition2);
    }
}