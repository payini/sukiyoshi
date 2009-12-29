using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightcoveAPI.Media;

namespace BrightcoveAPI
{
	public class BCVideoComparer : IEqualityComparer<BCVideo>
	{

		#region IEqualityComparer<BCVideo> Members

		public bool Equals(BCVideo x, BCVideo y) {
			return x.id.Equals(y.id);
		}

		public int GetHashCode(BCVideo obj) {
			return obj.GetHashCode();
		}

		#endregion
	}
}
