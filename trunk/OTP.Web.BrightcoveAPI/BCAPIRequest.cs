using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace OTP.Web.BrightcoveAPI
{
	public class BCAPIRequest
	{
		public static string Execute(Dictionary<String, String> reqParams) {

			if(!reqParams.ContainsKey("token")) reqParams.Add("token", BCAPIConfig.ReadToken);
			if (!reqParams.ContainsKey("get_item_count")) reqParams.Add("get_item_count", "true");

			String reqUrl = BCAPIConfig.ServiceURL + "?";
			int i = 0;
			foreach (String key in reqParams.Keys) {
				if (i > 0) reqUrl += "&";
				reqUrl += String.Format("{0}={1}", key, HttpUtility.UrlEncode(reqParams[key]));
				i++;
			}
			//HttpContext.Current.Response.Write(reqUrl + "<br/><br/>");
			
			HttpWebRequest webRequest = WebRequest.Create(reqUrl) as HttpWebRequest;
			HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse;
			TextReader textreader = new StreamReader(response.GetResponseStream());
			
			//otp added this to handle the embedded anchors that were not json compliant1
			//remove \" and replace with '
			string jsonStr = textreader.ReadToEnd();
			jsonStr = jsonStr.Replace("\\\"", "'");
			
			return jsonStr;
		}
	}


	public class JSONHelper
	{
		public static string Serialize<T>(T obj)
		{
			System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(obj.GetType());
			MemoryStream ms = new MemoryStream();
			serializer.WriteObject(ms, obj);
			string retVal = Encoding.Default.GetString(ms.ToArray());
			return retVal;
		}

		public static T Deserialize<T>(string json)
		{
			T obj = Activator.CreateInstance<T>();
			MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
			System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(obj.GetType());
			obj = (T)serializer.ReadObject(ms);
			ms.Close();
			return obj;
		}
	}
}
