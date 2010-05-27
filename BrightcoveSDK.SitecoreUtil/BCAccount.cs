using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using Sitecore.Data;
using BrightcoveSDK.SitecoreUtil.Extensions;

namespace BrightcoveSDK.SitecoreUtil
{
	public class BCAccount : Item {

		public BCAccount(ID itemID, ItemData data, Database database) : base(itemID, data, database) {

		}

		public long PublisherID {
			get {
				if (this.Fields["Publisher ID"].Value.Equals("")) {
					return 0;
				}
				else {
					return long.Parse(this.Fields["Publisher ID"].Value);
				}
			}
			set {
				this.Fields["Publisher ID"].Value = value.ToString();
			}
		}

		public PlaylistLibrary PlaylistLib {
			get {
				return new PlaylistLibrary(this.ChildByTemplate("Playlist Library"));
			}
		}
		
		public VideoLibrary VideoLib {
			get {
				return new VideoLibrary(this.ChildByTemplate("Video Library"));
			}
		}

		public PlayerLibrary PlayerLib {
			get {
				return new PlayerLibrary(this.ChildByTemplate("Video Player Library"));
			}
		}
	}
}
