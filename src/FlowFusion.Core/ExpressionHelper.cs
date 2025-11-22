namespace FlowFusion.Core;

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
        => obj?.GetType()
            .GetMethod(methodName, [.. args.Select(a => a?.GetType() ?? typeof(object))])?.Invoke(obj, args);

    public static bool Equal(object? left, object? right)
    {
        // Handle null cases
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;

        // Try numeric equality first
        if (TryEqualNumeric(left, right, out var result))
            return result;

        // Fallback to Equals method
        return left.Equals(right);
    }

    private static bool TryEqualNumeric(object left, object right, out bool result)
    {
        result = false;

        // Check if both are numeric types before conversion
        if (IsNumericType(left) && IsNumericType(right))
        {
            double leftDouble = Convert.ToDouble(left);
            double rightDouble = Convert.ToDouble(right);
            result = leftDouble == rightDouble;
            return true;
        }

        return false;
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
        // Handle null cases
        if (left == null || right == null)
            return 0; // Null comparison is undefined, return 0

        // Try numeric comparisons first
        if (TryCompareNumeric(left, right, out var result))
            return result;

        // Fallback to IComparable
        return CompareUsingIComparable(left, right);
    }

    private static bool TryCompareNumeric(object left, object right, out int result)
    {
        result = 0;

        // Check if both are numeric types before conversion
        if (IsNumericType(left) && IsNumericType(right))
        {
            double leftDouble = Convert.ToDouble(left);
            double rightDouble = Convert.ToDouble(right);
            result = leftDouble.CompareTo(rightDouble);
            return true;
        }

        return false;
    }

    private static bool IsNumericType(object obj)
    {
        var type = obj.GetType();
        return type.IsPrimitive && type != typeof(bool) && type != typeof(char);
    }

    private static int CompareUsingIComparable(object left, object right)
    {
        if (left is IComparable l && right is IComparable r && left.GetType() == right.GetType())
            return l.CompareTo(r);

        return 0; // Incomparable types
    }
}