using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;
using OTP.Web.BrightcoveAPI.Media;

namespace OTP.Web.BrightcoveAPI
{
	public static class BCAPI
	{
		#region Main Helper Methods

		private static BCQueryResult MultipleQueryHandler(Dictionary<String, String> reqparams, BCObjectType itemType) {

			//Get the JSon reader returned from the APIRequest
			BCQueryResult qr = new BCQueryResult();
			qr.TotalCount = 0;

			//set some global request paramameters
			reqparams.Add("page_number", "0");

			// workaround for search by creation date
			if (reqparams.ContainsKey("sort_by") && itemType.Equals(BCObjectType.videos) && reqparams["sort_by"].Equals("CREATION_DATE")) {
				reqparams["sort_by"] = "MODIFIED_DATE";
			}

			//set if not set or 
			if (!reqparams.ContainsKey("page_size")) {
				qr.MaxToGet = -1;
			}
			else {
				qr.MaxToGet = Convert.ToInt32(reqparams["page_size"]);
			}

			//get initial query
			int pageNum = 0;
			bool stillMore = true;

			//if there are more to get move to next page and keep getting them
			while (stillMore) {

				//update page each iteration
				reqparams["page_number"] = pageNum.ToString();
				QueryResultPair qrp = BCAPIRequest.Execute(reqparams);
				string jsonStr = qrp.JsonResult;
				jsonStr = jsonStr.Replace("\"items\":", "\"" + itemType.ToString() + "\":");
				QueryResultPair qrp2 = new QueryResultPair(qrp.Query, jsonStr);
				//merge results with other
				//HttpContext.Current.Response.Write(qrp2.JsonResult);
				BCQueryResult qr2 = JSONHelper.Deserialize<BCQueryResult>(qrp2.JsonResult);
				qr.QueryResults.Add(qrp2);
				qr.Merge(qr2);
								
				//check to see if there are any more to get
				if (!qr.TotalCount.Equals(-1)) {
					stillMore = false;
				}
				else if (itemType.Equals(BCObjectType.videos)) {
					stillMore = (qr.Videos.Count < Convert.ToInt32(qr.TotalCount)) ? true : false;
				}
				else if (itemType.Equals(BCObjectType.playlists)) {
					stillMore = (qr.Playlists.Count < Convert.ToInt32(qr.TotalCount)) ? true : false;
				}

				pageNum++;
			}

			//sorting on our end

			if (itemType.Equals(BCObjectType.videos) && reqparams.ContainsKey("sort_by")) {
				//PUBLISH_DATE, 
				if (reqparams["sort_by"].Equals("PUBLISH_DATE")) {
					qr.Videos.Sort(BCVideo.PublishDateComparison);
				}
				//PLAYS_TOTAL, 
				else if (reqparams["sort_by"].Equals("PLAYS_TOTAL")) {
					qr.Videos.Sort(BCVideo.TotalPlaysComparison);
				}
				//PLAYS_TRAILING_WEEK
				else if (reqparams["sort_by"].Equals("PLAYS_TRAILING_WEEK")) {
					qr.Videos.Sort(BCVideo.PlaysTrailingComparison);
				}
				//CREATION_DATE, 
				//MODIFIED_DATE,
				//because brightcove methods break on sort by creation date 
				//we've turned anything using creation date to using modified date and assume it means creation date
				//this is temporary until that is fixed
				else {
					qr.Videos.Sort(BCVideo.CreationDateComparison);
				}

				//if they want asc
				if (reqparams["sort_order"].Equals("DESC")) {
					qr.Videos.Reverse();
				}

				//trim if specified
				if (qr.Videos.Count > qr.MaxToGet && !qr.MaxToGet.Equals(-1) && qr.MaxToGet < Convert.ToInt32(qr.TotalCount)) {
					qr.Videos = (BCCollection<BCVideo>)qr.Videos.GetRange(0, Convert.ToInt32(qr.MaxToGet));
				}
			}

			return qr;
		}

		private static String Implode(List<String> values) {
			String result = "";
			foreach (String s in values) {
				result = result + s + ",";
			}
			return result.TrimEnd(Convert.ToChar(","));
		}

		private static String Implode(List<long> values) {
			String result = "";
			foreach (long l in values) {
				result = result + l.ToString() + ",";
			}
			return result.TrimEnd(Convert.ToChar(","));
		}

		#endregion Main Helper Methods

		#region Video Read

		#region Find All Videos

		public static BCQueryResult FindAllVideos() {
			return FindAllVideos(-1);
		}
		
		public static BCQueryResult FindAllVideos(int howMany) {
			return FindAllVideos(howMany, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null);
		}

		public static BCQueryResult FindAllVideos(BCSortOrderType sortOrder) {
			return FindAllVideos(-1, BCSortByType.CREATION_DATE, sortOrder);
		}

		public static BCQueryResult FindAllVideos(BCSortByType sortBy) {
			return FindAllVideos(-1, sortBy, BCSortOrderType.ASC);
		}

		public static BCQueryResult FindAllVideos(BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindAllVideos(-1, sortBy, sortOrder);
		}

		public static BCQueryResult FindAllVideos(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindAllVideos(howMany, sortBy, sortOrder, null);
		}

		public static BCQueryResult FindAllVideos(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<String> video_fields) {
			return FindAllVideos(howMany, sortBy, sortOrder, video_fields, null);
		}

		/// <summary>
		/// This will return a generic search for videos
		/// </summary>
		/// <param name="howMany">
		/// The number of videos to return (-1 will return all) defaults to -1
		/// </param>
		/// <param name="sortBy">
		/// The field by which to sort (defaults to CREATION_DATE)
		/// </param>
		/// <param name="sortOrder">
		/// The direction by which to sort (default to DESC)
		/// </param>
		/// <param name="video_fields">
		/// A comma-separated list of the fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <param name="custom_fields">
		/// A comma-separated list of the custom fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <returns>
		/// Returns a BCQueryResult item
		/// </returns>
		public static BCQueryResult FindAllVideos(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<string> video_fields, List<string> custom_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_all_videos");
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			reqparams.Add("sort_order", sortOrder.ToString());
			reqparams.Add("sort_by", sortBy.ToString());
			if (howMany >= 0) reqparams.Add("page_size", howMany.ToString());

			return MultipleQueryHandler(reqparams, BCObjectType.videos);
		}

		#endregion Find All Videos

		#region Find Videos By User ID

		public static BCQueryResult FindVideosByUserId(long userId) {
			return FindVideosByUserId(userId, -1, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null, null);
		}

		public static BCQueryResult FindVideosByUserId(long userId, int howMany) {
			return FindVideosByUserId(userId, howMany, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null, null);
		}

		public static BCQueryResult FindVideosByUserId(long userId, BCSortOrderType sortOrder) {
			return FindVideosByUserId(userId, -1, BCSortByType.CREATION_DATE, sortOrder, null, null);
		}

		public static BCQueryResult FindVideosByUserId(long userId, BCSortByType sortBy) {
			return FindVideosByUserId(userId, -1, sortBy, BCSortOrderType.ASC, null, null);
		}

		public static BCQueryResult FindVideosByUserId(long userId, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByUserId(userId, -1, sortBy, sortOrder, null, null);
		}

		public static BCQueryResult FindVideosByUserId(long userId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByUserId(userId, howMany, sortBy, sortOrder, null, null);
		}

		public static BCQueryResult FindVideosByUserId(long userId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<String> video_fields) {
			return FindVideosByUserId(userId, howMany, sortBy, sortOrder, video_fields, null);
		}

		/// <summary>
		/// Retrieves the videos uploaded by the specified user id.
		/// </summary>
		/// <param name="userId">
		///  The id of the user whose videos we'd like to retrieve.
		/// </param>
		/// <param name="howMany">
		/// Number of videos returned (-1 will return all) defaults to -1
		/// </param>
		/// <param name="sortBy">
		/// The field by which to sort (defaults to CREATION_DATE)
		/// </param>
		/// <param name="sortOrder">
		/// The direction by which to sort (default to DESC)
		/// </param>
		/// <param name="video_fields">
		/// A comma-separated list of the fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <param name="custom_fields">
		/// A comma-separated list of the custom fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <returns>
		/// Returns a BCQueryResult item
		/// </returns>
		public static BCQueryResult FindVideosByUserId(long userId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<String> video_fields, List<string> custom_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_videos_by_user_id");
			reqparams.Add("user_id", userId.ToString());
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			reqparams.Add("sort_order", sortOrder.ToString());
			reqparams.Add("sort_by", sortBy.ToString());
			if (howMany >= 0) reqparams.Add("page_size", howMany.ToString());

			return MultipleQueryHandler(reqparams, BCObjectType.videos);
		}

		#endregion Find Videos By User ID

		#region Find Videos By Campaign ID

		public static BCQueryResult FindVideosByCampaignId(long campaignId) {
			return FindVideosByCampaignId(campaignId, -1, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null, null);
		}

		public static BCQueryResult FindVideosByCampaignId(long campaignId, int howMany) {
			return FindVideosByCampaignId(campaignId, howMany, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null, null);
		}

		public static BCQueryResult FindVideosByCampaignId(long campaignId, BCSortOrderType sortOrder) {
			return FindVideosByCampaignId(campaignId, -1, BCSortByType.CREATION_DATE, sortOrder, null, null);
		}

		public static BCQueryResult FindVideosByCampaignId(long campaignId, BCSortByType sortBy) {
			return FindVideosByCampaignId(campaignId, -1, sortBy, BCSortOrderType.ASC, null, null);
		}

		public static BCQueryResult FindVideosByCampaignId(long campaignId, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByCampaignId(campaignId, -1, sortBy, sortOrder, null, null);
		}

		public static BCQueryResult FindVideosByCampaignId(long campaignId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByCampaignId(campaignId, howMany, sortBy, sortOrder, null, null);
		}

		public static BCQueryResult FindVideosByCampaignId(long campaignId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<String> video_fields) {
			return FindVideosByCampaignId(campaignId, howMany, sortBy, sortOrder, video_fields, null);
		}

		/// <summary>
		/// Gets all the videos associated with the given campaign id
		/// </summary>
		/// <param name="campaignId">
		/// The id of the campaign you'd like to fetch videos for.
		/// </param>
		/// <param name="howMany">
		/// Number of videos returned (-1 will return all) defaults to -1
		/// </param>
		/// <param name="sortBy">
		/// The field by which to sort (defaults to CREATION_DATE)
		/// </param>
		/// <param name="sortOrder">
		/// The direction by which to sort (default to DESC)
		/// </param>
		/// <param name="video_fields">
		/// A comma-separated list of the fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <param name="custom_fields">
		/// A comma-separated list of the custom fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <returns>
		/// Returns a BCQueryResult item
		/// </returns>
		public static BCQueryResult FindVideosByCampaignId(long campaignId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<string> video_fields, List<string> custom_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_videos_by_campaign_id");
			reqparams.Add("campaign_id", campaignId.ToString());
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			reqparams.Add("sort_order", sortOrder.ToString());
			reqparams.Add("sort_by", sortBy.ToString());
			if (howMany >= 0) reqparams.Add("page_size", howMany.ToString());

			return MultipleQueryHandler(reqparams, BCObjectType.videos);
		}

		#endregion Find Videos By Campaign ID

		#region Find Videos By Text

		public static BCQueryResult FindVideosByText(string text) {
			return FindVideosByText(text, -1, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null, null);
		}

		public static BCQueryResult FindVideosByText(string text, int howMany) {
			return FindVideosByText(text, howMany, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null, null);
		}

		public static BCQueryResult FindVideosByText(string text, BCSortOrderType sortOrder) {
			return FindVideosByText(text, -1, BCSortByType.CREATION_DATE, sortOrder, null, null);
		}

		public static BCQueryResult FindVideosByText(string text, BCSortByType sortBy) {
			return FindVideosByText(text, -1, sortBy, BCSortOrderType.ASC, null, null);
		}

		public static BCQueryResult FindVideosByText(string text, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByText(text, -1, sortBy, sortOrder, null, null);
		}

		public static BCQueryResult FindVideosByText(string text, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByText(text, howMany, sortBy, sortOrder, null, null);
		}

		public static BCQueryResult FindVideosByText(string text, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<string> video_fields) {
			return FindVideosByText(text, howMany, sortBy, sortOrder, video_fields, null);
		}

		/// <summary>
		/// Searches through all the videos in this account, and returns a collection of videos whose name, short description, or long description contain a match for the specified text. 
		/// </summary>
		/// <param name="text">
		/// The text we're searching for.
		/// </param>
		/// <param name="howMany">
		/// Number of videos returned (-1 will return all) defaults to -1
		/// </param>
		/// <param name="sortBy">
		/// The field by which to sort (defaults to CREATION_DATE)
		/// </param>
		/// <param name="sortOrder">
		/// The direction by which to sort (default to DESC)
		/// </param>
		/// <param name="video_fields">
		/// A comma-separated list of the fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <param name="custom_fields">
		/// A comma-separated list of the custom fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <returns>
		/// Returns a BCQueryResult item
		/// </returns>
		public static BCQueryResult FindVideosByText(string text, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<string> video_fields, List<string> custom_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_videos_by_text");
			reqparams.Add("text", text);
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			reqparams.Add("sort_order", sortOrder.ToString());
			reqparams.Add("sort_by", sortBy.ToString());
			if (howMany >= 0) reqparams.Add("page_size", howMany.ToString());

			return MultipleQueryHandler(reqparams, BCObjectType.videos);
		}

		#endregion Find Videos By Text

		#region Find Videos By Tags

		public static BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags) {
			return FindVideosByTags(and_tags, or_tags, -1, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null);
		}

		public static BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags, int howMany) {
			return FindVideosByTags(and_tags, or_tags, howMany, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null);
		}

		public static BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags, BCSortByType sortBy) {
			return FindVideosByTags(and_tags, or_tags, -1, sortBy, BCSortOrderType.ASC);
		}

		public static BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags, BCSortOrderType sortOrder) {
			return FindVideosByTags(and_tags, or_tags, -1, BCSortByType.CREATION_DATE, sortOrder);
		}

		public static BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByTags(and_tags, or_tags, -1, sortBy, sortOrder);
		}

		public static BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByTags(and_tags, or_tags, howMany, sortBy, sortOrder, null);
		}

		public static BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<string> video_fields) {
			return FindVideosByTags(and_tags, or_tags, howMany, sortBy, sortOrder, video_fields, null);
		}

		/// <summary>
		/// Performs a search on all the tags of the videos in this account, and returns a collection of videos that contain the specified tags. Note that tags are not case-sensitive. 
		/// </summary>
		/// <param name="and_tags">
		/// Limit the results to those that contain all of these tags.
		/// </param>
		/// <param name="or_tags">
		/// Limit the results to those that contain at least one of these tags.
		/// </param>
		/// <param name="howMany">
		/// Number of videos returned (-1 will return all) defaults to -1
		/// </param>
		/// <param name="sortBy">
		/// The field by which to sort (defaults to CREATION_DATE)
		/// </param>
		/// <param name="sortOrder">
		/// The direction by which to sort (default to DESC)
		/// </param>
		/// <param name="video_fields">
		/// A comma-separated list of the fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <param name="custom_fields">
		/// A comma-separated list of the custom fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <returns>
		/// Returns a BCQueryResult item
		/// </returns>
		public static BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<String> video_fields, List<String> custom_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();
			
			//Build the REST parameter list
			reqparams.Add("command", "find_videos_by_tags");
			if (and_tags != null) reqparams.Add("and_tags", Implode(and_tags));
			if (or_tags != null) reqparams.Add("or_tags", Implode(or_tags));
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			reqparams.Add("sort_order", sortOrder.ToString());
			reqparams.Add("sort_by", sortBy.ToString());
			if (howMany >= 0) reqparams.Add("page_size", howMany.ToString());

			return MultipleQueryHandler(reqparams, BCObjectType.videos);
		}

		#endregion Find Videos By Tags

		#region Find Related Videos

		public static BCQueryResult FindRelatedVideos(long videoId) {
			return FindRelatedVideos(videoId, -1);
		}

		public static BCQueryResult FindRelatedVideos(long videoId, int howMany) {
			return FindRelatedVideos(videoId, howMany, null);
		}

		public static BCQueryResult FindRelatedVideos(long videoId, int howMany, List<String> video_fields) {
			return FindRelatedVideos(videoId, howMany, video_fields, null);
		}

		/// <summary>
		/// Finds videos related to the given video. Combines the name and short description of the given 
		/// video and searches for any partial matches in the name, description, and tags of all videos in 
		/// the Brightcove media library for this account. More precise ways of finding related videos include 
		/// tagging your videos by subject and using the find_videos_by_tags method to find videos that share 
		/// the same tags: or creating a playlist that includes videos that you know are related. 
		/// </summary>
		/// <param name="videoId">
		/// The id of the video we'd like related videos for.
		/// </param>
		/// <param name="howMany">
		/// Number of videos returned (-1 will return all) defaults to -1
		/// </param>
		/// <param name="video_fields">
		/// A comma-separated list of the fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <param name="custom_fields">
		/// A comma-separated list of the custom fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <returns>
		/// Returns a BCQueryResult item
		/// </returns>
		public static BCQueryResult FindRelatedVideos(long videoId, int howMany, List<String> video_fields, List<String> custom_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_related_videos");
			reqparams.Add("video_id", videoId.ToString());
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			if (howMany >= 0) reqparams.Add("page_size", howMany.ToString());

			return MultipleQueryHandler(reqparams, BCObjectType.videos);
		}

		#endregion Find Related Videos

		#region Find Videos By IDs

		public static BCQueryResult FindVideosByIds(List<long> videoIds) {
			return FindVideosByIds(videoIds, null);
		}

		public static BCQueryResult FindVideosByIds(List<long> videoIds, List<String> video_fields) {
			return FindVideosByIds(videoIds, video_fields, null);
		}

		/// <summary>
		/// Find multiple videos, given their ids.
		/// </summary>
		/// <param name="videoIds">
		/// The list of video ids for the videos we'd like to retrieve.
		/// </param>
		/// <param name="video_fields">
		/// A comma-separated list of the fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <param name="custom_fields">
		/// A comma-separated list of the custom fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <returns>
		/// Returns a BCQueryResult item
		/// </returns>
		public static BCQueryResult FindVideosByIds(List<long> videoIds, List<String> video_fields, List<String> custom_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_videos_by_ids");
			reqparams.Add("video_ids", Implode(videoIds));
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			reqparams.Add("page_size", "-1");

			return MultipleQueryHandler(reqparams, BCObjectType.videos);
		}

		#endregion Find Videos By IDs

		#region Find Videos By Reference IDs

		public static BCQueryResult FindVideosByReferenceIds(List<String> referenceIds) {
			return FindVideosByReferenceIds(referenceIds, null);
		}

		public static BCQueryResult FindVideosByReferenceIds(List<String> referenceIds, List<String> video_fields) {
			return FindVideosByReferenceIds(referenceIds, video_fields, null);
		}

		/// <summary>
		/// Find multiple videos based on their publisher-assigned reference ids.
		/// </summary>
		/// <param name="referenceIds">
		/// The list of reference ids for the videos we'd like to retrieve
		/// </param>
		/// <param name="video_fields">
		/// A comma-separated list of the fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <param name="custom_fields">
		/// A comma-separated list of the custom fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <returns>
		/// Returns a BCQueryResult item
		/// </returns>
		public static BCQueryResult FindVideosByReferenceIds(List<String> referenceIds, List<String> video_fields, List<String> custom_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_videos_by_reference_ids");
			reqparams.Add("reference_ids", Implode(referenceIds));
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));

			return MultipleQueryHandler(reqparams, BCObjectType.videos);
		}

		#endregion Find Videos By Reference IDs

		#region Find Video By ID

		public static BCVideo FindVideoById(long videoId) {
			return FindVideoById(videoId, null);
		}

		public static BCVideo FindVideoById(long videoId, List<String> video_fields) {
			return FindVideoById(videoId, video_fields, null);
		}

		/// <summary>
		/// Finds a single video with the specified id.
		/// </summary>
		/// <param name="videoId">
		/// The id of the video you would like to retrieve.
		/// </param>
		/// <param name="video_fields">
		/// A comma-separated list of the fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <param name="custom_fields">
		/// A comma-separated list of the custom fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <returns>
		/// Returns a BCVideo item
		/// </returns>
		public static BCVideo FindVideoById(long videoId, List<String> video_fields, List<String> custom_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_video_by_id");
			reqparams.Add("video_id", videoId.ToString());
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));

			//Get the JSon reader returned from the APIRequest
			string jsonStr = BCAPIRequest.Execute(reqparams).JsonResult;
			
			return JSONHelper.Deserialize<BCVideo>(jsonStr);
		}

		#endregion Find Video By ID

		#region Find Video By Reference ID

		public static BCVideo FindVideoByReferenceId(String referenceId) {
			return FindVideoByReferenceId(referenceId, null);
		}

		public static BCVideo FindVideoByReferenceId(String referenceId, List<String> video_fields) {
			return FindVideoByReferenceId(referenceId, video_fields, null);
		}

		/// <summary>
		/// Find a video based on its publisher-assigned reference id.
		/// </summary>
		/// <param name="referenceId">
		/// The publisher-assigned reference id for the video we're searching for.
		/// </param>
		/// <param name="video_fields">
		/// A comma-separated list of the fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <param name="custom_fields">
		/// A comma-separated list of the custom fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <returns>
		/// Returns a BCVideo item
		/// </returns>
		public static BCVideo FindVideoByReferenceId(String referenceId, List<String> video_fields, List<String> custom_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_video_by_reference_id");
			reqparams.Add("reference_id", referenceId);
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));

			//Get the JSon reader returned from the APIRequest
			string jsonStr = BCAPIRequest.Execute(reqparams).JsonResult;
			return JSONHelper.Deserialize<BCVideo>(jsonStr);
		}

		#endregion Find Video By Reference ID

		#endregion Video Read

		#region Playlist Read

		#region Find All Playlists

		public static BCQueryResult FindAllPlaylists() {
			return FindAllPlaylists(-1);
		}
		
		public static BCQueryResult FindAllPlaylists(int howMany) {
			return FindAllPlaylists(howMany, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null);
		}

		public static BCQueryResult FindAllPlaylists(int howMany, BCSortOrderType sortOrder) {
			return FindAllPlaylists(howMany, BCSortByType.CREATION_DATE, sortOrder, null);
		}

		public static BCQueryResult FindAllPlaylists(int howMany, BCSortByType sortBy) {
			return FindAllPlaylists(howMany, sortBy, BCSortOrderType.ASC, null);
		}

		public static BCQueryResult FindAllPlaylists(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindAllPlaylists(howMany, sortBy, sortOrder, null);
		}

		public static BCQueryResult FindAllPlaylists(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<String> video_fields) {
			return FindAllPlaylists(howMany, sortBy, sortOrder, video_fields, null);
		}

		public static BCQueryResult FindAllPlaylists(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<String> video_fields, List<String> custom_fields) {
			return FindAllPlaylists(howMany, sortBy, sortOrder, video_fields, custom_fields, null);
		}

		/// <summary>
		/// Find all playlists in this account.
		/// </summary>
		/// <param name="howMany">
		/// Number of videos returned (-1 will return all) defaults to -1
		/// </param>
		/// <param name="sortBy">
		/// The field by which to sort (defaults to CREATION_DATE)
		/// </param>
		/// <param name="sortOrder">
		/// The direction by which to sort (default to DESC)
		/// </param>
		/// <param name="video_fields">
		/// A comma-separated list of the fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <param name="custom_fields">
		/// A comma-separated list of the custom fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <param name="playlist_fields">
		/// A comma-separated list of the fields you wish to have populated in the playlists contained in the returned object. Passing null populates with all fields. 
		/// </param>
		/// <returns>
		/// Returns a BCQueryResult item
		/// </returns>
		public static BCQueryResult FindAllPlaylists(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<String> video_fields, List<String> custom_fields, List<string> playlist_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_all_playlists");
			reqparams.Add("sort_order", sortOrder.ToString());
			reqparams.Add("sort_by", sortBy.ToString());
			if (howMany >= 0) reqparams.Add("page_size", howMany.ToString());
			if (playlist_fields != null) reqparams.Add("playlist_fields", Implode(playlist_fields));
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));

			BCQueryResult qr = MultipleQueryHandler(reqparams, BCObjectType.playlists);

			return qr;
		}

		#endregion Find All Playlists

		#region Find Playlist By Id

		public static BCPlaylist FindPlaylistById(long playlist_id) {
			return FindPlaylistById(playlist_id, null);
		}

		public static BCPlaylist FindPlaylistById(long playlist_id, List<string> video_fields) {
			return FindPlaylistById(playlist_id, video_fields, null);
		}

		public static BCPlaylist FindPlaylistById(long playlist_id, List<string> video_fields, List<string> custom_fields) {
			return FindPlaylistById(playlist_id, video_fields, custom_fields, null);
		}

		/// <summary>
		/// Finds a particular playlist based on its id.
		/// </summary>
		/// <param name="playlist_id">
		/// The id of the playlist requested.
		/// </param>
		/// <param name="video_fields">
		/// A comma-separated list of the fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <param name="custom_fields">
		/// A comma-separated list of the custom fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <param name="playlist_fields">
		/// A comma-separated list of the fields you wish to have populated in the playlists contained in the returned object. Passing null populates with all fields. 
		/// </param>
		/// <returns>
		/// Returns a BCPlaylist item
		/// </returns>
		public static BCPlaylist FindPlaylistById(long playlist_id, List<string> video_fields, List<string> custom_fields, List<string> playlist_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_playlist_by_id");
			reqparams.Add("playlist_id", playlist_id.ToString());
			if (playlist_fields != null) reqparams.Add("playlist_fields", Implode(playlist_fields));
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));

			//Get the JSon reader returned from the APIRequest
			QueryResultPair qrp = BCAPIRequest.Execute(reqparams);
			return JSONHelper.Deserialize<BCPlaylist>(qrp.JsonResult);
		}

		#endregion Find Playlist By Id

		#region Find Playlists By Ids

		public static BCQueryResult FindPlaylistsByIds(List<long> playlist_ids) {
			return FindPlaylistsByIds(playlist_ids, null);
		}

		public static BCQueryResult FindPlaylistsByIds(List<long> playlist_ids, List<string> video_fields) {
			return FindPlaylistsByIds(playlist_ids, video_fields, null);
		}

		public static BCQueryResult FindPlaylistsByIds(List<long> playlist_ids, List<string> video_fields, List<string> custom_fields) {
			return FindPlaylistsByIds(playlist_ids, video_fields, custom_fields, null);
		}

		/// <summary>
		/// Retrieve a set of playlists based on their ids.
		/// </summary>
		/// <param name="playlist_ids">
		/// The ids of the playlists you would like retrieved.
		/// </param>
		/// <param name="video_fields">
		/// A comma-separated list of the fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <param name="custom_fields">
		/// A comma-separated list of the custom fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <param name="playlist_fields">
		/// A comma-separated list of the fields you wish to have populated in the playlists contained in the returned object. Passing null populates with all fields. 
		/// </param>
		/// <returns>
		/// Returns a BCQueryResult item
		/// </returns>
		public static BCQueryResult FindPlaylistsByIds(List<long> playlist_ids, List<string> video_fields, List<string> custom_fields, List<string> playlist_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_playlists_by_ids");
			reqparams.Add("playlist_ids", Implode(playlist_ids));
			if (playlist_fields != null) reqparams.Add("playlist_fields", Implode(playlist_fields));
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));

			BCQueryResult qr = MultipleQueryHandler(reqparams, BCObjectType.playlists);

			return qr;

		}

		#endregion Find Playlists By Ids

		#region Find Playlist By Reference Id

		public static BCPlaylist FindPlaylistByReferenceId(string reference_id) {
			return FindPlaylistByReferenceId(reference_id, null);
		}

		public static BCPlaylist FindPlaylistByReferenceId(string reference_id, List<string> video_fields) {
			return FindPlaylistByReferenceId(reference_id, video_fields, null);
		}

		public static BCPlaylist FindPlaylistByReferenceId(string reference_id, List<string> video_fields, List<string> custom_fields) {
			return FindPlaylistByReferenceId(reference_id, video_fields, custom_fields, null);
		}

		/// <summary>
		/// Retrieve a playlist based on its publisher-assigned reference id.
		/// </summary>
		/// <param name="reference_id">
		/// The reference id of the playlist we'd like to retrieve.
		/// </param>
		/// <param name="video_fields">
		/// A comma-separated list of the fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <param name="custom_fields">
		/// A comma-separated list of the custom fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <param name="playlist_fields">
		/// A comma-separated list of the fields you wish to have populated in the playlists contained in the returned object. Passing null populates with all fields. 
		/// </param>
		/// <returns>
		/// Returns a BCPlaylist item
		/// </returns>
		public static BCPlaylist FindPlaylistByReferenceId(string reference_id, List<string> video_fields, List<string> custom_fields, List<string> playlist_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_playlist_by_reference_id");
			reqparams.Add("reference_id", reference_id);
			if (playlist_fields != null) reqparams.Add("playlist_fields", Implode(playlist_fields));
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));

			//Get the JSon reader returned from the APIRequest
			string jsonStr = BCAPIRequest.Execute(reqparams).JsonResult;
			return JSONHelper.Deserialize<BCPlaylist>(jsonStr);
		}

		#endregion Find Playlist By Reference Id

		#region Find Playlists By Reference Ids

		public static BCQueryResult FindPlaylistsByReferenceIds(List<string> reference_ids) {
			return FindPlaylistsByReferenceIds(reference_ids, null);
		}

		public static BCQueryResult FindPlaylistsByReferenceIds(List<string> reference_ids, List<string> video_fields) {
			return FindPlaylistsByReferenceIds(reference_ids, video_fields, null);
		}

		public static BCQueryResult FindPlaylistsByReferenceIds(List<string> reference_ids, List<string> video_fields, List<string> custom_fields) {
			return FindPlaylistsByReferenceIds(reference_ids, video_fields, custom_fields, null);
		}

		/// <summary>
		/// Retrieve multiple playlists based on their publisher-assigned reference ids.
		/// </summary>
		/// <param name="reference_ids">
		/// The reference ids of the playlists we'd like to retrieve.
		/// </param>
		/// <param name="video_fields">
		/// A comma-separated list of the fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <param name="custom_fields">
		/// A comma-separated list of the custom fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <param name="playlist_fields">
		/// A comma-separated list of the fields you wish to have populated in the playlists contained in the returned object. Passing null populates with all fields. 
		/// </param>
		/// <returns>
		/// Returns a BCQueryResult item
		/// </returns>
		public static BCQueryResult FindPlaylistsByReferenceIds(List<string> reference_ids, List<string> video_fields, List<string> custom_fields, List<string> playlist_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_playlists_by_reference_ids");
			reqparams.Add("reference_ids", Implode(reference_ids));
			if (playlist_fields != null) reqparams.Add("playlist_fields", Implode(playlist_fields));
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			
			BCQueryResult qr = MultipleQueryHandler(reqparams, BCObjectType.playlists);

			return qr;
		}

		#endregion Find Playlists By Ids

		#region Find Playlists For Player Id

		public static BCQueryResult FindPlaylistsForPlayerId(long player_id) {
			return FindPlaylistsForPlayerId(player_id, -1);
		}

		public static BCQueryResult FindPlaylistsForPlayerId(long player_id, int howMany) {
			return FindPlaylistsForPlayerId(player_id, howMany, null);
		}

		public static BCQueryResult FindPlaylistsForPlayerId(long player_id, int howMany, List<string> video_fields) {
			return FindPlaylistsForPlayerId(player_id, howMany, video_fields, null);
		}

		public static BCQueryResult FindPlaylistsForPlayerId(long player_id, int howMany, List<string> video_fields, List<string> custom_fields) {
			return FindPlaylistsForPlayerId(player_id, howMany, video_fields, custom_fields, null);
		}

		/// <summary>
		/// Given the id of a player, returns all the playlists assigned to that player.
		/// </summary>
		/// <param name="player_id">
		/// The player id whose playlists we want to return.
		/// </param>
		/// <param name="howMany">
		/// The number of videos to return (-1 will return all) defaults to -1
		/// </param>
		/// <param name="video_fields">
		/// A comma-separated list of the fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <param name="custom_fields">
		/// A comma-separated list of the custom fields you wish to have populated in the videos contained in the returned object. Passing null populates with all fields. (defaults to all) 
		/// </param>
		/// <param name="playlist_fields">
		/// A comma-separated list of the fields you wish to have populated in the playlists contained in the returned object. Passing null populates with all fields. 
		/// </param>
		/// <returns>
		/// Returns a BCQueryResult item
		/// </returns>
		public static BCQueryResult FindPlaylistsForPlayerId(long player_id, int howMany, List<string> video_fields, List<string> custom_fields, List<string> playlist_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_playlists_for_player_id");
			reqparams.Add("player_id", player_id.ToString());
			if (howMany >= 0) reqparams.Add("page_size", howMany.ToString());
			if (playlist_fields != null) reqparams.Add("playlist_fields", Implode(playlist_fields));
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));

			BCQueryResult qr = MultipleQueryHandler(reqparams, BCObjectType.playlists);

			return qr;
		}

		#endregion Find Playlists For Player Id

		#endregion Playlist Read

		#region Video Write

		#region Create Video
		
		public static long CreateVideo(BCVideo video, string file) {
			return CreateVideo(video, null, -1, file, null, BCEncodeType.FLV, false, false, false);
		}
		public static long CreateVideo(BCVideo video, string file, BCEncodeType encode_to) {
			return CreateVideo(video, null, -1, file, null, encode_to, false, false, false);
		}

		public static long CreateVideo(BCVideo video, string file, long maxsize) {
			return CreateVideo(video, null, maxsize, file, null, BCEncodeType.FLV, false, false, false);
		}
		public static long CreateVideo(BCVideo video, string file, BCEncodeType encode_to, long maxsize) {
			return CreateVideo(video, null, maxsize, file, null, encode_to, false, false, false);
		}

		public static long CreateVideo(BCVideo video, string file, long maxsize, string file_checksum) {
			return CreateVideo(video, null, maxsize, file, file_checksum, BCEncodeType.FLV, false, false, false);
		}
		public static long CreateVideo(BCVideo video, string file, BCEncodeType encode_to, long maxsize, string file_checksum) {
			return CreateVideo(video, null, maxsize, file, file_checksum, encode_to, false, false, false);
		}

		public static long CreateVideo(BCVideo video, string file, long maxsize, string file_checksum, string filename) {
			return CreateVideo(video, filename, maxsize, file, file_checksum, BCEncodeType.FLV, false, false, false);
		}
		public static long CreateVideo(BCVideo video, string file, BCEncodeType encode_to, long maxsize, string file_checksum, string filename) {
			return CreateVideo(video, filename, maxsize, file, file_checksum, encode_to, false, false, false);
		}

		//favors no processing renditions
		public static long CreateVideo(BCVideo video, string file, long maxsize, string file_checksum, string filename, bool H264NoProcessing) {
			return CreateVideo(video, filename, maxsize, file, file_checksum, BCEncodeType.MP4, false, H264NoProcessing, false);
		}

		//favors multiple renditions
		public static long CreateVideo(BCVideo video, string file, long maxsize, string file_checksum, string filename, bool create_multiple_renditions, bool preserve_source_rendition) {
			return CreateVideo(video, filename, maxsize, file, file_checksum, BCEncodeType.MP4, create_multiple_renditions, false, preserve_source_rendition);
		}

		/// <summary>
		/// Upload a file to your Brightcove account
		/// </summary>
		/// <param name="video">
		/// The metadata for the video you'd like to create. This takes the form of a 
		/// JSON object of name/value pairs, each of which corresponds to a settable 
		/// property of the Video object.
		/// </param>
		/// <param name="filename">
		/// The name of the file that's being uploaded. You don't need to specify this in 
		/// the JSON if it is specified in the file part of the POST. 
		/// </param>
		/// <param name="maxsize">
		/// The maximum size that the file will be. This is used as a limiter to know when 
		/// something has gone wrong with the upload. The maxSize is same as the file you uploaded. 
		/// You don't need to specify this in the JSON if it is specified in the file part of the POST.  
		/// </param>
		/// <param name="file">
		/// An input stream associated with the video file you're uploading. This takes the 
		/// form of a file part, in a multipart/form-data HTTP request. This input stream and 
		/// the filename and maxSide parameters are automatically inferred from that file part.
		/// </param>
		/// <param name="file_checksum">
		/// An optional MD5 hash of the file. The checksum can be used to verify that the file checked 
		/// into your Brightcove Media Library is the same as the file you uploaded. 
		/// </param>
		/// <param name="encode_to">
		/// If the file requires transcoding, use this parameter to specify the target encoding. Valid 
		/// values are MP4 or FLV, representing the H264 and VP6 codecs respectively. Note that transcoding 
		/// of FLV files to another codec is not currently supported. This parameter is optional and defaults to FLV.
		/// </param>
		/// <param name="create_multiple_renditions">
		/// If the file is a supported transcodeable type, this optional flag can be used to control the 
		/// number of transcoded renditions. If true (default), multiple renditions at varying encoding 
		/// rates and dimensions are created. Setting this to false will cause a single transcoded VP6 
		/// rendition to be created at the standard encoding rate and dimensions. 
		/// </param>
		/// <param name="H264NoProcessing">
		/// If the video file is H.264 encoded and if create_multiple_ renditions=true, then multiple 
		/// VP6 renditions are created and in addition the H.264 source is retained as an additional rendition. 
		/// </param>
		/// <param name="preserve_source_rendition">
		/// Use this option to prevent H.264 source files from being transcoded. This parameter cannot be 
		/// used in combination with create_multiple_renditions. It is optional and defaults to false.
		/// </param>
		/// <returns>
		/// The id of the video that's been created.
		/// </returns>
		private static long CreateVideo(BCVideo video, string filename, long maxsize, string file, string file_checksum, BCEncodeType encode_to, bool create_multiple_renditions, bool H264NoProcessing, bool preserve_source_rendition) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "create_video");
			reqparams.Add("video", JSONHelper.Serialize<BCVideo>(video));
			if (filename != null) reqparams.Add("filename", filename);
			if (maxsize >= 0) reqparams.Add("maxsize", maxsize.ToString());
			if (file != null) reqparams.Add("file", file);
			if (file_checksum != null) reqparams.Add("file_checksum", file_checksum);
			if (encode_to != null) reqparams.Add("encode_to", encode_to.ToString());
			if (create_multiple_renditions != null) reqparams.Add("create_multiple_renditions", create_multiple_renditions.ToString());
			if (H264NoProcessing != null) reqparams.Add("H264NoProcessing", H264NoProcessing.ToString());
			if (preserve_source_rendition != null) reqparams.Add("preserve_source_rendition", preserve_source_rendition.ToString());

			//Get the JSon reader returned from the APIRequest
			string jsonStr = BCAPIRequest.Execute(reqparams, ActionType.WRITE).JsonResult; 
			return JSONHelper.Deserialize<BCVideo>(jsonStr).id;
		}

		#endregion Create Video

		#region Update Video

		/// <summary>
		/// Updates the video you specify
		/// </summary>
		/// <param name="video">
		/// The metadata for the video you'd like to update. This takes the form of a JSON object of name/value pairs, each of which corresponds to a settable property of the Video object. 
		/// </param>
		/// <returns></returns>
		public static BCVideo UpdateVideo(BCVideo video) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "update_video");
			reqparams.Add("video", JSONHelper.Serialize<BCVideo>(video));

			//Get the JSon reader returned from the APIRequest
			string jsonStr = BCAPIRequest.Execute(reqparams, ActionType.WRITE).JsonResult;
			return JSONHelper.Deserialize<BCVideo>(jsonStr);
		}

		#endregion Update Video

		#region Delete Video

		public static void DeleteVideo(string reference_id) {
			DeleteVideo(-1, reference_id);
		}

		public static void DeleteVideo(long video_id) {
			DeleteVideo(video_id, null);
		}

		public static void DeleteVideo(long video_id, string reference_id) {
			DeleteVideo(video_id, reference_id, false);
		}

		public static void DeleteVideo(long video_id, string reference_id, bool cascade) {
			DeleteVideo(video_id, reference_id, cascade, false);
		}

		/// <summary>
		/// Deletes a video.
		/// </summary>
		/// <param name="video_id">
		/// The id of the video you'd like to delete
		/// </param>
		/// <param name="reference_id">
		/// The publisher-assigned reference id of the video you'd like to delete.
		/// </param>
		/// <param name="cascade">
		/// Set this to true if you want to delete this video even if it is part of a 
		/// manual playlist or assigned to a player. The video will be removed from 
		/// all playlists and players in which it appears, then deleted. 
		/// </param>
		/// <param name="delete_shares">
		/// Set this to true if you want also to delete shared copies of this video. 
		/// Note that this will delete all shared copies from sharee accounts, regardless 
		/// of whether or not those accounts are currently using the video in playlists or players.
		/// </param>
		public static void DeleteVideo(long video_id, string reference_id, bool cascade, bool delete_shares) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "delete_video");
			if (video_id >= 0) reqparams.Add("video_id", video_id.ToString());
			if (reference_id != null) reqparams.Add("reference_id", reference_id);
			if (cascade != null) reqparams.Add("cascade", cascade.ToString());
			if (delete_shares != null) reqparams.Add("delete_shares", delete_shares.ToString());

			//Get the JSon reader returned from the APIRequest
			string jsonStr = BCAPIRequest.Execute(reqparams, ActionType.WRITE).JsonResult;
		}

		#endregion Delete Video

		#region Get Upload Status

		public static UploadStatusEnum GetUploadStatus(string reference_id) {
			return GetUploadStatus(-1, reference_id);
		}

		public static UploadStatusEnum GetUploadStatus(long video_id) {
			return GetUploadStatus(video_id, null);
		}

		/// <summary>
		/// Call this function in an HTTP POST request to determine the status of an upload.
		/// </summary>
		/// <param name="video_id">
		/// The id of the video whose status you'd like to get.
		/// </param>
		/// <param name="reference_id">
		/// The publisher-assigned reference id of the video whose status you'd like to get.
		/// </param>
		/// <returns>
		/// an UploadStatusEnum that specifies the current state of the upload.
		/// </returns>
		public static UploadStatusEnum GetUploadStatus(long video_id, string reference_id) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "get_upload_status");
			if (video_id >= 0) reqparams.Add("video_id", video_id.ToString());
			if (reference_id != null) reqparams.Add("reference_id", reference_id);

			//Get the JSon reader returned from the APIRequest
			string jsonStr = BCAPIRequest.Execute(reqparams, ActionType.WRITE).JsonResult;
			switch (jsonStr) {
				case "COMPLETE":
					return UploadStatusEnum.COMPLETE;
				case "ERROR":
					return UploadStatusEnum.ERROR;
				case "PROCESSING":
					return UploadStatusEnum.PROCESSING;
				case "UPLOADING":
					return UploadStatusEnum.UPLOADING;
				default:
					return UploadStatusEnum.UNDEFINED;
			}
		}

		#endregion Get Upload Status

		#region Share Video

		public static BCCollection<long> ShareVideo(long video_id, List<long> sharee_account_ids) {
			return ShareVideo(video_id, false, sharee_account_ids);
		}

		/// <summary>
		/// Shares the specified video with a list of sharee accounts
		/// </summary>
		/// <param name="video_id">
		/// The id of the video whose status you'd like to get.
		/// </param>
		/// <param name="auto_accept">
		/// If the target account has the option enabled, setting this flag to true will bypass 
		/// the approval process, causing the shared video to automatically appear in the target 
		/// account's library. If the target account does not have the option enabled, or this 
		/// flag is unspecified or false, then the shared video will be queued up to be approved 
		/// by the target account before appearing in their library.	
		/// </param>
		/// <param name="sharee_account_ids">
		/// List of Account IDs to share video with.
		/// </param>
		/// <returns></returns>
		public static BCCollection<long> ShareVideo(long video_id, bool auto_accept, List<long> sharee_account_ids) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "share_video");
			reqparams.Add("video_id", video_id.ToString());
			if (auto_accept) reqparams.Add("auto_accept", auto_accept.ToString());
			reqparams.Add("sharee_account_ids", Implode(sharee_account_ids));

			//Get the JSon reader returned from the APIRequest
			string jsonStr = BCAPIRequest.Execute(reqparams, ActionType.WRITE).JsonResult;
			return JSONHelper.Deserialize<BCCollection<long>>(jsonStr);
		}

		#endregion Share Video

		#region Add Image

		//using video id
		public static BCImage AddImage(BCImage image, string file, long video_id) {
			return AddImage(image, file, video_id, false);
		}
		public static BCImage AddImage(BCImage image, string file, long video_id, bool resize) {
			return AddImage(image, file, video_id, null, resize);
		}
		public static BCImage AddImage(BCImage image, string file, long video_id, string filename, bool resize) {
			return AddImage(image, file, video_id, filename, -1, resize);
		}
		public static BCImage AddImage(BCImage image, string file, long video_id, string filename, long maxsize, bool resize) {
			return AddImage(image, file, video_id, filename, maxsize, null, resize);
		}
		public static BCImage AddImage(BCImage image, string file, long video_id, string filename, long maxsize, string file_checksum, bool resize) {
			return AddImage(image, filename, maxsize, file, file_checksum, video_id, null, resize);
		}

		//using ref id
		public static BCImage AddImage(BCImage image, string file, string video_reference_id) {
			return AddImage(image, file, video_reference_id, false);
		}
		public static BCImage AddImage(BCImage image, string file, string video_reference_id, bool resize) {
			return AddImage(image, file, video_reference_id, null, resize);
		}
		public static BCImage AddImage(BCImage image, string file, string video_reference_id, string filename, bool resize) {
			return AddImage(image, file, video_reference_id, filename, -1, resize);
		}
		public static BCImage AddImage(BCImage image, string file, string video_reference_id, string filename, long maxsize, bool resize) {
			return AddImage(image, file, video_reference_id, filename, maxsize, null, resize);
		}
		public static BCImage AddImage(BCImage image, string file, string video_reference_id, string filename, long maxsize, string file_checksum, bool resize) {
			return AddImage(image, filename, maxsize, file, file_checksum, -1, video_reference_id, resize);
		}

		/// <summary>
		/// Add a new thumbnail or video still image to a video, or assign an existing image to another video.
		/// </summary>
		/// <param name="image">
		/// The metadata for the image you'd like to create (or update). This takes the form of a 
		/// JSON object of name/value pairs, each of which corresponds to a property of the Image object. 
		/// </param>
		/// <param name="filename">
		/// The name of the file that's being uploaded. You don't need to specify this in the JSON 
		/// if it is specified in the file part of the POST. 
		/// </param>
		/// <param name="maxsize">
		/// The maximum size that the file will be. This is used as a limiter to know when something 
		/// has gone wrong with the upload. The maxSize is same as the file you uploaded. You don't 
		/// need to specify this in the JSON if it is specified in the file part of the POST.
		/// </param>
		/// <param name="file">
		/// An input stream associated with the image file you're uploading. This takes the form of a 
		/// file part, in a multipart/form-data HTTP request. This input stream and the filename and 
		/// maxSide parameters are automatically inferred from that file part. 
		/// </param>
		/// <param name="file_checksum">
		/// An optional MD5 hash of the file. The checksum can be used to verify that the file checked 
		/// into your Brightcove Media Library is the same as the file you uploaded. 
		/// </param>
		/// <param name="video_id">
		/// The ID of the video you'd like to assign an image to.
		/// </param>
		/// <param name="video_reference_id">
		/// The publisher-assigned reference ID of the video you'd like to assign an image to.
		/// </param>
		/// <param name="resize">
		/// Set this to false if you don't want your image to be automatically resized to the default 
		/// size for its type. By default images will be resized. 
		/// </param>
		/// <returns></returns>
		private static BCImage AddImage(BCImage image, string filename, long maxsize, string file, string file_checksum, long video_id, string video_reference_id, bool resize) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

		    //Build the REST parameter list
		    reqparams.Add("command", "add_image");
		    reqparams.Add("image", JSONHelper.Serialize<BCImage>(image));
		    if (filename != null) reqparams.Add("filename", filename);
		    if (maxsize >= 0) reqparams.Add("maxsize", maxsize.ToString());
		    if (file != null) reqparams.Add("file", file);
		    if (file_checksum != null) reqparams.Add("file_checksum", file_checksum);
		    if (video_id >= 0) reqparams.Add("video_id", video_id.ToString());
		    if (video_reference_id != null) reqparams.Add("video_reference_id", video_reference_id);
		    if (resize) reqparams.Add("resize", resize.ToString().ToLower());

		    //Get the JSon reader returned from the APIRequest
		    string jsonStr = BCAPIRequest.Execute(reqparams).JsonResult;
		    return JSONHelper.Deserialize<BCImage>(jsonStr);
		}

		#endregion Add Image

		#endregion Video Write

		#region Playlist Write

		#region Create Playlist

		/// <summary>
		/// Creates a playlist. This method must be called using an HTTP POST request and JSON parameters.
		/// </summary>
		/// <param name="playlist">
		/// The metadata for the playlist you'd like to create. This takes the form of a JSON object of 
		/// name/value pairs, each of which corresponds to a settable property of the Playlist object. 
		/// Populate the videoIds property of the playlist, not the videos property. 
		/// </param>
		/// <returns>
		/// The ID of the Playlist you created.
		/// </returns>
		public static long CreatePlaylist(BCPlaylist playlist) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "create_playlist");
			reqparams.Add("playlist", JSONHelper.Serialize <BCPlaylist>(playlist));

			//Get the JSon reader returned from the APIRequest
			string jsonStr = BCAPIRequest.Execute(reqparams).JsonResult;
			return long.Parse(jsonStr);
		}

		#endregion Create Playlist

		#region Update Playlist

		/// <summary>
		/// Updates a playlist, specified by playlist id. This method must be called 
		/// using an HTTP POST request and JSON parameters.
		/// </summary>
		/// <param name="playlist">
		/// The metadata for the playlist you'd like to create. This takes the form of a 
		/// JSON object of name/value pairs, each of which corresponds to a settable 
		/// property of the Playlist object. Populate the videoIds property of the 
		/// playlist, not the videos property. 
		/// </param>
		/// <returns></returns>
		public static BCPlaylist UpdatePlaylist(BCPlaylist playlist) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "update_playlist");
			reqparams.Add("playlist", JSONHelper.Serialize<BCPlaylist>(playlist));

			//Get the JSon reader returned from the APIRequest
			string jsonStr = BCAPIRequest.Execute(reqparams).JsonResult;
			return JSONHelper.Deserialize<BCPlaylist>(jsonStr);
		}

		#endregion Update Playlist

		#region Delete Playlist

		public static void DeletePlaylist(string reference_id) {
			DeletePlaylist(reference_id, false);
		}
		public static void DeletePlaylist(string reference_id, bool cascade) {
			DeletePlaylist(-1, reference_id, cascade);
		}

		public static void DeletePlaylist(long playlist_id) {
			DeletePlaylist(playlist_id, false);
		}
		public static void DeletePlaylist(long playlist_id, bool cascade) {
			DeletePlaylist(playlist_id, null, cascade);
		}

		/// <summary>
		/// Deletes a playlist, specified by playlist id.
		/// </summary>
		/// <param name="playlist_id">
		/// the id for the playlist to delete
		/// </param>
		/// <param name="reference_id">
		///	The publisher-assigned reference id of the playlist you'd like to delete.
		/// </param>
		private static void DeletePlaylist(long playlist_id, string reference_id, bool cascade) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "delete_playlist");
			if (playlist_id >= 0) reqparams.Add("playlist_id", playlist_id.ToString());
			if (reference_id != null) reqparams.Add("reference_id", reference_id);
			if (cascade) reqparams.Add("cascade", cascade.ToString().ToLower());

			//Get the JSon reader returned from the APIRequest
			string jsonStr = BCAPIRequest.Execute(reqparams).JsonResult;
			//return JSONHelper.Deserialize<BCVideo>(jsonStr);
		}

		#endregion Delete Playlist

		#endregion Playlist Write
	}
}
