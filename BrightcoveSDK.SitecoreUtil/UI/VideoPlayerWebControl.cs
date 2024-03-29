﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Sitecore.Data;
using Sitecore.Data.Items;
using BrightcoveSDK.SitecoreUtil.Extensions;
using Sitecore.Security.Accounts;
using BrightcoveSDK.SitecoreUtil.Entity;

namespace BrightcoveSDK.SitecoreUtil.UI
{
	public class VideoPlayerWebControl : Sitecore.Web.UI.WebControl
	{
		protected override void DoRender(HtmlTextWriter output) {
			Database db = Sitecore.Context.Database;
			StringBuilder sbOut = new StringBuilder();
			StringBuilder sbError = new StringBuilder();
			
			//get the player
			long playerid = -1;
			//check to see if the guid is there
			if(long.TryParse(this.Attributes["player"], out playerid)){
            	//get player obj
				PlayerItem p = PlayerLibraryItem.GetPlayer(playerid);
				if(p != null){		
				    //parse wmode
				    WMode wmode = WMode.Window;
				    try {
					    wmode = (WMode)Enum.Parse(wmode.GetType(), this.Attributes["wmode"], true);
				    }catch{}
    				
				    //get background color
				    string bgcolor = this.Attributes["bgcolor"];
				    bgcolor = (string.IsNullOrEmpty(bgcolor)) ? "#ffffff" : bgcolor;
					if (!bgcolor.StartsWith("#"))
						bgcolor = "#" + bgcolor;

					string oparams = this.Attributes["oparams"];
					Dictionary<string, string> objectParams = new Dictionary<string, string>();
					if (!string.IsNullOrEmpty(oparams)) {
						string[] keys = oparams.Split(',');
						foreach (string k in keys) {
							string[] pair = k.Split('=');
							if (pair.Length > 0)
								objectParams.Add(pair[0], pair[1]);
						}
					}

				    //parse autostart
				    bool autostart = false; 
				    try {
					    autostart = (this.Attributes["autostart"].Equals("true")) ? true : false;
				    }catch{}

				    //determine which embed code to display
				    if(p.PlaylistType.Equals(PlayerPlaylistType.None)){
					    //get the video id
                        long videoid = -1;
                        if(long.TryParse(this.Attributes["video"], out videoid)){
					        //try parse the id and get the item
							VideoItem v = VideoLibraryItem.GetVideo(videoid);
                            if (v != null) {
                                sbOut.Append(p.GetEmbedCode(v, bgcolor, autostart, wmode));
                            }
					    }
				    } else if(p.PlaylistType.Equals(PlayerPlaylistType.VideoList)){
					    long videolist = -1;
					    if(long.TryParse(this.Attributes["videolist"], out videolist)){
						    sbOut.Append(p.GetEmbedCode(videolist, bgcolor, autostart, wmode));
					    }
				    } else if(p.PlaylistType.Equals(PlayerPlaylistType.ComboBox) || p.PlaylistType.Equals(PlayerPlaylistType.Tabbed)){
						List<string> t = new List<string>();
						//get both the lists and build a string list
						if (p.PlaylistType.Equals(PlayerPlaylistType.ComboBox)) {
							string combo = (this.Attributes["playlistcombo"] != null) ? this.Attributes["playlistcombo"] : "";
							t.AddRange(combo.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
						} else if (p.PlaylistType.Equals(PlayerPlaylistType.Tabbed)) {
							string tabs = (this.Attributes["playlisttabs"] != null) ? this.Attributes["playlisttabs"] : "";
							t.AddRange(tabs.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
						}
					    //convert to a list of long
					    List<long> playlists = new List<long>();
					    foreach(string s in t){
						    long temp = -1;
                            if(long.TryParse(s, out temp)){
                                playlists.Add(temp);    
                            } 
					    }
						//get the embed code
						sbOut.Append(p.GetEmbedCode(playlists, bgcolor, autostart, wmode, PlayerExtensions.CreateEmbedID(), objectParams));
				    } 

				    //Notify the user
				    if(sbOut.Length < 1){
						sbError.AppendLine("Player playlist type wasn't selected.");
				    }
			    }
				else {
					sbError.AppendLine("Player doesn't exist in Sitecore.");
				}
			} else {
                sbError.AppendLine("Player value is not a long.");
            }
			//determine if it's an error or not
			if(sbError.Length > 0){
				output.WriteLine("<div style=\"display:none;\">" + sbError.ToString() + "</div>");
			}
			else {
				output.WriteLine(sbOut.ToString());
			}
		}
	}
}