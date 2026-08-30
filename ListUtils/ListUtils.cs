namespace ListUtils;

// Partial-class shell. The twenty-nine [OSAction] implementations and their helpers
// are split across ListUtils.Index.cs (List_Pop, List_PopMultiple, List_SplitAt),
// ListUtils.Condition.cs (Pop*ByCondition, Pop*ByConditions, Partition, PartitionByConditions),
// ListUtils.Relational.cs (List_Zip, List_GroupBy, List_ZipGroupBy, List_Difference, List_Intersect, List_Union),
// ListUtils.Transform.cs (List_Chunk, List_DistinctBy, List_Slice, List_Shuffle, List_UpdateAt, List_Reverse, List_Flatten, List_Sample, List_ReplaceWhere, List_UpdateMultipleAt),
// ListUtils.Aggregate.cs (List_MinBy, List_MaxBy, List_Aggregate),
// ListUtils.ZipMany.cs (List_ZipMany, List_ZipManyGroupBy),
// and ListUtils.Helpers.cs (property-path walker, condition evaluator, JsonOptions).
public partial class ListUtils : IListUtils
{
}
