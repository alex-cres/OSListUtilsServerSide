namespace OutSystems.NssListUtils;

public static class Operators
{
    // 'new' suppresses CS0108: name clash with object.Equals(object).
    public new const string Equals = "Equals";
    public const string NotEquals = "NotEquals";
    public const string Contains = "Contains";
    public const string StartsWith = "StartsWith";
    public const string EndsWith = "EndsWith";
    public const string GreaterThan = "GreaterThan";
    public const string LessThan = "LessThan";
    public const string GreaterOrEqual = "GreaterOrEqual";
    public const string LessOrEqual = "LessOrEqual";
}

public static class AggregateOperations
{
    public const string Sum = "Sum";
    public const string Avg = "Avg";
    public const string Min = "Min";
    public const string Max = "Max";
    public const string Count = "Count";
    public const string CountDistinct = "CountDistinct";
}

public static class LogicalOperators
{
    public const string AND = "AND";
    public const string OR = "OR";
}
