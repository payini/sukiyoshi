using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using BrightcoveSDK;
using System.Collections.Generic;
using BrightcoveSDK.Media;
using BrightcoveSDK.SitecoreUtil;
using BrightcoveSDK.SitecoreUtil.Extensions;
using Sitecore.Data.Items;
using Sitecore.Data;
using BrightcoveSDK.SitecoreUtil.Entity;
using BrightcoveSDK.SitecoreUtil.Entity.Container;

namespace BrightcoveSCUtil.EditorTabs
{
	public partial class BrightcoveAccountTab : System.Web.UI.Page
	{
		protected Item currentItem;
		protected Database currentDB;
		protected AccountItem accountItem;
		protected BCAPI bc;

		protected void Page_Load(object sender, EventArgs e) {
			
			try {
				//get the current item and database
				currentDB = Sitecore.Configuration.Factory.GetDatabase("master");
				currentItem = currentDB.Items[HttpContext.Current.Request.QueryString["id"].ToString()];
				accountItem = new AccountItem(currentItem.ID, currentItem.InnerData, currentItem.Database);
				bc = new BCAPI(accountItem.PublisherID);
							
				//display the current video and playlist count
				ltlTotalPlaylists.Text = accountItem.PlaylistLib.Playlists.Count.ToString();
				ltlTotalVideos.Text = accountItem.VideoLib.Videos.Count.ToString();
			}
			catch(Exception ex){
				ltlError.Text = "There was an error loading this page. You probably need to set the publisher Id<br/><br/>";
				ltlError.Text += ex.ToString();
			}
		}

		/// <summary>
		/// This handles syncing the local info with brightcove 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnUpdate_Click(object sender, EventArgs e) {

			pnlNewMessage.Visible = false;
			pnlUpdateMessage.Visible = false;

			UpdateType utype = UpdateType.NEW;
			if (radUpdate.SelectedValue.Equals("update")) {
				utype = UpdateType.UPDATE;
			}
			else if (radUpdate.SelectedValue.Equals("both")) {
				utype = UpdateType.BOTH;
			}

			//import/update the videos
			List<BCVideo> videos = bc.FindAllVideos().Videos;
			UpdateInsertPair<VideoItem> vidUIP = accountItem.ImportToSitecore(videos, utype);

			//import/update the playlists
			List<BCPlaylist> playlists = bc.FindAllPlaylists().Playlists;
			UpdateInsertPair<PlaylistItem> playUIP = accountItem.ImportToSitecore(playlists, utype);

			if (utype.Equals(UpdateType.BOTH) || utype.Equals(UpdateType.NEW)) {
				//show message over how many things changed
				pnlNewMessage.Visible = true;
				ltlNewItem.Text = vidUIP.NewItems.Count.ToString();
				ltlNewPlaylists.Text = playUIP.NewItems.Count.ToString();
			}
			if (utype.Equals(UpdateType.BOTH) || utype.Equals(UpdateType.UPDATE)) {
				pnlUpdateMessage.Visible = true;
				ltlUpdatedItems.Text = vidUIP.UpdatedItems.Count.ToString();
				ltlUpdatedPlaylists.Text = playUIP.UpdatedItems.Count.ToString();
			}

			//display the current video and playlist count
			ltlTotalPlaylists.Text = accountItem.PlaylistLib.Playlists.Count.ToString();
			ltlTotalVideos.Text = accountItem.VideoLib.Videos.Count.ToString();
		}
	}
}
