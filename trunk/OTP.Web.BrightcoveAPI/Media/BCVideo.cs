using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;
using System.Runtime.Serialization;

namespace OTP.Web.BrightcoveAPI.Media
{
	/// <summary>
	/// The Video object is an aggregation of metadata and asset information associated with a video
	/// </summary>
	[DataContract]
	public class BCVideo : BCObject, IComparable<BCVideo>
	{
		/// <summary>
		/// A number that uniquely identifies this Video, assigned by Brightcove when the Video is created.
		/// </summary>
		[DataMember]
		public long id { get; set; }

		/// <summary>
		/// The title of this Video.
		/// </summary> 
		[DataMember]
		public string name { get; set; }

		/// <summary>
		/// A short description describing this Video, limited to 256 characters.
		/// </summary> 
		[DataMember]
		public string shortDescription { get; set; }

		/// <summary>
		/// A longer description of this Video, bounded by a 1024 character limit.
		/// </summary> 
		[DataMember]
		public string longDescription { get; set; }

		[DataMember(Name = "creationDate")]
		private string createDate { get; set; }

		/// <summary>
		/// The date this Video was created, represented as the number of milliseconds since the Unix epoch.
		/// </summary> 
		public DateTime creationDate {
			get {
				return DateFromUnix(createDate);
			}
			set {
				createDate = DateToUnix(value);
			}
		}

		[DataMember(Name = "publishedDate")]
		private string publishDate { get; set; }

		/// <summary>
		/// The date this Video was last made active, represented as the number of milliseconds since the Unix epoch.
		/// </summary> 
		public DateTime publishedDate {
			get {
				return DateFromUnix(publishDate);
			}
			set {
				publishDate = DateToUnix(value);
			}
		}

		[DataMember(Name = "lastModifiedDate")]
		private string modifyDate { get; set; }

		/// <summary>
		/// The date this Video was last modified, represented as the number of milliseconds since the Unix epoch.
		/// </summary> 
		public DateTime lastModifiedDate {
			get {
				return DateFromUnix(modifyDate);
			}
			set {
				modifyDate = DateToUnix(value);
			}
		}

		/// <summary>
		/// An optional URL to a related item.
		/// </summary> 
		[DataMember]
		public string linkURL { get; set; }

		/// <summary>
		/// The text displayed for the linkURL.
		/// </summary> 
		[DataMember]
		public string linkText { get; set; }

		/// <summary>
		/// A list of Strings representing the tags assigned to this Video.
		/// </summary> 
		[DataMember]
		public BCCollection<string> tags;

		/// <summary>
		/// The URL to the video still image associated with this Video. Video stills are 480x360 pixels.
		/// </summary> 
		[DataMember]
		public string videoStillURL { get; set; }

		/// <summary>
		/// The URL to the thumbnail image associated with this Video. Thumbnails are 120x90 pixels.
		/// </summary> 
		[DataMember]
		public string thumbnailURL { get; set; }
				
		/// <summary>
		/// A user-specified id that uniquely identifies this Video. ReferenceID can be used as a foreign-key to identify this video in another system. 
		/// </summary> 
		[DataMember]
		public string referenceId { get; set; }

		/// <summary>
		/// The length of this video in milliseconds.
		/// </summary> 
		[DataMember]
		public string length { get; set; }

		[DataMember(Name = "economics")]
		private string ecs { get; set; }

		/// <summary>
		/// Either FREE or AD_SUPPORTED. AD_SUPPORTED means that ad requests are enabled for this Video.
		/// </summary> 
		public BCVideoEconomics economics {
			get {
				if (ecs == null) {
					return BCVideoEconomics.AD_SUPPORTED;
				}
				else if (ecs.Equals(BCVideoEconomics.AD_SUPPORTED.ToString())) {
					return BCVideoEconomics.AD_SUPPORTED;
				}
				else if (ecs.Equals(BCVideoEconomics.FREE.ToString())) {
					return BCVideoEconomics.FREE;
				}
				else {
					return BCVideoEconomics.AD_SUPPORTED;
				}
			}
			set {
				ecs = value.ToString();
			}
		}

		[DataMember(Name = "playsTotal")]
		private string plays { get; set; }

		/// <summary>
		/// How many times this Video has been played since its creation.
		/// </summary> 
		public long playsTotal {
			get {
				if (!String.IsNullOrEmpty(plays)) {
					return long.Parse(plays);
				} else {
					return 0;
				}
			}
			set {
				plays = value.ToString();
			}
		}

		[DataMember(Name = "playsTrailingWeek")]
		private string playsWeek { get; set; }

		/// <summary>
		/// How many times this Video has been played within the past seven days, exclusive of today.
		/// </summary> 
		public long playsTrailingWeek {
			get {
				if(!String.IsNullOrEmpty(playsWeek)) {
					return long.Parse(playsWeek);
				} else {
					return 0;
				}
			}
			set {
				playsWeek = value.ToString();
			}
		}

		public BCVideo() {
			tags = new BCCollection<string>();
		}

		#region IComparable Comparators

		public int CompareTo(BCVideo other) {
			return name.CompareTo(other.name);
		}

		//CREATION_DATE
		public static Comparison<BCVideo> CreationDateComparison =
			delegate(BCVideo v1, BCVideo v2)
			{
				return v1.creationDate.CompareTo(v2.creationDate);
			};

		//PLAYS_TOTAL
		public static Comparison<BCVideo> TotalPlaysComparison =
			delegate(BCVideo v1, BCVideo v2)
			{
				return v1.playsTotal.CompareTo(v2.playsTotal);
			};

		//PUBLISH_DATE
		public static Comparison<BCVideo> PublishDateComparison =
			delegate(BCVideo v1, BCVideo v2)
			{
				return v1.publishedDate.CompareTo(v2.publishedDate);
			};

		//MODIFIED_DATE
		public static Comparison<BCVideo> ModifiedDateComparison =
			delegate(BCVideo v1, BCVideo v2)
			{
				return v1.lastModifiedDate.CompareTo(v2.lastModifiedDate);
			};

		//PLAYS_TRAILING_WEEK
		public static Comparison<BCVideo> PlaysTrailingComparison =
			delegate(BCVideo v1, BCVideo v2)
			{
				return v1.playsTrailingWeek.CompareTo(v2.playsTrailingWeek);
			};

		#endregion
	}

	public static class BCVideoExtensions {
		
		#region Extension Methods

		public static string ToCreateJSON(this BCVideo video) {
			return ToJSON(video, JSONType.Create);
		}

		public static string ToJSON(this BCVideo video) {
			return ToJSON(video, JSONType.Update);
		}
		private static string ToJSON(this BCVideo video, JSONType type) {

			//--Build Video in JSON -------------------------------------//

			string jsonVideo = "{";

			if(type.Equals(JSONType.Update)){
				//id
				jsonVideo += "\"id\": " + video.id.ToString() + ",";
			}

			//name
			if (!string.IsNullOrEmpty(video.name)) {
				jsonVideo += "\"name\": \"" + video.name + "\"";
			}

			//shortDescription
			if (!string.IsNullOrEmpty(video.shortDescription)) {
				jsonVideo += ",\"shortDescription\": \"" + video.shortDescription + "\"";
			}

			//Tags should be a list of strings
			if (video.tags != null && video.tags.Count > 0) {
				jsonVideo += ",\"tags\": [";
				string append = "";
				foreach (string tag in video.tags) {
					jsonVideo += append + "\"" + tag + "\"";
					append = ",";
				}
				jsonVideo += "]";
			}

			//referenceId
			if (!string.IsNullOrEmpty(video.referenceId)) {
				jsonVideo += ",\"referenceId\": \"" + video.referenceId + "\"";
			}

			//longDescription
			if (!string.IsNullOrEmpty(video.longDescription)) {
				jsonVideo += ",\"longDescription\": \"" + video.longDescription + "\"";
			}

			//economics
			jsonVideo += ",\"economics\": " + video.economics.ToString();

			jsonVideo += "}";

			return jsonVideo;
		}

		#endregion
	}
}
