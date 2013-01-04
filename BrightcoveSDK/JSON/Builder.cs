using System.Collections.Generic;
using System.Text;
namespace BrightcoveSDK.JSON
{
	/// <summary>
	/// RJE 10-16-2012
	/// Helper class for building JSON objects.  
	/// The class supports chaining so you can do stuff like this:
	/// string json = new Builder()
	///      .Append("{")
	///         .AppendField("MyStringField", "blah").Append(",")
	///         .AppendField("MyLongField", 1000).Append(",")
	///         .AppendField("MyBoolField", true)
	///      .Append("}").ToString()
	/// </summary>
	public class Builder
	{
		System.Text.StringBuilder sb = new System.Text.StringBuilder();

		public string Delimiter = string.Empty;
		public string Prefix = string.Empty;
		public string Suffix = string.Empty;
		
		#region Constructors

		public Builder() : this(string.Empty, string.Empty, string.Empty) { }

		public Builder(string delimiter, string prefix, string suffix) {
			Delimiter = delimiter;
			Prefix = prefix;
			Suffix = suffix;
		}

		#endregion Constructors

		#region Top Level Methods

		public override string ToString() {
			return string.Format("{0}{1}{2}", Prefix, sb.ToString(), Suffix);
		}

		public Builder AppendFormat(string format, params object[] paramList) {
			if (sb.Length > 0)
				sb.Append(Delimiter);
			sb.AppendFormat(format, paramList);
			return this;
		}

		#endregion Top Level Methods

		#region Derived Methods

		public Builder Append(string s) {
			return AppendFormat("{0}", s);
		}

		/// <summary>
		/// Append Object
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public Builder AppendObject(string key, string value) {
			// Note: no quotes on value is intentional
			return AppendFormat("\"{0}\":{1}", key, value);
		}

		public Builder AppendObject(string key, long value) {
			return AppendObject(key, value.ToString());
		}

		/// <summary>
		/// Append Field
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public Builder AppendField(string key, string value) {
			return AppendObject(key, string.Format("\"{0}\"", value));
		}

		public Builder AppendField(string key, long value) {
			return AppendField(key, value.ToString());
		}

		public Builder AppendField(string key, bool value) {
			return AppendField(key, value ? "true" : "false");
		}

		/// <summary>
		/// Append Array
		/// </summary>
		/// <param name="key"></param>
		/// <param name="values"></param>
		/// <param name="objFormat"></param>
		/// <returns></returns>
		public Builder AppendObjectArray(string key, IEnumerable<string> values, string objFormat) {
			StringBuilder sb = new StringBuilder();
			foreach (string tag in values) {
				if (sb.Length > 0)
					sb.Append(",");
				sb.Append(string.Format(objFormat, tag));

			}
			return AppendObject(key, string.Format("[{0}]", sb.ToString()));
		}
		
		public Builder AppendObjectArray(string key, IEnumerable<string> values) {
			return AppendObjectArray(key, values, "{0}");
		}

		public Builder AppendStringArray(string key, IEnumerable<string> values) {
			return AppendObjectArray(key, values, "\"{0}\"");
		}

		public Builder AppendDictionaryArray(string key, Dictionary<string,string> values) {
			StringBuilder sbFields = new StringBuilder();
			foreach(KeyValuePair<string, string> field in values){
				if (sbFields.Length > 0)
					sbFields.Append(",");
				sbFields.AppendFormat("\"{0}\":\"{1}\"", field.Key, field.Value);
			}
			return AppendObject(key, "{" + sbFields.ToString() + "}");
		}

		#endregion Derived Methods
	}
}