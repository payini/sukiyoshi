using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Sitecore.Data;
using Sitecore.Data.Items;
using BrightcoveSDK.SitecoreUtil.Extensions;
using Sitecore.Security.Accounts;

namespace BrightcoveSDK.SitecoreUtil.UI
{
	public class VideoPlayerWebControl : Sitecore.Web.UI.WebControl
	{
		protected override void DoRender(HtmlTextWriter output) {
			Database db = Sitecore.Context.Database;
			StringBuilder sbOut = new StringBuilder();
			StringBuilder sbError = new StringBuilder();
			
			//get the player
			string playerid = this.Attributes["player"];
			//check to see if the guid is there
			if(playerid != "" && playerid.Length.Equals(32)){
				try {
					//try to get the item
					Item player = db.Items[ShortID.Parse(playerid).ToID()];
					//if parse doesn't break then make sure it's not null
					if (player != null) {
						//make sure it's the right item
						if(player.TemplateName.Equals("Brightcove Video Player")){
							//get player obj
							Player p = new Player(player);
							
							//parse wmode
							WMode wmode = WMode.Window;
							try {
								wmode = (WMode)Enum.Parse(wmode.GetType(), this.Attributes["wmode"], true);
							}catch{}
							
							//get background color
							string bgcolor = this.Attributes["bgcolor"];
							bgcolor = (bgcolor == "") ? "#ffffff" : bgcolor;
							
							//parse autostart
							bool autostart = false; 
							try {
								bool.Parse(this.Attributes["autostart"]);
							}catch{}

							//determine which embed code to display
							if(p.PlaylistType.Equals(PlayerPlaylistType.None)){
								//get the video id
								string videoid = this.Attributes["video"];
								videoid = (videoid == "") ? "0" : videoid;
								if(videoid != "" && videoid.Length.Equals(32)){
									try {
										//try parse the id and get the item
										Item video = db.Items[ShortID.Parse(videoid).ToID()];
										if (video != null) {
											//get the video object and the embed code
											Video v = new Video(video);
											sbOut.AppendFormat(p.GetEmbedCode(v, bgcolor, autostart, wmode));
										}
									}catch{}
								}
							} else if(p.PlaylistType.Equals(PlayerPlaylistType.VideoList)){
								long videolist = 0;
								try {
									//try to parse the video list and get the embed code
									videolist = long.Parse(this.Attributes["videolist"]);
									sbOut.AppendFormat(p.GetEmbedCode(videolist, bgcolor, autostart, wmode));
								}catch{}						
							} else if(p.PlaylistType.Equals(PlayerPlaylistType.ComboBox) || p.PlaylistType.Equals(PlayerPlaylistType.Tabbed)){
								//get both the lists and build a string list
								string tabs = this.Attributes["playlisttabs"];
								string combo = this.Attributes["playlistcombo"];
								List<string> t = tabs.Split(new string[] {","}, StringSplitOptions.RemoveEmptyEntries).ToList();
								t.AddRange(combo.Split(new string[] {","}, StringSplitOptions.RemoveEmptyEntries).ToList());

								//convert to a list of long
								List<long> playlists = new List<long>();
								foreach(string s in t){
									try {
										playlists.Add(long.Parse(s));
									} catch { }
								}

								//get the embed code
								sbOut.AppendFormat(p.GetEmbedCode(playlists, bgcolor, autostart, wmode));
							} 

							//if nothing then just get embed for player with nothing
							if(sbOut.Length.Equals(0)){
								sbOut.Append(p.GetEmbedCode(bgcolor, autostart, wmode));	
							}
						}
						else {
							sbError.AppendLine("Item in player field was not a Brightove Video Player.");
						}
					} else {
						sbError.AppendLine("Null player item.");
					}
				} catch {
					sbError.AppendLine("Player ID was in an invalid format.");
				}
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