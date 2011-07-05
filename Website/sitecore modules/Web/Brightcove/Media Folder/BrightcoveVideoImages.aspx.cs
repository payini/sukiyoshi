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
using BrightcoveSDK.SitecoreUtil;
using Sitecore.Data;
using BrightcoveSDK;
using Sitecore.Data.Items;
using BrightcoveSDK.Media;
using BrightcoveSDK.JSON;
using BrightcoveSDK.SitecoreUtil.Extensions;
using BrightcoveSDK.SitecoreUtil.Entity;

namespace BrightcoveSCUtil.EditorTabs
{
	public partial class BrightcoveVideoImagesTab : System.Web.UI.Page
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

				if (!currentItem.ThumbnailURL.Equals("")) {
					pnlThumb.Visible = true;
					imgThumb.ImageUrl = currentItem.ThumbnailURL;
				}
				if (!currentItem.VideoStillURL.Equals("")) {
					pnlStill.Visible = true;
					imgStill.ImageUrl = currentItem.VideoStillURL;
				}
			}
			catch (Exception ex) {
				ltlError.Text = ex.ToString();
			}
		}

		protected void btnAddImage_Click(object sender, EventArgs e) {

			BCImage newImg = iaAdd.GetBCImage();
			RPCResponse<BCImage> rpcr = bc.AddImage(newImg, iaAdd.FileName, iaAdd.FileBytes, currentItem.VideoID, iaAdd.Resize);

			if (rpcr.error.message != null) {
				pnlImageMessage.Visible = true;
				ltlImageMessage.Text = rpcr.error.code + ": " + rpcr.error.message;
			}
			else {
				pnlImageMessage.Visible = true;
				ltlImageMessage.Text = "The image has been uploaded to Brightcove.";
			}

			iaAdd.ClearForm();
		}
	}
}
