#These are the samples to use the playlist write methods

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
	public partial class PlaylistWriteMethods : System.Web.UI.Page {

		public BCAPI bc;

		protected void Page_Load(object sender, EventArgs e) {
			BrightcoveConfig config = (BrightcoveConfig)ConfigurationManager.GetSection("brightcove");
			bc = new BCAPI(config.Accounts[0].PublisherID);
		}

		protected void CreatePlaylist() {

			BCPlaylist p = new BCPlaylist();
			p.name = "testing create list";
			p.playlistType = PlaylistTypeEnum.NEWEST_TO_OLDEST;
			p.shortDescription = "some short description";
			p.referenceId = "deleteme";
			p.videoIds.Add(40896467001);
			p.videoIds.Add(40889130001);
			RPCResponse<long> l = bc.CreatePlaylist(p);
			if (l.error.message != null) {
				Response.Write(l.error.message);
			} else {
				Response.Write(l.result);
			}
		}

		protected void UpdatePlaylist() {
			BCPlaylist result = bc.FindPlaylistById(8696101001);
			result.name = "now list updated";
			RPCResponse<BCPlaylist> p = bc.UpdatePlaylist(result);
			Response.Write("json: " + result.ToUpdateJSON());
			if (p.error.message != null) {
				Response.Write(p.error.message);
			} else {
				Response.Write(p.result.name);
			}
		}

		protected void DeletePlaylist() {
			//delete by id
			RPCResponse rpcr = bc.DeletePlaylist(37744649001);

			if (rpcr.error.message != null) {
				Response.Write(rpcr.error.message);
			} else {
				Response.Write("worked");
			}
			//delete by ref id
			RPCResponse rpcr2 = bc.DeletePlaylist("deleteme");

			if (rpcr2.error.message != null) {
				Response.Write(rpcr2.error.message);
			} else {
				Response.Write("worked");
			}
		}
	}
}
```