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
				string bcVideoID = context.Request.QueryString.Get("video");
				
				//get player from querystring
				long playerid = -1;
                if (long.TryParse(context.Request.QueryString.Get("player"), out playerid)) {
                	Player p = PlayerLibrary.GetPlayer(playerid);
                    if (p != null) {
                        try {
                            //get the playlist ids from the querystring
                            string[] playlistids = context.Request.QueryString.Get("playlists").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                            StringBuilder listIDs = new StringBuilder();
                            //try to get the brightcove playlist id
                            foreach (string id in playlistids) {
                                long plist = -1;
                                if (long.TryParse(id, out plist)) {
                                    Playlist pl = PlaylistLibrary.GetPlaylist(plist);
                                    if(pl != null){
                                        if (listIDs.Length > 0) {
                                            listIDs.Append(",");
                                        }
                                        listIDs.Append(pl.PlaylistID.ToString());
                                    }
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
                            context.Response.Write("</head><body onload=\"addPlayer(" + bcVideoID + ", " + p.PlayerID + ", '" + p.Name + "', false, 'None', " + (p.Width + 20).ToString() + ", " + (p.Height + 20).ToString() + ", true, 'transparent', 'bcvideo', '" + listTab + "', '" + listCombo + "', '" + listSingle + "');\">");
                            context.Response.Write("<div id=\"bcvideo\"></div>");
                            context.Response.Write("</body></html>");
                        } catch {
                            context.Response.Write("There was a problem loading the video. Your player may not have been set up properly");
                            return false;
                        }
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

