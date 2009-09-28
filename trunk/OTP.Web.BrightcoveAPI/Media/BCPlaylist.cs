using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;
using System.Runtime.Serialization;

namespace OTP.Web.BrightcoveAPI.Media
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
				if (pType.Equals(PlaylistTypeEnum.ALPHABETICAL.ToString())) {
					return PlaylistTypeEnum.ALPHABETICAL;
				}
				if (pType.Equals(PlaylistTypeEnum.EXPLICIT.ToString())) {
					return PlaylistTypeEnum.EXPLICIT;
				}
				if (pType.Equals(PlaylistTypeEnum.OLDEST_TO_NEWEST.ToString())) {
					return PlaylistTypeEnum.OLDEST_TO_NEWEST;
				}
				if (pType.Equals(PlaylistTypeEnum.PLAYS_TOTAL.ToString())) {
					return PlaylistTypeEnum.PLAYS_TOTAL;
				}
				if (pType.Equals(PlaylistTypeEnum.PLAYS_TRAILING_WEEK.ToString())) {
					return PlaylistTypeEnum.PLAYS_TRAILING_WEEK;
				}
				else {
					return PlaylistTypeEnum.NEWEST_TO_OLDEST;
				}
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

			//id
			string jsonPlaylist = "{\"id\": " + playlist.id.ToString();

			//name
			if (!string.IsNullOrEmpty(playlist.name)) {
				jsonPlaylist += ",\"name\": \"" + playlist.name + "\"";
			}

			if(t.Equals(JSONType.Create)){
				//playlist type
				jsonPlaylist += ",\"playlistType\": \"" + playlist.playlistType.ToString() + "\"";

				//Video Ids should be a list of strings
				if (playlist.videoIds != null && playlist.videoIds.Count > 0) {
					jsonPlaylist += ",\"videoIds\": [";
					string append = "";
					foreach (long id in playlist.videoIds) {
						jsonPlaylist += append + "" + id.ToString() + "";
						append = ",";
					}
					jsonPlaylist += "]";
				}
			}

			//filter tags should be a list of strings
			if (playlist.filterTags != null && playlist.filterTags.Count > 0) {
				jsonPlaylist += ",\"filterTags\": [";
				string append = "";
				foreach (string tag in playlist.filterTags) {
					jsonPlaylist += append + "\"" + tag + "\"";
					append = ",";
				}
				jsonPlaylist += "]";
			}

			//referenceId
			if (playlist.referenceId != null) {
				jsonPlaylist += ",\"referenceId\": \"" + playlist.referenceId + "\"";
			}


			//shortDescription
			if (!string.IsNullOrEmpty(playlist.shortDescription)) {
				jsonPlaylist += ",\"shortDescription\": \"" + playlist.shortDescription + "\"";
			}
						
			jsonPlaylist += "}";

			return jsonPlaylist;
		}

		#endregion
	}
}
