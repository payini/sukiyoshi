using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using BrightcoveAPI.SitecoreUtil.Extensions;

namespace BrightcoveAPI.SitecoreUtil
{
	public class VideoPlayerLibrary
	{
		public Item playerLibraryItem;

		public VideoPlayerLibrary(Item i) {
			playerLibraryItem = i;
		}

		public List<SCVideoPlayer> Players {
			get {
				List<string> temps = new List<string>();
				temps.Add("Brightcove Video Player");
				temps.Add("Video Player Folder");
				List<Item> p = this.playerLibraryItem.ChildrenByTemplatesRecursive(temps, "Video Player Folder");
				List<SCVideoPlayer> players = new List<SCVideoPlayer>();
				foreach (Item i in p) {
				    players.Add(new SCVideoPlayer(i));
				}
				return players;
			}
		}
	}

	public enum PlayerPlaylistType { None, ComboBox, Tabbed, VideoList }

	public class SCVideoPlayer
	{
		public Item playerItem;

		public SCVideoPlayer(Item i) {
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
