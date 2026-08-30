using OutSystems.ExternalLibraries.SDK;

namespace ListUtils;

[OSStructure(Description = "A single filter condition used by the multi-condition actions (List_PopByConditions, List_PopMultipleByConditions, List_PartitionByConditions, List_ReplaceWhere). Groups a property path, a comparison operator, a comparison value and a case-sensitivity flag into one first-class OutSystems Structure so callers no longer need to hand-craft a JSON string.")]
public struct Condition
{
    [OSStructureField(Description = "Property path to check on each list element. Supports nested paths with dots (e.g. 'Address.City') and array indexing (e.g. 'Tags[0]', 'Items[-1].Name'). CamelCase fallback is applied per segment.")]
    public string Path;

    [OSStructureField(Description = "Comparison operator name. Prefer the constants on the ListUtils.Operators helper class (Equals, NotEquals, Contains, StartsWith, EndsWith, GreaterThan, LessThan, GreaterOrEqual, LessOrEqual). Symbol aliases '!=', '>', '<', '>=', '<=' are also accepted. Empty defaults to Equals.")]
    public string Operator;

    [OSStructureField(Description = "The value to compare against, as text. Numeric operators parse this as a decimal with InvariantCulture.")]
    public string Value;

    [OSStructureField(Description = "When true, string comparisons are case-sensitive. When false (default), case-insensitive. Ignored by numeric operators.")]
    public bool CaseSensitive;
}
