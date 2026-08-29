namespace ListUtils;

// Partial-class shell. The nine [OSAction] implementations and their helpers
// are split across ListUtils.Index.cs (List_Pop, List_PopMultiple),
// ListUtils.Condition.cs (Pop*ByCondition, Pop*ByConditions),
// ListUtils.Relational.cs (List_Zip, List_GroupBy, List_Difference) and
// ListUtils.Helpers.cs (property-path walker, condition evaluator, JsonOptions).
public partial class ListUtils : IListUtils
{
}
