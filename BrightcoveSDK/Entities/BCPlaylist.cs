using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;
using System.Runtime.Serialization;
using BrightcoveSDK.Containers;
using BrightcoveSDK.JSON;

namespace BrightcoveSDK.Media
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

		/// <summary>
		/// A user-specified id that uniquely identifies this Video. ReferenceID can be used as a foreign-key to identify this Playlist in another system. 
		/// </summary> 
		[DataMember]
		public string referenceId { get; set; }

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

		[DataMember(Name = "playlistType")]
		private string pType { get; set; }

		/// <summary>
		/// The type of this Playlist.
		/// </summary> 
		public PlaylistTypeEnum playlistType {
			get {
				return (string.IsNullOrEmpty(pType)) ? PlaylistTypeEnum.NEWEST_TO_OLDEST : (PlaylistTypeEnum)Enum.Parse(typeof(PlaylistTypeEnum), pType, true);
			}
			set {
				pType = value.ToString();
			}
		}

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

	public static class BCPlaylistExtensions
	{		

		#region Extension Methods

		public static string ToUpdateJSON(this BCPlaylist playlist) {
			return ToJSON(playlist, JSONType.Update);
		}

		public static string ToCreateJSON(this BCPlaylist playlist) {
			return ToJSON(playlist, JSONType.Create);
		}

		private static string ToJSON(this BCPlaylist playlist, JSONType t) {

			//--Build Playlist in JSON -------------------------------------//

			Builder jsonPlaylist = new Builder(",", "{", "}");
			
			//id
			if(t.Equals(JSONType.Update))
				jsonPlaylist.AppendObject("id", playlist.id.ToString());

			//name
			if (!string.IsNullOrEmpty(playlist.name))
				jsonPlaylist.AppendField("name", playlist.name);
			
			//referenceId
			if (!string.IsNullOrEmpty(playlist.referenceId))
				jsonPlaylist.AppendField("referenceId", playlist.referenceId);
			
			//playlist type
			jsonPlaylist.AppendField("playlistType", playlist.playlistType.ToString());

			if(t.Equals(JSONType.Create)){
				//Video Ids should be a list of strings
				if (playlist.videoIds != null && playlist.videoIds.Count > 0) {
					IEnumerable<string> values = from val in playlist.videoIds
												 select val.ToString();
					jsonPlaylist.AppendObjectArray("videoIds", values);
				}
			}

			//filter tags should be a list of strings
			if (playlist.filterTags != null && playlist.filterTags.Count > 0)
				jsonPlaylist.AppendStringArray("filterTags", playlist.filterTags);
			
			//shortDescription
			if (!string.IsNullOrEmpty(playlist.shortDescription)) {
				jsonPlaylist.AppendField("shortDescription", playlist.shortDescription);
			}
						
			return jsonPlaylist.ToString();
		}

		#endregion
	}
}
