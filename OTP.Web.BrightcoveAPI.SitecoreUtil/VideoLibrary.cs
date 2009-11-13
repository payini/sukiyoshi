using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using Sitecore.Data;
using System.Web;
using Sitecore.Data.Fields;
using OTP.Web.BrightcoveAPI.SitecoreUtil.Extensions;

namespace OTP.Web.BrightcoveAPI.SitecoreUtil
{
	public class VideoLibrary
	{
		public Item videoLibraryItem;

		public VideoLibrary(Item i) {
			videoLibraryItem = i;
		}

		public List<Video> Videos {
			get {
				List<string> temps = new List<string>();
				temps.Add("Brightcove Video");
				temps.Add("Video Folder");
				List<Item> v = this.videoLibraryItem.ChildrenByTemplatesRecursive(temps, "Video Folder");
				List<Video> videos = new List<Video>();
				foreach (Item i in v) {
					videos.Add(new Video(i));
				}
				return videos;
			}
		}
	}

	public class Video
	{
		public Item videoItem;

		public Video(Item i) {
			videoItem = i;
		}

		#region Editable Fields
				
		public string VideoName {
			get {
				return videoItem.Fields["Name"].Value;
			}
			set {
				videoItem.Fields["Name"].Value = value;
			}
		}

		public string ShortDescription {
			get {
				return videoItem.Fields["Short Description"].Value;
			}
			set {
				videoItem.Fields["Short Description"].Value = value;
			}
		}

		public string LongDescription {
			get {
				return videoItem.Fields["Long Description"].Value;
			}
			set {
				videoItem.Fields["Long Description"].Value = value;
			}
		}

		public string ReferenceID {
			get {
				return videoItem.Fields["Reference Id"].Value;
			}
			set {
				videoItem.Fields["Reference Id"].Value = value;
			}
		}

		public BCVideoEconomics Economics {
			get {
				string val = videoItem.Fields["Economics"].Value;
				if (val.Equals(BCVideoEconomics.AD_SUPPORTED.ToString())) {
					return BCVideoEconomics.AD_SUPPORTED;
				}
				else {
					return BCVideoEconomics.FREE;
				}
			}
			set {
				videoItem.Fields["Economics"].Value = value.ToString();
			}
		}

		public BCCollection<string> Tags {
			get {
				DelimitedField df = videoItem.Fields["Tags"];
				BCCollection<string> bc = new BCCollection<string>();
				foreach (string s in df.Items) {
					bc.Add(s);
				}
				return bc;
			}
			set {
				videoItem.Fields["Tags"].Value = value.ToDelimitedString(",");
			}
		}

		#endregion

		#region Non-Editable Fields

		public long VideoID {
			get {
				return long.Parse(videoItem.Fields["ID"].Value);
			}
		}
		
		public DateTime CreationDate {
			get {
				DateField df = videoItem.Fields["Creation Date"];
				return df.DateTime;
			}
		}

		public DateTime PublishedDate {
			get {
				DateField df = videoItem.Fields["Published Date"];
				return df.DateTime;
			}
		}

		public DateTime LastModifiedDate {
			get {
				DateField df = videoItem.Fields["Last Modified Date"];
				return df.DateTime;
			}
		}

		public string LinkURL {
			get {
				return videoItem.Fields["Link URL"].Value;
			}
		}

		public string LinkText {
			get {
				return videoItem.Fields["Link Text"].Value;
			}
		}
		
		public string VideoStillURL {
			get {
				return videoItem.Fields["Video Still URL"].Value;
			}
			//set {
			//    this.Fields["Video Still URL"].Value = value;
			//}
		}

		public string ThumbnailURL {
			get {
				return videoItem.Fields["Thumbnail URL"].Value;
			}
			//set {
			//    this.Fields["Thumbnail URL"].Value = value;
			//}
		}

		public long Length {
			get {
				return long.Parse(videoItem.Fields["Length"].Value);
			}
		}

		public long PlaysTotal {
			get {
				return long.Parse(videoItem.Fields["Plays Total"].Value);
			}
		}

		public long PlaysTrailingWeek {
			get {
				return long.Parse(videoItem.Fields["Plays Trailing Week"].Value);
			}
		}

		#endregion
	}
}
