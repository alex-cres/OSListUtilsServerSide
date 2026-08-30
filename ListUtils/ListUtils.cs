namespace ListUtils;

// Partial-class shell. The twenty-eight [OSAction] implementations and their
// helpers are split across ListUtils.Index.cs (List_Pop, List_PopMultiple),
// ListUtils.Condition.cs (Pop*ByCondition, Pop*ByConditions),
// ListUtils.Relational.cs (List_Zip, List_GroupBy, List_Difference),
// ListUtils.Transform.cs (List_Chunk, List_DistinctBy, List_Slice, List_Shuffle, List_UpdateAt),
// ListUtils.InPlace.cs (the fourteen List_*InPlace ref/Input-Output variants) and
// ListUtils.Helpers.cs (property-path walker, condition evaluator, JsonOptions).
public partial class ListUtils : IListUtils
{
}
