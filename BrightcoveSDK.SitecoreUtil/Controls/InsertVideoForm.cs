using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using Sitecore.Web.UI.Sheer;
using Sitecore.Text;
using Sitecore;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Shell.Framework;
using Sitecore.Web.UI.Pages;
using Sitecore.Web;
using Sitecore.Data;
using Sitecore.IO;
using Sitecore.Resources.Media;
using Sitecore.Resources;
using System.Web.UI;
using Sitecore.Shell;
using System.IO;
using System.Collections.Specialized;
using System.Web;
using System.Drawing;
using Sitecore.Configuration;
using Sitecore.Web.UI.WebControls;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Links;

namespace BrightcoveSDK.SitecoreUtil.Controls
{
	public class InsertVideoForm : DialogForm
	{
		// Fields
		protected DataContext VideoDataContext;
		protected DataContext PlayerDataContext;
		protected DataContext PlaylistDataContext;
		protected TreePicker VideoTreeview;
		protected TreePicker PlayerTreeview;
		protected TreeviewEx PlaylistTreeview;
		protected Scrollbox SelectedList;
		protected Checkbox chkAutoStart;
		protected Edit txtBGColor;
		protected Combobox WMode;
		
		//TreePicker = DropTree
		//Combobox = DropList
		//Listview = folder explorer
		//Taskbox = kind of looks like the workbox with list expansion header

		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLoad(EventArgs e) {
			Assert.ArgumentNotNull(e, "e");
			base.OnLoad(e);
			if (!Context.ClientPage.IsEvent) {
				this.Mode = WebUtil.GetQueryString("mo");
				
				VideoDataContext.GetFromQueryString();
				PlayerDataContext.GetFromQueryString();
				PlaylistDataContext.GetFromQueryString();
				
				//populate video from querystring
				string vidID = WebUtil.GetQueryString("video");
				if (vidID.Length.Equals(32)) {
					try {
						VideoDataContext.Folder = ShortID.Parse(vidID).ToID().ToString();
					}catch{}
				}
				
				//populate player from querystring
				string playID = WebUtil.GetQueryString("player");
				if (playID.Length.Equals(32)) {
					Item b = Sitecore.Client.ContentDatabase.Items[ShortID.Parse(playID).ToID()];
					txtBGColor.Value += b.DisplayName;
					try {
						PlayerDataContext.Folder = ShortID.Parse(playID).ToID().ToString();
					} catch { }
				}
								
				//populate playlists from querystring
				string[] listIDs = WebUtil.GetQueryString("playlists").Split(new string[] {","}, StringSplitOptions.RemoveEmptyEntries);
				foreach (string listID in listIDs) {
					if (listID.Length.Equals(32)) {
						try {
							//set the folder so it's opened
							PlaylistDataContext.Folder = ShortID.Parse(listID).ToID().ToString();
							//set selected items
							PlaylistTreeview.SelectedIDs.Add(listID);	
						} catch { }
					}
				}

				//setup the drop list of wmode
				Item wmodeRoot = Sitecore.Client.ContentDatabase.Items[PlayerDataContext.Root + "/Settings/WMode"];
				string wmode = WebUtil.GetQueryString("wmode");
				if (wmodeRoot != null) {
					foreach (Item wmodeItem in wmodeRoot.Children) {
						ListItem listitem = new ListItem();
						listitem.Header = wmodeItem.Name;
						listitem.Value = wmodeItem.ID.ToString();
						listitem.ID = Sitecore.Web.UI.HtmlControls.Control.GetUniqueID("I");
						listitem.Selected = (wmodeItem.DisplayName.ToLower().Equals(wmode.ToLower())) ? true : false;
						WMode.Controls.Add(listitem);
					}
				}

				//get and set the autostart
				string autostart = WebUtil.GetQueryString("autostart");
				try {
					chkAutoStart.Checked = (autostart == "") ? false : bool.Parse(autostart);
				}catch{}

				//get and set the bgcolor
				string bgcolor = WebUtil.GetQueryString("bgcolor");
				try {
					txtBGColor.Value = (bgcolor == "") ? "#ffffff" : bgcolor;
				} catch { txtBGColor.Value = "#ffffff"; }
			}
		}

		/// <summary>
		/// this makes sure you've selected an item from the video treeview
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected override void OnOK(object sender, EventArgs args) {
			Assert.ArgumentNotNull(sender, "sender");
			Assert.ArgumentNotNull(args, "args");

			//get the selected player
			Item player = Client.ContentDatabase.Items[PlayerTreeview.Value];
			if (player == null || !player.TemplateName.Equals("Brightcove Video Player")) {
				SheerResponse.Alert("Select a player.", new string[0]);
				return;
			}

			//get the selected video
			Item video = Client.ContentDatabase.Items[VideoTreeview.Value];
			string videoid = "";
			if (video != null && video.TemplateName.Equals("Brightcove Video")) {
				videoid = video.ID.ToShortID().ToString();
			}
			
			//get the selected playlists
			Item[] playlists = this.PlaylistTreeview.GetSelectedItems();
			Player vpl = new Player(player);

			//set the playlists
			StringBuilder playlistStr = new StringBuilder();
			int plistCount = 0;
			foreach (Item p in playlists) {
				if (p.TemplateName.Equals("Brightcove Playlist")) {
					if (playlistStr.Length > 0) {
						playlistStr.Append(",");
					}
					playlistStr.Append(p.ID.ToShortID().ToString());
					plistCount++;
				}
			}

			//check if the player can handle the playlists selected
			if (vpl.PlaylistType.Equals(PlayerPlaylistType.None) && plistCount > 0) {
				SheerResponse.Alert("This player does not support playlists.\nTo deselect, select the Brightcove Media item.", new string[0]);
				return;
			} else if (vpl.PlaylistType.Equals(PlayerPlaylistType.VideoList) && plistCount > 1) {
				SheerResponse.Alert("This player only supports one playlist.", new string[0]);
				return;
			} else if ((vpl.PlaylistType.Equals(PlayerPlaylistType.VideoList) ||
				vpl.PlaylistType.Equals(PlayerPlaylistType.ComboBox) ||
				vpl.PlaylistType.Equals(PlayerPlaylistType.Tabbed)) && !videoid.Equals("")) {
				SheerResponse.Alert("This player does not support videos. \nTo deselect, select the Brightcove Media item.", new string[0]);
				return;
			}

			//use settings to determine what kind of modal window to use like thickbox or prettyphoto
			//id = {3EE8D1E1-1421-4546-8127-4D576FB8DA5F}
			Item settings = Client.ContentDatabase.Items["/sitecore/system/Modules/Brightcove Settings/Modal Link Settings"];
			StringBuilder sbAttr = new StringBuilder();
			StringBuilder sbQstring = new StringBuilder();
			if (settings != null) {
				foreach(Item child in settings.GetChildren()){
					if(child.TemplateName.Equals("Link Attribute")){
						sbAttr.Append(" " + child["Key"] + "=\"" + child["Value"] + "\"");
					}
					else if(child.TemplateName.Equals("Link Querystring")){
						sbQstring.Append("&" + child["Key"] + "=" + child["Value"]);
					}
				}
			}
			
			//selected text is the link text
			string selectedText = HttpUtility.UrlDecode(WebUtil.GetQueryString("selectedText"));
			if (selectedText.Contains("href=")) {
				selectedText = selectedText.Split('>')[1];
				selectedText = selectedText.Split('<')[0];
			}
			if (selectedText.Equals("")) {
				Video vd = new Video(video);
				selectedText = "Click To Watch " + vd.VideoName;
			}
			
			//build link then send it back
			int height = int.Parse(player.Fields["Height"].Value) + 20;
			int width = int.Parse(player.Fields["Width"].Value) + 20;
			StringBuilder mediaUrl = new StringBuilder();
			mediaUrl.Append("<a href=\"/BrightcoveVideo.ashx?video=" + videoid + "&player=" + player.ID.ToShortID());
			mediaUrl.Append("&playlists=" + playlistStr.ToString() + "&autoStart=" + chkAutoStart.Checked.ToString().ToLower() + "&bgcolor=" + txtBGColor.Value + "&wmode=" + WMode.SelectedItem.Header);
			mediaUrl.Append(sbQstring.ToString());
			mediaUrl.Append("&height=" + height + "&width=" + width + "\"" + sbAttr.ToString() + ">" + selectedText + "</a>");
						
			if (this.Mode == "webedit") {
				SheerResponse.SetDialogValue(StringUtil.EscapeJavascriptString(mediaUrl.ToString()));
				base.OnOK(sender, args);
			} else {
				SheerResponse.Eval("scClose(" + StringUtil.EscapeJavascriptString(mediaUrl.ToString()) + "," + StringUtil.EscapeJavascriptString(player.DisplayName) + ")");
			}
		}

		// Properties
		protected string Mode {
			get {
				string str = StringUtil.GetString(base.ServerProperties["Mode"]);
				if (!string.IsNullOrEmpty(str)) {
					return str;
				}
				return "shell";
			}
			set {
				Assert.ArgumentNotNull(value, "value");
				base.ServerProperties["Mode"] = value;
			}
		}
	}
}
