using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;
using System.Runtime.Serialization;

namespace OTP.Web.BrightcoveAPI
{
	/// <summary>
	/// The Playlist object is an aggregation of metadata and asset information associated with a Playlist
	/// </summary>
	[DataContract]
	public class BCPlaylist : BCObject, IComparable<BCPlaylist>
	{
		
		/// <summary>
		/// A number that uniquely identifies this Playlist, assigned by Brightcove when the Playlist is created.
		/// </summary>
		[DataMember]
		public long id { get; set; }

		[DataMember(Name = "referenceId")]
		private string refId { get; set; }

		/// <summary>
		/// A user-specified id that uniquely identifies this Video. ReferenceID can be used as a foreign-key to identify this Playlist in another system. 
		/// </summary> 
		public long referenceId {
			get {
				try {
					return long.Parse(refId);
				}
				catch (ArgumentNullException ex) {
					return -1;
				}
			}
			set {
				refId = value.ToString();
			}
		}
	
		/// <summary>
		/// The name of this Playlist.
		/// </summary> 
		[DataMember]
		public string name { get; set; }

		/// <summary>
		/// A short description describing this Playlist, limited to 256 characters.
		/// </summary> 
		[DataMember]
		public string shortDescription { get; set; }

		/// <summary>
		/// A list of Strings representing the video ids assigned to this Playlist.
		/// </summary> 
		[DataMember]
		public BCCollection<long> videoIds;

		/// <summary>
		/// A list of Strings representing the videos assigned to this Playlist.
		/// </summary> 
		[DataMember]
		public BCCollection<BCVideo> videos;

		/// <summary>
		/// A url for the thumbnail of this Playlist.
		/// </summary> 
		[DataMember]
		public string thumbnailURL { get; set; }

		/// <summary>
		/// A list of Strings representing the tags assigned to this Playlist.
		/// </summary> 
		[DataMember]
		public BCCollection<string> filterTags;

		/// <summary>
		/// The type of this Playlist.
		/// </summary> 
		[DataMember]
		public string playlistType { get; set; }

		/// <summary>
		/// The account id associated with this Playlist.
		/// </summary> 
		public long accountId { get; set; }

		public BCPlaylist() {
			videos = new BCCollection<BCVideo>();
			filterTags = new BCCollection<string>();
			videoIds = new BCCollection<long>();
		}

		#region IComparable Comparators

		public int CompareTo(BCPlaylist other) {
			return name.CompareTo(other.name);
		}
				
		#endregion
	}
}
