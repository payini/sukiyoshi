using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightcoveSDK.Media;
using Sitecore.Data.Items;
using System.Collections;
using System.Web;
using System.Text.RegularExpressions;

namespace BrightcoveSDK.SitecoreUtil.Extensions
{
	public enum UpdateType { NEW, UPDATE, BOTH };

	public static class BCVideoExtensions	{

		public static UpdateInsertPair<Video> ImportToSitecore(this BCAccount account, BCVideo Video, UpdateType utype) {
			List<BCVideo> Videos = new List<BCVideo>();
			Videos.Add(Video);
			return ImportToSitecore(account, Videos, utype);
		}

		/// <summary>
		/// This method will import / update a list of videos into the Brightcove Video Library
		/// </summary>
		/// <param name="Videos">
		/// The Videos to import / update
		/// </param>
		/// <returns>
		/// returns a list of the new videos imported
		/// </returns>
		public static UpdateInsertPair<Video> ImportToSitecore(this BCAccount account, List<BCVideo> Videos, UpdateType utype) {

			UpdateInsertPair<Video> uip = new UpdateInsertPair<Video>();
						
			//set all BCVideos into hashtable for quick access
			Hashtable ht = new Hashtable();
			foreach (Video exVid in account.VideoLib.Videos) {
				if (!ht.ContainsKey(exVid.VideoID.ToString())) {
					//set as string, Item pair
					ht.Add(exVid.VideoID.ToString(), exVid);
				}
			}
			
			//Loop through the data source and add them
			foreach (BCVideo vid in Videos) {

				try {
					//remove access filter
					using (new Sitecore.SecurityModel.SecurityDisabler()) {

						Video currentItem;

						//if it exists then update it
						if (ht.ContainsKey(vid.id.ToString()) && (utype.Equals(UpdateType.BOTH) || utype.Equals(UpdateType.UPDATE))) {
							currentItem = (Video)ht[vid.id.ToString()];

							//add it to the new items
							uip.UpdatedItems.Add(currentItem);

							using (new EditContext(currentItem.videoItem, true, false)) {
								SetVideoFields(ref currentItem.videoItem, vid);
							}
						}
						//else just add it
						else if (!ht.ContainsKey(vid.id.ToString()) && (utype.Equals(UpdateType.BOTH) || utype.Equals(UpdateType.NEW))) {
							//Create new item
							TemplateItem templateType = account.Database.Templates["Modules/Brightcove/Brightcove Video"];

							currentItem = new Video(account.VideoLib.videoLibraryItem.Add(vid.name.StripInvalidChars(), templateType));

							//add it to the new items
							uip.NewItems.Add(currentItem);

							using (new EditContext(currentItem.videoItem, true, false)) {
								SetVideoFields(ref currentItem.videoItem, vid);
							}
						}
					}
				}
				catch (System.Exception ex) {
					//HttpContext.Current.Response.Write(vid.name + "<br/>");
					throw new Exception("Failed on video: " + vid.name + ". " + ex.ToString());
				}
			}

			return uip;
		}
	
		private static void SetVideoFields(ref Item currentItem, BCVideo vid) {
			
			//Set the appropriate field values for the new item
			currentItem.Fields["Name"].Value = vid.name;
			currentItem.Fields["Short Description"].Value = vid.shortDescription;
			currentItem.Fields["Long Description"].Value = vid.longDescription;
			currentItem.Fields["Reference Id"].Value = vid.referenceId;
			currentItem.Fields["Economics"].Value = vid.economics.ToString();
			currentItem.Fields["ID"].Value = vid.id.ToString();
			currentItem.Fields["Creation Date"].Value = vid.creationDate.ToDateFieldValue();
			currentItem.Fields["Published Date"].Value = vid.publishedDate.ToDateFieldValue();
			currentItem.Fields["Last Modified Date"].Value = vid.lastModifiedDate.ToDateFieldValue();
			currentItem.Fields["Link URL"].Value = vid.linkURL;
			currentItem.Fields["Link Text"].Value = vid.linkText;
			string taglist = "";
			foreach (string tag in vid.tags) {
				if (taglist.Length > 0) {
					taglist += ",";
				}
				taglist += tag;
			}
			currentItem.Fields["Tags"].Value = taglist;
			currentItem.Fields["Video Still URL"].Value = vid.videoStillURL;
			currentItem.Fields["Thumbnail URL"].Value = vid.thumbnailURL;
			currentItem.Fields["Length"].Value = vid.length;
			currentItem.Fields["Plays Total"].Value = vid.playsTotal.ToString();
			currentItem.Fields["Plays Trailing Week"].Value = vid.playsTrailingWeek.ToString();
		}
	}

	public static class BCPlaylistExtensions {

		public static UpdateInsertPair<Playlist> ImportToSitecore(this BCAccount account, BCPlaylist Playlist, UpdateType utype) {
			List<BCPlaylist> Playlists = new List<BCPlaylist>();
			Playlists.Add(Playlist);
			return ImportToSitecore(account, Playlists, utype);
		}

		public static UpdateInsertPair<Playlist> ImportToSitecore(this BCAccount account, List<BCPlaylist> Playlists, UpdateType utype) {

			UpdateInsertPair<Playlist> uip = new UpdateInsertPair<Playlist>();

			//set all BCVideos into hashtable for quick access
			Hashtable ht = new Hashtable();
			foreach (Playlist exPlay in account.PlaylistLib.Playlists) {
				if (!ht.ContainsKey(exPlay.playlistItem.ToString())) {
					//set as string, Item pair
					ht.Add(exPlay.PlaylistID.ToString(), exPlay);
				}
			}

			//Loop through the data source and add them
			foreach (BCPlaylist play in Playlists) {

				try {
					//remove access filter
					using (new Sitecore.SecurityModel.SecurityDisabler()) {

						Playlist currentItem;

						//if it exists then update it
						if (ht.ContainsKey(play.id.ToString()) && (utype.Equals(UpdateType.BOTH) || utype.Equals(UpdateType.UPDATE))) {
							currentItem = (Playlist)ht[play.id.ToString()];

							//add it to the new items
							uip.UpdatedItems.Add(currentItem);
							
							using (new EditContext(currentItem.playlistItem, true, false)) {
								SetPlaylistFields(ref currentItem.playlistItem, play);
							}
						}
						//else just add it
						else if (!ht.ContainsKey(play.id.ToString()) && (utype.Equals(UpdateType.BOTH) || utype.Equals(UpdateType.NEW))) {
							//Create new item
							TemplateItem templateType = account.Database.Templates["Modules/Brightcove/Brightcove Playlist"];
							currentItem = new Playlist(account.PlaylistLib.playlistLibraryItem.Add(play.name.StripInvalidChars(), templateType));

							//add it to the new items
							uip.NewItems.Add(currentItem);

							using (new EditContext(currentItem.playlistItem, true, false)) {
								SetPlaylistFields(ref currentItem.playlistItem, play);
							}
						}
					}
				}
				catch (System.Exception ex) {
					throw new Exception("Failed on playlist: " + play.name + ". " + ex.ToString());
				}
			}

			return uip;
		}

		private static void SetPlaylistFields(ref Item currentItem, BCPlaylist play) {
			
			//Set the appropriate field values for the new item
			currentItem.Fields["Name"].Value = play.name;
			currentItem.Fields["Reference Id"].Value = play.referenceId;
			currentItem.Fields["Short Description"].Value = play.shortDescription;
			string vidIdList = "";
			foreach (long vidId in play.videoIds) {
				if (vidIdList.Length > 0) {
					vidIdList += ",";
				}
				vidIdList += vidId.ToString();
			}
			currentItem.Fields["Video Ids"].Value = vidIdList;
			currentItem.Fields["ID"].Value = play.id.ToString();
			currentItem.Fields["Thumbnail URL"].Value = play.thumbnailURL;
			string filterTags = "";
			if (play.filterTags != null) {
				foreach (string tag in play.filterTags) {
					if (filterTags.Length > 0) {
						filterTags += ",";
					}
					filterTags += tag;
				}
			}
			currentItem.Fields["Filter Tags"].Value = filterTags;
			currentItem.Fields["Playlist Type"].Value = play.playlistType.ToString();
			currentItem.Fields["Account Id"].Value = play.accountId.ToString();
		}
	}

	public static class StringExtensions
	{

		public static string StripInvalidChars(this string val) {
			val = val.Replace(" ", "_");
			val = val.Replace("+", "");
			val = val.Replace("!", "");
			val = val.Replace("@", "");
			val = val.Replace("#", "");
			val = val.Replace("$", "");
			val = val.Replace("%", "");
			val = val.Replace("^", "");
			val = val.Replace("*", "");
			val = val.Replace("=", "");
			val = val.Replace("<", "");
			val = val.Replace(">", "");
			val = val.Replace("&", "_");
			val = val.Replace(",", "_");
			val = val.Replace("/", "_");
			val = val.Replace(@"\", "");
			val = val.Replace("|", "");
			val = val.Replace(";", "");
			val = val.Replace(":", "_");
			val = val.Replace("\"", "");
			val = val.Replace("’", "");
			val = val.Replace("é", "e");
			val = val.Replace("(", "");
			val = val.Replace(")", "");
			val = val.Replace("]", "");
			val = val.Replace("[", "");
			val = val.Replace("}", "");
			val = val.Replace("{", "");
			val = val.Replace("?", "");
			val = val.Replace("'", string.Empty);
			val = val.Replace(".", string.Empty);
			val = val.Replace("–", "_");
					 

			//Cleanup double underscores
			val = val.Replace("__", "_");

			//Remove all underscores
			val = val.Replace("_", "");

			return val.Trim();
		}
	}

	public static class SitecoreItemExtensions
	{
		/// <summary>
		/// This returns the first child it finds that has the required template or a null
		/// </summary>
		/// <param name="Parent">
		/// Parent
		/// </param>
		/// <param name="Templatename">
		/// this is the template name of the items you want
		/// </param>
		/// <returns>
		/// Returns the first item that matches the templatename or null
		/// </returns>
		public static Item ChildByTemplate(this Item Parent, string Templatename) {

			try {
				return (from child in Parent.GetChildren().ToArray() where child.TemplateName.Equals(Templatename) select child).First();
			}
			catch (Exception ex) {
				return null;
			}
		}

		/// <summary>
		/// This gets a child item that matches the template name and item name provided
		/// </summary>
		/// <param name="Parent">
		/// The parent to search under for a result
		/// </param>
		/// <param name="Templatename">
		/// The template name of the child to find
		/// </param>
		/// <param name="ItemName">
		/// The item name of the child to find
		/// </param>
		/// <returns>
		/// Returns and item that matches the criteria or null
		/// </returns>
		public static Item ChildByTemplateAndName(this Item Parent, string Templatename, string ItemName) {

			try {
				return (from child in Parent.GetChildren().ToArray() where (child.TemplateName.Equals(Templatename) && child.DisplayName.Equals(ItemName)) select child).First();
			}
			catch (Exception ex) {
				return null;
			}
		}

		/// <summary>
		/// This gets a child item that matches the template name or item name provided
		/// </summary>
		/// <param name="Parent">
		/// The parent to search under for a result
		/// </param>
		/// <param name="Templatename">
		/// The template name of the child to find
		/// </param>
		/// <param name="ItemName">
		/// The item name of the child to find
		/// </param>
		/// <returns>
		/// Returns and item that matches one of the criteria or null
		/// </returns>
		public static Item ChildByTemplateOrName(this Item Parent, string Templatename, string ItemName) {

			try {
				return (from child in Parent.GetChildren().ToArray() where (child.TemplateName.Equals(Templatename) || child.DisplayName.Equals(ItemName)) select child).First();
			}
			catch (Exception ex) {
				return null;
			}
		}

		/// <summary>
		/// this returns all the children who have a required template
		/// </summary>
		/// <param name="Parent">
		/// Parent Item
		/// </param>
		/// <param name="Templatename">
		/// this is the template name of the items that you want
		/// </param>
		/// <returns>
		/// Returns a list of items that match the template name
		/// </returns>
		public static List<Item> ChildrenByTemplate(this Item Parent, string Templatename) {
			List<string> types = new List<string>();
			types.Add(Templatename);
			return ChildrenByTemplates(Parent, types);
		}

		/// <summary>
		/// This returns a list of child items based on a list of templates names provided
		/// </summary>
		/// <param name="Parent">
		/// Parent Item to search for children
		/// </param>
		/// <param name="Templatenames">
		/// The list of template names to look for
		/// </param>
		/// <returns>
		/// Returns a list of items that match the templatenames provided
		/// </returns>
		public static List<Item> ChildrenByTemplates(this Item Parent, List<string> Templatenames) {

			return (from child in Parent.GetChildren().ToArray() where Templatenames.Contains(child.TemplateName) select child).ToList();
		}

		/// <summary>
		/// This will look for children of a specified templatename recursively. It will only recursively query under items that match the templatename.
		/// </summary>
		/// <param name="Parent">
		/// Parent item to search under
		/// </param>
		/// <param name="Templatename">
		/// Templatename of the items you want to return
		/// </param>
		/// <returns>
		/// Returns a list of Items that match the template name
		/// </returns>
		public static List<Item> ChildrenByTemplateRecursive(this Item Parent, string Templatename) {

			List<string> types = new List<string>();
			types.Add(Templatename);
			return ChildrenByTemplatesRecursive(Parent, types);
		}

		/// <summary>
		/// This will look for children of a specified templatenames recursively. It will only recursively query under items that match the templatenames.
		/// </summary>
		/// <param name="Parent">
		/// Parent item to search under
		/// </param>
		/// <param name="Templatenames">
		/// Templatenames of the items you want to return
		/// </param>
		/// <returns>
		/// Returns a list of items that match the templatenames provided
		/// </returns>
		public static List<Item> ChildrenByTemplatesRecursive(this Item Parent, List<string> Templatenames) {
			return ChildrenByTemplatesRecursive(Parent, Templatenames, new List<string>());
		}

		public static List<Item> ChildrenByTemplatesRecursive(this Item Parent, List<string> Templatenames, string ignoreTemplatename) {
			List<string> ignore = new List<string>();
			ignore.Add(ignoreTemplatename);
			return ChildrenByTemplatesRecursive(Parent, Templatenames, ignore);
		}

		public static List<Item> ChildrenByTemplatesRecursive(this Item Parent, List<string> Templatenames, List<string> IgnoreTemplates) {

			List<Item> list = new List<Item>();
			//get the first level of items
			List<Item> thisLevel = (from child in Parent.GetChildren().ToArray() where Templatenames.Contains(child.TemplateName) select child).ToList();

			//foreach item found look for children of it's type
			foreach (Item i in thisLevel) {
				//if this item's templatename is not in the ignore list then add it
				if (!IgnoreTemplates.Contains(i.TemplateName)) {
					list.Add(i);
				}
				//either way continue to search below it for values
				list.AddRange(i.ChildrenByTemplatesRecursive(Templatenames, IgnoreTemplates));
			}

			return list;
		}
	}

	public static class DateTimeExtensions
	{
		/// <summary>
		/// Gets a DateField from a DateTime
		/// </summary>
		/// <param name="Date">The current date field</param>
		/// <returns></returns>
		public static string ToDateFieldValue(this DateTime Date) {

			return Date.ToString("yyyyMMddTHHmmss");
		}
	}
}
