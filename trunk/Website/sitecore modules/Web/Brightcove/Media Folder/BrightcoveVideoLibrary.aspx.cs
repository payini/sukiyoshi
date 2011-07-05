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
	public partial class BrightcoveVideoLibraryTab : System.Web.UI.Page
	{
		protected Item currentItem;
		protected Database currentDB;
		protected AccountItem accountItem;
		protected BCAPI bc;

		protected void Page_Load(object sender, EventArgs e) {

			//available querystring values for context info (id, language, version, database)
			//disabling this for now
			pnlUpload.Visible = false;
			try {
				//get the current item and database
				currentDB = Sitecore.Configuration.Factory.GetDatabase("master");
				currentItem = currentDB.Items[HttpContext.Current.Request.QueryString["id"].ToString()];
				accountItem = new AccountItem(currentItem.Parent.ID, currentItem.Parent.InnerData, currentItem.Parent.Database);
				bc = new BCAPI(accountItem.PublisherID);
				
				//display the current video and playlist count
				ltlTotalVideos.Text = accountItem.VideoLib.Videos.Count.ToString();
			}
			catch (Exception ex) {
				ltlError.Text = ex.ToString();
			}
		}

		protected void btnSubmit_Click(object sender, EventArgs e) {

			//build bcvideo
			BCVideo vid = uvVideo.GetBCVideo();

			//upload the video
			RPCResponse<long> rpcr = bc.CreateVideo(vid, uvVideo.FileName, uvVideo.FileBytes);

			if (rpcr.error.message != null) {
				ltlMessage.Text = rpcr.error.code + ": " + rpcr.error.message;
			}
			else {
				ltlMessage.Text = "Video Created Successfully with ID: " + rpcr.result.ToString();
				
				vid.id = rpcr.result;
				vid.creationDate = DateTime.Now;
				vid.lastModifiedDate = DateTime.Now;
				vid.publishedDate = DateTime.Now;

				UpdateInsertPair<VideoItem> a = accountItem.ImportToSitecore(vid, UpdateType.NEW);
			}

			//blank out the other fields after upload
			uvVideo.ClearForm();

			//display the current video and playlist count
			ltlTotalVideos.Text = accountItem.VideoLib.Videos.Count.ToString();
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

			////import/update the videos
			List<BCVideo> videos = bc.FindAllVideos().Videos;
			UpdateInsertPair<VideoItem> vidUIP = accountItem.ImportToSitecore(videos, utype);
			
			if (utype.Equals(UpdateType.BOTH) || utype.Equals(UpdateType.NEW)) {
				//show message over how many things changed
				pnlNewMessage.Visible = true;
				ltlNewItem.Text = vidUIP.NewItems.Count.ToString();
			}
			if (utype.Equals(UpdateType.BOTH) || utype.Equals(UpdateType.UPDATE)) {
				//show message over how many things changed
				pnlUpdateMessage.Visible = true;
				ltlUpdatedItems.Text = vidUIP.UpdatedItems.Count.ToString();
			}

			//display the current video and playlist count
			ltlTotalVideos.Text = accountItem.VideoLib.Videos.Count.ToString();
		}
	}
}
