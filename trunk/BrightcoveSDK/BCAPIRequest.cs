using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Collections.Specialized;
using System.Web.UI.WebControls;
using BrightcoveSDK.Media;
using BrightcoveSDK.JSON;
using BrightcoveSDK.HTTP;

namespace BrightcoveSDK
{
	public class BCAPIRequest
	{
		public static QueryResultPair ExecuteRead(Dictionary<String, String> reqParams, AccountConfigElement a) {
						
			String reqUrl = BuildReadQuery(reqParams, a);

			HttpWebRequest webRequest = WebRequest.Create(reqUrl) as HttpWebRequest;
			HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse;
			TextReader textreader = new StreamReader(response.GetResponseStream());

			string jsonStr = textreader.ReadToEnd();
			
			QueryResultPair qrp = new QueryResultPair(reqUrl, jsonStr);

			return qrp;
		}

		public static RPCResponse<ResultType> ExecuteWrite<ResultType>(Dictionary<String, Object> postParams, AccountConfigElement a) {
						
			// Create request and receive response
			HttpWebResponse webResponse = PostRequests.MultipartFormDataPost(a.WriteURL.Value, "BCAPI SDK Write Request", postParams);

			// Process response
			StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
			string jsonResult = responseReader.ReadToEnd();
			webResponse.Close();
			
			//this is so that we don't cast null as an object (replaces null with empty error)
			jsonResult = jsonResult.Replace("\"error\": null", "\"error\": { \"name\" : null, \"message\" : null, \"code\" : null }");
			RPCResponse<ResultType> rpcr = JSON.Converter.Deserialize<RPCResponse<ResultType>>(jsonResult);
			
			return rpcr;
		}

		public static RPCResponse ExecuteWrite(Dictionary<String, Object> postParams, AccountConfigElement a) {

			// Create request and receive response
			HttpWebResponse webResponse = PostRequests.MultipartFormDataPost(a.WriteURL.Value, "BCAPI SDK Write Request", postParams);

			// Process response
			StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
			string jsonResult = responseReader.ReadToEnd();
			webResponse.Close();
			
			//this is so that we don't cast null as an object (replaces null with empty error)
			jsonResult = jsonResult.Replace("\"error\": null", "\"error\": { \"name\" : null, \"message\" : null, \"code\" : null }");
			RPCResponse rpcr = JSON.Converter.Deserialize<RPCResponse>(jsonResult);

			return rpcr;
		}

		public static String BuildReadQuery(Dictionary<String, String> reqParams, AccountConfigElement a) {

			String reqUrl = "";

			//append url tokens and count if necessary
			reqUrl += a.ReadURL.Value;
			reqUrl += "?token=" + a.ReadToken.Value;
			reqUrl += "&get_item_count=true";
			
			foreach (String key in reqParams.Keys) {
				reqUrl += "&" + String.Format("{0}={1}", key, HttpUtility.UrlEncode(reqParams[key]));
			}

			return reqUrl;
		}
	
	}
}
