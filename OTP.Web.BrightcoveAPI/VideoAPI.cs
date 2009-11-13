using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;
using OTP.Web.BrightcoveAPI.HTTP;
using OTP.Web.BrightcoveAPI.Media;
using OTP.Web.BrightcoveAPI.JSON;
using System.Configuration;

namespace OTP.Web.BrightcoveAPI
{
	public class VideoAPI
	{
		#region Properties

		protected AccountConfigElement Account;

		#endregion Properties

		#region Constructors

		public VideoAPI(string accountName) {
			BrightcoveConfig bc = (BrightcoveConfig)ConfigurationManager.GetSection("brightcove");
			foreach (AccountConfigElement a in bc.Accounts) {
				if(a.Name.Equals(accountName)){
					Account = a;
				}
			}
		}

		public VideoAPI(long publisherId) {
			BrightcoveConfig bc = (BrightcoveConfig)ConfigurationManager.GetSection("brightcove");
			foreach (AccountConfigElement a in bc.Accounts) {
				if (a.PublisherID.Equals(publisherId)) {
					Account = a;
				}
			}
		}

		#endregion Constructors

		#region Main Helper Methods

		private static BCQueryResult MultipleQueryHandler(Dictionary<String, String> reqparams, BCObjectType itemType, AccountConfigElement account) {

			//Get the JSon reader returned from the APIRequest
			BCQueryResult qr = new BCQueryResult();
			qr.TotalCount = 0;

			try {

				//set some global request paramameters
				reqparams.Add("page_number", "0");

				// workaround for search by creation date
				bool isCreationSort = false;
				if (reqparams.ContainsKey("sort_by") && itemType.Equals(BCObjectType.videos) && reqparams["sort_by"].Equals("CREATION_DATE")) {
					isCreationSort = true;
					reqparams["sort_by"] = "MODIFIED_DATE";
				}

				//set if not set or 
				if (!reqparams.ContainsKey("page_size")) {
					qr.MaxToGet = -1;
				}
				else {
					qr.MaxToGet = Convert.ToInt32(reqparams["page_size"]);
					reqparams["page_size"] = "100";
				}

				//get initial query
				int pageNum = 0;
				bool stillMore = true;

				//if there are more to get move to next page and keep getting them
				while (stillMore) {

					//update page each iteration
					reqparams["page_number"] = pageNum.ToString();
					QueryResultPair qrp = BCAPIRequest.ExecuteRead(reqparams, account);
					//convert the result for deserialization
					string jsonStr = qrp.JsonResult;
					jsonStr = jsonStr.Replace("\"items\":", "\"" + itemType.ToString() + "\":");
					QueryResultPair qrp2 = new QueryResultPair(qrp.Query, jsonStr);
					//merge results with other
					//HttpContext.Current.Response.Write(qrp2.JsonResult);
					BCQueryResult qr2 = JSON.Converter.Deserialize<BCQueryResult>(qrp2.JsonResult);
					qr.QueryResults.Add(qrp2);
					qr.Merge(qr2);

					//check to see if there are any more to get
					if (itemType.Equals(BCObjectType.videos)) {
						if (!isCreationSort) {
							stillMore = (qr.Videos.Count < qr.MaxToGet && qr.Videos.Count < qr.TotalCount) ? true : false;
						}
						else {
							stillMore = (qr.Videos.Count < qr.TotalCount) ? true : false;
						}
					}
					else if (itemType.Equals(BCObjectType.playlists)) {
						stillMore = (qr.Playlists.Count < qr.TotalCount) ? true : false;
					}
					else if (qr.Videos.Count >= qr.MaxToGet) {
						stillMore = false;
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
					if (qr.Videos.Count > qr.MaxToGet && !qr.MaxToGet.Equals(-1) && qr.MaxToGet < qr.TotalCount) {
						List<BCVideo> vidTemp = qr.Videos.GetRange(0, Convert.ToInt32(qr.MaxToGet));

						qr.Videos.Clear();
						qr.Videos.AddRange(vidTemp);
					}
				}
			}
			catch(Exception ex){
				throw new Exception(ex.ToString());
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

		public BCQueryResult FindAllVideos() {
			return FindAllVideos(-1);
		}
		
		public BCQueryResult FindAllVideos(int howMany) {
			return FindAllVideos(howMany, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null);
		}

		public BCQueryResult FindAllVideos(BCSortOrderType sortOrder) {
			return FindAllVideos(-1, BCSortByType.CREATION_DATE, sortOrder);
		}

		public BCQueryResult FindAllVideos(BCSortByType sortBy) {
			return FindAllVideos(-1, sortBy, BCSortOrderType.ASC);
		}

		public BCQueryResult FindAllVideos(BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindAllVideos(-1, sortBy, sortOrder);
		}

		public BCQueryResult FindAllVideos(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindAllVideos(howMany, sortBy, sortOrder, null);
		}

		public BCQueryResult FindAllVideos(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<String> video_fields) {
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
		public BCQueryResult FindAllVideos(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<string> video_fields, List<string> custom_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_all_videos");
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			reqparams.Add("sort_order", sortOrder.ToString());
			reqparams.Add("sort_by", sortBy.ToString());
			if (howMany >= 0) reqparams.Add("page_size", howMany.ToString());

			return MultipleQueryHandler(reqparams, BCObjectType.videos, Account);
		}

		#endregion Find All Videos

		#region Find Videos By User ID

		public BCQueryResult FindVideosByUserId(long userId) {
			return FindVideosByUserId(userId, -1, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null, null);
		}

		public BCQueryResult FindVideosByUserId(long userId, int howMany) {
			return FindVideosByUserId(userId, howMany, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null, null);
		}

		public BCQueryResult FindVideosByUserId(long userId, BCSortOrderType sortOrder) {
			return FindVideosByUserId(userId, -1, BCSortByType.CREATION_DATE, sortOrder, null, null);
		}

		public BCQueryResult FindVideosByUserId(long userId, BCSortByType sortBy) {
			return FindVideosByUserId(userId, -1, sortBy, BCSortOrderType.ASC, null, null);
		}

		public BCQueryResult FindVideosByUserId(long userId, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByUserId(userId, -1, sortBy, sortOrder, null, null);
		}

		public BCQueryResult FindVideosByUserId(long userId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByUserId(userId, howMany, sortBy, sortOrder, null, null);
		}

		public BCQueryResult FindVideosByUserId(long userId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<String> video_fields) {
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
		public BCQueryResult FindVideosByUserId(long userId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<String> video_fields, List<string> custom_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_videos_by_user_id");
			reqparams.Add("user_id", userId.ToString());
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			reqparams.Add("sort_order", sortOrder.ToString());
			reqparams.Add("sort_by", sortBy.ToString());
			if (howMany >= 0) reqparams.Add("page_size", howMany.ToString());

			return MultipleQueryHandler(reqparams, BCObjectType.videos, Account);
		}

		#endregion Find Videos By User ID

		#region Find Videos By Campaign ID

		public BCQueryResult FindVideosByCampaignId(long campaignId) {
			return FindVideosByCampaignId(campaignId, -1, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null, null);
		}

		public BCQueryResult FindVideosByCampaignId(long campaignId, int howMany) {
			return FindVideosByCampaignId(campaignId, howMany, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null, null);
		}

		public BCQueryResult FindVideosByCampaignId(long campaignId, BCSortOrderType sortOrder) {
			return FindVideosByCampaignId(campaignId, -1, BCSortByType.CREATION_DATE, sortOrder, null, null);
		}

		public BCQueryResult FindVideosByCampaignId(long campaignId, BCSortByType sortBy) {
			return FindVideosByCampaignId(campaignId, -1, sortBy, BCSortOrderType.ASC, null, null);
		}

		public BCQueryResult FindVideosByCampaignId(long campaignId, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByCampaignId(campaignId, -1, sortBy, sortOrder, null, null);
		}

		public BCQueryResult FindVideosByCampaignId(long campaignId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByCampaignId(campaignId, howMany, sortBy, sortOrder, null, null);
		}

		public BCQueryResult FindVideosByCampaignId(long campaignId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<String> video_fields) {
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
		public BCQueryResult FindVideosByCampaignId(long campaignId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<string> video_fields, List<string> custom_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_videos_by_campaign_id");
			reqparams.Add("campaign_id", campaignId.ToString());
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			reqparams.Add("sort_order", sortOrder.ToString());
			reqparams.Add("sort_by", sortBy.ToString());
			if (howMany >= 0) reqparams.Add("page_size", howMany.ToString());

			return MultipleQueryHandler(reqparams, BCObjectType.videos, Account);
		}

		#endregion Find Videos By Campaign ID

		#region Find Videos By Text

		public BCQueryResult FindVideosByText(string text) {
			return FindVideosByText(text, -1, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null, null);
		}

		public BCQueryResult FindVideosByText(string text, int howMany) {
			return FindVideosByText(text, howMany, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null, null);
		}

		public BCQueryResult FindVideosByText(string text, BCSortOrderType sortOrder) {
			return FindVideosByText(text, -1, BCSortByType.CREATION_DATE, sortOrder, null, null);
		}

		public BCQueryResult FindVideosByText(string text, BCSortByType sortBy) {
			return FindVideosByText(text, -1, sortBy, BCSortOrderType.ASC, null, null);
		}

		public BCQueryResult FindVideosByText(string text, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByText(text, -1, sortBy, sortOrder, null, null);
		}

		public BCQueryResult FindVideosByText(string text, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByText(text, howMany, sortBy, sortOrder, null, null);
		}

		public BCQueryResult FindVideosByText(string text, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<string> video_fields) {
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
		public BCQueryResult FindVideosByText(string text, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<string> video_fields, List<string> custom_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_videos_by_text");
			reqparams.Add("text", text);
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			reqparams.Add("sort_order", sortOrder.ToString());
			reqparams.Add("sort_by", sortBy.ToString());
			if (howMany >= 0) reqparams.Add("page_size", howMany.ToString());

			return MultipleQueryHandler(reqparams, BCObjectType.videos, Account);
		}

		#endregion Find Videos By Text

		#region Find Videos By Tags

		public BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags) {
			return FindVideosByTags(and_tags, or_tags, -1, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null);
		}

		public BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags, int howMany) {
			return FindVideosByTags(and_tags, or_tags, howMany, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null);
		}

		public BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags, BCSortByType sortBy) {
			return FindVideosByTags(and_tags, or_tags, -1, sortBy, BCSortOrderType.ASC);
		}

		public BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags, BCSortOrderType sortOrder) {
			return FindVideosByTags(and_tags, or_tags, -1, BCSortByType.CREATION_DATE, sortOrder);
		}

		public BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByTags(and_tags, or_tags, -1, sortBy, sortOrder);
		}

		public BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByTags(and_tags, or_tags, howMany, sortBy, sortOrder, null);
		}

		public BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<string> video_fields) {
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
		public BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<String> video_fields, List<String> custom_fields) {

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

			return MultipleQueryHandler(reqparams, BCObjectType.videos, Account);
		}

		#endregion Find Videos By Tags

		#region Find Related Videos

		public BCQueryResult FindRelatedVideos(long videoId) {
			return FindRelatedVideos(videoId, -1);
		}

		public BCQueryResult FindRelatedVideos(long videoId, int howMany) {
			return FindRelatedVideos(videoId, howMany, null);
		}

		public BCQueryResult FindRelatedVideos(long videoId, int howMany, List<String> video_fields) {
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
		public BCQueryResult FindRelatedVideos(long videoId, int howMany, List<String> video_fields, List<String> custom_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_related_videos");
			reqparams.Add("video_id", videoId.ToString());
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			if (howMany >= 0) reqparams.Add("page_size", howMany.ToString());

			return MultipleQueryHandler(reqparams, BCObjectType.videos, Account);
		}

		#endregion Find Related Videos

		#region Find Videos By IDs

		public BCQueryResult FindVideosByIds(List<long> videoIds) {
			return FindVideosByIds(videoIds, null);
		}

		public BCQueryResult FindVideosByIds(List<long> videoIds, List<String> video_fields) {
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
		public BCQueryResult FindVideosByIds(List<long> videoIds, List<String> video_fields, List<String> custom_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_videos_by_ids");
			reqparams.Add("video_ids", Implode(videoIds));
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			reqparams.Add("page_size", "-1");

			return MultipleQueryHandler(reqparams, BCObjectType.videos, Account);
		}

		#endregion Find Videos By IDs

		#region Find Videos By Reference IDs

		public BCQueryResult FindVideosByReferenceIds(List<String> referenceIds) {
			return FindVideosByReferenceIds(referenceIds, null);
		}

		public BCQueryResult FindVideosByReferenceIds(List<String> referenceIds, List<String> video_fields) {
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
		public BCQueryResult FindVideosByReferenceIds(List<String> referenceIds, List<String> video_fields, List<String> custom_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_videos_by_reference_ids");
			reqparams.Add("reference_ids", Implode(referenceIds));
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));

			return MultipleQueryHandler(reqparams, BCObjectType.videos, Account);
		}

		#endregion Find Videos By Reference IDs

		#region Find Video By ID

		public BCVideo FindVideoById(long videoId) {
			return FindVideoById(videoId, null);
		}

		public BCVideo FindVideoById(long videoId, List<String> video_fields) {
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
		public BCVideo FindVideoById(long videoId, List<String> video_fields, List<String> custom_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_video_by_id");
			reqparams.Add("video_id", videoId.ToString());
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));

			//Get the JSon reader returned from the APIRequest
			QueryResultPair qrp = BCAPIRequest.ExecuteRead(reqparams, Account);
								
			return JSON.Converter.Deserialize<BCVideo>(qrp.JsonResult);
		}

		#endregion Find Video By ID

		#region Find Video By Reference ID

		public BCVideo FindVideoByReferenceId(String referenceId) {
			return FindVideoByReferenceId(referenceId, null);
		}

		public BCVideo FindVideoByReferenceId(String referenceId, List<String> video_fields) {
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
		public BCVideo FindVideoByReferenceId(String referenceId, List<String> video_fields, List<String> custom_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_video_by_reference_id");
			reqparams.Add("reference_id", referenceId);
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));

			//Get the JSon reader returned from the APIRequest
			string jsonStr = BCAPIRequest.ExecuteRead(reqparams, Account).JsonResult;
			return JSON.Converter.Deserialize<BCVideo>(jsonStr);
		}

		#endregion Find Video By Reference ID

		#endregion Video Read

		#region Playlist Read

		#region Find All Playlists

		public BCQueryResult FindAllPlaylists() {
			return FindAllPlaylists(-1);
		}
		
		public BCQueryResult FindAllPlaylists(int howMany) {
			return FindAllPlaylists(howMany, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null);
		}

		public BCQueryResult FindAllPlaylists(int howMany, BCSortOrderType sortOrder) {
			return FindAllPlaylists(howMany, BCSortByType.CREATION_DATE, sortOrder, null);
		}

		public BCQueryResult FindAllPlaylists(int howMany, BCSortByType sortBy) {
			return FindAllPlaylists(howMany, sortBy, BCSortOrderType.ASC, null);
		}

		public BCQueryResult FindAllPlaylists(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindAllPlaylists(howMany, sortBy, sortOrder, null);
		}

		public BCQueryResult FindAllPlaylists(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<String> video_fields) {
			return FindAllPlaylists(howMany, sortBy, sortOrder, video_fields, null);
		}

		public BCQueryResult FindAllPlaylists(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<String> video_fields, List<String> custom_fields) {
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
		public BCQueryResult FindAllPlaylists(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<String> video_fields, List<String> custom_fields, List<string> playlist_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_all_playlists");
			reqparams.Add("sort_order", sortOrder.ToString());
			reqparams.Add("sort_by", sortBy.ToString());
			if (howMany >= 0) reqparams.Add("page_size", howMany.ToString());
			if (playlist_fields != null) reqparams.Add("playlist_fields", Implode(playlist_fields));
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));

			BCQueryResult qr = MultipleQueryHandler(reqparams, BCObjectType.playlists, Account);

			return qr;
		}

		#endregion Find All Playlists

		#region Find Playlist By Id

		public BCPlaylist FindPlaylistById(long playlist_id) {
			return FindPlaylistById(playlist_id, null);
		}

		public BCPlaylist FindPlaylistById(long playlist_id, List<string> video_fields) {
			return FindPlaylistById(playlist_id, video_fields, null);
		}

		public BCPlaylist FindPlaylistById(long playlist_id, List<string> video_fields, List<string> custom_fields) {
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
		public BCPlaylist FindPlaylistById(long playlist_id, List<string> video_fields, List<string> custom_fields, List<string> playlist_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_playlist_by_id");
			reqparams.Add("playlist_id", playlist_id.ToString());
			if (playlist_fields != null) reqparams.Add("playlist_fields", Implode(playlist_fields));
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));

			//Get the JSon reader returned from the APIRequest
			QueryResultPair qrp = BCAPIRequest.ExecuteRead(reqparams, Account);

			return JSON.Converter.Deserialize<BCPlaylist>(qrp.JsonResult);
		}

		#endregion Find Playlist By Id

		#region Find Playlists By Ids

		public BCQueryResult FindPlaylistsByIds(List<long> playlist_ids) {
			return FindPlaylistsByIds(playlist_ids, null);
		}

		public BCQueryResult FindPlaylistsByIds(List<long> playlist_ids, List<string> video_fields) {
			return FindPlaylistsByIds(playlist_ids, video_fields, null);
		}

		public BCQueryResult FindPlaylistsByIds(List<long> playlist_ids, List<string> video_fields, List<string> custom_fields) {
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
		public BCQueryResult FindPlaylistsByIds(List<long> playlist_ids, List<string> video_fields, List<string> custom_fields, List<string> playlist_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_playlists_by_ids");
			reqparams.Add("playlist_ids", Implode(playlist_ids));
			if (playlist_fields != null) reqparams.Add("playlist_fields", Implode(playlist_fields));
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));

			BCQueryResult qr = MultipleQueryHandler(reqparams, BCObjectType.playlists, Account);

			return qr;

		}

		#endregion Find Playlists By Ids

		#region Find Playlist By Reference Id

		public BCPlaylist FindPlaylistByReferenceId(string reference_id) {
			return FindPlaylistByReferenceId(reference_id, null);
		}

		public BCPlaylist FindPlaylistByReferenceId(string reference_id, List<string> video_fields) {
			return FindPlaylistByReferenceId(reference_id, video_fields, null);
		}

		public BCPlaylist FindPlaylistByReferenceId(string reference_id, List<string> video_fields, List<string> custom_fields) {
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
		public BCPlaylist FindPlaylistByReferenceId(string reference_id, List<string> video_fields, List<string> custom_fields, List<string> playlist_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_playlist_by_reference_id");
			reqparams.Add("reference_id", reference_id);
			if (playlist_fields != null) reqparams.Add("playlist_fields", Implode(playlist_fields));
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));

			//Get the JSon reader returned from the APIRequest
			string jsonStr = BCAPIRequest.ExecuteRead(reqparams, Account).JsonResult;
			return JSON.Converter.Deserialize<BCPlaylist>(jsonStr);
		}

		#endregion Find Playlist By Reference Id

		#region Find Playlists By Reference Ids

		public BCQueryResult FindPlaylistsByReferenceIds(List<string> reference_ids) {
			return FindPlaylistsByReferenceIds(reference_ids, null);
		}

		public BCQueryResult FindPlaylistsByReferenceIds(List<string> reference_ids, List<string> video_fields) {
			return FindPlaylistsByReferenceIds(reference_ids, video_fields, null);
		}

		public BCQueryResult FindPlaylistsByReferenceIds(List<string> reference_ids, List<string> video_fields, List<string> custom_fields) {
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
		public BCQueryResult FindPlaylistsByReferenceIds(List<string> reference_ids, List<string> video_fields, List<string> custom_fields, List<string> playlist_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_playlists_by_reference_ids");
			reqparams.Add("reference_ids", Implode(reference_ids));
			if (playlist_fields != null) reqparams.Add("playlist_fields", Implode(playlist_fields));
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			
			BCQueryResult qr = MultipleQueryHandler(reqparams, BCObjectType.playlists, Account);

			return qr;
		}

		#endregion Find Playlists By Ids

		#region Find Playlists For Player Id

		public BCQueryResult FindPlaylistsForPlayerId(long player_id) {
			return FindPlaylistsForPlayerId(player_id, -1);
		}

		public BCQueryResult FindPlaylistsForPlayerId(long player_id, int howMany) {
			return FindPlaylistsForPlayerId(player_id, howMany, null);
		}

		public BCQueryResult FindPlaylistsForPlayerId(long player_id, int howMany, List<string> video_fields) {
			return FindPlaylistsForPlayerId(player_id, howMany, video_fields, null);
		}

		public BCQueryResult FindPlaylistsForPlayerId(long player_id, int howMany, List<string> video_fields, List<string> custom_fields) {
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
		public BCQueryResult FindPlaylistsForPlayerId(long player_id, int howMany, List<string> video_fields, List<string> custom_fields, List<string> playlist_fields) {

			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_playlists_for_player_id");
			reqparams.Add("player_id", player_id.ToString());
			if (howMany >= 0) reqparams.Add("page_size", howMany.ToString());
			if (playlist_fields != null) reqparams.Add("playlist_fields", Implode(playlist_fields));
			if (video_fields != null) reqparams.Add("video_fields", Implode(video_fields));
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));

			BCQueryResult qr = MultipleQueryHandler(reqparams, BCObjectType.playlists, Account);

			return qr;
		}

		#endregion Find Playlists For Player Id

		#endregion Playlist Read

		#region Video Write

		#region Create Video
				
		//favors no processing renditions
		public RPCResponse<long> CreateVideo(BCVideo video, string filename, byte[] file, bool H264NoProcessing) {
			return CreateVideo(video, filename, file, H264NoProcessing, false);
		}
		public RPCResponse<long> CreateVideo(BCVideo video, string filename, byte[] file, bool H264NoProcessing, bool preserve_source_rendition) {
			return CreateVideo(video, filename, file, H264NoProcessing, preserve_source_rendition, -1);
		}
		public RPCResponse<long> CreateVideo(BCVideo video, string filename, byte[] file, bool H264NoProcessing, bool preserve_source_rendition, long maxsize) {
			return CreateVideo(video, filename, file, H264NoProcessing, preserve_source_rendition, maxsize, null);
		}
		public RPCResponse<long> CreateVideo(BCVideo video, string filename, byte[] file, bool H264NoProcessing, bool preserve_source_rendition, long maxsize, string file_checksum) {
			return CreateVideo(video, filename, file, BCEncodeType.UNDEFINED , false, H264NoProcessing, preserve_source_rendition, maxsize, file_checksum);
		}
	
		//favors multiple renditions
		public RPCResponse<long> CreateVideo(BCVideo video, string filename, byte[] file) {
			return CreateVideo(video, filename, file, BCEncodeType.UNDEFINED);
		}
		public RPCResponse<long> CreateVideo(BCVideo video, string filename, byte[] file, BCEncodeType encode_to) {
			return CreateVideo(video, filename, file, encode_to, false);
		}
		public RPCResponse<long> CreateVideo(BCVideo video, string filename, byte[] file, BCEncodeType encode_to, bool create_multiple_renditions) {
			return CreateVideo(video, filename, file, encode_to, create_multiple_renditions, false);
		}
		public RPCResponse<long> CreateVideo(BCVideo video, string filename, byte[] file, BCEncodeType encode_to, bool create_multiple_renditions, bool preserve_source_rendition) {
			return CreateVideo(video, filename, file, encode_to, create_multiple_renditions, preserve_source_rendition, -1);
		}
		public RPCResponse<long> CreateVideo(BCVideo video, string filename, byte[] file, BCEncodeType encode_to, bool create_multiple_renditions, bool preserve_source_rendition, long maxsize) {
			return CreateVideo(video, filename, file, encode_to, create_multiple_renditions, preserve_source_rendition, maxsize, null);
		}
		public RPCResponse<long> CreateVideo(BCVideo video, string filename, byte[] file, BCEncodeType encode_to, bool create_multiple_renditions, bool preserve_source_rendition, long maxsize, string file_checksum) {
			return CreateVideo(video, filename, file, encode_to, create_multiple_renditions, false, preserve_source_rendition, maxsize, file_checksum);
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
		/// <param name="file">
		/// A byte array of the video file you're uploading. This takes the 
		/// form of a file part, in a multipart/form-data HTTP request. This input stream and 
		/// the filename and maxSide parameters are automatically inferred from that file part.
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
		/// <param name="maxsize">
		/// The maximum size that the file will be. This is used as a limiter to know when 
		/// something has gone wrong with the upload. The maxSize is same as the file you uploaded. 
		/// You don't need to specify this in the JSON if it is specified in the file part of the POST.  
		/// </param>
		/// <param name="file_checksum">
		/// An optional MD5 hash of the file. The checksum can be used to verify that the file checked 
		/// into your Brightcove Media Library is the same as the file you uploaded. 
		/// </param>
		/// <returns>
		/// The id of the video that's been created. if null or error returns -1
		/// </returns>
		private RPCResponse<long> CreateVideo(BCVideo video, string filename, byte[] file, BCEncodeType encode_to, bool create_multiple_renditions, bool H264NoProcessing, bool preserve_source_rendition, long maxsize, string file_checksum) {

			// Generate post objects
			Dictionary<string, object> postParams = new Dictionary<string, object>();
						
			//add video to the post params
			RPCRequest rpc = new RPCRequest();
			rpc.method = "create_video";
			rpc.parameters = "\"video\": " + video.ToCreateJSON() + ", \"token\": \"" + Account.WriteToken.Value + "\"";
			if (maxsize > -1) {
				rpc.parameters += ", \"maxsize\": " + maxsize.ToString();
			}
			if (file_checksum != null) {
				rpc.parameters += ", \"file_checksum\": \"" + file_checksum + "\"";
			}
			rpc.parameters += ", \"filename\": \"" + filename + "\"";
			if (!encode_to.Equals(BCEncodeType.UNDEFINED)) {
				rpc.parameters += ", \"encode_to\": " + encode_to.ToString();
			}
			rpc.parameters += ", \"create_multiple_renditions\": " + create_multiple_renditions.ToString().ToLower();
			rpc.parameters += ", \"H264NoProcessing\": " + H264NoProcessing.ToString().ToLower();
			rpc.parameters += ", \"preserve_source_rendition\": " + preserve_source_rendition.ToString().ToLower();
			postParams.Add("json", rpc.ToJSON());
			
			//add the file to the post
			postParams.Add("file", new FileParameter(file, filename));

			//Get the JSon reader returned from the APIRequest
			RPCResponse rpcr = BCAPIRequest.ExecuteWrite(postParams, Account);
			RPCResponse<long> rpcr2 = new RPCResponse<long>();
			rpcr2.error = rpcr.error;
			rpcr2.id = rpcr.id;
			if (!string.IsNullOrEmpty(rpcr.result)) {
				rpcr2.result = long.Parse(rpcr.result);
			}
			else {
				rpcr2.result = -1;
			}

			return rpcr2;
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
		public RPCResponse<BCVideo> UpdateVideo(BCVideo video) {

			// Generate post objects
			Dictionary<string, object> postParams = new Dictionary<string, object>();
						
			//add video to the post params
			RPCRequest rpc = new RPCRequest();
			rpc.method = "update_video";
			rpc.parameters = "\"video\": " + video.ToJSON() + " ,\"token\": \"" + Account.WriteToken.Value + "\"";
			postParams.Add("json", rpc.ToJSON());
						
			//Get the JSon reader returned from the APIRequest
			RPCResponse<BCVideo> rpcr = BCAPIRequest.ExecuteWrite<BCVideo>(postParams, Account);
			
			return rpcr;
		}

		#endregion Update Video

		#region Delete Video

		//delete by video id
		public RPCResponse DeleteVideo(long video_id) {
			return DeleteVideo(video_id, true);
		}

		public RPCResponse DeleteVideo(long video_id, bool cascade) {
			return DeleteVideo(video_id, cascade, true);
		}

		public RPCResponse DeleteVideo(long video_id, bool cascade, bool delete_shares) {
			return DeleteVideo(video_id, null, cascade, delete_shares);
		}

		//delete by reference id
		public RPCResponse DeleteVideo(string reference_id) {
			return DeleteVideo(reference_id, true);
		}

		public RPCResponse DeleteVideo(string reference_id, bool cascade) {
			return DeleteVideo(reference_id, cascade, true);
		}

		public RPCResponse DeleteVideo(string reference_id, bool cascade, bool delete_shares) {
			return DeleteVideo(-1, reference_id, cascade, delete_shares);
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
		/// defaults to true
		/// </param>
		/// <param name="delete_shares">
		/// Set this to true if you want also to delete shared copies of this video. 
		/// Note that this will delete all shared copies from sharee accounts, regardless 
		/// of whether or not those accounts are currently using the video in playlists or players.
		/// defaults to true
		/// </param>
		private RPCResponse DeleteVideo(long video_id, string reference_id, bool cascade, bool delete_shares) {

			// Generate post objects
			Dictionary<string, object> postParams = new Dictionary<string, object>();
						
			//add video to the post params
			RPCRequest rpc = new RPCRequest();
			rpc.method = "delete_video";
			if (video_id > -1) {
				rpc.parameters = "\"video_id\": " + video_id.ToString();
			}
			else if (reference_id != null) {
				rpc.parameters = "\"reference_id\": \"" + reference_id.ToString() + "\"";
			}
			rpc.parameters += ", \"token\": \"" + Account.WriteToken.Value + "\"";
			rpc.parameters += ", \"cascade\": " + cascade.ToString().ToLower();
			rpc.parameters += ", \"delete_shares\": " + delete_shares.ToString().ToLower();
			postParams.Add("json", rpc.ToJSON());
			
			//Get the JSon reader returned from the APIRequest
			RPCResponse rpcr = BCAPIRequest.ExecuteWrite(postParams, Account);

			return rpcr;
		}

		#endregion Delete Video

		#region Get Upload Status

		public RPCResponse<UploadStatusEnum>	 GetUploadStatus(string reference_id) {
			return GetUploadStatus(-1, reference_id);
		}

		public RPCResponse<UploadStatusEnum> GetUploadStatus(long video_id) {
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
		private RPCResponse<UploadStatusEnum> GetUploadStatus(long video_id, string reference_id) {

			// Generate post objects
			Dictionary<string, object> postParams = new Dictionary<string, object>();

			//add video to the post params
			RPCRequest rpc = new RPCRequest();
			rpc.method = "get_upload_status";
			if (video_id > -1) {
				rpc.parameters = "\"video_id\": " + video_id.ToString();
			}
			else if (reference_id != null) {
				rpc.parameters = "\"reference_id\": " + video_id.ToString();
			}
			rpc.parameters += " ,\"token\": \"" + Account.WriteToken.Value + "\"";
			
			postParams.Add("json", rpc.ToJSON());

			//Get the JSon reader returned from the APIRequest
			RPCResponse rpcr = BCAPIRequest.ExecuteWrite(postParams, Account);
			RPCResponse<UploadStatusEnum> rpcr2 = new RPCResponse<UploadStatusEnum>();
			rpcr2.error = rpcr.error;
			rpcr2.id = rpcr.id;
			
			switch (rpcr.result) {
			    case "COMPLETE":
			        rpcr2.result = UploadStatusEnum.COMPLETE;
					break;
			    case "ERROR":
			        rpcr2.result = UploadStatusEnum.ERROR;
					break;
				case "PROCESSING":
			        rpcr2.result = UploadStatusEnum.PROCESSING;
					break;
				case "UPLOADING":
			        rpcr2.result = UploadStatusEnum.UPLOADING;
					break;
				default:
			        rpcr2.result = UploadStatusEnum.UNDEFINED;
					break;
			}
			return rpcr2;
		}

		#endregion Get Upload Status

		#region Share Video

		public RPCResponse<BCCollection<long>> ShareVideo(long video_id, long sharee_account_id) {
			return ShareVideo(video_id, false, sharee_account_id);
		}

		public RPCResponse<BCCollection<long>> ShareVideo(long video_id, bool auto_accept, long sharee_account_id) {
			
			List<long> sharee_account_ids = new List<long>();
			sharee_account_ids.Add(sharee_account_id);
			
			return ShareVideo(video_id, false, sharee_account_ids);
		}

		public RPCResponse<BCCollection<long>> ShareVideo(long video_id, List<long> sharee_account_ids) {
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
		/// defaults to false
		/// </param>
		/// <param name="sharee_account_ids">
		/// List of Account IDs to share video with.
		/// </param>
		/// <returns></returns>
		public RPCResponse<BCCollection<long>> ShareVideo(long video_id, bool auto_accept, List<long> sharee_account_ids) {

			// Generate post objects
			Dictionary<string, object> postParams = new Dictionary<string, object>();

			//add video to the post params
			RPCRequest rpc = new RPCRequest();
			rpc.method = "share_video";
			rpc.parameters = "\"video_id\": " + video_id;
			rpc.parameters += ", \"auto_accept\": " + auto_accept.ToString().ToLower();
			rpc.parameters += ", \"sharee_account_ids\": [";
			for(int i = 0; i < sharee_account_ids.Count; i++){
				if (i > 0) {
					rpc.parameters += ", ";
				}
				rpc.parameters += "\"" + sharee_account_ids[i].ToString() + "\"";
			}
			rpc.parameters += "]";
			rpc.parameters += ", \"token\": \"" + Account.WriteToken.Value + "\"";
			postParams.Add("json", rpc.ToJSON());

			//Get the JSon reader returned from the APIRequest
			RPCResponse<BCCollection<long>> rpcr = BCAPIRequest.ExecuteWrite<BCCollection<long>>(postParams, Account);

			return rpcr;
		}

		#endregion Share Video

		#region Add Image

		//using video id
		public RPCResponse<BCImage> AddImage(BCImage image, string filename, byte[] file, long video_id) {
			return AddImage(image, filename, file, video_id, true);
		}
		public RPCResponse<BCImage> AddImage(BCImage image, string filename, byte[] file, long video_id, bool resize) {
			return AddImage(image, filename, file, video_id, resize, -1);
		}
		public RPCResponse<BCImage> AddImage(BCImage image, string filename, byte[] file, long video_id, bool resize, long maxsize) {
			return AddImage(image, filename, file, video_id, resize, maxsize, null);
		}
		public RPCResponse<BCImage> AddImage(BCImage image, string filename, byte[] file, long video_id, bool resize, long maxsize, string file_checksum) {
			return AddImage(image, filename, file, video_id, null, resize, maxsize, file_checksum);
		}

		//using ref id
		public RPCResponse<BCImage> AddImage(BCImage image, string filename, byte[] file, string video_reference_id) {
			return AddImage(image, filename, file, video_reference_id, true);
		}
		public RPCResponse<BCImage> AddImage(BCImage image, string filename, byte[] file, string video_reference_id, bool resize) {
			return AddImage(image, filename, file, video_reference_id, resize, -1);
		}
		public RPCResponse<BCImage> AddImage(BCImage image, string filename, byte[] file, string video_reference_id, bool resize, long maxsize) {
			return AddImage(image, filename, file, video_reference_id, resize, maxsize, null);
		}
		public RPCResponse<BCImage> AddImage(BCImage image, string filename, byte[] file, string video_reference_id, bool resize, long maxsize, string file_checksum) {
			return AddImage(image, filename, file, -1, video_reference_id, resize, maxsize, file_checksum);
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
		/// maxSize parameters are automatically inferred from that file part. 
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
		private RPCResponse<BCImage> AddImage(BCImage image, string filename, byte[] file, long video_id, string video_reference_id, bool resize, long maxsize, string file_checksum) {

			// Generate post objects
			Dictionary<string, object> postParams = new Dictionary<string, object>();

			//add video to the post params
			RPCRequest rpc = new RPCRequest();
			rpc.method = "add_image";
			rpc.parameters = "\"image\": " + image.ToJSON();
			rpc.parameters += ", \"filename\": \"" + filename + "\"";
			if(video_id > -1){
				rpc.parameters += ",\"video_id\": \"" + video_id.ToString() + "\"";
			}
			else if (video_reference_id != null) {
				rpc.parameters += ",\"video_reference_id\": \"" + video_reference_id + "\"";
			}
			if (maxsize > -1) {
				rpc.parameters += ", \"maxsize\": \"" + maxsize.ToString() + "\"";
			}
			
			if (file_checksum != null) {
				rpc.parameters += ", \"file_checksum\": \"" + file_checksum + "\"";
			}
			rpc.parameters += ", \"token\": \"" + Account.WriteToken.Value + "\"";
			postParams.Add("json", rpc.ToJSON());

			//add the file to the post
			postParams.Add("file", new FileParameter(file, filename));

			//Get the JSon reader returned from the APIRequest
			RPCResponse<BCImage> rpcr = BCAPIRequest.ExecuteWrite<BCImage>(postParams, Account);
					
			return rpcr;
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
		public RPCResponse<long> CreatePlaylist(BCPlaylist playlist) {

			// Generate post objects
			Dictionary<string, object> postParams = new Dictionary<string, object>();

			//add video to the post params
			RPCRequest rpc = new RPCRequest();
			rpc.method = "create_playlist";
			rpc.parameters = "\"playlist\": " + playlist.ToCreateJSON() + " ,\"token\": \"" + Account.WriteToken.Value + "\"";
			postParams.Add("json", rpc.ToJSON());

			//Get the JSon reader returned from the APIRequest
			RPCResponse<long> rpcr = BCAPIRequest.ExecuteWrite<long>(postParams, Account);

			return rpcr;
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
		public RPCResponse<BCPlaylist> UpdatePlaylist(BCPlaylist playlist) {

			// Generate post objects
			Dictionary<string, object> postParams = new Dictionary<string, object>();

			//add video to the post params
			RPCRequest rpc = new RPCRequest();
			rpc.method = "update_playlist";
			rpc.parameters = "\"playlist\": " + playlist.ToUpdateJSON() + " ,\"token\": \"" + Account.WriteToken.Value + "\"";
			postParams.Add("json", rpc.ToJSON());

			//Get the JSon reader returned from the APIRequest
			RPCResponse<BCPlaylist> rpcr = BCAPIRequest.ExecuteWrite<BCPlaylist>(postParams, Account);
			
			return rpcr;
		}

		#endregion Update Playlist

		#region Delete Playlist

		//by reference id
		public RPCResponse DeletePlaylist(string reference_id) {
			return DeletePlaylist(reference_id, false);
		}
		public RPCResponse DeletePlaylist(string reference_id, bool cascade) {
			return DeletePlaylist(-1, reference_id, cascade);
		}

		//by video id
		public RPCResponse DeletePlaylist(long playlist_id) {
			return DeletePlaylist(playlist_id, false);
		}
		public RPCResponse DeletePlaylist(long playlist_id, bool cascade) {
			return DeletePlaylist(playlist_id, null, cascade);
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
		/// <returns>
		/// RPC Response Object
		/// </returns>
		private RPCResponse DeletePlaylist(long playlist_id, string reference_id, bool cascade) {
			
			// Generate post objects
			Dictionary<string, object> postParams = new Dictionary<string, object>();

			//add video to the post params
			RPCRequest rpc = new RPCRequest();
			rpc.method = "delete_playlist";
			if (playlist_id > -1) {
				rpc.parameters = "\"playlist_id\": " + playlist_id.ToString();
			}
			else if (reference_id != null) {
				rpc.parameters = "\"reference_id\": " + reference_id.ToString();
			}
			rpc.parameters += ", \"token\": \"" + Account.WriteToken.Value + "\"";
			rpc.parameters += ", \"cascade\": " + cascade.ToString().ToLower();
			postParams.Add("json", rpc.ToJSON());
			
			//Get the JSon reader returned from the APIRequest
			RPCResponse rpcr = BCAPIRequest.ExecuteWrite(postParams, Account);
			
			return rpcr;
		}

		#endregion Delete Playlist

		#endregion Playlist Write
	}
}
