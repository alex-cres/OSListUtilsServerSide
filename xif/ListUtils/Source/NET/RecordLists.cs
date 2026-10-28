using System;
using System.Data;
using System.Collections;
using System.Runtime.Serialization;
using System.Reflection;
using System.Xml;
using OutSystems.ObjectKeys;
using OutSystems.RuntimeCommon;
using OutSystems.HubEdition.RuntimePlatform;
using OutSystems.HubEdition.RuntimePlatform.Db;
using OutSystems.Internal.Db;
using OutSystems.HubEdition.RuntimePlatform.NewRuntime;

namespace OutSystems.NssListUtils {

	/// <summary>
	/// RecordList type <code>RLConditionRecordList</code> that represents a record list of
	///  <code>Condition</code>
	/// </summary>
	[Serializable()]
	public partial class RLConditionRecordList: GenericRecordList<RCConditionRecord>, IEnumerable, IEnumerator, ISerializable {
		public static void EnsureInitialized() {}

		protected override RCConditionRecord GetElementDefaultValue() {
			return new RCConditionRecord("");
		}

		public T[] ToArray<T>(Func<RCConditionRecord, T> converter) {
			return ToArray(this, converter);
		}

		public static T[] ToArray<T>(RLConditionRecordList recordlist, Func<RCConditionRecord, T> converter) {
			return InnerToArray(recordlist, converter);
		}
		public static implicit operator RLConditionRecordList(RCConditionRecord[] array) {
			RLConditionRecordList result = new RLConditionRecordList();
			result.InnerFromArray(array);
			return result;
		}

		public static RLConditionRecordList ToList<T>(T[] array, Func <T, RCConditionRecord> converter) {
			RLConditionRecordList result = new RLConditionRecordList();
			result.InnerFromArray(array, converter);
			return result;
		}

		public static RLConditionRecordList FromRestList<T>(RestList<T> restList, Func <T, RCConditionRecord> converter) {
			RLConditionRecordList result = new RLConditionRecordList();
			result.InnerFromRestList(restList, converter);
			return result;
		}
		/// <summary>
		/// Default Constructor
		/// </summary>
		public RLConditionRecordList(): base() {
		}

		/// <summary>
		/// Constructor with transaction parameter
		/// </summary>
		/// <param name="trans"> IDbTransaction Parameter</param>
		[Obsolete("Use the Default Constructor and set the Transaction afterwards.")]
		public RLConditionRecordList(IDbTransaction trans): base(trans) {
		}

		/// <summary>
		/// Constructor with transaction parameter and alternate read method
		/// </summary>
		/// <param name="trans"> IDbTransaction Parameter</param>
		/// <param name="alternateReadDBMethod"> Alternate Read Method</param>
		[Obsolete("Use the Default Constructor and set the Transaction afterwards.")]
		public RLConditionRecordList(IDbTransaction trans, ReadDBMethodDelegate alternateReadDBMethod): this(trans) {
			this.alternateReadDBMethod = alternateReadDBMethod;
		}

		/// <summary>
		/// Constructor declaration for serialization
		/// </summary>
		/// <param name="info"> SerializationInfo</param>
		/// <param name="context"> StreamingContext</param>
		public RLConditionRecordList(SerializationInfo info, StreamingContext context): base(info, context) {
		}

		public override BitArray[] GetDefaultOptimizedValues() {
			BitArray[] def = new BitArray[1];
			def[0] = null;
			return def;
		}
		/// <summary>
		/// Create as new list
		/// </summary>
		/// <returns>The new record list</returns>
		protected override OSList<RCConditionRecord> NewList() {
			return new RLConditionRecordList();
		}


	} // RLConditionRecordList

	/// <summary>
	/// RecordList type <code>RLTextItemRecordList</code> that represents a record list of
	///  <code>TextItem</code>
	/// </summary>
	[Serializable()]
	public partial class RLTextItemRecordList: GenericRecordList<RCTextItemRecord>, IEnumerable, IEnumerator, ISerializable {
		public static void EnsureInitialized() {}

		protected override RCTextItemRecord GetElementDefaultValue() {
			return new RCTextItemRecord("");
		}

		public T[] ToArray<T>(Func<RCTextItemRecord, T> converter) {
			return ToArray(this, converter);
		}

		public static T[] ToArray<T>(RLTextItemRecordList recordlist, Func<RCTextItemRecord, T> converter) {
			return InnerToArray(recordlist, converter);
		}
		public static implicit operator RLTextItemRecordList(RCTextItemRecord[] array) {
			RLTextItemRecordList result = new RLTextItemRecordList();
			result.InnerFromArray(array);
			return result;
		}

		public static RLTextItemRecordList ToList<T>(T[] array, Func <T, RCTextItemRecord> converter) {
			RLTextItemRecordList result = new RLTextItemRecordList();
			result.InnerFromArray(array, converter);
			return result;
		}

		public static RLTextItemRecordList FromRestList<T>(RestList<T> restList, Func <T, RCTextItemRecord> converter) {
			RLTextItemRecordList result = new RLTextItemRecordList();
			result.InnerFromRestList(restList, converter);
			return result;
		}
		/// <summary>
		/// Default Constructor
		/// </summary>
		public RLTextItemRecordList(): base() {
		}

		/// <summary>
		/// Constructor with transaction parameter
		/// </summary>
		/// <param name="trans"> IDbTransaction Parameter</param>
		[Obsolete("Use the Default Constructor and set the Transaction afterwards.")]
		public RLTextItemRecordList(IDbTransaction trans): base(trans) {
		}

		/// <summary>
		/// Constructor with transaction parameter and alternate read method
		/// </summary>
		/// <param name="trans"> IDbTransaction Parameter</param>
		/// <param name="alternateReadDBMethod"> Alternate Read Method</param>
		[Obsolete("Use the Default Constructor and set the Transaction afterwards.")]
		public RLTextItemRecordList(IDbTransaction trans, ReadDBMethodDelegate alternateReadDBMethod): this(trans) {
			this.alternateReadDBMethod = alternateReadDBMethod;
		}

		/// <summary>
		/// Constructor declaration for serialization
		/// </summary>
		/// <param name="info"> SerializationInfo</param>
		/// <param name="context"> StreamingContext</param>
		public RLTextItemRecordList(SerializationInfo info, StreamingContext context): base(info, context) {
		}

		public override BitArray[] GetDefaultOptimizedValues() {
			BitArray[] def = new BitArray[1];
			def[0] = null;
			return def;
		}
		/// <summary>
		/// Create as new list
		/// </summary>
		/// <returns>The new record list</returns>
		protected override OSList<RCTextItemRecord> NewList() {
			return new RLTextItemRecordList();
		}


	} // RLTextItemRecordList
}
