using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace OTP.Web.BrightcoveAPI
{
	[DataContract]
	public class BCQueryResult
	{
		[DataMember(Name = "videos")]
		public BCCollection<BCVideo> Videos;
		[DataMember(Name = "playlists")]
		public BCCollection<BCPlaylist> Playlists;
		[DataMember(Name = "page_number")]
		public string PageNumber { get; set; }
		[DataMember(Name = "page_size")]
		public string PageSize { get; set; }
		[DataMember(Name = "total_count")]
		public string TotalCount { get; set; }

		public int MaxToGet = 0;
		public List<string> RequestCall = new List<string>();

		public BCQueryResult() {
			Playlists = new BCCollection<BCPlaylist>();
			Videos = new BCCollection<BCVideo>();
		}

		public void Merge(BCQueryResult qr) {
			
			//qr.RequestCall.AddRange(qr.RequestCall);
			if (qr.Videos != null) Videos.AddRange(qr.Videos);
			if(qr.Playlists != null) Playlists.AddRange(qr.Playlists);
			PageNumber = qr.PageNumber;
			TotalCount = qr.TotalCount;
			PageSize = qr.PageSize;
		}
	}
}
