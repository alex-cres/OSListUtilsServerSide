using System;
using System.Collections;
using System.Data;
using System.Runtime.Serialization;
using System.Reflection;
using System.Xml;
using OutSystems.ObjectKeys;
using OutSystems.RuntimeCommon;
using OutSystems.HubEdition.RuntimePlatform;
using OutSystems.HubEdition.RuntimePlatform.Db;
using OutSystems.Internal.Db;

namespace OutSystems.NssListUtils {

	/// <summary>
	/// Structure <code>RCConditionRecord</code>
	/// </summary>
	[Serializable()]
	public partial struct RCConditionRecord: ISerializable, ITypedRecord<RCConditionRecord> {
		internal static readonly GlobalObjectKey IdCondition = GlobalObjectKey.Parse("2UmDmepsh0WSfJ_D1JexCA*ZdGOjzD3npLd0ZeSDqC6MQ");

		public static void EnsureInitialized() {}
		[System.Xml.Serialization.XmlElement("Condition")]
		public STConditionStructure ssSTCondition;


		public static implicit operator STConditionStructure(RCConditionRecord r) {
			return r.ssSTCondition;
		}

		public static implicit operator RCConditionRecord(STConditionStructure r) {
			RCConditionRecord res = new RCConditionRecord(null);
			res.ssSTCondition = r;
			return res;
		}

		public BitArray OptimizedAttributes;

		public RCConditionRecord(params string[] dummy) {
			OptimizedAttributes = null;
			ssSTCondition = new STConditionStructure(null);
		}

		public BitArray[] GetDefaultOptimizedValues() {
			BitArray[] all = new BitArray[1];
			all[0] = null;
			return all;
		}

		public BitArray[] AllOptimizedAttributes {
			set {
				if (value == null) {
				} else {
					ssSTCondition.OptimizedAttributes = value[0];
				}
			}
			get {
				BitArray[] all = new BitArray[1];
				all[0] = null;
				return all;
			}
		}

		/// <summary>
		/// Read a record from database
		/// </summary>
		/// <param name="r"> Data base reader</param>
		/// <param name="index"> index</param>
		public void Read(IDataReader r, ref int index) {
			ssSTCondition.Read(r, ref index);
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
		public void ReadIM(RCConditionRecord r) {
			this = r;
		}


		public static bool operator == (RCConditionRecord a, RCConditionRecord b) {
			if (a.ssSTCondition != b.ssSTCondition) return false;
			return true;
		}

		public static bool operator != (RCConditionRecord a, RCConditionRecord b) {
			return !(a==b);
		}

		public override bool Equals(object o) {
			if (o.GetType() != typeof(RCConditionRecord)) return false;
			return (this == (RCConditionRecord) o);
		}

		public override int GetHashCode() {
			try {
				return base.GetHashCode()
				^ ssSTCondition.GetHashCode()
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

		public RCConditionRecord(SerializationInfo info, StreamingContext context) {
			OptimizedAttributes = null;
			ssSTCondition = new STConditionStructure(null);
			Type objInfo = this.GetType();
			FieldInfo fieldInfo = null;
			fieldInfo = objInfo.GetField("ssSTCondition", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			if (fieldInfo == null) {
				throw new Exception("The field named 'ssSTCondition' was not found.");
			}
			if (fieldInfo.FieldType.IsSerializable) {
				ssSTCondition = (STConditionStructure) info.GetValue(fieldInfo.Name, fieldInfo.FieldType);
			}
		}

		public void RecursiveReset() {
			ssSTCondition.RecursiveReset();
		}

		public void InternalRecursiveSave() {
			ssSTCondition.InternalRecursiveSave();
		}


		public RCConditionRecord Duplicate() {
			RCConditionRecord t;
			t.ssSTCondition = (STConditionStructure) this.ssSTCondition.Duplicate();
			t.OptimizedAttributes = null;
			return t;
		}

		IRecord IRecord.Duplicate() {
			return Duplicate();
		}

		public void ToXml(Object parent, System.Xml.XmlElement baseElem, String fieldName, int detailLevel) {
			System.Xml.XmlElement recordElem = VarValue.AppendChild(baseElem, "Record");
			if (fieldName != null) {
				VarValue.AppendAttribute(recordElem, "debug.field", fieldName);
			}
			if (detailLevel > 0) {
				ssSTCondition.ToXml(this, recordElem, "Condition", detailLevel - 1);
			} else {
				VarValue.AppendDeferredEvaluationElement(recordElem);
			}
		}

		public void EvaluateFields(VarValue variable, Object parent, String baseName, String fields) {
			String head = VarValue.GetHead(fields);
			String tail = VarValue.GetTail(fields);
			variable.Found = false;
			if (head == "condition") {
				if (!VarValue.FieldIsOptimized(parent, baseName + ".Condition")) variable.Value = ssSTCondition; else variable.Optimized = true;
				variable.SetFieldName("condition");
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
			if (key == IdCondition) {
				return ssSTCondition;
			} else {
				throw new Exception("Invalid key");
			}
		}
		public void FillFromOther(IRecord other) {
			if (other == null) return;
			ssSTCondition.FillFromOther((IRecord) other.AttributeGet(IdCondition));
		}
		public bool IsDefault() {
			RCConditionRecord defaultStruct = new RCConditionRecord(null);
			if (this.ssSTCondition != defaultStruct.ssSTCondition) return false;
			return true;
		}
	} // RCConditionRecord

	/// <summary>
	/// Structure <code>RCTextItemRecord</code>
	/// </summary>
	[Serializable()]
	public partial struct RCTextItemRecord: ISerializable, ITypedRecord<RCTextItemRecord> {
		internal static readonly GlobalObjectKey IdTextItem = GlobalObjectKey.Parse("2UmDmepsh0WSfJ_D1JexCA*Js_bUwNIHahS0rwpM7P8cw");

		public static void EnsureInitialized() {}
		[System.Xml.Serialization.XmlElement("TextItem")]
		public STTextItemStructure ssSTTextItem;


		public static implicit operator STTextItemStructure(RCTextItemRecord r) {
			return r.ssSTTextItem;
		}

		public static implicit operator RCTextItemRecord(STTextItemStructure r) {
			RCTextItemRecord res = new RCTextItemRecord(null);
			res.ssSTTextItem = r;
			return res;
		}

		public BitArray OptimizedAttributes;

		public RCTextItemRecord(params string[] dummy) {
			OptimizedAttributes = null;
			ssSTTextItem = new STTextItemStructure(null);
		}

		public BitArray[] GetDefaultOptimizedValues() {
			BitArray[] all = new BitArray[1];
			all[0] = null;
			return all;
		}

		public BitArray[] AllOptimizedAttributes {
			set {
				if (value == null) {
				} else {
					ssSTTextItem.OptimizedAttributes = value[0];
				}
			}
			get {
				BitArray[] all = new BitArray[1];
				all[0] = null;
				return all;
			}
		}

		/// <summary>
		/// Read a record from database
		/// </summary>
		/// <param name="r"> Data base reader</param>
		/// <param name="index"> index</param>
		public void Read(IDataReader r, ref int index) {
			ssSTTextItem.Read(r, ref index);
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
		public void ReadIM(RCTextItemRecord r) {
			this = r;
		}


		public static bool operator == (RCTextItemRecord a, RCTextItemRecord b) {
			if (a.ssSTTextItem != b.ssSTTextItem) return false;
			return true;
		}

		public static bool operator != (RCTextItemRecord a, RCTextItemRecord b) {
			return !(a==b);
		}

		public override bool Equals(object o) {
			if (o.GetType() != typeof(RCTextItemRecord)) return false;
			return (this == (RCTextItemRecord) o);
		}

		public override int GetHashCode() {
			try {
				return base.GetHashCode()
				^ ssSTTextItem.GetHashCode()
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

		public RCTextItemRecord(SerializationInfo info, StreamingContext context) {
			OptimizedAttributes = null;
			ssSTTextItem = new STTextItemStructure(null);
			Type objInfo = this.GetType();
			FieldInfo fieldInfo = null;
			fieldInfo = objInfo.GetField("ssSTTextItem", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			if (fieldInfo == null) {
				throw new Exception("The field named 'ssSTTextItem' was not found.");
			}
			if (fieldInfo.FieldType.IsSerializable) {
				ssSTTextItem = (STTextItemStructure) info.GetValue(fieldInfo.Name, fieldInfo.FieldType);
			}
		}

		public void RecursiveReset() {
			ssSTTextItem.RecursiveReset();
		}

		public void InternalRecursiveSave() {
			ssSTTextItem.InternalRecursiveSave();
		}


		public RCTextItemRecord Duplicate() {
			RCTextItemRecord t;
			t.ssSTTextItem = (STTextItemStructure) this.ssSTTextItem.Duplicate();
			t.OptimizedAttributes = null;
			return t;
		}

		IRecord IRecord.Duplicate() {
			return Duplicate();
		}

		public void ToXml(Object parent, System.Xml.XmlElement baseElem, String fieldName, int detailLevel) {
			System.Xml.XmlElement recordElem = VarValue.AppendChild(baseElem, "Record");
			if (fieldName != null) {
				VarValue.AppendAttribute(recordElem, "debug.field", fieldName);
			}
			if (detailLevel > 0) {
				ssSTTextItem.ToXml(this, recordElem, "TextItem", detailLevel - 1);
			} else {
				VarValue.AppendDeferredEvaluationElement(recordElem);
			}
		}

		public void EvaluateFields(VarValue variable, Object parent, String baseName, String fields) {
			String head = VarValue.GetHead(fields);
			String tail = VarValue.GetTail(fields);
			variable.Found = false;
			if (head == "textitem") {
				if (!VarValue.FieldIsOptimized(parent, baseName + ".TextItem")) variable.Value = ssSTTextItem; else variable.Optimized = true;
				variable.SetFieldName("textitem");
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
			if (key == IdTextItem) {
				return ssSTTextItem;
			} else {
				throw new Exception("Invalid key");
			}
		}
		public void FillFromOther(IRecord other) {
			if (other == null) return;
			ssSTTextItem.FillFromOther((IRecord) other.AttributeGet(IdTextItem));
		}
		public bool IsDefault() {
			RCTextItemRecord defaultStruct = new RCTextItemRecord(null);
			if (this.ssSTTextItem != defaultStruct.ssSTTextItem) return false;
			return true;
		}
	} // RCTextItemRecord
}
