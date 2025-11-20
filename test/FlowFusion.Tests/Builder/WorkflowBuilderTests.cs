namespace FlowFusion.Tests.Builder;
[TestClass]
public class WorkflowBuilderTests
{
    private Mock<IBlock>? _mockBlock1;
    private Mock<IBlock>? _mockBlock2;
    private Mock<IBlock>? _mockBlock3;
    [TestInitialize]
    public void Setup()
    {
        _mockBlock1 = new Mock<IBlock>();
        _mockBlock1.Setup(b => b.Id).Returns("block1");
        _mockBlock2 = new Mock<IBlock>();
        _mockBlock2.Setup(b => b.Id).Returns("block2");
        _mockBlock3 = new Mock<IBlock>();
        _mockBlock3.Setup(b => b.Id).Returns("block3");
    }
    [TestMethod]
    public void Constructor_WithId_SetsWorkflowId()
    {
        // Arrange & Act
        var builder = new WorkflowBuilder("test-workflow");
        // Assert - We can't directly test the id, but we can test the built workflow
        var workflow = builder.StartWith(_mockBlock1?.Object!).Build();
        Assert.IsNotNull(workflow);
        Assert.AreEqual("test-workflow", workflow.Id);
    }
    [TestMethod]
    public void StartWith_SetsStartBlockAndAddsToBlocks()
    {
        // Arrange
        var builder = new WorkflowBuilder("test-workflow");
        // Act
        builder.StartWith(_mockBlock1?.Object!);
        var workflow = builder.Build();
        // Assert
        Assert.AreEqual("block1", workflow.StartBlockId);
        Assert.IsTrue(workflow.Blocks.ContainsKey("block1"));
        Assert.AreEqual(_mockBlock1?.Object!, workflow.Blocks["block1"]);
    }
    [TestMethod]
    public void Add_AddsBlockToWorkflow()
    {
        // Arrange
        var builder = new WorkflowBuilder("test-workflow");
        // Act
        builder.StartWith(_mockBlock1?.Object!)
               .Add(_mockBlock2?.Object!)
               .Add(_mockBlock3?.Object!);
        var workflow = builder.Build();
        // Assert
        Assert.HasCount(3, workflow.Blocks);
        Assert.IsTrue(workflow.Blocks.ContainsKey("block1"));
        Assert.IsTrue(workflow.Blocks.ContainsKey("block2"));
        Assert.IsTrue(workflow.Blocks.ContainsKey("block3"));
    }
    [TestMethod]
    public void AddConditionalTransition_AddsTransitionToWorkflow()
    {
        // Arrange
        var builder = new WorkflowBuilder("test-workflow");
        // Act
        builder.StartWith(_mockBlock1?.Object!)
               .Add(_mockBlock2?.Object!)
               .AddConditionalTransition("block1", "block2", "x > 5");
        var workflow = builder.Build();
        // Assert
        Assert.HasCount(1, workflow.ConditionalTransitions);
        var transition = workflow.ConditionalTransitions[0];
        Assert.AreEqual("block1", transition.SourceBlockId);
        Assert.AreEqual("block2", transition.TargetBlockId);
        Assert.AreEqual("x > 5", transition.Expression);
    }
    [TestMethod]
    public void AddConditionalTransition_MultipleTransitions_AddsAll()
    {
        // Arrange
        var builder = new WorkflowBuilder("test-workflow");
        // Act
        builder.StartWith(_mockBlock1?.Object!)
               .Add(_mockBlock2?.Object!)
               .Add(_mockBlock3?.Object!)
               .AddConditionalTransition("block1", "block2", "x > 5")
               .AddConditionalTransition("block2", "block3", "y < 10");
        var workflow = builder.Build();
        // Assert
        Assert.HasCount(2, workflow.ConditionalTransitions);
    }
    [TestMethod]
    public void Build_WithoutStartBlock_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = new WorkflowBuilder("test-workflow");
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }
    [TestMethod]
    public void Build_ReturnsWorkflowWithCorrectProperties()
    {
        // Arrange
        var builder = new WorkflowBuilder("test-workflow");
        // Act
        builder.StartWith(_mockBlock1?.Object!)
               .Add(_mockBlock2?.Object!)
               .AddConditionalTransition("block1", "block2", "condition");
        var workflow = builder.Build();
        // Assert
        Assert.AreEqual("test-workflow", workflow.Id);
        Assert.AreEqual("block1", workflow.StartBlockId);
        Assert.HasCount(2, workflow.Blocks);
        Assert.HasCount(1, workflow.ConditionalTransitions);
    }
    [TestMethod]
    public void Build_CreatesReadOnlyCollections()
    {
        // Arrange
        var builder = new WorkflowBuilder("test-workflow");
        // Act
        builder.StartWith(_mockBlock1?.Object!)
               .Add(_mockBlock2?.Object!)
               .AddConditionalTransition("block1", "block2", "condition");
        var workflow = builder.Build();
        // Assert
        Assert.IsInstanceOfType<IReadOnlyDictionary<string, IBlock>>(workflow.Blocks);
        Assert.IsInstanceOfType<IReadOnlyList<ConditionalTransition>>(workflow.ConditionalTransitions);
    }
    [TestMethod]
    public void GetBlockById_WithValidId_ReturnsBlock()
    {
        // Arrange
        var builder = new WorkflowBuilder("test-workflow");
        builder.StartWith(_mockBlock1?.Object!).Add(_mockBlock2?.Object!);
        var workflow = builder.Build();
        // Act
        var block = workflow.GetBlockById("block1");
        // Assert
        Assert.AreEqual(_mockBlock1?.Object!, block);
    }
    [TestMethod]
    public void GetBlockById_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var builder = new WorkflowBuilder("test-workflow");
        builder.StartWith(_mockBlock1?.Object!);
        var workflow = builder.Build();
        // Act
        var block = workflow.GetBlockById("nonexistent");
        // Assert
        Assert.IsNull(block);
    }
    [TestMethod]
    public void GetBlockById_WithNullId_ReturnsNull()
    {
        // Arrange
        var builder = new WorkflowBuilder("test-workflow");
        builder.StartWith(_mockBlock1?.Object!);
        var workflow = builder.Build();
        // Act
        var block = workflow.GetBlockById(null);
        // Assert
        Assert.IsNull(block);
    }
    [TestMethod]
    public void Builder_IsFluent_ReturnsSelf()
    {
        // Arrange
        var builder = new WorkflowBuilder("test-workflow");
        // Act & Assert - All methods should return the builder for chaining
        var result = builder.StartWith(_mockBlock1?.Object!);
        Assert.AreEqual(builder, result);
        result = builder.Add(_mockBlock2?.Object!);
        Assert.AreEqual(builder, result);
        result = builder.AddConditionalTransition("block1", "block2", "expr");
        Assert.AreEqual(builder, result);
    }
}