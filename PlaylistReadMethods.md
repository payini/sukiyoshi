#These are samples to use the playlist read methods

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
	public partial class PlaylistReadMethods : System.Web.UI.Page {

		public BCAPI bc;

		protected void Page_Load(object sender, EventArgs e) {

			BrightcoveConfig config = (BrightcoveConfig)ConfigurationManager.GetSection("brightcove");
			bc = new BCAPI(config.Accounts[0].PublisherID);
		}

		protected void FindPlaylistById() {
			BCPlaylist result = bc.FindPlaylistById(30631628001);
			Response.Write(result.id);
		}

		protected void FindAllPlaylists() {
			BCQueryResult result = bc.FindAllPlaylists();
			int i = 0;
			foreach (QueryResultPair qrp in result.QueryResults) {
				Response.Write(result.QueryResults[i].JsonResult);
				i++;
			}
		}

		protected void FindPlaylistsByIds() {
			List<long> ids = new List<long>();
			ids.Add(8696101001);
			ids.Add(30631628001);
			BCQueryResult result = bc.FindPlaylistsByIds(ids);
			int i = 0;
			foreach (QueryResultPair qrp in result.QueryResults) {
				Response.Write(result.QueryResults[i].Query);
				Response.Write(result.QueryResults[i].JsonResult);
				i++;
			}
		}

		protected void FindPlaylistByReferenceId() {
			BCPlaylist result = bc.FindPlaylistByReferenceId("new list 1");
			Response.Write(result.id);
		}

		protected void FindPlaylistsByReferenceIds() {
			List<string> ids = new List<string>();
			ids.Add("new list 1");
			ids.Add("new list 2");
			BCQueryResult result = bc.FindPlaylistsByReferenceIds(ids);
			int i = 0;
			foreach (QueryResultPair qrp in result.QueryResults) {
				Response.Write(result.QueryResults[i].Query);
				Response.Write(result.QueryResults[i].JsonResult);
				i++;
			}
		}

		protected void FindPlaylistsForPlayerId() {
			BCQueryResult result = bc.FindPlaylistsForPlayerId(8670898001);
			int i = 0;
			foreach (QueryResultPair qrp in result.QueryResults) {
				Response.Write(result.QueryResults[i].Query);
				Response.Write(result.QueryResults[i].JsonResult);
				i++;
			}
		}
	}
}
```