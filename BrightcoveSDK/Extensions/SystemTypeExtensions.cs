using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightcoveSDK.Extensions
{
	public static class ListOfLongExtensions
	{
		public static string ToDelimString(this List<long> Values, string Delimiter) {

			StringBuilder sb = new StringBuilder();

			foreach (long s in Values) {
				if (sb.Length > 0) {
					sb.Append(Delimiter);
				}
				sb.Append(s.ToString());
			}

			return sb.ToString();
		}
	}
}
