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

namespace BrightcoveSDK.SitecoreUtil.Handlers
{
	public class BrightcoveVideoHandler : System.Web.UI.Page, IHttpHandler
	{		
		// Override the ProcessRequest method.
		public void ProcessRequest(HttpContext context) {
			Assert.ArgumentNotNull(context, "context");
			if (!this.DoProcessRequest(context)) {
				context.Response.StatusCode = 0x194;
				context.Response.ContentType = "text/html";
			}
		}
		
		// Methods
		private bool DoProcessRequest(HttpContext context) {
			Assert.ArgumentNotNull(context, "context");
			
			//return DoProcessRequest(context, request, media);
			if (context != null) {
				//get video from querystring
				string videoid = context.Request.QueryString.Get("video");
				string bcVideoID = "0";
				if (videoid != null && videoid.Length.Equals(32)) {
					Item video = Sitecore.Context.Database.Items.GetItem(ShortID.Parse(videoid).ToID());
					Video v = new Video(video);
					bcVideoID = v.VideoID.ToString();
				}

				//get player from querystring
				string playerid = context.Request.QueryString.Get("player");
				if (playerid != null && playerid.Length.Equals(32)) {
					try {
						Item player = Sitecore.Context.Database.Items.GetItem(ShortID.Parse(playerid).ToID());
						Player p = new Player(player);

						//get the playlist ids from the querystring
						string[] playlistids = context.Request.QueryString.Get("playlists").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
						StringBuilder listIDs = new StringBuilder();
						//try to get the brightcove playlist id
						foreach (string id in playlistids) {
							Item list = Sitecore.Context.Database.Items.GetItem(ShortID.Parse(id).ToID());
							if (list != null) {
								Playlist pl = new Playlist(list);
								if (listIDs.Length > 0) {
									listIDs.Append(",");
								}
								listIDs.Append(pl.PlaylistID.ToString());
							}
						}

						//get the playlists from the querystring
						string listTab = "";
						string listCombo = "";
						string listSingle = "";

						switch (p.PlaylistType) {
							case PlayerPlaylistType.VideoList:
								listSingle = listIDs.ToString();
								break;
							case PlayerPlaylistType.Tabbed:
								listTab = listIDs.ToString();
								break;
							case PlayerPlaylistType.ComboBox:
								listCombo = listIDs.ToString();
								break;
						}

						context.Response.Write("<html><head>");
						context.Response.Write("<script type=\"text/javascript\" src=\"http://admin.brightcove.com/js/BrightcoveExperiences.js\"></script>");
						context.Response.Write("<script type=\"text/javascript\" src=\"http://admin.brightcove.com/js/APIModules_all.js\"></script>");
						context.Response.Write("<script type=\"text/javascript\" src=\"" + Page.ClientScript.GetWebResourceUrl(BrightcoveSDK.ActionType.READ.GetType(), "BrightcoveSDK.UI.Resources.AddRemovePlayer.js") + "\"></script>");
						context.Response.Write("</head><body onload=\"addPlayer(" + bcVideoID + ", " + p.PlayerID + ", '" + p.Name + "', false, 'None', " + p.Width + ", " + p.Height + ", true, 'transparent', 'bcvideo', '" + listTab + "', '" + listCombo + "', '" + listSingle + "');\">");
						context.Response.Write("<div id=\"bcvideo\"></div>");
						context.Response.Write("</body></html>");
					}catch{
						context.Response.Write("There was a problem loading the video. Your player may not have been set up properly");
						return false;
					}
				}
				return true;
			} else {
				return false;
			}
		}

		// Override the IsReusable property.
		public bool IsReusable {
			get { return true; }
		}
	}
}

