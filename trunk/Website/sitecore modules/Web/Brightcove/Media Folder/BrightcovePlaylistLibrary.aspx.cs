using System;
using System.Configuration;
using System.Web;
using Sitecore.Data.Items;
using Sitecore.Data;
using BrightcoveSDK.SitecoreUtil;
using BrightcoveSDK.SitecoreUtil.Extensions;
using BrightcoveSDK;
using System.Collections.Generic;
using BrightcoveSDK.Media;
using BrightcoveSDK.JSON;
using BrightcoveSDK.SitecoreUtil.Entity;
using BrightcoveSDK.SitecoreUtil.Entity.Container;

namespace BrightcoveSCUtil.EditorTabs
{
	public partial class BrightcovePlaylistLibraryTab : System.Web.UI.Page
	{
		protected Item currentItem;
		protected Database currentDB;
		protected AccountItem accountItem;
		protected BCAPI bc;

		protected void Page_Load(object sender, EventArgs e) {

			//available querystring values for context info (id, language, version, database)
			//disable create
			pnlCreate.Visible = false;
			try {
				//get the current item and database
				currentDB = Sitecore.Configuration.Factory.GetDatabase("master");
				currentItem = currentDB.Items[HttpContext.Current.Request.QueryString["id"].ToString()];
				accountItem = new AccountItem(currentItem.Parent.ID, currentItem.Parent.InnerData, currentItem.Parent.Database);
				bc = new BCAPI(accountItem.PublisherID);
				
				//display the current video and playlist count
				ltlTotalPlaylists.Text = accountItem.PlaylistLib.Playlists.Count.ToString();
			}
			catch (Exception ex) {
				ltlError.Text = ex.ToString();
			}
		}

		protected void btnSubmit_Click(object sender, EventArgs e) {

			//build bcvideo
			BCPlaylist plist = plPlaylist.GetBCPlaylist();
			
			//upload the video
			RPCResponse<long> rpcr = bc.CreatePlaylist(plist);

			if (rpcr.error.message != null) {
				ltlMessage.Text = rpcr.error.code + ": " + rpcr.error.message;
			}
			else {
				ltlMessage.Text = "Playlist Created Successfully with ID: " + rpcr.result.ToString();

				plist.id = rpcr.result;
				UpdateInsertPair<PlaylistItem> a = accountItem.ImportToSitecore(plist, UpdateType.NEW);
			}

			//blank out the other fields after upload
			plPlaylist.ClearForm();

			//display the current video and playlist count
			ltlTotalPlaylists.Text = accountItem.PlaylistLib.Playlists.Count.ToString();
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
			if(radUpdate.SelectedValue.Equals("update")){
				utype = UpdateType.UPDATE;
			}
			else if (radUpdate.SelectedValue.Equals("both")) {
				utype = UpdateType.BOTH;
			}
			
			//import/update the playlists
			List<BCPlaylist> playlists = bc.FindAllPlaylists().Playlists;
			UpdateInsertPair<PlaylistItem> playUIP = accountItem.ImportToSitecore(playlists, utype);
			
			if(utype.Equals(UpdateType.BOTH) || utype.Equals(UpdateType.NEW)){
				pnlNewMessage.Visible = true;
				ltlNewPlaylists.Text = playUIP.NewItems.Count.ToString();
			}
			if (utype.Equals(UpdateType.BOTH) || utype.Equals(UpdateType.UPDATE)) {
				//show message over how many things changed
				pnlUpdateMessage.Visible = true;
				ltlUpdatedPlaylists.Text = playUIP.UpdatedItems.Count.ToString();
			}
						
			//display the current video and playlist count
			ltlTotalPlaylists.Text = accountItem.PlaylistLib.Playlists.Count.ToString();
		}
	}
}
