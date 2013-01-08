using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightcoveSDK.Extensions;
using System.Collections.Specialized;

namespace BrightcoveSDK.Utils
{
	public static class EmbedCode
	{
		#region GetEmbedCode

		//based on just player
		public static string GetVideoPlayerEmbedCode(long PlayerID, long VideoID, int height, int width, string BackgroundColor, bool AutoStart, WMode WMode, string objectTagID) {
			return GetEmbedCode(PlayerID, VideoID, PlayerPlaylistType.None, -1, null, height, width, BackgroundColor, AutoStart, WMode, objectTagID, new Dictionary<string, string>());
		}
		public static string GetVideoPlayerEmbedCode(long PlayerID, long VideoID, int height, int width, string BackgroundColor, bool AutoStart, WMode WMode, string objectTagID, Dictionary<string, string> nvc) {
			return GetEmbedCode(PlayerID, VideoID, PlayerPlaylistType.None, -1, null, height, width, BackgroundColor, AutoStart, WMode, objectTagID, nvc);
		}

		//based on list of ids
		public static string GetTabbedPlayerEmbedCode(long PlayerID, List<long> PlaylistIDs, int height, int width, string BackgroundColor, bool AutoStart, WMode WMode, string objectTagID) {
			return GetEmbedCode(PlayerID, -1, PlayerPlaylistType.Tabbed, -1, PlaylistIDs, height, width, BackgroundColor, AutoStart, WMode, objectTagID, new Dictionary<string, string>());
		}
		public static string GetTabbedPlayerEmbedCode(long PlayerID, List<long> PlaylistIDs, int height, int width, string BackgroundColor, bool AutoStart, WMode WMode, string objectTagID, Dictionary<string, string> nvc) {
			return GetEmbedCode(PlayerID, -1, PlayerPlaylistType.Tabbed, -1, PlaylistIDs, height, width, BackgroundColor, AutoStart, WMode, objectTagID, nvc);
		}

		//based on single playlist
		public static string GetVideoListPlayerEmbedCode(long PlayerID, long PlaylistID, int height, int width, string BackgroundColor, bool AutoStart, WMode WMode, string objectTagID) {
			return GetEmbedCode(PlayerID, -1, PlayerPlaylistType.VideoList, PlaylistID, null, height, width, BackgroundColor, AutoStart, WMode, objectTagID, new Dictionary<string, string>());
		}
		public static string GetVideoListPlayerEmbedCode(long PlayerID, long PlaylistID, int height, int width, string BackgroundColor, bool AutoStart, WMode WMode, string objectTagID, Dictionary<string, string> nvc) {
			return GetEmbedCode(PlayerID, -1, PlayerPlaylistType.VideoList, PlaylistID, null, height, width, BackgroundColor, AutoStart, WMode, objectTagID, nvc);
		}

		//based on video	
		public static string GetComboBoxPlayerEmbedCode(long PlayerID, List<long> PlaylistIDs, int height, int width, string BackgroundColor, bool AutoStart, WMode WMode, string objectTagID) {
			return GetEmbedCode(PlayerID, -1, PlayerPlaylistType.ComboBox, -1, PlaylistIDs, height, width, BackgroundColor, AutoStart, WMode, objectTagID, new Dictionary<string, string>());
		}
		public static string GetComboBoxPlayerEmbedCode(long PlayerID, List<long> PlaylistIDs, int height, int width, string BackgroundColor, bool AutoStart, WMode WMode, string objectTagID, Dictionary<string, string> nvc) {
			return GetEmbedCode(PlayerID, -1, PlayerPlaylistType.ComboBox, -1, PlaylistIDs, height, width, BackgroundColor, AutoStart, WMode, objectTagID, nvc);
		}

		/// <summary>
		/// This will build an html object tag based on the information provided
		/// </summary>
		/// <param name="player">The player defined under BrightcoveSDK.SitecoreUtil</param>
		/// <param name="objectTagID">The HTML Object ID Tag</param>
		/// <param name="video">The video defined under BrightcoveSDK.SitecoreUtil</param>
		/// <param name="PlaylistID">A Playlist ID for a single playlist video player</param>
		/// <param name="PlaylistIDs">The List of Playlist IDs for a multi playlist video player</param>
		/// <param name="BackgroundColor">The Hex Value in the form: #ffffff</param>
		/// <param name="AutoStart">A flag to cause the video to automatically start playing</param>
		/// <param name="WMode">The wmode </param>
		/// <param name="objectParams">Specifies any additional object params that should be added</param>
		/// <returns></returns>
		public static string GetEmbedCode(long PlayerID, long VideoID, PlayerPlaylistType PlaylistType, long PlaylistID, List<long> PlaylistIDs, int height, int width, string BackgroundColor, bool AutoStart, WMode WMode, string objectTagID) {
			return GetEmbedCode(PlayerID, VideoID, PlaylistType, PlaylistID, PlaylistIDs, height, width, BackgroundColor, AutoStart, WMode, objectTagID, new Dictionary<string, string>());
		}
		public static string GetEmbedCode(long PlayerID, long VideoID, PlayerPlaylistType PlaylistType, long PlaylistID, List<long> PlaylistIDs, int height, int width, string BackgroundColor, bool AutoStart, WMode WMode, string objectTagID, Dictionary<string, string> objectParams) {

			if (objectParams == null)
				objectParams = new Dictionary<string, string>();

			objectParams.Add("bgcolor", BackgroundColor);
			objectParams.Add("width", width.ToString());
			objectParams.Add("height", height.ToString());
			objectParams.Add("playerID", PlayerID.ToString());

			//add in video ids or playlist ids
			if (PlaylistType.Equals(PlayerPlaylistType.None) && VideoID != -1) {
				objectParams.Add("@videoPlayer", VideoID.ToString());
			} else if (PlaylistType.Equals(PlayerPlaylistType.Tabbed) && PlaylistIDs != null) {
				objectParams.Add("@playlistTabs", PlaylistIDs.ToDelimString(","));
			} else if (PlaylistType.Equals(PlayerPlaylistType.ComboBox) && PlaylistIDs != null) {
				objectParams.Add("@playlistCombo", PlaylistIDs.ToDelimString(","));
			} else if (PlaylistType.Equals(PlayerPlaylistType.VideoList) && PlaylistID != -1) {
				objectParams.Add("@videoList", PlaylistID.ToString());
			}

			objectParams.Add("isVid", "true");
			objectParams.Add("autoStart", AutoStart.ToString().ToLower());
			objectParams.Add("isUI", "true");
			objectParams.Add("dynamicStreaming", "true");
			objectParams.Add("wmode", WMode.ToString());

			return GetEmbedCode(objectTagID, objectParams);
		}

		public static string GetEmbedCode(string objectTagID, Dictionary<string, string> objectParams) {
			
			StringBuilder embed = new StringBuilder();

			//start the brightcove js embed
			embed.AppendLine("<!-- Start of Brightcove Player -->");
			embed.AppendLine("<div style=\"display:none\"></div>");
			embed.AppendLine("<!-- By use of this code snippet, I agree to the Brightcove Publisher T and C found at https://accounts.brightcove.com/en/terms-and-conditions/. -->");
			embed.AppendLine("<script language=\"JavaScript\" type=\"text/javascript\" src=\"http://admin.brightcove.com/js/BrightcoveExperiences_all.js\"></script>");
			embed.AppendLine("<object id=\"" + objectTagID + "\" class=\"BrightcoveExperience\">");
			
			foreach(KeyValuePair<string, string> pair in objectParams){
				embed.AppendLine(string.Format("<param name=\"{0}\" value=\"{1}\" />", pair.Key, pair.Value));
			}

			embed.AppendLine("</object>");
			embed.AppendLine("<script type=\"text/javascript\">brightcove.createExperiences();</script>");
			embed.AppendLine("<!-- End of Brightcove Player -->");

			return embed.ToString();
		}

		#endregion GetEmbedCode
	}
}
