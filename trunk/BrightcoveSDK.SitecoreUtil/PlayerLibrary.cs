using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using Sitecore.Data;
using System.Web;
using Sitecore.Data.Fields;
using BrightcoveSDK;
using BrightcoveSDK.SitecoreUtil.Extensions;

namespace BrightcoveSDK.SitecoreUtil
{
	public class PlayerLibrary
	{
		public Item playerLibraryItem;

		public PlayerLibrary(Item i) {
			playerLibraryItem = i;
		}

		public List<Player> Players {
			get {
                List<Item> p = this.playerLibraryItem.ChildrenByTemplateRecursive("Brightcove Video Player");
				List<Player> players = new List<Player>();
				foreach (Item i in p) {
					players.Add(new Player(i));
				}
				return players;
			}
        }

        #region Static Methods

        public static Player GetPlayer(long PlayerID) {
            return GetPlayer(PlayerID, Sitecore.Context.Database);
        }
        public static Player GetPlayer(long PlayerID, Database DB) {
            Item i = DB.GetItem(Constants.BrightcoveLibID);
            if (i != null) {
                Item j = i.ChildrenByTemplateRecursive(Constants.PlayerTemplate).Where(player => player["Player ID"] == PlayerID.ToString()).ToList().FirstOrDefault();
                if (j != null) {
                    return new Player(j);
                }
            }
            return null;
        }

        #endregion Static Methods
    }

	public class Player
	{
		public Item playerItem;

		public Player(Item i) {
			playerItem = i;
		}

		public string Name {
			get {
				return playerItem.Fields["Name"].Value;
			}
			set {
				playerItem.Fields["Name"].Value = value;
			}
		}

		public long PlayerID {
			get {
				return long.Parse(playerItem.Fields["Player ID"].Value);
			}
			set {
				playerItem.Fields["Player ID"].Value = value.ToString();
			}
		}

		public PlayerPlaylistType PlaylistType {
			get {
				string type = playerItem.Fields["Playlist Type"].Value.Replace(" ", "");
				if (type.Equals(PlayerPlaylistType.ComboBox.ToString())) {
					return PlayerPlaylistType.ComboBox;
				}
				else if (type.Equals(PlayerPlaylistType.Tabbed.ToString())) {
					return PlayerPlaylistType.Tabbed;
				}
				else if (type.Equals(PlayerPlaylistType.VideoList.ToString())) {
					return PlayerPlaylistType.VideoList;
				}
				else {
					return PlayerPlaylistType.None;
				}	
			}
			set {
				playerItem.Fields["Playlist Type"].Value = value.ToString();
			}
		}

		public string Description {
			get {
				return playerItem.Fields["Description"].Value;
			}
			set {
				playerItem.Fields["Description"].Value = value;
			}
		}

		public int Width {
			get {
				return int.Parse(playerItem.Fields["Width"].Value);
			}
			set {
				playerItem.Fields["Width"].Value = value.ToString();
			}
		}

		public int Height {
			get {
				return int.Parse(playerItem.Fields["Height"].Value);
			}
			set {
				playerItem.Fields["Height"].Value = value.ToString();
			}
		}
	}
}
