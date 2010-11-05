using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BrightcoveSDK.JSON
{
	public class Converter
	{
		public static string Serialize<SendType>(SendType obj) {
			System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(obj.GetType());
			MemoryStream ms = new MemoryStream();
			serializer.WriteObject(ms, obj);
			string retVal = Encoding.Default.GetString(ms.ToArray());
			return retVal;
		}

		public static ReturnType Deserialize<ReturnType>(string json) {
			ReturnType obj = Activator.CreateInstance<ReturnType>();
			MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
			System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(obj.GetType());
			obj = (ReturnType)serializer.ReadObject(ms);
			ms.Close();
			return obj;
		}
	}
}
