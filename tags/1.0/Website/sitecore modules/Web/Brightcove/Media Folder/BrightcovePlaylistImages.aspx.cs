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
	public partial class BrightcovePlaylistImagesTab : System.Web.UI.Page
	{
		protected PlaylistItem currentItem;
		protected Database currentDB;
		protected AccountItem accountItem;
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
		
				if (!currentItem.ThumbnailURL.Equals("")) {
					pnlThumb.Visible = true;
					imgThumb.ImageUrl = currentItem.ThumbnailURL;
				}
			}
			catch (Exception ex) {
				ltlError.Text = ex.ToString();
			}
		}
	}
}
