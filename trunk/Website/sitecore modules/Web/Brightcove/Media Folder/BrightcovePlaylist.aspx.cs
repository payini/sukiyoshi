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
	public partial class BrightcovePlaylistTab : System.Web.UI.Page
	{
		protected PlaylistItem currentItem;
		protected Database currentDB;
		protected AccountItem accountItem;
		protected BrightcoveConfig config;
		protected BCAPI bc;

		protected void Page_Load(object sender, EventArgs e) {

			try {
				//get the current item and database
				currentDB = Sitecore.Configuration.Factory.GetDatabase("master");
				currentItem = new PlaylistItem(currentDB.Items[HttpContext.Current.Request.QueryString["id"].ToString()]);
				Item acct = currentItem.playlistItem.Parent.Parent;
				//if current parent isn't the account then it's the parent of the folder
				if (!acct.TemplateName.Equals("Account Folder")) {
					acct = acct.Parent;
				}
				accountItem = new AccountItem(acct.ID, acct.InnerData, acct.Database);
				bc = new BCAPI(accountItem.PublisherID);
				
				if (!IsPostBack) {
					loadFormWithCurrentValues();

					//load non-editable information
					ltlPlaylistID.Text = currentItem.PlaylistID.ToString();
					ltlVideos.Text = currentItem.VideoIds.Count.ToString(); 
					ltlAccount.Text = currentItem.AccountId.ToString();
				}
			}
			catch (Exception ex) {
				ltlError.Text = ex.ToString();
			}
		}

		protected void loadFormWithCurrentValues() {

			//set form fields with current item data
			plPlaylist.PlaylistName = currentItem.PlaylistName;
			plPlaylist.PlaylistType = currentItem.PlaylistType;
			plPlaylist.ShortDescription = currentItem.ShortDescription;
			plPlaylist.FilterTags = currentItem.FilterTags;
			plPlaylist.ReferenceID = currentItem.ReferenceID;
		}

		protected void btnSave_Click(object sender, EventArgs e) {

			//remove access filter
			using (new Sitecore.SecurityModel.SecurityDisabler()) {
				using (new EditContext(currentItem.playlistItem, true, false)) {
					
					//set form fields with current item data
					currentItem.PlaylistName = plPlaylist.PlaylistName;
					currentItem.PlaylistType = plPlaylist.PlaylistType;
					currentItem.ShortDescription = plPlaylist.ShortDescription;
					currentItem.FilterTags = plPlaylist.FilterTags;
					currentItem.ReferenceID = plPlaylist.ReferenceID;
				}
			}

			BCPlaylist p = plPlaylist.GetBCPlaylist();
			p.id = currentItem.PlaylistID;
			RPCResponse<BCPlaylist> rpcr = bc.UpdatePlaylist(p);

			pnlSaveMessage.Visible = true;

			if (rpcr.error.message != null) {
				ltlSaveMessage.Text = rpcr.error.code + ": " + rpcr.error.message;
			}
			else {
				ltlSaveMessage.Text = "The playlist settings have been saved to Brightcove Successfully with name: " + rpcr.result.name;
			}
		}
		
		protected void btnDelete_Click(object sender, EventArgs e) {

			pnlDeleteMessage.Visible = true;
				
			try {
				RPCResponse rpcr = bc.DeletePlaylist(currentItem.PlaylistID);
				currentItem.playlistItem.Delete();

				if (rpcr.error.message != null) {
					ltlDeleteMessage.Text = rpcr.error.code + ": " + rpcr.error.message;
				}
				else {
					ltlDeleteMessage.Text = "This playlist has been removed from Brightcove and Sitecore Successfully.<br/>Please Refresh your content tree";
				}
			}
			catch(Exception ex){
				ltlDeleteMessage.Text = "There was a problem removing this playlist: " + ex.ToString();
			}
		}

		/// <summary>
		/// This handles syncing the local info with brightcove 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnUpdate_Click(object sender, EventArgs e) {

			pnlUpdateMessage.Visible = true;
			
			//import/update the playlists

			BCPlaylist b = bc.FindPlaylistById(currentItem.PlaylistID);
			UpdateInsertPair<PlaylistItem> playUIP = accountItem.ImportToSitecore(b, UpdateType.UPDATE);
			
			//show message over how many things changed
			if (playUIP.UpdatedItems.Count > 0) {
				loadFormWithCurrentValues();
				ltlUpdateMessage.Text = "The settings have been updated from Brightcove.";
			}
			else {
				ltlUpdateMessage.Text = "There was a problem updating this playlist.";
			}
		}
	}
}
