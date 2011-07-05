using System;
using System.Linq;
using System.Configuration;
using System.Web;
using BrightcoveSDK.SitecoreUtil;
using Sitecore.Data;
using BrightcoveSDK;
using Sitecore.Data.Items;
using BrightcoveSDK.Media;
using BrightcoveSDK.JSON;
using BrightcoveSDK.SitecoreUtil.Extensions;
using BrightcoveSDK.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using BrightcoveSDK.SitecoreUtil.Entity;


namespace BrightcoveSCUtil.EditorTabs
{
	public partial class BrightcoveVideoPreviewTab : System.Web.UI.Page
	{
		protected PlayerItem currentItem;
		protected Database currentDB;
		protected AccountItem accountItem;
		protected BCAPI bc;

		protected void Page_Load(object sender, EventArgs e) {

			//available querystring values for context info (id, language, version, database)

			try {
				//get the current item and database
				currentDB = Sitecore.Configuration.Factory.GetDatabase("master");
				currentItem = new PlayerItem(currentDB.Items[HttpContext.Current.Request.QueryString["id"].ToString()]);
				Item acct = currentItem.playerItem.Parent.Parent;
				//if current parent isn't the account then it's the parent of the folder
				if (!acct.TemplateName.Equals("Account Folder")) {
					acct = acct.Parent;
				}
				accountItem = new AccountItem(acct.ID, acct.InnerData, acct.Database);
				bc = new BCAPI(accountItem.PublisherID);
				
				//populate drop down
				if (!IsPostBack) {
					foreach (Item itm in accountItem.Parent.ChildByTemplateAndName("Settings Folder", "Settings").ChildByTemplateAndName("Enum", "WMode").GetChildren()) {
						ddlWMode.Items.Add(new ListItem(itm.DisplayName, itm.DisplayName));
					}
					//fill player drop down
					foreach (VideoItem vp in accountItem.VideoLib.Videos) {
						ddlVideo.Items.Add(new ListItem(vp.VideoName, vp.videoItem.ID.ToString()));
					}
					//fill playlist drop down
					foreach (PlaylistItem pl in accountItem.PlaylistLib.Playlists) {
						cblPlaylist.Items.Add(new ListItem(pl.PlaylistName, pl.playlistItem.ID.ToString()));
					}
					if (!currentItem.PlaylistType.Equals(PlayerPlaylistType.None) && cblPlaylist.Items.Count > 0) {
						cblPlaylist.SelectedIndex = 0;
					}

					//set the initial player up
					if (ddlVideo.Items.Count > 0) {
						SetVideoPlayer();
					}
					else {
						ltlMessage.Text = "To use the preview section you must have already defined at least one player";
					}
				}
			}
			catch(Exception ex){
				ltlError.Text = ex.ToString();
			}
		}

		/// <summary>
		/// This resets the player to the selected drop down values
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnPreview_Click(object sender, EventArgs e) {
			SetVideoPlayer();
		}

		protected void SetVideoPlayer() {

			VideoItem vp = new VideoItem(currentDB.Items[ddlVideo.SelectedValue]);
			vpPlayer.PlayerID = currentItem.PlayerID;
			vpPlayer.VideoID = vp.VideoID;
			vpPlayer.PlayerName = currentItem.Name;
			vpPlayer.Height = currentItem.Height;
			vpPlayer.Width = currentItem.Width;
			if (chkAutoStart.Checked) {
				vpPlayer.AutoStart = true;
			}
			vpPlayer.WMode = ddlWMode.SelectedValue;
						
			IEnumerable<string> selected = from li in cblPlaylist.Items.Cast<ListItem>() where li.Selected == true select li.Value;
			vpPlayer.VideoList = -1;
			vpPlayer.PlaylistCombos.Clear();
			vpPlayer.PlaylistTabs.Clear();
			vpPlayer.CssClass = "BrightcoveExperience";

			WMode wmode = WMode.Window;
			try {
				wmode = (WMode)Enum.Parse(wmode.GetType(), ddlWMode.SelectedValue, true);
			}catch{}
			
			switch (currentItem.PlaylistType) {
				case PlayerPlaylistType.VideoList:
					if (selected.Count() > 0) {
						PlaylistItem p = new PlaylistItem(currentDB.Items[selected.First()]);
						vpPlayer.VideoList = p.PlaylistID;
						pnlVideo.Visible = false;
					}
					break;
				case PlayerPlaylistType.Tabbed:
					List<long> pids = new List<long>();
					foreach (string pID in selected) {
						PlaylistItem p = new PlaylistItem(currentDB.Items[pID]);
						vpPlayer.PlaylistTabs.Add(new PlaylistTab(p.PlaylistID));
						pids.Add(p.PlaylistID);
					}
					pnlVideo.Visible = false;
					break;
				case PlayerPlaylistType.ComboBox:
					List<long> cpids = new List<long>();
					foreach (string pID in selected) {
						PlaylistItem p = new PlaylistItem(currentDB.Items[pID]);
						vpPlayer.PlaylistCombos.Add(new PlaylistCombo(p.PlaylistID));
						cpids.Add(p.PlaylistID);
					}
					pnlVideo.Visible = false;
					break;
				case PlayerPlaylistType.None:
					pnlPlaylist.Visible = false;
					break;
			}
		}
	}
}
