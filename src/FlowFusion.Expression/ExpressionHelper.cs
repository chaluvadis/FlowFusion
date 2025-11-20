namespace FlowFusion.Expression;

public static class ExpressionHelper
{
    public static object? GetProperty(object? obj, string propertyName)
        => obj?.GetType().GetProperty(propertyName)?.GetValue(obj);

    public static object? GetIndexer(object? obj, object? index)
    {
        if (obj == null) return null;
        var type = obj.GetType();
        var methods = type.GetMethods().Where(m => m.Name == "get_Item" && m.GetParameters().Length == 1);
        foreach (var method in methods)
        {
            var paramType = method.GetParameters()[0].ParameterType;
            try
            {
                var convertedIndex = Convert.ChangeType(index, paramType);
                return method.Invoke(obj, [convertedIndex]);
            }
            catch
            {
                // try next
            }
        }
        return null;
    }

    public static object? CallMethod(object? obj, string methodName, params object?[] args)
        => obj?.GetType().GetMethod(methodName, args.Select(a => a?.GetType() ?? typeof(object)).ToArray())?.Invoke(obj, args);

    public static bool Equal(object? left, object? right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;
        if (left is double d1 && right is double d2) return d1 == d2;
        if (left is double d && right is int i) return d == i;
        if (left is int i2 && right is double d3) return i2 == d3;
        if (left is int i1 && right is int i3) return i1 == i3;
        return left.Equals(right);
    }
    public static bool NotEqual(object? left, object? right) => !Equal(left, right);
    public static bool LessThan(object? left, object? right) => Compare(left, right) < 0;
    public static bool GreaterThan(object? left, object? right) => Compare(left, right) > 0;
    public static bool LessEqual(object? left, object? right) => Compare(left, right) <= 0;
    public static bool GreaterEqual(object? left, object? right) => Compare(left, right) >= 0;
    public static bool And(object? left, object? right) => Convert.ToBoolean(left) && Convert.ToBoolean(right);
    public static bool Or(object? left, object? right) => Convert.ToBoolean(left) || Convert.ToBoolean(right);
    public static bool Not(object? value) => !Convert.ToBoolean(value);
    public static object? Add(object? left, object? right) => Convert.ToDouble(left) + Convert.ToDouble(right);
    public static object? Subtract(object? left, object? right) => Convert.ToDouble(left) - Convert.ToDouble(right);
    public static object? Multiply(object? left, object? right) => Convert.ToDouble(left) * Convert.ToDouble(right);
    public static object? Divide(object? left, object? right) => Convert.ToDouble(left) / Convert.ToDouble(right);
    public static object? Modulo(object? left, object? right) => Convert.ToDouble(left) % Convert.ToDouble(right);
    private static int Compare(object? left, object? right)
    {
        if (left is double d1 && right is double d2) return d1.CompareTo(d2);
        if (left is double d && right is int i) return d.CompareTo(i);
        if (left is int i4 && right is double d4) return ((double)i4).CompareTo(d4);
        if (left is int i1 && right is int i2) return i1.CompareTo(i2);
        if (left is IComparable l && right is IComparable r && left.GetType() == right.GetType()) return l.CompareTo(r);
        return 0;
    }
}
