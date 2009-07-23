using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace OTP.Web.BrightcoveAPI
{
	[DataContract]
	public class BCObject
	{
		public static DateTime DateFromUnix(object value) {
			long millisecs = long.Parse(value.ToString());
			double secs = millisecs / 1000;
			return new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(secs);
		}
	}
}
