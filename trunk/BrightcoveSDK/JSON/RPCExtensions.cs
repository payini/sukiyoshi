using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightcoveSDK.JSON
{
	public static class RPCExtensions
	{
		#region Extension Methods

		public static string ToJSON(this RPCRequest jsonRPC) {

			Builder rpc = new Builder(",", "{", "}");

			rpc.AppendField("method", jsonRPC.method);
			string parameters = (!jsonRPC.parameters.Equals("null")) ? "{" + jsonRPC.parameters + "}" : "null";
			rpc.AppendObject("params", parameters);
			rpc.AppendObject("id", jsonRPC.id);

			return rpc.ToString();
		}

		public static string ToJSON(this RPCResponse jsonRPC) {

			Builder rpc = new Builder(",", "{", "}");
			rpc.AppendObject("result", "{" + jsonRPC.result + "}");
			string error = (!jsonRPC.error.Equals("null")) ? jsonRPC.error.ToJSON() : "null";
			rpc.AppendObject("error", error);
			rpc.AppendObject("id", jsonRPC.id);

			return rpc.ToString();
		}

		public static string ToJSON(this RPCError jsonRPC) {

			Builder rpc = new Builder(",", "{", "}");
			rpc.AppendObject("name", jsonRPC.name);
			rpc.AppendObject("message", jsonRPC.message);
			rpc.AppendObject("code", jsonRPC.code);

			return rpc.ToString();
		}

		#endregion
	}
}
