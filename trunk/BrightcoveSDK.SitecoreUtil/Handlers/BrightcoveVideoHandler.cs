using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Sitecore.Diagnostics;
using Sitecore;
using Sitecore.Events;
using Sitecore.Resources.Media;
using Sitecore.Web;
using Sitecore.Configuration;
using Sitecore.SecurityModel;
using System.IO;
using BrightcoveSDK.UI;
using Sitecore.Data.Items;
using System.Web.UI;
using Sitecore.Data;
using BrightcoveSDK.SitecoreUtil.Entity;
using BrightcoveSDK.Extensions;
using System.Collections.Specialized;
using BrightcoveSDK.Utils;

namespace BrightcoveSDK.SitecoreUtil.Handlers
{
	public class BrightcoveVideoHandler : System.Web.UI.Page, IHttpHandler
	{		
		// Override the ProcessRequest method.
		public void ProcessRequest(HttpContext context) {
			Assert.ArgumentNotNull(context, "context");
			DoProcessRequest(context);
		}
		
		// Methods
		private void DoProcessRequest(HttpContext context) {
			Assert.ArgumentNotNull(context, "context");
			
			//return DoProcessRequest(context, request, media);
			if (context != null) {
				NameValueCollection source = context.Request.QueryString;
				var dict = source.Cast<string>()
						.Select(s => new { Key = s, Value = source[s] })
						.ToDictionary(x => x.Key, y => y.Value);

				//player
				long qPlayer = 0;
				string playerKey = "player";
				if (dict.ContainsKey(playerKey)) {
					long.TryParse(dict[playerKey], out qPlayer);
					dict.Remove(playerKey);
				}
				PlayerItem p = PlayerLibraryItem.GetPlayer(qPlayer);
                if (p == null) { context.Response.Write("The player is null"); return; }

				//video 
				long qVideo = 0;
				string videoKey = "video";
				if (dict.ContainsKey(videoKey)) {
					long.TryParse(dict[videoKey], out qVideo);
					dict.Remove(videoKey);
				}
								
				//playlist ids
				long qPlaylist = 0;
				List<long> qPlaylistIds = new List<long>();
				string playlistKey = "playlists";
				if(dict.ContainsKey(playlistKey)) {
					string[] playlistids = dict[playlistKey].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
					//if you only want a single
					if (p.PlaylistType.Equals(PlayerPlaylistType.None)) {
						if (playlistids.Any())
							long.TryParse(playlistids[0], out qPlaylist);
					} else { //you're looking for multiple
						foreach (string id in playlistids) {
							long plist = -1;
							if (long.TryParse(id, out plist))
								qPlaylistIds.Add(plist);
						}
					}
					dict.Remove(playlistKey);
				}

				//remove height, width and iframe
				string heightKey = "height", widthKey = "width", iframeKey = "iframe";
				if (dict.ContainsKey(heightKey))
					dict.Remove(heightKey);
				if (dict.ContainsKey(widthKey))
					dict.Remove(widthKey);
				if (dict.ContainsKey(iframeKey))
					dict.Remove(iframeKey);
				
				//auto start
				bool qAutoStart = false;
				string autoStartKey = "autoStart";
				if (dict.ContainsKey(autoStartKey)) {
					bool.TryParse(dict[autoStartKey], out qAutoStart);
					dict.Remove(autoStartKey);
				}

				//bg color
				string bgKey = "bgcolor";
				string qBgColor = (dict.ContainsKey(bgKey)) ? dict[bgKey] : "";
				if (!qBgColor.Contains("#"))
					qBgColor = "#" + qBgColor;
				dict.Remove(bgKey);

				//converts oparams to key value pairs
				string paramKey = "oparams";
				string qParams = string.Empty;
				if (dict.ContainsKey(paramKey)) {
					string[] oparams = dict[paramKey].Split(',');
					foreach (string pair in oparams) {
						string[] kv = pair.Split('=');
						if (kv.Length > 1)
							dict.Add(kv[0], kv[1]);
					}
					dict.Remove(paramKey);
				}
				
				//wmode 
				string wmodeKey = "wmode";
				WMode qWMode = (!dict.ContainsKey(wmodeKey) || string.IsNullOrEmpty(dict[wmodeKey])) ? BrightcoveSDK.WMode.Transparent : (BrightcoveSDK.WMode)Enum.Parse(typeof(BrightcoveSDK.WMode), dict[wmodeKey], true);
				dict.Remove(wmodeKey);

                StringBuilder sb = new StringBuilder();
				sb.AppendLine("<html><head>");
                sb.AppendLine("</head><body>");
				
				string uniqueID = "video_" + DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss.FFFF");
				switch (p.PlaylistType) {
                    case PlayerPlaylistType.VideoList:
						sb.AppendLine(EmbedCode.GetVideoListPlayerEmbedCode(qPlayer, qPlaylist, p.Height, p.Width, qBgColor, qAutoStart, qWMode, uniqueID, dict));
                        break;
                    case PlayerPlaylistType.Tabbed:
						sb.AppendLine(EmbedCode.GetTabbedPlayerEmbedCode(qPlayer, qPlaylistIds, p.Height, p.Width, qBgColor, qAutoStart, qWMode, uniqueID, dict));
						break;
					case PlayerPlaylistType.ComboBox:
                        sb.AppendLine(EmbedCode.GetComboBoxPlayerEmbedCode(qPlayer, qPlaylistIds, p.Height, p.Width, qBgColor, qAutoStart, qWMode, uniqueID, dict));
						break;
					case PlayerPlaylistType.None:
						sb.AppendLine(EmbedCode.GetVideoPlayerEmbedCode(qPlayer, qVideo, p.Height, p.Width, qBgColor, qAutoStart, qWMode, uniqueID, dict));
						break;
				}

                sb.AppendLine("</body></html>");
				context.Response.Write(sb.ToString());
			}
			return;
		}

		// Override the IsReusable property.
		public bool IsReusable {
			get { return true; }
		}
	}
}

