using System;
using System.Configuration;
using System.Web;
using BrightcoveSDK.SitecoreUtil;
using Sitecore.Data;
using BrightcoveSDK;
using Sitecore.Data.Items;
using BrightcoveSDK.Media;
using BrightcoveSDK.JSON;
using BrightcoveSDK.SitecoreUtil.Extensions;
using BrightcoveSDK.SitecoreUtil.Entity;
using BrightcoveSDK.SitecoreUtil.Entity.Container;

namespace BrightcoveSCUtil.EditorTabs
{
	public partial class BrightcoveVideoTab : System.Web.UI.Page
	{
		protected VideoItem currentItem;
		protected Database currentDB;
		protected AccountItem accountItem;
		protected BCAPI bc;

		protected void Page_Load(object sender, EventArgs e) {
			
			//available querystring values for context info (id, language, version, database)

			try {
				//get the current item and database
				currentDB = Sitecore.Configuration.Factory.GetDatabase("master");
				currentItem = new VideoItem(currentDB.Items[HttpContext.Current.Request.QueryString["id"].ToString()]);
				Item acct = currentItem.videoItem.Parent.Parent;
				//if current parent isn't the account then it's the parent of the folder
				if (!acct.TemplateName.Equals("Account Folder")) {
					acct = acct.Parent;
				}
				accountItem = new AccountItem(acct.ID, acct.InnerData, acct.Database);
				bc = new BCAPI(accountItem.PublisherID);
				
				//set the form values for the video
				if (!IsPostBack) {
					loadFormWithCurrentValues();
				}

				//show the video id
				ltlVideoID.Text = currentItem.VideoID.ToString();
				
				//show the video upload status
				RPCResponse<UploadStatusEnum> rpcr = bc.GetUploadStatus(currentItem.VideoID);
				ltlStatus.Text = rpcr.result.ToString();

				ltlCreation.Text = currentItem.CreationDate.ToString("MMMM d, yyyy");
				ltlModified.Text = currentItem.LastModifiedDate.ToString("MMMM d, yyyy");
				ltlPublished.Text = currentItem.PublishedDate.ToString("MMMM d, yyyy");
				try {
					long milliseconds = currentItem.Length;
					string lengthText = "";
					long hours = (milliseconds / 60000000);
					if (hours >= 1) {
						milliseconds -= hours * 60000000;
						lengthText += hours + " hours, ";
					}
					long mins = (milliseconds / 60000);
					if (mins >= 1) {
						milliseconds -= mins * 60000;
						lengthText += mins + " minutes, ";
					}
					long secs = (milliseconds / 1000);
					if (secs >= 1) {
						milliseconds -= secs * 1000;
						lengthText += secs + " seconds";
					}
					ltlLength.Text = lengthText;
				}
				catch(Exception ex){
				}
				ltlPlays.Text = currentItem.PlaysTotal.ToString();
				ltlPlaysTrailing.Text = currentItem.PlaysTrailingWeek.ToString();
			}
			catch (Exception ex) {
				ltlError.Text = ex.ToString();
			}
		}

		protected void loadFormWithCurrentValues() {

			uvVideo.VideoName = currentItem.VideoName;
			uvVideo.ShortDescription = currentItem.ShortDescription;
			uvVideo.LongDescription = currentItem.LongDescription;
			uvVideo.Tags = currentItem.Tags;
			uvVideo.ReferenceID = currentItem.ReferenceID;
			uvVideo.Economics = currentItem.Economics;
		}

		protected void btnSave_Click(object sender, EventArgs e) {

			//remove access filter
			using (new Sitecore.SecurityModel.SecurityDisabler()) {
				using (new EditContext(currentItem.videoItem, true, false)) {

					currentItem.VideoName = uvVideo.VideoName;
					currentItem.ShortDescription = uvVideo.ShortDescription;
					currentItem.LongDescription = uvVideo.LongDescription;
					currentItem.Tags = uvVideo.Tags;
					currentItem.ReferenceID = uvVideo.ReferenceID;
					currentItem.Economics = uvVideo.Economics;
				}
			}

			BCVideo v = uvVideo.GetBCVideo();
			v.id = currentItem.VideoID;
			RPCResponse<BCVideo> rpcr = bc.UpdateVideo(v);

			pnlSaveMessage.Visible = true;

			if (rpcr.error.message != null) {
				ltlSaveMessage.Text = rpcr.error.code + ": " + rpcr.error.message;
			}
			else {
				ltlSaveMessage.Text = "The video settings have been saved to Brightcove Successfully with name: " + rpcr.result.name;
			}
		}

		protected void btnDelete_Click(object sender, EventArgs e) {

			pnlDeleteMessage.Visible = true;

			try {
				RPCResponse rpcr = bc.DeleteVideo(currentItem.VideoID);
				currentItem.videoItem.Delete();

				if (rpcr.error.message != null) {
					ltlDeleteMessage.Text = rpcr.error.code + ": " + rpcr.error.message;
				}
				else {
					ltlDeleteMessage.Text = "This video has been removed from Brightcove and Sitecore Successfully.<br/>Please Refresh your content tree";
				}
			}
			catch (Exception ex) {
				ltlDeleteMessage.Text = "There was a problem removing this video: " + ex.ToString();
			}
		}

		/// <summary>
		/// This handles syncing the local info with brightcove 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void btnUpdate_Click(object sender, EventArgs e) {

			pnlUpdateMessage.Visible = true;

			//import/update the video

			BCVideo v = bc.FindVideoById(currentItem.VideoID);
			UpdateInsertPair<VideoItem> vidUIP = accountItem.ImportToSitecore(v, UpdateType.UPDATE);

			//show message over how many things changed
			if (vidUIP.UpdatedItems.Count > 0) {
				loadFormWithCurrentValues();
				ltlUpdateMessage.Text = "The settings have been updated from Brightcove.";
			}
			else {
				ltlUpdateMessage.Text = "There was a problem updating this video.";
			}
		}
	}
}
