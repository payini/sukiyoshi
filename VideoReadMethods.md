#These are samples of how to use the Video Read Methods

# Details #

```

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using BrightcoveSDK;
using BrightcoveSDK.Media;
using BrightcoveSDK.JSON;
using BrightcoveSDK.Containers;
using BrightcoveSDK.Entities.Containers;

namespace testsite.BC {
	public partial class VideoReadMethods : System.Web.UI.Page {

	public BCAPI bc;

	protected void Page_Load(object sender, EventArgs e) {

		BrightcoveConfig config = (BrightcoveConfig)ConfigurationManager.GetSection("brightcove");
		bc = new BCAPI(config.Accounts[0].PublisherID);

	}
	protected void FindAllVideos() {
        	BCQueryResult result = bc.FindAllVideos(BCSortByType.PUBLISH_DATE);
	        int i = 0;
        	foreach (QueryResultPair qrp in result.QueryResults) {
                	Response.Write(result.QueryResults[i].JsonResult);
                	i++;
        	}
	}

	protected void FindVideosByUserId(){
		BCQueryResult result = bc.FindVideosByUserId(8385540001);
		int i = 0;
		foreach (QueryResultPair qrp in result.QueryResults) {
			Response.Write(result.QueryResults[i].JsonResult);
			i++;
		}
	}

	protected void FindVideosByTags() {
		List<String> l = new List<string>();
		l.Add("something");
		l.Add("funny");

		BCQueryResult result = bc.FindVideosByTags(l, null);
		int i = 0;
		foreach (QueryResultPair qrp in result.QueryResults) {
			Response.Write(result.QueryResults[i].Query);
			Response.Write(result.QueryResults[i].JsonResult);
			i++;
		}
	}

	protected void FindVideosByText() {
		BCQueryResult result = bc.FindVideosByText("lic");
		int i = 0;
		foreach (QueryResultPair qrp in result.QueryResults) {
			Response.Write(result.QueryResults[i].Query);
			Response.Write(result.QueryResults[i].JsonResult);
			i++;
		}
	}

	protected void FindVideosByCampaignId() {
		BCQueryResult result = bc.FindVideosByCampaignId(30865112001);
		int i = 0;
		foreach (QueryResultPair qrp in result.QueryResults) {
		    Response.Write(result.QueryResults[i].Query);
		    Response.Write(result.QueryResults[i].JsonResult);
		    i++;
		}
	}

	protected void FindRelatedVideos(){
		BCQueryResult result = bc.FindRelatedVideos(30865112001);
		int i = 0;
		foreach (QueryResultPair qrp in result.QueryResults) {
			Response.Write(result.QueryResults[i].Query);
			Response.Write(result.QueryResults[i].JsonResult);
			i++;
		}
	}

	protected void FindVideosByIds(){
		List<long> ids = new List<long>();
		ids.Add(30865112001);
		ids.Add(31267231001);
		ids.Add(31270419001);
		BCQueryResult result = bc.FindVideosByIds(ids);
		int i = 0;
		foreach (QueryResultPair qrp in result.QueryResults) {
			Response.Write(result.QueryResults[i].Query);
			Response.Write(result.QueryResults[i].JsonResult);
			i++;
		}
	}

	protected void FindVideoByReferenceId() {
		BCVideo result = bc.FindVideoByReferenceId("nightnight");
		Response.Write(result.id);
	}

	protected void FindVideosByReferenceIds(){
		List<string> ids = new List<string>();
		ids.Add("nightnight");
		ids.Add("asd");
		BCQueryResult result = bc.FindVideosByReferenceIds(ids);
		int i = 0;
		foreach (QueryResultPair qrp in result.QueryResults) {
			Response.Write(result.QueryResults[i].Query);
			Response.Write(result.QueryResults[i].JsonResult);
			i++;
		}
	}

	protected void FindVideoById(){
		BCVideo result = bc.FindVideoById(30865112001);
		Response.Write(result.id);
	}
	}
}
```