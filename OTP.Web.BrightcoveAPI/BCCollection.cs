using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace OTP.Web.BrightcoveAPI
{
	[CollectionDataContract]
	public class BCCollection<T> : List<T>
	{}
}
