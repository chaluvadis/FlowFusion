namespace FlowFusion.Tests.Core;

[TestClass]
public class ExpressionHelperTests
{
    [TestMethod]
    public void GetProperty_WithValidProperty_ReturnsPropertyValue()
    {
        // Arrange
        var obj = new { Name = "Test", Value = 42 };

        // Act
        var result = ExpressionHelper.GetProperty(obj, "Name");

        // Assert
        Assert.AreEqual("Test", result);
    }

    [TestMethod]
    public void GetProperty_WithInvalidProperty_ReturnsNull()
    {
        // Arrange
        var obj = new { Name = "Test" };

        // Act
        var result = ExpressionHelper.GetProperty(obj, "InvalidProperty");

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetProperty_WithNullObject_ReturnsNull()
    {
        // Act
        var result = ExpressionHelper.GetProperty(null, "Name");

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetIndexer_WithValidIntIndex_ReturnsValue()
    {
        // Arrange
        var list = new List<string> { "first", "second", "third" };

        // Act
        var result = ExpressionHelper.GetIndexer(list, 1);

        // Assert
        Assert.AreEqual("second", result);
    }

    [TestMethod]
    public void GetIndexer_WithValidStringIndex_ReturnsValue()
    {
        // Arrange
        var dict = new Dictionary<string, string> { ["key1"] = "value1", ["key2"] = "value2" };

        // Act
        var result = ExpressionHelper.GetIndexer(dict, "key1");

        // Assert
        Assert.AreEqual("value1", result);
    }

    [TestMethod]
    public void GetIndexer_WithInvalidIndex_ReturnsNull()
    {
        // Arrange
        var list = new List<string> { "first", "second" };

        // Act
        var result = ExpressionHelper.GetIndexer(list, 10);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetIndexer_WithNullObject_ReturnsNull()
    {
        // Act
        var result = ExpressionHelper.GetIndexer(null, 0);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetIndexer_WithTypeConversion_ReturnsValue()
    {
        // Arrange
        var list = new List<string> { "first", "second" };

        // Act
        var result = ExpressionHelper.GetIndexer(list, "1");

        // Assert
        Assert.AreEqual("second", result);
    }

    [TestMethod]
    public void CallMethod_WithValidMethod_ReturnsResult()
    {
        // Arrange
        var obj = "Hello World";

        // Act
        var result = ExpressionHelper.CallMethod(obj, "Substring", 6, 5);

        // Assert
        Assert.AreEqual("World", result);
    }

    [TestMethod]
    public void CallMethod_WithInvalidMethod_ReturnsNull()
    {
        // Arrange
        var obj = "Hello World";

        // Act
        var result = ExpressionHelper.CallMethod(obj, "InvalidMethod");

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void CallMethod_WithNullObject_ReturnsNull()
    {
        // Act
        var result = ExpressionHelper.CallMethod(null, "ToString");

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void Equal_BothNull_ReturnsTrue()
    {
        // Act
        var result = ExpressionHelper.Equal(null, null);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Equal_LeftNull_ReturnsFalse()
    {
        // Act
        var result = ExpressionHelper.Equal(null, "test");

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Equal_RightNull_ReturnsFalse()
    {
        // Act
        var result = ExpressionHelper.Equal("test", null);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Equal_BothDoublesEqual_ReturnsTrue()
    {
        // Act
        var result = ExpressionHelper.Equal(5.0, 5.0);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Equal_BothDoublesNotEqual_ReturnsFalse()
    {
        // Act
        var result = ExpressionHelper.Equal(5.0, 6.0);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Equal_DoubleAndIntEqual_ReturnsTrue()
    {
        // Act
        var result = ExpressionHelper.Equal(5.0, 5);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Equal_IntAndDoubleEqual_ReturnsTrue()
    {
        // Act
        var result = ExpressionHelper.Equal(5, 5.0);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Equal_BothIntsEqual_ReturnsTrue()
    {
        // Act
        var result = ExpressionHelper.Equal(5, 5);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Equal_BothIntsNotEqual_ReturnsFalse()
    {
        // Act
        var result = ExpressionHelper.Equal(5, 6);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Equal_OtherTypesEqual_ReturnsTrue()
    {
        // Act
        var result = ExpressionHelper.Equal("test", "test");

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Equal_OtherTypesNotEqual_ReturnsFalse()
    {
        // Act
        var result = ExpressionHelper.Equal("test", "other");

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void NotEqual_BothNull_ReturnsFalse()
    {
        // Act
        var result = ExpressionHelper.NotEqual(null, null);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void NotEqual_LeftNull_ReturnsTrue()
    {
        // Act
        var result = ExpressionHelper.NotEqual(null, "test");

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void LessThan_LeftLessThanRight_ReturnsTrue()
    {
        // Act
        var result = ExpressionHelper.LessThan(5, 10);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void LessThan_LeftGreaterThanRight_ReturnsFalse()
    {
        // Act
        var result = ExpressionHelper.LessThan(10, 5);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void LessThan_LeftEqualRight_ReturnsFalse()
    {
        // Act
        var result = ExpressionHelper.LessThan(5, 5);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void GreaterThan_LeftGreaterThanRight_ReturnsTrue()
    {
        // Act
        var result = ExpressionHelper.GreaterThan(10, 5);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void GreaterThan_LeftLessThanRight_ReturnsFalse()
    {
        // Act
        var result = ExpressionHelper.GreaterThan(5, 10);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void GreaterThan_LeftEqualRight_ReturnsFalse()
    {
        // Act
        var result = ExpressionHelper.GreaterThan(5, 5);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void LessEqual_LeftLessThanRight_ReturnsTrue()
    {
        // Act
        var result = ExpressionHelper.LessEqual(5, 10);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void LessEqual_LeftEqualRight_ReturnsTrue()
    {
        // Act
        var result = ExpressionHelper.LessEqual(5, 5);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void LessEqual_LeftGreaterThanRight_ReturnsFalse()
    {
        // Act
        var result = ExpressionHelper.LessEqual(10, 5);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void GreaterEqual_LeftGreaterThanRight_ReturnsTrue()
    {
        // Act
        var result = ExpressionHelper.GreaterEqual(10, 5);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void GreaterEqual_LeftEqualRight_ReturnsTrue()
    {
        // Act
        var result = ExpressionHelper.GreaterEqual(5, 5);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void GreaterEqual_LeftLessThanRight_ReturnsFalse()
    {
        // Act
        var result = ExpressionHelper.GreaterEqual(5, 10);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void And_BothTrue_ReturnsTrue()
    {
        // Act
        var result = ExpressionHelper.And(true, true);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void And_LeftFalse_ReturnsFalse()
    {
        // Act
        var result = ExpressionHelper.And(false, true);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void And_RightFalse_ReturnsFalse()
    {
        // Act
        var result = ExpressionHelper.And(true, false);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void And_BothFalse_ReturnsFalse()
    {
        // Act
        var result = ExpressionHelper.And(false, false);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Or_BothFalse_ReturnsFalse()
    {
        // Act
        var result = ExpressionHelper.Or(false, false);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Or_LeftTrue_ReturnsTrue()
    {
        // Act
        var result = ExpressionHelper.Or(true, false);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Or_RightTrue_ReturnsTrue()
    {
        // Act
        var result = ExpressionHelper.Or(false, true);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Or_BothTrue_ReturnsTrue()
    {
        // Act
        var result = ExpressionHelper.Or(true, true);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Not_True_ReturnsFalse()
    {
        // Act
        var result = ExpressionHelper.Not(true);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Not_False_ReturnsTrue()
    {
        // Act
        var result = ExpressionHelper.Not(false);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Add_Ints_ReturnsSum()
    {
        // Act
        var result = ExpressionHelper.Add(5, 3);

        // Assert
        Assert.AreEqual(8.0, result);
    }

    [TestMethod]
    public void Add_Doubles_ReturnsSum()
    {
        // Act
        var result = ExpressionHelper.Add(5.5, 3.2);

        // Assert
        Assert.AreEqual(8.7, result);
    }

    [TestMethod]
    public void Subtract_Ints_ReturnsDifference()
    {
        // Act
        var result = ExpressionHelper.Subtract(10, 3);

        // Assert
        Assert.AreEqual(7.0, result);
    }

    [TestMethod]
    public void Subtract_Doubles_ReturnsDifference()
    {
        // Act
        var result = ExpressionHelper.Subtract(10.5, 3.2);

        // Assert
        Assert.AreEqual(7.3, result);
    }

    [TestMethod]
    public void Multiply_Ints_ReturnsProduct()
    {
        // Act
        var result = ExpressionHelper.Multiply(5, 3);

        // Assert
        Assert.AreEqual(15.0, result);
    }

    [TestMethod]
    public void Multiply_Doubles_ReturnsProduct()
    {
        // Act
        var result = ExpressionHelper.Multiply(5.5, 3.0);

        // Assert
        Assert.AreEqual(16.5, result);
    }

    [TestMethod]
    public void Divide_Ints_ReturnsQuotient()
    {
        // Act
        var result = ExpressionHelper.Divide(10, 2);

        // Assert
        Assert.AreEqual(5.0, result);
    }

    [TestMethod]
    public void Divide_Doubles_ReturnsQuotient()
    {
        // Act
        var result = ExpressionHelper.Divide(10.0, 3.0);

        // Assert
        Assert.AreEqual(10.0 / 3.0, result);
    }

    [TestMethod]
    public void Modulo_Ints_ReturnsRemainder()
    {
        // Act
        var result = ExpressionHelper.Modulo(10, 3);

        // Assert
        Assert.AreEqual(1.0, result);
    }

    [TestMethod]
    public void Modulo_Doubles_ReturnsRemainder()
    {
        // Act
        var result = ExpressionHelper.Modulo(10.5, 3.0);

        // Assert
        Assert.AreEqual(10.5 % 3.0, result);
    }
}