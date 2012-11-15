using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace BrightcoveSDK.Media
{
	[DataContract]
	public class BCObject
	{
		public static DateTime DateFromUnix(string value) {
			double unixTimeStamp = double.Parse(value.ToString())/1000;
			System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
			return dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
		}

        public static string DateToUnix(DateTime value) {
			DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
			TimeSpan span = (value - epoch);
			return (span.TotalSeconds*1000).ToString();
        }
	}

    public static class BCObjectExtensions {
        
        public static string ToUnixTime(this DateTime value) {
            return BCObject.DateToUnix(value);
        }
    }
}
