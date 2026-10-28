namespace OutSystems.NssListUtils;

// Mirror of the ODC ListUtils.Condition [OSStructure] with the same field names.
// Fields are PascalCase (matching the ODC surface) rather than the traditional
// O11 ss-prefixed convention because this project is hand-written without
// Integration Studio generating the type. Consumers who wrap this extension
// through Integration Studio should define an IS Structure "Condition" with
// the same four fields (Path Text, Operator Text, Value Text, CaseSensitive Boolean).
public struct Condition
{
    public string Path;
    public string Operator;
    public string Value;
    public bool CaseSensitive;
}
