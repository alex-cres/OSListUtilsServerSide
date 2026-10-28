using System;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Runtime.Serialization;
using OutSystems.ObjectKeys;
using OutSystems.RuntimeCommon;
using OutSystems.HubEdition.RuntimePlatform;
using OutSystems.HubEdition.RuntimePlatform.Db;
using OutSystems.Internal.Db;

namespace OutSystems.NssListUtils {

	/// <summary>
	/// Structure <code>STConditionStructure</code> that represents the Service Studio structure
	///  <code>Condition</code> <p> Description: A single filter condition used by the multi-conditio
	/// n actions (List_PopByConditions, List_PopMultipleByConditions, List_PartitionByConditions,
	///  List_ReplaceWhere). Groups a property path, a comparison operator, a comparison value and
	/// a case-sensitivity flag into one first-class OutSystems Structure so callers no longer need to
	///  hand-craft a JSON string.</p>
	/// </summary>
	[Serializable()]
	public partial struct STConditionStructure: ISerializable, ITypedRecord<STConditionStructure>, ISimpleRecord {
		internal static readonly GlobalObjectKey IdPath = GlobalObjectKey.Parse("B0albkdSBki06lXcGfP+Zg*VtURmRKeSACW76NbLfs94w");
		internal static readonly GlobalObjectKey IdOperator = GlobalObjectKey.Parse("B0albkdSBki06lXcGfP+Zg*r6FE5668RtekvAUAbOak7A");
		internal static readonly GlobalObjectKey IdValue = GlobalObjectKey.Parse("B0albkdSBki06lXcGfP+Zg*7If9+M62SjKJzTj_siGi6Q");
		internal static readonly GlobalObjectKey IdCaseSensitive = GlobalObjectKey.Parse("B0albkdSBki06lXcGfP+Zg*boC2RvjbQs+w981fiC_8iA");

		public static void EnsureInitialized() {}
		[System.Xml.Serialization.XmlElement("Path")]
		public string ssPath;

		[System.Xml.Serialization.XmlElement("Operator")]
		public string ssOperator;

		[System.Xml.Serialization.XmlElement("Value")]
		public string ssValue;

		[System.Xml.Serialization.XmlElement("CaseSensitive")]
		public bool ssCaseSensitive;


		public BitArray OptimizedAttributes;

		public STConditionStructure(params string[] dummy) {
			OptimizedAttributes = null;
			ssPath = "";
			ssOperator = "";
			ssValue = "";
			ssCaseSensitive = false;
		}

		public BitArray[] GetDefaultOptimizedValues() {
			BitArray[] all = new BitArray[0];
			return all;
		}

		public BitArray[] AllOptimizedAttributes {
			set {
				if (value == null) {
				} else {
				}
			}
			get {
				BitArray[] all = new BitArray[0];
				return all;
			}
		}

		/// <summary>
		/// Read a record from database
		/// </summary>
		/// <param name="r"> Data base reader</param>
		/// <param name="index"> index</param>
		public void Read(IDataReader r, ref int index) {
			ssPath = r.ReadText(index++, "Condition.Path", "");
			ssOperator = r.ReadText(index++, "Condition.Operator", "");
			ssValue = r.ReadText(index++, "Condition.Value", "");
			ssCaseSensitive = r.ReadBoolean(index++, "Condition.CaseSensitive", false);
		}
		/// <summary>
		/// Read from database
		/// </summary>
		/// <param name="r"> Data reader</param>
		public void ReadDB(IDataReader r) {
			int index = 0;
			Read(r, ref index);
		}

		/// <summary>
		/// Read from record
		/// </summary>
		/// <param name="r"> Record</param>
		public void ReadIM(STConditionStructure r) {
			this = r;
		}


		public static bool operator == (STConditionStructure a, STConditionStructure b) {
			if (a.ssPath != b.ssPath) return false;
			if (a.ssOperator != b.ssOperator) return false;
			if (a.ssValue != b.ssValue) return false;
			if (a.ssCaseSensitive != b.ssCaseSensitive) return false;
			return true;
		}

		public static bool operator != (STConditionStructure a, STConditionStructure b) {
			return !(a==b);
		}

		public override bool Equals(object o) {
			if (o.GetType() != typeof(STConditionStructure)) return false;
			return (this == (STConditionStructure) o);
		}

		public override int GetHashCode() {
			try {
				return base.GetHashCode()
				^ ssPath.GetHashCode()
				^ ssOperator.GetHashCode()
				^ ssValue.GetHashCode()
				^ ssCaseSensitive.GetHashCode()
				;
			} catch {
				return base.GetHashCode();
			}
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			Type objInfo = this.GetType();
			FieldInfo[] fields;
			fields = objInfo.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			for (int i = 0; i < fields.Length; i++)
			if (fields[i] .FieldType.IsSerializable)
			info.AddValue(fields[i] .Name, fields[i] .GetValue(this));
		}

		public STConditionStructure(SerializationInfo info, StreamingContext context) {
			OptimizedAttributes = null;
			ssPath = "";
			ssOperator = "";
			ssValue = "";
			ssCaseSensitive = false;
			Type objInfo = this.GetType();
			FieldInfo fieldInfo = null;
			fieldInfo = objInfo.GetField("ssPath", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			if (fieldInfo == null) {
				throw new Exception("The field named 'ssPath' was not found.");
			}
			if (fieldInfo.FieldType.IsSerializable) {
				ssPath = (string) info.GetValue(fieldInfo.Name, fieldInfo.FieldType);
			}
			fieldInfo = objInfo.GetField("ssOperator", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			if (fieldInfo == null) {
				throw new Exception("The field named 'ssOperator' was not found.");
			}
			if (fieldInfo.FieldType.IsSerializable) {
				ssOperator = (string) info.GetValue(fieldInfo.Name, fieldInfo.FieldType);
			}
			fieldInfo = objInfo.GetField("ssValue", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			if (fieldInfo == null) {
				throw new Exception("The field named 'ssValue' was not found.");
			}
			if (fieldInfo.FieldType.IsSerializable) {
				ssValue = (string) info.GetValue(fieldInfo.Name, fieldInfo.FieldType);
			}
			fieldInfo = objInfo.GetField("ssCaseSensitive", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			if (fieldInfo == null) {
				throw new Exception("The field named 'ssCaseSensitive' was not found.");
			}
			if (fieldInfo.FieldType.IsSerializable) {
				ssCaseSensitive = (bool) info.GetValue(fieldInfo.Name, fieldInfo.FieldType);
			}
		}

		public void RecursiveReset() {
		}

		public void InternalRecursiveSave() {
		}


		public STConditionStructure Duplicate() {
			STConditionStructure t;
			t.ssPath = this.ssPath;
			t.ssOperator = this.ssOperator;
			t.ssValue = this.ssValue;
			t.ssCaseSensitive = this.ssCaseSensitive;
			t.OptimizedAttributes = null;
			return t;
		}

		IRecord IRecord.Duplicate() {
			return Duplicate();
		}

		public void ToXml(Object parent, System.Xml.XmlElement baseElem, String fieldName, int detailLevel) {
			System.Xml.XmlElement recordElem = VarValue.AppendChild(baseElem, "Structure");
			if (fieldName != null) {
				VarValue.AppendAttribute(recordElem, "debug.field", fieldName);
				fieldName = fieldName.ToLowerInvariant();
			}
			if (detailLevel > 0) {
				if (!VarValue.FieldIsOptimized(parent, fieldName + ".Path")) VarValue.AppendAttribute(recordElem, "Path", ssPath, detailLevel, TypeKind.Text); else VarValue.AppendOptimizedAttribute(recordElem, "Path");
				if (!VarValue.FieldIsOptimized(parent, fieldName + ".Operator")) VarValue.AppendAttribute(recordElem, "Operator", ssOperator, detailLevel, TypeKind.Text); else VarValue.AppendOptimizedAttribute(recordElem, "Operator");
				if (!VarValue.FieldIsOptimized(parent, fieldName + ".Value")) VarValue.AppendAttribute(recordElem, "Value", ssValue, detailLevel, TypeKind.Text); else VarValue.AppendOptimizedAttribute(recordElem, "Value");
				if (!VarValue.FieldIsOptimized(parent, fieldName + ".CaseSensitive")) VarValue.AppendAttribute(recordElem, "CaseSensitive", ssCaseSensitive, detailLevel, TypeKind.Boolean); else VarValue.AppendOptimizedAttribute(recordElem, "CaseSensitive");
			} else {
				VarValue.AppendDeferredEvaluationElement(recordElem);
			}
		}

		public void EvaluateFields(VarValue variable, Object parent, String baseName, String fields) {
			String head = VarValue.GetHead(fields);
			String tail = VarValue.GetTail(fields);
			variable.Found = false;
			if (head == "path") {
				if (!VarValue.FieldIsOptimized(parent, baseName + ".Path")) variable.Value = ssPath; else variable.Optimized = true;
			} else if (head == "operator") {
				if (!VarValue.FieldIsOptimized(parent, baseName + ".Operator")) variable.Value = ssOperator; else variable.Optimized = true;
			} else if (head == "value") {
				if (!VarValue.FieldIsOptimized(parent, baseName + ".Value")) variable.Value = ssValue; else variable.Optimized = true;
			} else if (head == "casesensitive") {
				if (!VarValue.FieldIsOptimized(parent, baseName + ".CaseSensitive")) variable.Value = ssCaseSensitive; else variable.Optimized = true;
			}
			if (variable.Found && tail != null) variable.EvaluateFields(this, head, tail);
		}

		public bool ChangedAttributeGet(GlobalObjectKey key) {
			throw new Exception("Method not Supported");
		}

		public bool OptimizedAttributeGet(GlobalObjectKey key) {
			throw new Exception("Method not Supported");
		}

		public object AttributeGet(GlobalObjectKey key) {
			if (key == IdPath) {
				return ssPath;
			} else if (key == IdOperator) {
				return ssOperator;
			} else if (key == IdValue) {
				return ssValue;
			} else if (key == IdCaseSensitive) {
				return ssCaseSensitive;
			} else {
				throw new Exception("Invalid key");
			}
		}
		public void FillFromOther(IRecord other) {
			if (other == null) return;
			ssPath = (string) other.AttributeGet(IdPath);
			ssOperator = (string) other.AttributeGet(IdOperator);
			ssValue = (string) other.AttributeGet(IdValue);
			ssCaseSensitive = (bool) other.AttributeGet(IdCaseSensitive);
		}
		public bool IsDefault() {
			STConditionStructure defaultStruct = new STConditionStructure(null);
			if (this.ssPath != defaultStruct.ssPath) return false;
			if (this.ssOperator != defaultStruct.ssOperator) return false;
			if (this.ssValue != defaultStruct.ssValue) return false;
			if (this.ssCaseSensitive != defaultStruct.ssCaseSensitive) return false;
			return true;
		}
	} // STConditionStructure

	/// <summary>
	/// Structure <code>STTextItemStructure</code> that represents the Service Studio structure
	///  <code>TextItem</code> <p> Description: One Text value. Integration Studio has no List-of-Tex
	/// t parameter type, so Text lists (property paths, key names, JSON chunks) are passed as a Record
	///  List of TextItem.</p>
	/// </summary>
	[Serializable()]
	public partial struct STTextItemStructure: ISerializable, ITypedRecord<STTextItemStructure>, ISimpleRecord {
		internal static readonly GlobalObjectKey IdValue = GlobalObjectKey.Parse("B0albkdSBki06lXcGfP+Zg*9mjr_+TdT4KCgs5gVjtnOg");

		public static void EnsureInitialized() {}
		[System.Xml.Serialization.XmlElement("Value")]
		public string ssValue;


		public BitArray OptimizedAttributes;

		public STTextItemStructure(params string[] dummy) {
			OptimizedAttributes = null;
			ssValue = "";
		}

		public BitArray[] GetDefaultOptimizedValues() {
			BitArray[] all = new BitArray[0];
			return all;
		}

		public BitArray[] AllOptimizedAttributes {
			set {
				if (value == null) {
				} else {
				}
			}
			get {
				BitArray[] all = new BitArray[0];
				return all;
			}
		}

		/// <summary>
		/// Read a record from database
		/// </summary>
		/// <param name="r"> Data base reader</param>
		/// <param name="index"> index</param>
		public void Read(IDataReader r, ref int index) {
			ssValue = r.ReadText(index++, "TextItem.Value", "");
		}
		/// <summary>
		/// Read from database
		/// </summary>
		/// <param name="r"> Data reader</param>
		public void ReadDB(IDataReader r) {
			int index = 0;
			Read(r, ref index);
		}

		/// <summary>
		/// Read from record
		/// </summary>
		/// <param name="r"> Record</param>
		public void ReadIM(STTextItemStructure r) {
			this = r;
		}


		public static bool operator == (STTextItemStructure a, STTextItemStructure b) {
			if (a.ssValue != b.ssValue) return false;
			return true;
		}

		public static bool operator != (STTextItemStructure a, STTextItemStructure b) {
			return !(a==b);
		}

		public override bool Equals(object o) {
			if (o.GetType() != typeof(STTextItemStructure)) return false;
			return (this == (STTextItemStructure) o);
		}

		public override int GetHashCode() {
			try {
				return base.GetHashCode()
				^ ssValue.GetHashCode()
				;
			} catch {
				return base.GetHashCode();
			}
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			Type objInfo = this.GetType();
			FieldInfo[] fields;
			fields = objInfo.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			for (int i = 0; i < fields.Length; i++)
			if (fields[i] .FieldType.IsSerializable)
			info.AddValue(fields[i] .Name, fields[i] .GetValue(this));
		}

		public STTextItemStructure(SerializationInfo info, StreamingContext context) {
			OptimizedAttributes = null;
			ssValue = "";
			Type objInfo = this.GetType();
			FieldInfo fieldInfo = null;
			fieldInfo = objInfo.GetField("ssValue", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			if (fieldInfo == null) {
				throw new Exception("The field named 'ssValue' was not found.");
			}
			if (fieldInfo.FieldType.IsSerializable) {
				ssValue = (string) info.GetValue(fieldInfo.Name, fieldInfo.FieldType);
			}
		}

		public void RecursiveReset() {
		}

		public void InternalRecursiveSave() {
		}


		public STTextItemStructure Duplicate() {
			STTextItemStructure t;
			t.ssValue = this.ssValue;
			t.OptimizedAttributes = null;
			return t;
		}

		IRecord IRecord.Duplicate() {
			return Duplicate();
		}

		public void ToXml(Object parent, System.Xml.XmlElement baseElem, String fieldName, int detailLevel) {
			System.Xml.XmlElement recordElem = VarValue.AppendChild(baseElem, "Structure");
			if (fieldName != null) {
				VarValue.AppendAttribute(recordElem, "debug.field", fieldName);
				fieldName = fieldName.ToLowerInvariant();
			}
			if (detailLevel > 0) {
				if (!VarValue.FieldIsOptimized(parent, fieldName + ".Value")) VarValue.AppendAttribute(recordElem, "Value", ssValue, detailLevel, TypeKind.Text); else VarValue.AppendOptimizedAttribute(recordElem, "Value");
			} else {
				VarValue.AppendDeferredEvaluationElement(recordElem);
			}
		}

		public void EvaluateFields(VarValue variable, Object parent, String baseName, String fields) {
			String head = VarValue.GetHead(fields);
			String tail = VarValue.GetTail(fields);
			variable.Found = false;
			if (head == "value") {
				if (!VarValue.FieldIsOptimized(parent, baseName + ".Value")) variable.Value = ssValue; else variable.Optimized = true;
			}
			if (variable.Found && tail != null) variable.EvaluateFields(this, head, tail);
		}

		public bool ChangedAttributeGet(GlobalObjectKey key) {
			throw new Exception("Method not Supported");
		}

		public bool OptimizedAttributeGet(GlobalObjectKey key) {
			throw new Exception("Method not Supported");
		}

		public object AttributeGet(GlobalObjectKey key) {
			if (key == IdValue) {
				return ssValue;
			} else {
				throw new Exception("Invalid key");
			}
		}
		public void FillFromOther(IRecord other) {
			if (other == null) return;
			ssValue = (string) other.AttributeGet(IdValue);
		}
		public bool IsDefault() {
			STTextItemStructure defaultStruct = new STTextItemStructure(null);
			if (this.ssValue != defaultStruct.ssValue) return false;
			return true;
		}
	} // STTextItemStructure

} // OutSystems.NssListUtils
