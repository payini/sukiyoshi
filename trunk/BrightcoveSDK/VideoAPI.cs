using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;
using BrightcoveSDK.Extensions;
using BrightcoveSDK.HTTP;
using BrightcoveSDK.Media;
using BrightcoveSDK.JSON;
using System.Configuration;

namespace BrightcoveSDK
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
			return (BCQueryResult)MultipleQueryHandler<object>(reqparams, itemType, account);
		}

		private static BCQueryResult<CustomFieldType> MultipleQueryHandler<CustomFieldType>(Dictionary<String, String> reqparams, BCObjectType itemType, AccountConfigElement account) {
			
			//Get the JSon reader returned from the APIRequest
			BCQueryResult<CustomFieldType> qr = new BCQueryResult<CustomFieldType>();
			qr.TotalCount = 0;

			try {

				//set some global request paramameters
                if (!reqparams.ContainsKey("page_number")) {
                    reqparams.Add("page_number", "0");
                }

				//set if not set or 
				if (!reqparams.ContainsKey("page_size")) {
					qr.MaxToGet = -1;
				}
				else {
					qr.MaxToGet = Convert.ToInt32(reqparams["page_size"]);
				}

				//get initial query
				double maxPageNum = 0;

				QueryResultPair qrp = BCAPIRequest.ExecuteRead(reqparams, account);
				//convert the result for deserialization
				qrp.JsonResult = qrp.JsonResult.Replace("\"items\":", "\"" + itemType.ToString() + "\":");
				qr.QueryResults.Add(qrp);
                qr.Merge(JSON.Converter.Deserialize<BCQueryResult<CustomFieldType>>(qrp.JsonResult));

                //make sure you get the correct page num
                if (qr.TotalCount > 0) {
                    //if you want all use the total count to calculate the number of pages
                    if (qr.MaxToGet.Equals(-1)) {
                        maxPageNum = Math.Ceiling((double)(qr.TotalCount / 100));
                    }
                    //or just use the max you want to calculate the number of pages
				    else {
					    maxPageNum = Math.Ceiling((double)(qr.MaxToGet / 100));
				    }
                }

				//if there are more to get move to next page and keep getting them
				for (int pageNum = 1; pageNum <= maxPageNum; pageNum++ ) {

					//update page each iteration
					reqparams["page_number"] = pageNum.ToString();
					
					QueryResultPair qrp2 = BCAPIRequest.ExecuteRead(reqparams, account);
					//convert the result for deserialization
					qrp2.JsonResult = qrp2.JsonResult.Replace("\"items\":", "\"" + itemType.ToString() + "\":");
					qr.QueryResults.Add(qrp2);
					qr.Merge(JSON.Converter.Deserialize<BCQueryResult<CustomFieldType>>(qrp2.JsonResult));				
				}

				//sorting on our end

				if (itemType.Equals(BCObjectType.videos) && reqparams.ContainsKey("sort_by")) {
					//PUBLISH_DATE, 
					if (reqparams["sort_by"].Equals("PUBLISH_DATE")) {
						qr.Videos.Sort(BCVideo<CustomFieldType>.PublishDateComparison);
					}
					//PLAYS_TOTAL, 
					else if (reqparams["sort_by"].Equals("PLAYS_TOTAL")) {
						qr.Videos.Sort(BCVideo<CustomFieldType>.TotalPlaysComparison);
					}
					//PLAYS_TRAILING_WEEK
					else if (reqparams["sort_by"].Equals("PLAYS_TRAILING_WEEK")) {
						qr.Videos.Sort(BCVideo<CustomFieldType>.PlaysTrailingComparison);
					}
                    //MODIFIED_DATE,
                    else if (reqparams["sort_by"].Equals("MODIFIED_DATE")) {
						qr.Videos.Sort(BCVideo<CustomFieldType>.ModifiedDateComparison);
					}
                    //CREATION_DATE, 
					else {
						qr.Videos.Sort(BCVideo<CustomFieldType>.CreationDateComparison);
					}

					//if they want asc
					if (reqparams["sort_order"].Equals("DESC")) {
						qr.Videos.Reverse();
					}
					
					//trim if specified
					if (qr.Videos.Count > qr.MaxToGet && !qr.MaxToGet.Equals(-1) && qr.MaxToGet < qr.TotalCount) {
						List<BCVideo<CustomFieldType>> vidTemp = qr.Videos.GetRange(0, Convert.ToInt32(qr.MaxToGet));

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

        #region Search Videos

        /*
        public BCQueryResult SearchVideos(int howMany, Dictionary<VideoFields, string> required_matches) {
            return SearchVideos(howMany, required_matches, null);
        }

        public BCQueryResult SearchVideos(int howMany, Dictionary<VideoFields, string> required_matches, Dictionary<VideoFields, string> at_least_one_match) {
            return SearchVideos(howMany, required_matches, at_least_one_match, null);
        }

        public BCQueryResult SearchVideos(int howMany, Dictionary<VideoFields, string> required_matches, Dictionary<VideoFields, string> at_least_one_match, Dictionary<VideoFields, string> must_not_match) {
            return SearchVideos(howMany, required_matches, at_least_one_match, must_not_match, BCSortOrderType.ASC);
        }

        public BCQueryResult SearchVideos(int howMany, Dictionary<VideoFields, string> required_matches, Dictionary<VideoFields, string> at_least_one_match, Dictionary<VideoFields, string> must_not_match, BCSortOrderType sortOrder) {
            return SearchVideos(howMany, required_matches, at_least_one_match, must_not_match, sortOrder, true);
        }

        public BCQueryResult SearchVideos(int howMany, Dictionary<VideoFields, string> required_matches, Dictionary<VideoFields, string> at_least_one_match, Dictionary<VideoFields, string> must_not_match, BCSortOrderType sortOrder, bool exact) {
            return SearchVideos(howMany, required_matches, at_least_one_match, must_not_match, sortOrder, exact, null);
        }

        public BCQueryResult SearchVideos(int howMany, Dictionary<VideoFields, string> required_matches, Dictionary<VideoFields, string> at_least_one_match, Dictionary<VideoFields, string> must_not_match, BCSortOrderType sortOrder, bool exact, List<VideoFields> video_fields) {
            return SearchVideos(howMany, required_matches, at_least_one_match, must_not_match, sortOrder, exact, video_fields, null);
        }

        public BCQueryResult SearchVideos(int howMany, Dictionary<VideoFields, string> required_matches, Dictionary<VideoFields, string> at_least_one_match, Dictionary<VideoFields, string> must_not_match, BCSortOrderType sortOrder, bool exact, List<VideoFields> video_fields, List<string> custom_fields) {
            return SearchVideos(howMany, required_matches, at_least_one_match, must_not_match, sortOrder, exact, video_fields, custom_fields, MediaDeliveryTypeEnum.DEFAULT);
        }

        public BCQueryResult SearchVideos(int howMany, BCSortOrderType sortOrder) {
            return SearchVideos(howMany, sortOrder, true);
        }

        public BCQueryResult SearchVideos(int howMany, BCSortOrderType sortOrder, bool exact) {
            return SearchVideos(howMany, null, null, null, sortOrder, exact);
        }

        public BCQueryResult SearchVideos(int howMany, BCSortOrderType sortOrder, List<VideoFields> video_fields) {
            return SearchVideos(howMany, sortOrder, true, video_fields, null);
        }

        public BCQueryResult SearchVideos(int howMany, bool exact, List<VideoFields> video_fields) {
            return SearchVideos(howMany, BCSortOrderType.ASC, exact, video_fields, null);
        }

        public BCQueryResult SearchVideos(int howMany, bool exact, List<VideoFields> video_fields) {
            return SearchVideos(howMany, BCSortOrderType.ASC, exact, video_fields, null);
        }

        public BCQueryResult SearchVideos(int howMany, BCSortOrderType sortOrder, bool exact, List<VideoFields> video_fields) {
            return SearchVideos(howMany, sortOrder, exact, video_fields, null);
        }

        public BCQueryResult SearchVideos(int howMany, BCSortOrderType sortOrder, bool exact, List<VideoFields> video_fields, List<string> custom_fields) {
            return SearchVideos(howMany, sortOrder, exact, video_fields, custom_fields, MediaDeliveryTypeEnum.DEFAULT);
        }

        public BCQueryResult SearchVideos(int howMany, BCSortOrderType sortOrder, bool exact, List<VideoFields> video_fields, List<string> custom_fields, MediaDeliveryTypeEnum media_delivery) {
            return SearchVideos(howMany, null, null, null, sortOrder, exact, video_fields, custom_fields, MediaDeliveryTypeEnum.DEFAULT);
        }
        */

		public BCQueryResult SearchVideos(int howMany, Dictionary<VideoFields, string> required_matches, Dictionary<VideoFields, string> at_least_one_match, Dictionary<VideoFields, string> must_not_match, BCSortOrderType sortOrder, bool exact, List<VideoFields> video_fields, List<string> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			
			Dictionary<String, String> reqparams = SearchVideosReqParams(howMany, required_matches, at_least_one_match, must_not_match, sortOrder, exact, video_fields, custom_fields, media_delivery);
			return MultipleQueryHandler(reqparams, BCObjectType.videos, Account);
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="howMany">
        /// Number of items returned per page. A page is a subset of all of the items that satisfy the request. The maximum page size is 100; if you do not set this argument, or if you set it to an integer > 100, your results will come back as if you had set page_size=100.
        /// </param>
        /// <param name="required_matches">
        /// Specifies the field:value pairs for search criteria that MUST be present in the index in order to return a hit in the result set. The format is fieldName:value. If the field's name is not present, it is assumed to be name and shortDescription.
        /// </param>
        /// <param name="at_least_one_match">
        /// Specifies the field:value pairs for search criteria AT LEAST ONE of which must be present to return a hit in the result set. The format is fieldName:value. If the field's name is not present, it is assumed to be name and shortDescription.
        /// </param>
        /// <param name="must_not_match">
        /// Specifies the field:value pairs for search criteria that MUST NOT be present to return a hit in the result set. The format is fieldName:value. If the field's name is not present, it is assumed to be name and shortDescription.
        /// </param>
        /// <param name="sortOrder">
        /// Specifies the field to sort by, and the direction to sort in. This is specified as: sortFieldName:direction If the direction is not provided, it is assumed to be in ascending order Specify the direction as "asc" for ascending or "desc" for descending.
        /// </param>
        /// <param name="exact">
        /// If true, disables fuzzy search and requires an exact match of search terms. A fuzzy search does not require an exact match of the indexed terms, but will return a hit for terms that are closely related based on language-specific criteria. The fuzzy search is available only if your account is based in the United States.
        /// </param>
        /// <param name="video_fields">
        /// A comma-separated list of the fields you wish to have populated in the Videos  contained in the returned object. If you omit this parameter, the method returns the following fields of the video: id, name, shortDescription, longDescription, creationDate, publisheddate, lastModifiedDate, linkURL, linkText, tags, videoStillURL, thumbnailURL, referenceId, length, economics, playsTotal, playsTrailingWeek. If you use a token with URL access, this method also returns FLVURL, renditions, FLVFullLength, videoFullLength.
        /// </param>
        /// <param name="custom_fields">
        /// A comma-separated list of the custom fields  you wish to have populated in the videos contained in the returned object. If you omit this parameter, no custom fields are returned, unless you include the value 'customFields' in the video_fields parameter.
        /// </param>
        /// <param name="media_delivery">
        /// If universal delivery service  is enabled for your account, set this optional parameter to http to return video by HTTP, rather than streaming. Meaningful only if used together with the video_fields=FLVURL, videoFullLength, or renditions parameters. This is a MediaDeliveryTypeEnum with a value of http or default.
        /// </param>
        /// <returns></returns>
		public BCQueryResult<CustomFieldType> SearchVideos<CustomFieldType>(int howMany, Dictionary<VideoFields, string> required_matches, Dictionary<VideoFields, string> at_least_one_match, Dictionary<VideoFields, string> must_not_match, BCSortOrderType sortOrder, bool exact, List<VideoFields> video_fields, List<string> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			
			Dictionary<String, String> reqparams = SearchVideosReqParams(howMany, required_matches, at_least_one_match, must_not_match, sortOrder, exact, video_fields, custom_fields, media_delivery);
			return MultipleQueryHandler<CustomFieldType>(reqparams, BCObjectType.videos, Account);
		}

		private Dictionary<String, String> SearchVideosReqParams(int howMany, Dictionary<VideoFields, string> required_matches, Dictionary<VideoFields, string> at_least_one_match, Dictionary<VideoFields, string> must_not_match, BCSortOrderType sortOrder, bool exact, List<VideoFields> video_fields, List<string> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "search_videos");
			if (required_matches != null) reqparams.Add("all", required_matches.DicToString());
			if (at_least_one_match != null) reqparams.Add("any", at_least_one_match.DicToString());
			if (must_not_match != null) reqparams.Add("none", must_not_match.DicToString());
			reqparams.Add("exact", exact.ToString());
			reqparams.Add("media_delivery", media_delivery.ToString());
			if (video_fields != null) reqparams.Add("video_fields", video_fields.ToFieldString());
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			reqparams.Add("sort_order", sortOrder.ToString());
			if (howMany >= 0) reqparams.Add("page_size", howMany.ToString());
			return reqparams;
		}

        #endregion Search Videos

		#region Find All Videos

		public BCQueryResult FindAllVideos() {
			return FindAllVideos(-1);
		}
		public BCQueryResult<CustomFieldType> FindAllVideos<CustomFieldType>() {
			return FindAllVideos<CustomFieldType>(-1);
		}
		
		public BCQueryResult FindAllVideos(int howMany) {
			return FindAllVideos(howMany, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null);
		}
		public BCQueryResult<CustomFieldType> FindAllVideos<CustomFieldType>(int howMany) {
			return FindAllVideos<CustomFieldType>(howMany, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null);
		}

		public BCQueryResult FindAllVideos(BCSortOrderType sortOrder) {
			return FindAllVideos(-1, BCSortByType.CREATION_DATE, sortOrder);
		}
		public BCQueryResult<CustomFieldType> FindAllVideos<CustomFieldType>(BCSortOrderType sortOrder) {
			return FindAllVideos<CustomFieldType>(-1, BCSortByType.CREATION_DATE, sortOrder);
		}

		public BCQueryResult FindAllVideos(BCSortByType sortBy) {
			return FindAllVideos(-1, sortBy, BCSortOrderType.ASC);
		}
		public BCQueryResult<CustomFieldType> FindAllVideos<CustomFieldType>(BCSortByType sortBy) {
			return FindAllVideos<CustomFieldType>(-1, sortBy, BCSortOrderType.ASC);
		}

		public BCQueryResult FindAllVideos(BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindAllVideos(-1, sortBy, sortOrder);
		}
		public BCQueryResult<CustomFieldType> FindAllVideos<CustomFieldType>(BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindAllVideos<CustomFieldType>(-1, sortBy, sortOrder);
		}

		public BCQueryResult FindAllVideos(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindAllVideos(howMany, sortBy, sortOrder, null);
		}
		public BCQueryResult<CustomFieldType> FindAllVideos<CustomFieldType>(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindAllVideos<CustomFieldType>(howMany, sortBy, sortOrder, null);
		}

		public BCQueryResult FindAllVideos(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields) {
			return FindAllVideos(howMany, sortBy, sortOrder, video_fields, null);
		}
		public BCQueryResult<CustomFieldType> FindAllVideos<CustomFieldType>(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields) {
			return FindAllVideos<CustomFieldType>(howMany, sortBy, sortOrder, video_fields, null);
		}

        public BCQueryResult FindAllVideos(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields) {
            return FindAllVideos(howMany, sortBy, sortOrder, video_fields, custom_fields, MediaDeliveryTypeEnum.DEFAULT);
        }
		public BCQueryResult<CustomFieldType> FindAllVideos<CustomFieldType>(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields) {
			return FindAllVideos<CustomFieldType>(howMany, sortBy, sortOrder, video_fields, custom_fields, MediaDeliveryTypeEnum.DEFAULT);
		}

		public BCQueryResult FindAllVideos(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			
			Dictionary<String, String> reqparams = FindAllVideosReqParams(howMany, sortBy, sortOrder, video_fields, custom_fields, media_delivery);
			return MultipleQueryHandler(reqparams, BCObjectType.videos, Account);
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
		public BCQueryResult<CustomFieldType> FindAllVideos<CustomFieldType>(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			
			Dictionary<String, String> reqparams = FindAllVideosReqParams(howMany, sortBy, sortOrder, video_fields, custom_fields, media_delivery);
			return MultipleQueryHandler<CustomFieldType>(reqparams, BCObjectType.videos, Account);
		}

		private Dictionary<String, String> FindAllVideosReqParams(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_all_videos");
			if (video_fields != null) reqparams.Add("video_fields", video_fields.ToFieldString());
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			reqparams.Add("sort_order", sortOrder.ToString());
			reqparams.Add("sort_by", sortBy.ToString());
			reqparams.Add("media_delivery", media_delivery.ToString());
			if (howMany >= 0) reqparams.Add("page_size", howMany.ToString());
			return reqparams;
		}

		#endregion Find All Videos

        #region Find Video By ID

        public BCVideo FindVideoById(long videoId) {
            return FindVideoById(videoId, null);
        }
		public BCVideo<CustomFieldType> FindVideoById<CustomFieldType>(long videoId) {
			return FindVideoById<CustomFieldType>(videoId, null);
		}

        public BCVideo FindVideoById(long videoId, List<VideoFields> video_fields) {
            return FindVideoById(videoId, video_fields, null);
        }
		public BCVideo<CustomFieldType> FindVideoById<CustomFieldType>(long videoId, List<VideoFields> video_fields) {
			return FindVideoById<CustomFieldType>(videoId, video_fields, null);
		}

        public BCVideo FindVideoById(long videoId, List<VideoFields> video_fields, List<String> custom_fields) {
            return FindVideoById(videoId, video_fields, custom_fields, MediaDeliveryTypeEnum.DEFAULT);
        }
		public BCVideo<CustomFieldType> FindVideoById<CustomFieldType>(long videoId, List<VideoFields> video_fields, List<String> custom_fields) {
			return FindVideoById<CustomFieldType>(videoId, video_fields, custom_fields, MediaDeliveryTypeEnum.DEFAULT);
		}

		public BCVideo FindVideoById(long videoId, List<VideoFields> video_fields, List<String> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			
			Dictionary<String, String> reqparams = FindVideoByIdReqParam(videoId, video_fields, custom_fields, media_delivery);
            //Get the JSon reader returned from the APIRequest
            QueryResultPair qrp = BCAPIRequest.ExecuteRead(reqparams, Account);

            return JSON.Converter.Deserialize<BCVideo>(qrp.JsonResult);
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
		public BCVideo<CustomFieldType> FindVideoById<CustomFieldType>(long videoId, List<VideoFields> video_fields, List<String> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			
            Dictionary<String, String> reqparams = FindVideoByIdReqParam(videoId, video_fields, custom_fields, media_delivery);
            //Get the JSon reader returned from the APIRequest
            QueryResultPair qrp = BCAPIRequest.ExecuteRead(reqparams, Account);

			return JSON.Converter.Deserialize<BCVideo<CustomFieldType>>(qrp.JsonResult);
        }

		private Dictionary<String, String> FindVideoByIdReqParam(long videoId, List<VideoFields> video_fields, List<String> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_video_by_id");
			reqparams.Add("video_id", videoId.ToString());
			reqparams.Add("media_delivery", media_delivery.ToString());
			if (video_fields != null) reqparams.Add("video_fields", video_fields.ToFieldString());
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			return reqparams;
		}

        #endregion Find Video By ID
        
        #region Find Related Videos

        public BCQueryResult FindRelatedVideos(long videoId) {
            return FindRelatedVideos(videoId, -1);
        }
		public BCQueryResult<CustomFieldType> FindRelatedVideos<CustomFieldType>(long videoId) {
			return FindRelatedVideos<CustomFieldType>(videoId, -1);
		}

        public BCQueryResult FindRelatedVideos(long videoId, int howMany) {
            return FindRelatedVideos(videoId, howMany, null);
        }
		public BCQueryResult<CustomFieldType> FindRelatedVideos<CustomFieldType>(long videoId, int howMany) {
			return FindRelatedVideos<CustomFieldType>(videoId, howMany, null);
		}

        public BCQueryResult FindRelatedVideos(long videoId, int howMany, List<VideoFields> video_fields) {
            return FindRelatedVideos(videoId, howMany, video_fields, null);
        }
		public BCQueryResult<CustomFieldType> FindRelatedVideos<CustomFieldType>(long videoId, int howMany, List<VideoFields> video_fields) {
			return FindRelatedVideos<CustomFieldType>(videoId, howMany, video_fields, null);
		}

        public BCQueryResult FindRelatedVideos(long videoId, int howMany, List<VideoFields> video_fields, List<string> custom_fields) {
            return FindRelatedVideos(videoId, howMany, video_fields, custom_fields, MediaDeliveryTypeEnum.DEFAULT);
        }
		public BCQueryResult<CustomFieldType> FindRelatedVideos<CustomFieldType>(long videoId, int howMany, List<VideoFields> video_fields, List<string> custom_fields) {
			return FindRelatedVideos<CustomFieldType>(videoId, howMany, video_fields, custom_fields, MediaDeliveryTypeEnum.DEFAULT);
		}

		public BCQueryResult FindRelatedVideos(long videoId, int howMany, List<VideoFields> video_fields, List<string> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			
            Dictionary<String, String> reqparams = FindRelatedVideosReqParams(videoId, howMany, video_fields, custom_fields, media_delivery);
            return MultipleQueryHandler(reqparams, BCObjectType.videos, Account);
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
		public BCQueryResult<CustomFieldType> FindRelatedVideos<CustomFieldType>(long videoId, int howMany, List<VideoFields> video_fields, List<string> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			
            Dictionary<String, String> reqparams = FindRelatedVideosReqParams(videoId, howMany, video_fields, custom_fields, media_delivery);
			return MultipleQueryHandler<CustomFieldType>(reqparams, BCObjectType.videos, Account);
        }

		private Dictionary<String, String> FindRelatedVideosReqParams(long videoId, int howMany, List<VideoFields> video_fields, List<string> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_related_videos");
			reqparams.Add("video_id", videoId.ToString());
			reqparams.Add("media_delivery", media_delivery.ToString());
			if (video_fields != null) reqparams.Add("video_fields", video_fields.ToFieldString());
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			if (howMany >= 0) reqparams.Add("page_size", howMany.ToString());
			return reqparams;
		}

        #endregion Find Related Videos

        #region Find Videos By IDs

        public BCQueryResult FindVideosByIds(List<long> videoIds) {
            return FindVideosByIds(videoIds, null);
        }
		public BCQueryResult<CustomFieldType> FindVideosByIds<CustomFieldType>(List<long> videoIds) {
			return FindVideosByIds<CustomFieldType>(videoIds, null);
		}

        public BCQueryResult FindVideosByIds(List<long> videoIds, List<VideoFields> video_fields) {
            return FindVideosByIds(videoIds, video_fields, null);
        }
		public BCQueryResult<CustomFieldType> FindVideosByIds<CustomFieldType>(List<long> videoIds, List<VideoFields> video_fields) {
			return FindVideosByIds<CustomFieldType>(videoIds, video_fields, null);
		}

        public BCQueryResult FindVideosByIds(List<long> videoIds, List<VideoFields> video_fields, List<String> custom_fields) {
            return FindVideosByIds(videoIds, video_fields, custom_fields, MediaDeliveryTypeEnum.DEFAULT);
        }
		public BCQueryResult<CustomFieldType> FindVideosByIds<CustomFieldType>(List<long> videoIds, List<VideoFields> video_fields, List<String> custom_fields) {
			return FindVideosByIds<CustomFieldType>(videoIds, video_fields, custom_fields, MediaDeliveryTypeEnum.DEFAULT);
		}

		public BCQueryResult FindVideosByIds(List<long> videoIds, List<VideoFields> video_fields, List<String> custom_fields, MediaDeliveryTypeEnum media_delivery) {

			Dictionary<String, String> reqparams = FindVideosByIdsReqParams(videoIds, video_fields, custom_fields, media_delivery);
            return MultipleQueryHandler(reqparams, BCObjectType.videos, Account);
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
		public BCQueryResult<CustomFieldType> FindVideosByIds<CustomFieldType>(List<long> videoIds, List<VideoFields> video_fields, List<String> custom_fields, MediaDeliveryTypeEnum media_delivery) {

			Dictionary<String, String> reqparams = FindVideosByIdsReqParams(videoIds, video_fields, custom_fields, media_delivery);
			return MultipleQueryHandler<CustomFieldType>(reqparams, BCObjectType.videos, Account);
        }

		private Dictionary<String, String> FindVideosByIdsReqParams(List<long> videoIds, List<VideoFields> video_fields, List<String> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_videos_by_ids");
			reqparams.Add("video_ids", Implode(videoIds));
			reqparams.Add("media_delivery", media_delivery.ToString());
			if (video_fields != null) reqparams.Add("video_fields", video_fields.ToFieldString());
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			reqparams.Add("page_size", "-1");
			return reqparams;
		}

        #endregion Find Videos By IDs

        #region Find Video By Reference ID

        public BCVideo FindVideoByReferenceId(String referenceId) {
            return FindVideoByReferenceId(referenceId, null);
        }
		public BCVideo<CustomFieldType> FindVideoByReferenceId<CustomFieldType>(String referenceId) {
			return FindVideoByReferenceId<CustomFieldType>(referenceId, null);
		}

        public BCVideo FindVideoByReferenceId(String referenceId, List<VideoFields> video_fields) {
            return FindVideoByReferenceId(referenceId, video_fields, null);
        }
		public BCVideo<CustomFieldType> FindVideoByReferenceId<CustomFieldType>(String referenceId, List<VideoFields> video_fields) {
			return FindVideoByReferenceId<CustomFieldType>(referenceId, video_fields, null);
		}

        public BCVideo FindVideoByReferenceId(String referenceId, List<VideoFields> video_fields, List<String> custom_fields) {
            return FindVideoByReferenceId(referenceId, video_fields, custom_fields, MediaDeliveryTypeEnum.DEFAULT);
        }
		public BCVideo<CustomFieldType> FindVideoByReferenceId<CustomFieldType>(String referenceId, List<VideoFields> video_fields, List<String> custom_fields) {
			return FindVideoByReferenceId<CustomFieldType>(referenceId, video_fields, custom_fields, MediaDeliveryTypeEnum.DEFAULT);
		}

		public BCVideo FindVideoByReferenceId(String referenceId, List<VideoFields> video_fields, List<String> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			
			Dictionary<String, String> reqparams = FindVideoByReferenceIdReqParams(referenceId, video_fields, custom_fields, media_delivery);
            //Get the JSon reader returned from the APIRequest
            string jsonStr = BCAPIRequest.ExecuteRead(reqparams, Account).JsonResult;
            return JSON.Converter.Deserialize<BCVideo>(jsonStr);
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
		public BCVideo<CustomFieldType> FindVideoByReferenceId<CustomFieldType>(String referenceId, List<VideoFields> video_fields, List<String> custom_fields, MediaDeliveryTypeEnum media_delivery) {
				
            Dictionary<String, String> reqparams = FindVideoByReferenceIdReqParams(referenceId, video_fields, custom_fields, media_delivery);
            //Get the JSon reader returned from the APIRequest
            string jsonStr = BCAPIRequest.ExecuteRead(reqparams, Account).JsonResult;
			return JSON.Converter.Deserialize<BCVideo<CustomFieldType>>(jsonStr);
        }

		private Dictionary<String, String> FindVideoByReferenceIdReqParams(String referenceId, List<VideoFields> video_fields, List<String> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_video_by_reference_id");
			reqparams.Add("reference_id", referenceId);
			reqparams.Add("media_delivery", media_delivery.ToString());
			if (video_fields != null) reqparams.Add("video_fields", video_fields.ToFieldString());
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			return reqparams;
		}

        #endregion Find Video By Reference ID

        #region Find Videos By Reference IDs

        public BCQueryResult FindVideosByReferenceIds(List<String> referenceIds) {
            return FindVideosByReferenceIds(referenceIds, null);
        }
		public BCQueryResult<CustomFieldType> FindVideosByReferenceIds<CustomFieldType>(List<String> referenceIds) {
			return FindVideosByReferenceIds<CustomFieldType>(referenceIds, null);
		}

        public BCQueryResult FindVideosByReferenceIds(List<String> referenceIds, List<VideoFields> video_fields) {
            return FindVideosByReferenceIds(referenceIds, video_fields, null);
        }
		public BCQueryResult<CustomFieldType> FindVideosByReferenceIds<CustomFieldType>(List<String> referenceIds, List<VideoFields> video_fields) {
			return FindVideosByReferenceIds<CustomFieldType>(referenceIds, video_fields, null);
		}

        public BCQueryResult FindVideosByReferenceIds(List<String> referenceIds, List<VideoFields> video_fields, List<String> custom_fields) {
            return FindVideosByReferenceIds(referenceIds, video_fields, custom_fields, MediaDeliveryTypeEnum.DEFAULT);
        }
		public BCQueryResult<CustomFieldType> FindVideosByReferenceIds<CustomFieldType>(List<String> referenceIds, List<VideoFields> video_fields, List<String> custom_fields) {
			return FindVideosByReferenceIds<CustomFieldType>(referenceIds, video_fields, custom_fields, MediaDeliveryTypeEnum.DEFAULT);
		}

		public BCQueryResult FindVideosByReferenceIds(List<String> referenceIds, List<VideoFields> video_fields, List<String> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			
            Dictionary<String, String> reqparams = FindVideosByReferenceIdsReqParams(referenceIds, video_fields, custom_fields, media_delivery);
            return MultipleQueryHandler(reqparams, BCObjectType.videos, Account);
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
		public BCQueryResult<CustomFieldType> FindVideosByReferenceIds<CustomFieldType>(List<String> referenceIds, List<VideoFields> video_fields, List<String> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			
            Dictionary<String, String> reqparams = FindVideosByReferenceIdsReqParams(referenceIds, video_fields, custom_fields, media_delivery);
			return MultipleQueryHandler<CustomFieldType>(reqparams, BCObjectType.videos, Account);
        }

		private Dictionary<String, String> FindVideosByReferenceIdsReqParams(List<String> referenceIds, List<VideoFields> video_fields, List<String> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_videos_by_reference_ids");
			reqparams.Add("reference_ids", Implode(referenceIds));
			reqparams.Add("media_delivery", media_delivery.ToString());
			if (video_fields != null) reqparams.Add("video_fields", video_fields.ToFieldString());
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			return reqparams;
		}

        #endregion Find Videos By Reference IDs

		#region Find Videos By User ID

		public BCQueryResult FindVideosByUserId(long userId) {
			return FindVideosByUserId(userId, -1, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null, null);
		}
		public BCQueryResult<CustomFieldType> FindVideosByUserId<CustomFieldType>(long userId) {
			return FindVideosByUserId<CustomFieldType>(userId, -1, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null, null);
		}

		public BCQueryResult FindVideosByUserId(long userId, int howMany) {
			return FindVideosByUserId(userId, howMany, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null, null);
		}
		public BCQueryResult<CustomFieldType> FindVideosByUserId<CustomFieldType>(long userId, int howMany) {
			return FindVideosByUserId<CustomFieldType>(userId, howMany, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null, null);
		}

		public BCQueryResult FindVideosByUserId(long userId, BCSortOrderType sortOrder) {
			return FindVideosByUserId(userId, -1, BCSortByType.CREATION_DATE, sortOrder, null, null);
		}
		public BCQueryResult<CustomFieldType> FindVideosByUserId<CustomFieldType>(long userId, BCSortOrderType sortOrder) {
			return FindVideosByUserId<CustomFieldType>(userId, -1, BCSortByType.CREATION_DATE, sortOrder, null, null);
		}

		public BCQueryResult FindVideosByUserId(long userId, BCSortByType sortBy) {
			return FindVideosByUserId(userId, -1, sortBy, BCSortOrderType.ASC, null, null);
		}
		public BCQueryResult<CustomFieldType> FindVideosByUserId<CustomFieldType>(long userId, BCSortByType sortBy) {
			return FindVideosByUserId<CustomFieldType>(userId, -1, sortBy, BCSortOrderType.ASC, null, null);
		}

		public BCQueryResult FindVideosByUserId(long userId, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByUserId(userId, -1, sortBy, sortOrder, null, null);
		}
		public BCQueryResult<CustomFieldType> FindVideosByUserId<CustomFieldType>(long userId, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByUserId<CustomFieldType>(userId, -1, sortBy, sortOrder, null, null);
		}

		public BCQueryResult FindVideosByUserId(long userId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByUserId(userId, howMany, sortBy, sortOrder, null, null);
		}
		public BCQueryResult<CustomFieldType> FindVideosByUserId<CustomFieldType>(long userId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByUserId<CustomFieldType>(userId, howMany, sortBy, sortOrder, null, null);
		}

		public BCQueryResult FindVideosByUserId(long userId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields) {
			return FindVideosByUserId(userId, howMany, sortBy, sortOrder, video_fields, null);
		}
		public BCQueryResult<CustomFieldType> FindVideosByUserId<CustomFieldType>(long userId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields) {
			return FindVideosByUserId<CustomFieldType>(userId, howMany, sortBy, sortOrder, video_fields, null);
		}

        public BCQueryResult FindVideosByUserId(long userId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields) {
            return FindVideosByUserId(userId, howMany, sortBy, sortOrder, video_fields, custom_fields, MediaDeliveryTypeEnum.DEFAULT);
        }
		public BCQueryResult<CustomFieldType> FindVideosByUserId<CustomFieldType>(long userId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields) {
			return FindVideosByUserId<CustomFieldType>(userId, howMany, sortBy, sortOrder, video_fields, custom_fields, MediaDeliveryTypeEnum.DEFAULT);
		}

		public BCQueryResult FindVideosByUserId(long userId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			
			Dictionary<String, String> reqparams = FindVideosByUserIdReqParams(userId, howMany, sortBy, sortOrder, video_fields, custom_fields, media_delivery);
			return MultipleQueryHandler(reqparams, BCObjectType.videos, Account);
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
		public BCQueryResult<CustomFieldType> FindVideosByUserId<CustomFieldType>(long userId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			
			Dictionary<String, String> reqparams = FindVideosByUserIdReqParams(userId, howMany, sortBy, sortOrder, video_fields, custom_fields, media_delivery);
			return MultipleQueryHandler<CustomFieldType>(reqparams, BCObjectType.videos, Account);
		}

		private Dictionary<String, String> FindVideosByUserIdReqParams(long userId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_videos_by_user_id");
			reqparams.Add("user_id", userId.ToString());
			reqparams.Add("media_delivery", media_delivery.ToString());
			if (video_fields != null) reqparams.Add("video_fields", video_fields.ToFieldString());
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			reqparams.Add("sort_order", sortOrder.ToString());
			reqparams.Add("sort_by", sortBy.ToString());
			if (howMany >= 0) reqparams.Add("page_size", howMany.ToString());
			return reqparams;
		}

		#endregion Find Videos By User ID

		#region Find Videos By Campaign ID

		public BCQueryResult FindVideosByCampaignId(long campaignId) {
			return FindVideosByCampaignId(campaignId, -1, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null, null);
		}
		public BCQueryResult<CustomFieldType> FindVideosByCampaignId<CustomFieldType>(long campaignId) {
			return FindVideosByCampaignId<CustomFieldType>(campaignId, -1, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null, null);
		}

		public BCQueryResult FindVideosByCampaignId(long campaignId, int howMany) {
			return FindVideosByCampaignId(campaignId, howMany, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null, null);
		}
		public BCQueryResult<CustomFieldType> FindVideosByCampaignId<CustomFieldType>(long campaignId, int howMany) {
			return FindVideosByCampaignId<CustomFieldType>(campaignId, howMany, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null, null);
		}

		public BCQueryResult FindVideosByCampaignId(long campaignId, BCSortOrderType sortOrder) {
			return FindVideosByCampaignId(campaignId, -1, BCSortByType.CREATION_DATE, sortOrder, null, null);
		}
		public BCQueryResult<CustomFieldType> FindVideosByCampaignId<CustomFieldType>(long campaignId, BCSortOrderType sortOrder) {
			return FindVideosByCampaignId<CustomFieldType>(campaignId, -1, BCSortByType.CREATION_DATE, sortOrder, null, null);
		}

		public BCQueryResult FindVideosByCampaignId(long campaignId, BCSortByType sortBy) {
			return FindVideosByCampaignId(campaignId, -1, sortBy, BCSortOrderType.ASC, null, null);
		}
		public BCQueryResult<CustomFieldType> FindVideosByCampaignId<CustomFieldType>(long campaignId, BCSortByType sortBy) {
			return FindVideosByCampaignId<CustomFieldType>(campaignId, -1, sortBy, BCSortOrderType.ASC, null, null);
		}

		public BCQueryResult FindVideosByCampaignId(long campaignId, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByCampaignId(campaignId, -1, sortBy, sortOrder, null, null);
		}
		public BCQueryResult<CustomFieldType> FindVideosByCampaignId<CustomFieldType>(long campaignId, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByCampaignId<CustomFieldType>(campaignId, -1, sortBy, sortOrder, null, null);
		}

		public BCQueryResult FindVideosByCampaignId(long campaignId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByCampaignId(campaignId, howMany, sortBy, sortOrder, null, null);
		}
		public BCQueryResult<CustomFieldType> FindVideosByCampaignId<CustomFieldType>(long campaignId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByCampaignId<CustomFieldType>(campaignId, howMany, sortBy, sortOrder, null, null);
		}

		public BCQueryResult FindVideosByCampaignId(long campaignId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields) {
			return FindVideosByCampaignId(campaignId, howMany, sortBy, sortOrder, video_fields, null);
		}
		public BCQueryResult<CustomFieldType> FindVideosByCampaignId<CustomFieldType>(long campaignId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields) {
			return FindVideosByCampaignId<CustomFieldType>(campaignId, howMany, sortBy, sortOrder, video_fields, null);
		}

        public BCQueryResult FindVideosByCampaignId(long campaignId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields) {
            return FindVideosByCampaignId(campaignId, howMany, sortBy, sortOrder, video_fields, custom_fields, MediaDeliveryTypeEnum.DEFAULT);
        }
		public BCQueryResult<CustomFieldType> FindVideosByCampaignId<CustomFieldType>(long campaignId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields) {
			return FindVideosByCampaignId<CustomFieldType>(campaignId, howMany, sortBy, sortOrder, video_fields, custom_fields, MediaDeliveryTypeEnum.DEFAULT);
		}

		public BCQueryResult FindVideosByCampaignId(long campaignId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			
			Dictionary<String, String> reqparams = FindVideosByCampaignIdReqParams(campaignId, howMany, sortBy, sortOrder, video_fields, custom_fields, media_delivery);
			return MultipleQueryHandler(reqparams, BCObjectType.videos, Account);
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
		public BCQueryResult<CustomFieldType> FindVideosByCampaignId<CustomFieldType>(long campaignId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			
			Dictionary<String, String> reqparams = FindVideosByCampaignIdReqParams(campaignId, howMany, sortBy, sortOrder, video_fields, custom_fields, media_delivery);
			return MultipleQueryHandler<CustomFieldType>(reqparams, BCObjectType.videos, Account);
		}

		private Dictionary<String, String> FindVideosByCampaignIdReqParams(long campaignId, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_videos_by_campaign_id");
			reqparams.Add("campaign_id", campaignId.ToString());
			reqparams.Add("media_delivery", media_delivery.ToString());
			if (video_fields != null) reqparams.Add("video_fields", video_fields.ToFieldString());
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			reqparams.Add("sort_order", sortOrder.ToString());
			reqparams.Add("sort_by", sortBy.ToString());
			if (howMany >= 0) reqparams.Add("page_size", howMany.ToString());
			return reqparams;
		}

		#endregion Find Videos By Campaign ID

        #region Find Modified Videos

        public BCQueryResult FindModifiedVideos(DateTime from_date) {
            return FindModifiedVideos(from_date, -1, BCSortOrderType.ASC);
        }
		public BCQueryResult<CustomFieldType> FindModifiedVideos<CustomFieldType>(DateTime from_date) {
			return FindModifiedVideos<CustomFieldType>(from_date, -1, BCSortOrderType.ASC);
		}

        public BCQueryResult FindModifiedVideos(DateTime from_date, BCSortOrderType sortOrder) {
            return FindModifiedVideos(from_date, -1, BCSortByType.CREATION_DATE, sortOrder);
        }
		public BCQueryResult<CustomFieldType> FindModifiedVideos<CustomFieldType>(DateTime from_date, BCSortOrderType sortOrder) {
			return FindModifiedVideos<CustomFieldType>(from_date, -1, BCSortByType.CREATION_DATE, sortOrder);
		}

        public BCQueryResult FindModifiedVideos(DateTime from_date, BCSortByType sortBy) {
            return FindModifiedVideos(from_date, -1, sortBy, BCSortOrderType.ASC);
        }
		public BCQueryResult<CustomFieldType> FindModifiedVideos<CustomFieldType>(DateTime from_date, BCSortByType sortBy) {
			return FindModifiedVideos<CustomFieldType>(from_date, -1, sortBy, BCSortOrderType.ASC);
		}

        public BCQueryResult FindModifiedVideos(DateTime from_date, int howMany) {
            return FindModifiedVideos(from_date, howMany, BCSortOrderType.ASC);
        }
		public BCQueryResult<CustomFieldType> FindModifiedVideos<CustomFieldType>(DateTime from_date, int howMany) {
			return FindModifiedVideos<CustomFieldType>(from_date, howMany, BCSortOrderType.ASC);
		}

        public BCQueryResult FindModifiedVideos(DateTime from_date, int howMany, BCSortOrderType sortOrder) {
            return FindModifiedVideos(from_date, howMany, BCSortByType.CREATION_DATE, sortOrder);
        }
		public BCQueryResult<CustomFieldType> FindModifiedVideos<CustomFieldType>(DateTime from_date, int howMany, BCSortOrderType sortOrder) {
			return FindModifiedVideos<CustomFieldType>(from_date, howMany, BCSortByType.CREATION_DATE, sortOrder);
		}

        public BCQueryResult FindModifiedVideos(DateTime from_date, int howMany, BCSortByType sortBy) {
            return FindModifiedVideos(from_date, howMany, sortBy, BCSortOrderType.ASC);
        }
		public BCQueryResult<CustomFieldType> FindModifiedVideos<CustomFieldType>(DateTime from_date, int howMany, BCSortByType sortBy) {
			return FindModifiedVideos<CustomFieldType>(from_date, howMany, sortBy, BCSortOrderType.ASC);
		}

        public BCQueryResult FindModifiedVideos(DateTime from_date, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder) {
            return FindModifiedVideos(from_date, howMany, sortBy, sortOrder, null);
        }
		public BCQueryResult<CustomFieldType> FindModifiedVideos<CustomFieldType>(DateTime from_date, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindModifiedVideos<CustomFieldType>(from_date, howMany, sortBy, sortOrder, null);
		}

        public BCQueryResult FindModifiedVideos(DateTime from_date, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields) {
            return FindModifiedVideos(from_date, howMany, sortBy, sortOrder, video_fields, null);
        }
		public BCQueryResult<CustomFieldType> FindModifiedVideos<CustomFieldType>(DateTime from_date, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields) {
			return FindModifiedVideos<CustomFieldType>(from_date, howMany, sortBy, sortOrder, video_fields, null);
		}

        public BCQueryResult FindModifiedVideos(DateTime from_date, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields) {
            return FindModifiedVideos(from_date, howMany, sortBy, sortOrder, video_fields, custom_fields, null);
        }
		public BCQueryResult<CustomFieldType> FindModifiedVideos<CustomFieldType>(DateTime from_date, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields) {
			return FindModifiedVideos<CustomFieldType>(from_date, howMany, sortBy, sortOrder, video_fields, custom_fields, null);
		}

        public BCQueryResult FindModifiedVideos(DateTime from_date, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields, List<string> filter) {
            return FindModifiedVideos(from_date, howMany, sortBy, sortOrder, video_fields, custom_fields, filter, MediaDeliveryTypeEnum.DEFAULT);
        }
		public BCQueryResult<CustomFieldType> FindModifiedVideos<CustomFieldType>(DateTime from_date, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields, List<string> filter) {
			return FindModifiedVideos<CustomFieldType>(from_date, howMany, sortBy, sortOrder, video_fields, custom_fields, filter, MediaDeliveryTypeEnum.DEFAULT);
		}

		public BCQueryResult FindModifiedVideos(DateTime from_date, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields, List<string> filter, MediaDeliveryTypeEnum media_delivery) {
			
			Dictionary<String, String> reqparams = FindModifiedVideosReqParams(from_date, howMany, sortBy, sortOrder, video_fields, custom_fields, filter, media_delivery);
            return MultipleQueryHandler(reqparams, BCObjectType.videos, Account);
        }
        /// <summary>
        /// This will find all modified videos
        /// </summary>
        /// <param name="from_date">The date, specified in minutes since January 1st, 1970 00:00:00 GMT, of the oldest Video which you would like returned.</param>
        /// <param name="howMany">Number of items returned per page. A page is a subset of all of the items that satisfy the request. The maximum page size is 25; if you do not set this argument, or if you set it to an integer > 25, your results will come back as if you had set page_size=25.</param>
        /// <param name="sortBy">The field by which to sort the results. A SortByType: One of PUBLISH_DATE, CREATION_DATE, MODIFIED_DATE, PLAYS_TOTAL, PLAYS_TRAILING_WEEK.</param>
        /// <param name="sortOrder">How to order the results: ascending (ASC) or descending (DESC).</param>
        /// <param name="video_fields">A comma-separated list of the fields you wish to have populated in the videos contained in the returned object. If you omit this parameter, the method returns the following fields of the video: id, name, shortDescription, longDescription, creationDate, publisheddate, lastModifiedDate, linkURL, linkText, tags, videoStillURL, thumbnailURL, referenceId, length, economics, playsTotal, playsTrailingWeek. If you use a token with URL access, this method also returns FLVURL, renditions, FLVFullLength, videoFullLength.</param>
        /// <param name="custom_fields">A comma-separated list of the custom fields you wish to have populated in the videos contained in the returned object. If you omit this parameter, no custom fields are returned, unless you include the value 'customFields' in the video_fields parameter.</param>
        /// <param name="filter">A comma-separated list of filters, specifying which categories of videos you would like returned. Valid filter values are PLAYABLE, UNSCHEDULED, INACTIVE, and DELETED.</param>
        /// <param name="media_delivery">If universal delivery service is enabled for your account, set this optional parameter to http to return video by HTTP, rather than streaming. Meaningful only if used together with the video_fields=FLVURL, videoFullLength, or renditions parameters. This is a MediaDeliveryTypeEnum with a value of http or default.</param>
        /// <returns>Returns a BCQueryResult Item</returns>
		public BCQueryResult<CustomFieldType> FindModifiedVideos<CustomFieldType>(DateTime from_date, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields, List<string> filter, MediaDeliveryTypeEnum media_delivery) {
			
            Dictionary<String, String> reqparams = FindModifiedVideosReqParams(from_date, howMany, sortBy, sortOrder, video_fields, custom_fields, filter, media_delivery);
			return MultipleQueryHandler<CustomFieldType>(reqparams, BCObjectType.videos, Account);
        }

		private Dictionary<String, String> FindModifiedVideosReqParams(DateTime from_date, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields, List<string> filter, MediaDeliveryTypeEnum media_delivery) {
			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_modified_videos");
			if (from_date != null) reqparams.Add("from_date", from_date.ToUnixTime());
			if (video_fields != null) reqparams.Add("video_fields", video_fields.ToFieldString());
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			if (filter != null) reqparams.Add("filter", Implode(filter));
			reqparams.Add("sort_order", sortOrder.ToString());
			reqparams.Add("sort_by", sortBy.ToString());
			reqparams.Add("media_delivery", media_delivery.ToString());
			if (howMany >= 0) reqparams.Add("page_size", howMany.ToString());
			return reqparams;
		}

        #endregion Find Modified Videos

		#region Find Videos By Text

		public BCQueryResult FindVideosByText(string text) {
			return FindVideosByText(text, -1, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null, null);
		}
		public BCQueryResult<CustomFieldType> FindVideosByText<CustomFieldType>(string text) {
			return FindVideosByText<CustomFieldType>(text, -1, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null, null);
		}

		public BCQueryResult FindVideosByText(string text, int howMany) {
			return FindVideosByText(text, howMany, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null, null);
		}
		public BCQueryResult<CustomFieldType> FindVideosByText<CustomFieldType>(string text, int howMany) {
			return FindVideosByText<CustomFieldType>(text, howMany, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null, null);
		}

		public BCQueryResult FindVideosByText(string text, BCSortOrderType sortOrder) {
			return FindVideosByText(text, -1, BCSortByType.CREATION_DATE, sortOrder, null, null);
		}
		public BCQueryResult<CustomFieldType> FindVideosByText<CustomFieldType>(string text, BCSortOrderType sortOrder) {
			return FindVideosByText<CustomFieldType>(text, -1, BCSortByType.CREATION_DATE, sortOrder, null, null);
		}

		public BCQueryResult FindVideosByText(string text, BCSortByType sortBy) {
			return FindVideosByText(text, -1, sortBy, BCSortOrderType.ASC, null, null);
		}
		public BCQueryResult<CustomFieldType> FindVideosByText<CustomFieldType>(string text, BCSortByType sortBy) {
			return FindVideosByText<CustomFieldType>(text, -1, sortBy, BCSortOrderType.ASC, null, null);
		}

		public BCQueryResult FindVideosByText(string text, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByText(text, -1, sortBy, sortOrder, null, null);
		}
		public BCQueryResult<CustomFieldType> FindVideosByText<CustomFieldType>(string text, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByText<CustomFieldType>(text, -1, sortBy, sortOrder, null, null);
		}

		public BCQueryResult FindVideosByText(string text, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByText(text, howMany, sortBy, sortOrder, null, null);
		}
		public BCQueryResult<CustomFieldType> FindVideosByText<CustomFieldType>(string text, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByText<CustomFieldType>(text, howMany, sortBy, sortOrder, null, null);
		}

		public BCQueryResult FindVideosByText(string text, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields) {
			return FindVideosByText(text, howMany, sortBy, sortOrder, video_fields, null);
		}
		public BCQueryResult<CustomFieldType> FindVideosByText<CustomFieldType>(string text, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields) {
			return FindVideosByText<CustomFieldType>(text, howMany, sortBy, sortOrder, video_fields, null);
		}

        public BCQueryResult FindVideosByText(string text, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields) {
            return FindVideosByText(text, howMany, sortBy, sortOrder, video_fields, custom_fields, MediaDeliveryTypeEnum.DEFAULT);
        }
		public BCQueryResult<CustomFieldType> FindVideosByText<CustomFieldType>(string text, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields) {
			return FindVideosByText<CustomFieldType>(text, howMany, sortBy, sortOrder, video_fields, custom_fields, MediaDeliveryTypeEnum.DEFAULT);
		}

		public BCQueryResult FindVideosByText(string text, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			
			Dictionary<String, String> reqparams = FindVideosByTextReqParams(text, howMany, sortBy, sortOrder, video_fields, custom_fields, media_delivery);
			return MultipleQueryHandler(reqparams, BCObjectType.videos, Account);
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
		public BCQueryResult<CustomFieldType> FindVideosByText<CustomFieldType>(string text, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			
			Dictionary<String, String> reqparams = FindVideosByTextReqParams(text, howMany, sortBy, sortOrder, video_fields, custom_fields, media_delivery);
			return MultipleQueryHandler<CustomFieldType>(reqparams, BCObjectType.videos, Account);
		}

		private Dictionary<String, String> FindVideosByTextReqParams(string text, int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<string> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_videos_by_text");
			reqparams.Add("text", text);
			reqparams.Add("media_delivery", media_delivery.ToString());
			if (video_fields != null) reqparams.Add("video_fields", video_fields.ToFieldString());
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			reqparams.Add("sort_order", sortOrder.ToString());
			reqparams.Add("sort_by", sortBy.ToString());
			if (howMany >= 0) reqparams.Add("page_size", howMany.ToString());
			return reqparams;
		}

		#endregion Find Videos By Text

		#region Find Videos By Tags

		public BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags) {
			return FindVideosByTags(and_tags, or_tags, -1, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null);
		}
		public BCQueryResult<CustomFieldType> FindVideosByTags<CustomFieldType>(List<String> and_tags, List<String> or_tags) {
			return FindVideosByTags<CustomFieldType>(and_tags, or_tags, -1, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null);
		}

        public BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags, int pageSize) {
            return FindVideosByTags(and_tags, or_tags, pageSize, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null);
		}
		public BCQueryResult<CustomFieldType> FindVideosByTags<CustomFieldType>(List<String> and_tags, List<String> or_tags, int pageSize) {
			return FindVideosByTags<CustomFieldType>(and_tags, or_tags, pageSize, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null);
		}

		public BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags, BCSortByType sortBy) {
			return FindVideosByTags(and_tags, or_tags, -1, sortBy, BCSortOrderType.ASC);
		}
		public BCQueryResult<CustomFieldType> FindVideosByTags<CustomFieldType>(List<String> and_tags, List<String> or_tags, BCSortByType sortBy) {
			return FindVideosByTags<CustomFieldType>(and_tags, or_tags, -1, sortBy, BCSortOrderType.ASC);
		}

		public BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags, BCSortOrderType sortOrder) {
			return FindVideosByTags(and_tags, or_tags, -1, BCSortByType.CREATION_DATE, sortOrder);
		}
		public BCQueryResult<CustomFieldType> FindVideosByTags<CustomFieldType>(List<String> and_tags, List<String> or_tags, BCSortOrderType sortOrder) {
			return FindVideosByTags<CustomFieldType>(and_tags, or_tags, -1, BCSortByType.CREATION_DATE, sortOrder);
		}

		public BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByTags(and_tags, or_tags, -1, sortBy, sortOrder);
		}
		public BCQueryResult<CustomFieldType> FindVideosByTags<CustomFieldType>(List<String> and_tags, List<String> or_tags, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByTags<CustomFieldType>(and_tags, or_tags, -1, sortBy, sortOrder);
		}

        public BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags, int pageSize, BCSortByType sortBy, BCSortOrderType sortOrder) {
            return FindVideosByTags(and_tags, or_tags, pageSize, sortBy, sortOrder, null);
		}
		public BCQueryResult<CustomFieldType> FindVideosByTags<CustomFieldType>(List<String> and_tags, List<String> or_tags, int pageSize, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindVideosByTags<CustomFieldType>(and_tags, or_tags, pageSize, sortBy, sortOrder, null);
		}

        public BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags, int pageSize, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields) {
            return FindVideosByTags(and_tags, or_tags, pageSize, sortBy, sortOrder, video_fields, null);
		}
		public BCQueryResult<CustomFieldType> FindVideosByTags<CustomFieldType>(List<String> and_tags, List<String> or_tags, int pageSize, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields) {
			return FindVideosByTags<CustomFieldType>(and_tags, or_tags, pageSize, sortBy, sortOrder, video_fields, null);
		}

        public BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags, int pageSize, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<String> custom_fields) {
            return FindVideosByTags(and_tags, or_tags, pageSize, 0, sortBy, sortOrder, video_fields, custom_fields, MediaDeliveryTypeEnum.DEFAULT);
        }
		public BCQueryResult<CustomFieldType> FindVideosByTags<CustomFieldType>(List<String> and_tags, List<String> or_tags, int pageSize, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<String> custom_fields) {
			return FindVideosByTags<CustomFieldType>(and_tags, or_tags, pageSize, 0, sortBy, sortOrder, video_fields, custom_fields, MediaDeliveryTypeEnum.DEFAULT);
		}

        public BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags, int pageSize, int pageNumber, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<String> custom_fields) {
            return FindVideosByTags(and_tags, or_tags, pageSize, pageNumber, sortBy, sortOrder, video_fields, custom_fields, MediaDeliveryTypeEnum.DEFAULT);
        }
		public BCQueryResult<CustomFieldType> FindVideosByTags<CustomFieldType>(List<String> and_tags, List<String> or_tags, int pageSize, int pageNumber, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<String> custom_fields) {
			return FindVideosByTags<CustomFieldType>(and_tags, or_tags, pageSize, pageNumber, sortBy, sortOrder, video_fields, custom_fields, MediaDeliveryTypeEnum.DEFAULT);
		}

		public BCQueryResult FindVideosByTags(List<String> and_tags, List<String> or_tags, int pageSize, int pageNumber, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<String> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			Dictionary<String, String> reqparams = FindVideosByTagsReqParams(and_tags, or_tags, pageSize, pageNumber, sortBy, sortOrder, video_fields, custom_fields, media_delivery);
			return MultipleQueryHandler(reqparams, BCObjectType.videos, Account);
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
        /// <param name="pageSize">
        /// Number of videos returned (-1 will return all) defaults to -1 max is 100
        /// </param>
        /// <param name="pageNumber">
        /// The number of page to return. Default is 0 (First page)
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
		public BCQueryResult<CustomFieldType> FindVideosByTags<CustomFieldType>(List<String> and_tags, List<String> or_tags, int pageSize, int pageNumber, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<String> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			
			Dictionary<String, String> reqparams = FindVideosByTagsReqParams(and_tags, or_tags, pageSize, pageNumber, sortBy, sortOrder, video_fields, custom_fields, media_delivery);
			return MultipleQueryHandler<CustomFieldType>(reqparams, BCObjectType.videos, Account);
		}

		private Dictionary<String, String> FindVideosByTagsReqParams(List<String> and_tags, List<String> or_tags, int pageSize, int pageNumber, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<String> custom_fields, MediaDeliveryTypeEnum media_delivery) {
			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_videos_by_tags");
			if (and_tags != null) reqparams.Add("and_tags", Implode(and_tags));
			if (or_tags != null) reqparams.Add("or_tags", Implode(or_tags));
			if (video_fields != null) reqparams.Add("video_fields", video_fields.ToFieldString());
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			reqparams.Add("sort_order", sortOrder.ToString());
			reqparams.Add("sort_by", sortBy.ToString());
			reqparams.Add("media_delivery", media_delivery.ToString());
			if (pageSize >= 0) reqparams.Add("page_size", pageSize.ToString());
			if (pageNumber >= 0) reqparams.Add("page_number", pageNumber.ToString());
			return reqparams;
		}

		#endregion Find Videos By Tags
        
		#endregion Video Read

		#region Playlist Read

		#region Find All Playlists

		public BCQueryResult FindAllPlaylists() {
			return FindAllPlaylists(-1);
		}
		public BCQueryResult<CustomFieldType> FindAllPlaylists<CustomFieldType>() {
			return FindAllPlaylists<CustomFieldType>(-1);
		}
		
		public BCQueryResult FindAllPlaylists(int howMany) {
			return FindAllPlaylists(howMany, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null);
		}
		public BCQueryResult<CustomFieldType> FindAllPlaylists<CustomFieldType>(int howMany) {
			return FindAllPlaylists<CustomFieldType>(howMany, BCSortByType.CREATION_DATE, BCSortOrderType.ASC, null);
		}

		public BCQueryResult FindAllPlaylists(int howMany, BCSortOrderType sortOrder) {
			return FindAllPlaylists(howMany, BCSortByType.CREATION_DATE, sortOrder, null);
		}
		public BCQueryResult<CustomFieldType> FindAllPlaylists<CustomFieldType>(int howMany, BCSortOrderType sortOrder) {
			return FindAllPlaylists<CustomFieldType>(howMany, BCSortByType.CREATION_DATE, sortOrder, null);
		}

		public BCQueryResult FindAllPlaylists(int howMany, BCSortByType sortBy) {
			return FindAllPlaylists(howMany, sortBy, BCSortOrderType.ASC, null);
		}
		public BCQueryResult<CustomFieldType> FindAllPlaylists<CustomFieldType>(int howMany, BCSortByType sortBy) {
			return FindAllPlaylists<CustomFieldType>(howMany, sortBy, BCSortOrderType.ASC, null);
		}

		public BCQueryResult FindAllPlaylists(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindAllPlaylists(howMany, sortBy, sortOrder, null);
		}
		public BCQueryResult<CustomFieldType> FindAllPlaylists<CustomFieldType>(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder) {
			return FindAllPlaylists<CustomFieldType>(howMany, sortBy, sortOrder, null);
		}

		public BCQueryResult FindAllPlaylists(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields) {
			return FindAllPlaylists(howMany, sortBy, sortOrder, video_fields, null);
		}
		public BCQueryResult<CustomFieldType> FindAllPlaylists<CustomFieldType>(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields) {
			return FindAllPlaylists<CustomFieldType>(howMany, sortBy, sortOrder, video_fields, null);
		}

		public BCQueryResult FindAllPlaylists(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<String> custom_fields) {
			return FindAllPlaylists(howMany, sortBy, sortOrder, video_fields, custom_fields, null);
		}
		public BCQueryResult<CustomFieldType> FindAllPlaylists<CustomFieldType>(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<String> custom_fields) {
			return FindAllPlaylists<CustomFieldType>(howMany, sortBy, sortOrder, video_fields, custom_fields, null);
		}

        public BCQueryResult FindAllPlaylists(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<String> custom_fields, List<string> playlist_fields) {
            return FindAllPlaylists(howMany, sortBy, sortOrder, video_fields, custom_fields, playlist_fields, MediaDeliveryTypeEnum.DEFAULT);
        }
		public BCQueryResult<CustomFieldType> FindAllPlaylists<CustomFieldType>(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<String> custom_fields, List<string> playlist_fields) {
			return FindAllPlaylists<CustomFieldType>(howMany, sortBy, sortOrder, video_fields, custom_fields, playlist_fields, MediaDeliveryTypeEnum.DEFAULT);
		}

		public BCQueryResult FindAllPlaylists(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<String> custom_fields, List<string> playlist_fields, MediaDeliveryTypeEnum media_delivery) {
			
			Dictionary<String, String> reqparams = FindAllPlaylistsReqParams(howMany, sortBy, sortOrder, video_fields, custom_fields, playlist_fields, media_delivery);
			BCQueryResult qr = MultipleQueryHandler(reqparams, BCObjectType.playlists, Account);

			return qr;
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
		public BCQueryResult<CustomFieldType> FindAllPlaylists<CustomFieldType>(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<String> custom_fields, List<string> playlist_fields, MediaDeliveryTypeEnum media_delivery) {
			
			Dictionary<String, String> reqparams = FindAllPlaylistsReqParams(howMany, sortBy, sortOrder, video_fields, custom_fields, playlist_fields, media_delivery);
			BCQueryResult<CustomFieldType> qr = MultipleQueryHandler<CustomFieldType>(reqparams, BCObjectType.playlists, Account);

			return qr;
		}

		private Dictionary<String, String> FindAllPlaylistsReqParams(int howMany, BCSortByType sortBy, BCSortOrderType sortOrder, List<VideoFields> video_fields, List<String> custom_fields, List<string> playlist_fields, MediaDeliveryTypeEnum media_delivery) {
			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_all_playlists");
			reqparams.Add("sort_order", sortOrder.ToString());
			reqparams.Add("sort_by", sortBy.ToString());
			reqparams.Add("media_delivery", media_delivery.ToString());
			if (howMany >= 0) reqparams.Add("page_size", howMany.ToString());
			if (playlist_fields != null) reqparams.Add("playlist_fields", Implode(playlist_fields));
			if (video_fields != null) reqparams.Add("video_fields", video_fields.ToFieldString());
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			return reqparams;
		}

		#endregion Find All Playlists

		#region Find Playlist By Id

		public BCPlaylist FindPlaylistById(long playlist_id) {
			return FindPlaylistById(playlist_id, null);
		}
		public BCPlaylist<CustomFieldType> FindPlaylistById<CustomFieldType>(long playlist_id) {
			return FindPlaylistById<CustomFieldType>(playlist_id, null);
		}

		public BCPlaylist FindPlaylistById(long playlist_id, List<VideoFields> video_fields) {
			return FindPlaylistById(playlist_id, video_fields, null);
		}
		public BCPlaylist<CustomFieldType> FindPlaylistById<CustomFieldType>(long playlist_id, List<VideoFields> video_fields) {
			return FindPlaylistById<CustomFieldType>(playlist_id, video_fields, null);
		}

		public BCPlaylist FindPlaylistById(long playlist_id, List<VideoFields> video_fields, List<string> custom_fields) {
			return FindPlaylistById(playlist_id, video_fields, custom_fields, null);
		}
		public BCPlaylist<CustomFieldType> FindPlaylistById<CustomFieldType>(long playlist_id, List<VideoFields> video_fields, List<string> custom_fields) {
			return FindPlaylistById<CustomFieldType>(playlist_id, video_fields, custom_fields, null);
		}

        public BCPlaylist FindPlaylistById(long playlist_id, List<VideoFields> video_fields, List<string> custom_fields, List<PlaylistFields> playlist_fields) {
            return FindPlaylistById(playlist_id, video_fields, custom_fields, playlist_fields, MediaDeliveryTypeEnum.DEFAULT);
        }
		public BCPlaylist<CustomFieldType> FindPlaylistById<CustomFieldType>(long playlist_id, List<VideoFields> video_fields, List<string> custom_fields, List<PlaylistFields> playlist_fields) {
			return FindPlaylistById<CustomFieldType>(playlist_id, video_fields, custom_fields, playlist_fields, MediaDeliveryTypeEnum.DEFAULT);
		}

		public BCPlaylist FindPlaylistById(long playlist_id, List<VideoFields> video_fields, List<string> custom_fields, List<PlaylistFields> playlist_fields, MediaDeliveryTypeEnum media_delivery) {
			
			Dictionary<String, String> reqparams = FindPlaylistByIdReqParams(playlist_id, video_fields, custom_fields, playlist_fields, media_delivery);
			//Get the JSon reader returned from the APIRequest
			QueryResultPair qrp = BCAPIRequest.ExecuteRead(reqparams, Account);

			return JSON.Converter.Deserialize<BCPlaylist>(qrp.JsonResult);
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
		public BCPlaylist<CustomFieldType> FindPlaylistById<CustomFieldType>(long playlist_id, List<VideoFields> video_fields, List<string> custom_fields, List<PlaylistFields> playlist_fields, MediaDeliveryTypeEnum media_delivery) {
			
			Dictionary<String, String> reqparams = FindPlaylistByIdReqParams(playlist_id, video_fields, custom_fields, playlist_fields, media_delivery);
			//Get the JSon reader returned from the APIRequest
			QueryResultPair qrp = BCAPIRequest.ExecuteRead(reqparams, Account);

			return JSON.Converter.Deserialize<BCPlaylist<CustomFieldType>>(qrp.JsonResult);
		}

		private Dictionary<String, String> FindPlaylistByIdReqParams(long playlist_id, List<VideoFields> video_fields, List<string> custom_fields, List<PlaylistFields> playlist_fields, MediaDeliveryTypeEnum media_delivery) {
			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_playlist_by_id");
			reqparams.Add("playlist_id", playlist_id.ToString());
			reqparams.Add("media_delivery", media_delivery.ToString());
			if (playlist_fields != null) reqparams.Add("playlist_fields", playlist_fields.ToFieldString());
			if (video_fields != null) reqparams.Add("video_fields", video_fields.ToFieldString());
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			return reqparams;
		}

		#endregion Find Playlist By Id

		#region Find Playlists By Ids

		public BCQueryResult FindPlaylistsByIds(List<long> playlist_ids) {
			return FindPlaylistsByIds(playlist_ids, null);
		}
		public BCQueryResult<CustomFieldType> FindPlaylistsByIds<CustomFieldType>(List<long> playlist_ids) {
			return FindPlaylistsByIds<CustomFieldType>(playlist_ids, null);
		}

		public BCQueryResult FindPlaylistsByIds(List<long> playlist_ids, List<VideoFields> video_fields) {
			return FindPlaylistsByIds(playlist_ids, video_fields, null);
		}
		public BCQueryResult<CustomFieldType> FindPlaylistsByIds<CustomFieldType>(List<long> playlist_ids, List<VideoFields> video_fields) {
			return FindPlaylistsByIds<CustomFieldType>(playlist_ids, video_fields, null);
		}

		public BCQueryResult FindPlaylistsByIds(List<long> playlist_ids, List<VideoFields> video_fields, List<string> custom_fields) {
			return FindPlaylistsByIds(playlist_ids, video_fields, custom_fields, null);
		}
		public BCQueryResult<CustomFieldType> FindPlaylistsByIds<CustomFieldType>(List<long> playlist_ids, List<VideoFields> video_fields, List<string> custom_fields) {
			return FindPlaylistsByIds<CustomFieldType>(playlist_ids, video_fields, custom_fields, null);
		}

        public BCQueryResult FindPlaylistsByIds(List<long> playlist_ids, List<VideoFields> video_fields, List<string> custom_fields, List<PlaylistFields> playlist_fields) {
            return FindPlaylistsByIds(playlist_ids, video_fields, custom_fields, playlist_fields, MediaDeliveryTypeEnum.DEFAULT);
        }
		public BCQueryResult<CustomFieldType> FindPlaylistsByIds<CustomFieldType>(List<long> playlist_ids, List<VideoFields> video_fields, List<string> custom_fields, List<PlaylistFields> playlist_fields) {
			return FindPlaylistsByIds<CustomFieldType>(playlist_ids, video_fields, custom_fields, playlist_fields, MediaDeliveryTypeEnum.DEFAULT);
		}

		public BCQueryResult FindPlaylistsByIds(List<long> playlist_ids, List<VideoFields> video_fields, List<string> custom_fields, List<PlaylistFields> playlist_fields, MediaDeliveryTypeEnum media_delivery) {

			Dictionary<String, String> reqparams = FindPlaylistsByIdsReqParams(playlist_ids, video_fields, custom_fields, playlist_fields, media_delivery);
			BCQueryResult qr = MultipleQueryHandler(reqparams, BCObjectType.playlists, Account);

			return qr;

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
		public BCQueryResult<CustomFieldType> FindPlaylistsByIds<CustomFieldType>(List<long> playlist_ids, List<VideoFields> video_fields, List<string> custom_fields, List<PlaylistFields> playlist_fields, MediaDeliveryTypeEnum media_delivery) {

			Dictionary<String, String> reqparams = FindPlaylistsByIdsReqParams(playlist_ids, video_fields, custom_fields, playlist_fields, media_delivery);
			BCQueryResult<CustomFieldType> qr = MultipleQueryHandler<CustomFieldType>(reqparams, BCObjectType.playlists, Account);

			return qr;

		}

		public Dictionary<String, String> FindPlaylistsByIdsReqParams(List<long> playlist_ids, List<VideoFields> video_fields, List<string> custom_fields, List<PlaylistFields> playlist_fields, MediaDeliveryTypeEnum media_delivery) {
			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_playlists_by_ids");
			reqparams.Add("playlist_ids", Implode(playlist_ids));
			reqparams.Add("media_delivery", media_delivery.ToString());
			if (playlist_fields != null) reqparams.Add("playlist_fields", playlist_fields.ToFieldString());
			if (video_fields != null) reqparams.Add("video_fields", video_fields.ToFieldString());
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			return reqparams;
		}

		#endregion Find Playlists By Ids

		#region Find Playlist By Reference Id

		public BCPlaylist FindPlaylistByReferenceId(string reference_id) {
			return FindPlaylistByReferenceId(reference_id, null);
		}
		public BCPlaylist<CustomFieldType> FindPlaylistByReferenceId<CustomFieldType>(string reference_id) {
			return FindPlaylistByReferenceId<CustomFieldType>(reference_id, null);
		}

		public BCPlaylist FindPlaylistByReferenceId(string reference_id, List<VideoFields> video_fields) {
			return FindPlaylistByReferenceId(reference_id, video_fields, null);
		}
		public BCPlaylist<CustomFieldType> FindPlaylistByReferenceId<CustomFieldType>(string reference_id, List<VideoFields> video_fields) {
			return FindPlaylistByReferenceId<CustomFieldType>(reference_id, video_fields, null);
		}

		public BCPlaylist FindPlaylistByReferenceId(string reference_id, List<VideoFields> video_fields, List<string> custom_fields) {
			return FindPlaylistByReferenceId(reference_id, video_fields, custom_fields, null);
		}
		public BCPlaylist<CustomFieldType> FindPlaylistByReferenceId<CustomFieldType>(string reference_id, List<VideoFields> video_fields, List<string> custom_fields) {
			return FindPlaylistByReferenceId<CustomFieldType>(reference_id, video_fields, custom_fields, null);
		}

        public BCPlaylist FindPlaylistByReferenceId(string reference_id, List<VideoFields> video_fields, List<string> custom_fields, List<PlaylistFields> playlist_fields) {
            return FindPlaylistByReferenceId(reference_id, video_fields, custom_fields, playlist_fields, MediaDeliveryTypeEnum.DEFAULT);
        }
		public BCPlaylist<CustomFieldType> FindPlaylistByReferenceId<CustomFieldType>(string reference_id, List<VideoFields> video_fields, List<string> custom_fields, List<PlaylistFields> playlist_fields) {
			return FindPlaylistByReferenceId<CustomFieldType>(reference_id, video_fields, custom_fields, playlist_fields, MediaDeliveryTypeEnum.DEFAULT);
		}

		public BCPlaylist FindPlaylistByReferenceId(string reference_id, List<VideoFields> video_fields, List<string> custom_fields, List<PlaylistFields> playlist_fields, MediaDeliveryTypeEnum media_delivery) {
			
			Dictionary<String, String> reqparams = FindPlaylistByReferenceIdReqParams(reference_id, video_fields, custom_fields, playlist_fields, media_delivery);

			//Get the JSon reader returned from the APIRequest
			string jsonStr = BCAPIRequest.ExecuteRead(reqparams, Account).JsonResult;
			return JSON.Converter.Deserialize<BCPlaylist>(jsonStr);
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
		public BCPlaylist<CustomFieldType> FindPlaylistByReferenceId<CustomFieldType>(string reference_id, List<VideoFields> video_fields, List<string> custom_fields, List<PlaylistFields> playlist_fields, MediaDeliveryTypeEnum media_delivery) {
			
			Dictionary<String, String> reqparams = FindPlaylistByReferenceIdReqParams(reference_id, video_fields, custom_fields, playlist_fields, media_delivery);

			//Get the JSon reader returned from the APIRequest
			string jsonStr = BCAPIRequest.ExecuteRead(reqparams, Account).JsonResult;
			return JSON.Converter.Deserialize<BCPlaylist<CustomFieldType>>(jsonStr);
		}

		private Dictionary<String, String> FindPlaylistByReferenceIdReqParams(string reference_id, List<VideoFields> video_fields, List<string> custom_fields, List<PlaylistFields> playlist_fields, MediaDeliveryTypeEnum media_delivery) {
			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_playlist_by_reference_id");
			reqparams.Add("reference_id", reference_id);
			reqparams.Add("media_delivery", media_delivery.ToString());
			if (playlist_fields != null) reqparams.Add("playlist_fields", playlist_fields.ToFieldString());
			if (video_fields != null) reqparams.Add("video_fields", video_fields.ToFieldString());
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			return reqparams;
		}

		#endregion Find Playlist By Reference Id

		#region Find Playlists By Reference Ids

		public BCQueryResult FindPlaylistsByReferenceIds(List<string> reference_ids) {
			return FindPlaylistsByReferenceIds(reference_ids, null);
		}
		public BCQueryResult<CustomFieldType> FindPlaylistsByReferenceIds<CustomFieldType>(List<string> reference_ids) {
			return FindPlaylistsByReferenceIds<CustomFieldType>(reference_ids, null);
		}

		public BCQueryResult FindPlaylistsByReferenceIds(List<string> reference_ids, List<VideoFields> video_fields) {
			return FindPlaylistsByReferenceIds(reference_ids, video_fields, null);
		}
		public BCQueryResult<CustomFieldType> FindPlaylistsByReferenceIds<CustomFieldType>(List<string> reference_ids, List<VideoFields> video_fields) {
			return FindPlaylistsByReferenceIds<CustomFieldType>(reference_ids, video_fields, null);
		}

		public BCQueryResult FindPlaylistsByReferenceIds(List<string> reference_ids, List<VideoFields> video_fields, List<string> custom_fields) {
			return FindPlaylistsByReferenceIds(reference_ids, video_fields, custom_fields, null);
		}
		public BCQueryResult<CustomFieldType> FindPlaylistsByReferenceIds<CustomFieldType>(List<string> reference_ids, List<VideoFields> video_fields, List<string> custom_fields) {
			return FindPlaylistsByReferenceIds<CustomFieldType>(reference_ids, video_fields, custom_fields, null);
		}

        public BCQueryResult FindPlaylistsByReferenceIds(List<string> reference_ids, List<VideoFields> video_fields, List<string> custom_fields, List<PlaylistFields> playlist_fields) {
            return FindPlaylistsByReferenceIds(reference_ids, video_fields, custom_fields, playlist_fields, MediaDeliveryTypeEnum.DEFAULT);
        }
		public BCQueryResult<CustomFieldType> FindPlaylistsByReferenceIds<CustomFieldType>(List<string> reference_ids, List<VideoFields> video_fields, List<string> custom_fields, List<PlaylistFields> playlist_fields) {
			return FindPlaylistsByReferenceIds<CustomFieldType>(reference_ids, video_fields, custom_fields, playlist_fields, MediaDeliveryTypeEnum.DEFAULT);
		}

		public BCQueryResult FindPlaylistsByReferenceIds(List<string> reference_ids, List<VideoFields> video_fields, List<string> custom_fields, List<PlaylistFields> playlist_fields, MediaDeliveryTypeEnum media_delivery) {

			Dictionary<String, String> reqparams = FindPlaylistsByReferenceIdsReqParams(reference_ids, video_fields, custom_fields, playlist_fields, media_delivery);
			BCQueryResult qr = MultipleQueryHandler(reqparams, BCObjectType.playlists, Account);

			return qr;
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
		public BCQueryResult<CustomFieldType> FindPlaylistsByReferenceIds<CustomFieldType>(List<string> reference_ids, List<VideoFields> video_fields, List<string> custom_fields, List<PlaylistFields> playlist_fields, MediaDeliveryTypeEnum media_delivery) {

			Dictionary<String, String> reqparams = FindPlaylistsByReferenceIdsReqParams(reference_ids, video_fields, custom_fields, playlist_fields, media_delivery);
			BCQueryResult<CustomFieldType> qr = MultipleQueryHandler<CustomFieldType>(reqparams, BCObjectType.playlists, Account);

			return qr;
		}

		private Dictionary<String, String> FindPlaylistsByReferenceIdsReqParams(List<string> reference_ids, List<VideoFields> video_fields, List<string> custom_fields, List<PlaylistFields> playlist_fields, MediaDeliveryTypeEnum media_delivery) {
			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_playlists_by_reference_ids");
			reqparams.Add("reference_ids", Implode(reference_ids));
			reqparams.Add("media_delivery", media_delivery.ToString());
			if (playlist_fields != null) reqparams.Add("playlist_fields", playlist_fields.ToFieldString());
			if (video_fields != null) reqparams.Add("video_fields", video_fields.ToFieldString());
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			return reqparams;
		}

		#endregion Find Playlists By Ids

		#region Find Playlists For Player Id

		public BCQueryResult FindPlaylistsForPlayerId(long player_id) {
			return FindPlaylistsForPlayerId(player_id, -1);
		}
		public BCQueryResult<CustomFieldType> FindPlaylistsForPlayerId<CustomFieldType>(long player_id) {
			return FindPlaylistsForPlayerId<CustomFieldType>(player_id, -1);
		}

		public BCQueryResult FindPlaylistsForPlayerId(long player_id, int howMany) {
			return FindPlaylistsForPlayerId(player_id, howMany, null);
		}
		public BCQueryResult<CustomFieldType> FindPlaylistsForPlayerId<CustomFieldType>(long player_id, int howMany) {
			return FindPlaylistsForPlayerId<CustomFieldType>(player_id, howMany, null);
		}

		public BCQueryResult FindPlaylistsForPlayerId(long player_id, int howMany, List<VideoFields> video_fields) {
			return FindPlaylistsForPlayerId(player_id, howMany, video_fields, null);
		}
		public BCQueryResult<CustomFieldType> FindPlaylistsForPlayerId<CustomFieldType>(long player_id, int howMany, List<VideoFields> video_fields) {
			return FindPlaylistsForPlayerId<CustomFieldType>(player_id, howMany, video_fields, null);
		}

		public BCQueryResult FindPlaylistsForPlayerId(long player_id, int howMany, List<VideoFields> video_fields, List<string> custom_fields) {
			return FindPlaylistsForPlayerId(player_id, howMany, video_fields, custom_fields, null);
		}
		public BCQueryResult<CustomFieldType> FindPlaylistsForPlayerId<CustomFieldType>(long player_id, int howMany, List<VideoFields> video_fields, List<string> custom_fields) {
			return FindPlaylistsForPlayerId<CustomFieldType>(player_id, howMany, video_fields, custom_fields, null);
		}

        public BCQueryResult FindPlaylistsForPlayerId(long player_id, int howMany, List<VideoFields> video_fields, List<string> custom_fields, List<PlaylistFields> playlist_fields) {
            return FindPlaylistsForPlayerId(player_id, howMany, video_fields, custom_fields, playlist_fields, MediaDeliveryTypeEnum.DEFAULT);
        }
		public BCQueryResult<CustomFieldType> FindPlaylistsForPlayerId<CustomFieldType>(long player_id, int howMany, List<VideoFields> video_fields, List<string> custom_fields, List<PlaylistFields> playlist_fields) {
			return FindPlaylistsForPlayerId<CustomFieldType>(player_id, howMany, video_fields, custom_fields, playlist_fields, MediaDeliveryTypeEnum.DEFAULT);
		}

		public BCQueryResult FindPlaylistsForPlayerId(long player_id, int howMany, List<VideoFields> video_fields, List<string> custom_fields, List<PlaylistFields> playlist_fields, MediaDeliveryTypeEnum media_delivery) {

			Dictionary<String, String> reqparams = FindPlaylistsForPlayerIdReqParams(player_id, howMany, video_fields, custom_fields, playlist_fields, media_delivery);
			BCQueryResult qr = MultipleQueryHandler(reqparams, BCObjectType.playlists, Account);

			return qr;
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
		public BCQueryResult<CustomFieldType> FindPlaylistsForPlayerId<CustomFieldType>(long player_id, int howMany, List<VideoFields> video_fields, List<string> custom_fields, List<PlaylistFields> playlist_fields, MediaDeliveryTypeEnum media_delivery) {

			Dictionary<String, String> reqparams = FindPlaylistsForPlayerIdReqParams(player_id, howMany, video_fields, custom_fields, playlist_fields, media_delivery);
			BCQueryResult<CustomFieldType> qr = MultipleQueryHandler<CustomFieldType>(reqparams, BCObjectType.playlists, Account);

			return qr;
		}

		private Dictionary<String, String> FindPlaylistsForPlayerIdReqParams(long player_id, int howMany, List<VideoFields> video_fields, List<string> custom_fields, List<PlaylistFields> playlist_fields, MediaDeliveryTypeEnum media_delivery) {
			
			Dictionary<String, String> reqparams = new Dictionary<string, string>();

			//Build the REST parameter list
			reqparams.Add("command", "find_playlists_for_player_id");
			reqparams.Add("player_id", player_id.ToString());
			reqparams.Add("media_delivery", media_delivery.ToString());
			if (howMany >= 0) reqparams.Add("page_size", howMany.ToString());
			if (playlist_fields != null) reqparams.Add("playlist_fields", playlist_fields.ToFieldString());
			if (video_fields != null) reqparams.Add("video_fields", video_fields.ToFieldString());
			if (custom_fields != null) reqparams.Add("custom_fields", Implode(custom_fields));
			return reqparams;
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

		public RPCResponse<BCVideo> UpdateVideo(BCVideo video) {

			//Get the JSon reader returned from the APIRequest
			RPCResponse<BCVideo> rpcr = BCAPIRequest.ExecuteWrite<BCVideo>(UpdateVideoPostParams(video), Account);

			return rpcr;
		}
		/// <summary>
		/// Updates the video you specify
		/// </summary>
		/// <param name="video">
		/// The metadata for the video you'd like to update. This takes the form of a JSON object of name/value pairs, each of which corresponds to a settable property of the Video object. 
		/// </param>
		/// <returns></returns>
		public RPCResponse<BCVideo<CustomFieldType>> UpdateVideo<CustomFieldType>(BCVideo<CustomFieldType> video) {

			//Get the JSon reader returned from the APIRequest
			RPCResponse<BCVideo<CustomFieldType>> rpcr = BCAPIRequest.ExecuteWrite<BCVideo<CustomFieldType>>(UpdateVideoPostParams<CustomFieldType>(video), Account);
			
			return rpcr;
		}

		private Dictionary<string, object> UpdateVideoPostParams<CustomFieldType>(BCVideo<CustomFieldType> video){
			// Generate post objects
			Dictionary<string, object> postParams = new Dictionary<string, object>();

			//add video to the post params
			RPCRequest rpc = new RPCRequest();
			rpc.method = "update_video";
			rpc.parameters = "\"video\": " + video.ToJSON() + " ,\"token\": \"" + Account.WriteToken.Value + "\"";
			postParams.Add("json", rpc.ToJSON());
			return postParams;
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

		public RPCResponse<UploadStatusEnum> GetUploadStatus(string reference_id) {
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

		#region Remove Logo Overlay

		private RPCResponse<BCVideo> RemoveLogoOverlay(string video_reference_id) {
			return RemoveLogoOverlay(-1, video_reference_id);
		}
		private RPCResponse<BCVideo<CustomFieldType>> RemoveLogoOverlay<CustomFieldType>(string video_reference_id) {
			return RemoveLogoOverlay<CustomFieldType>(-1, video_reference_id);
		}
		public RPCResponse<BCVideo> RemoveLogoOverlay(long videoId) {
			return RemoveLogoOverlay(videoId, null);
		}
		public RPCResponse<BCVideo<CustomFieldType>> RemoveLogoOverlay<CustomFieldType>(long videoId) {
			return RemoveLogoOverlay<CustomFieldType>(videoId, null);
		}

		private RPCResponse<BCVideo> RemoveLogoOverlay(long video_id, string video_reference_id) {
			//Get the JSon reader returned from the APIRequest
			RPCResponse<BCVideo> rpcr = BCAPIRequest.ExecuteWrite<BCVideo>(RemoveLogoOverlayPostParams(video_id, video_reference_id), Account);
			return rpcr;
		}
		/// <summary>
		/// This will remove the logo overlay on the video that you've specified
		/// </summary>
		/// <param name="videoId">The ID of the video you want updated</param>
		/// <param name="reference_id">The Reference ID of the video you want updated</param>
		/// <returns></returns>
		private RPCResponse<BCVideo<CustomFieldType>> RemoveLogoOverlay<CustomFieldType>(long video_id, string video_reference_id) {
			//Get the JSon reader returned from the APIRequest
			RPCResponse<BCVideo<CustomFieldType>> rpcr = BCAPIRequest.ExecuteWrite<BCVideo<CustomFieldType>>(RemoveLogoOverlayPostParams(video_id, video_reference_id), Account);
			return rpcr;
		}

		private Dictionary<string, object> RemoveLogoOverlayPostParams(long video_id, string video_reference_id) {
			
			// Generate post objects
			Dictionary<string, object> postParams = new Dictionary<string, object>();

			//add video to the post params
			RPCRequest rpc = new RPCRequest();
			rpc.method = "remove_logo_overlay";
			if (video_id > -1) {
				rpc.parameters += ",\"video_id\": \"" + video_id.ToString() + "\"";
			} else if (video_reference_id != null) {
				rpc.parameters += ",\"video_reference_id\": \"" + video_reference_id + "\"";
			}
			rpc.parameters += ", \"token\": \"" + Account.WriteToken.Value + "\"";
			postParams.Add("json", rpc.ToJSON());
			return postParams;
		}

		#endregion Remove Logo Overlay

		#endregion Video Write

		#region Playlist Write

		#region Create Playlist

		public RPCResponse<long> CreatePlaylist(BCPlaylist playlist) {

			//Get the JSon reader returned from the APIRequest
			RPCResponse<long> rpcr = BCAPIRequest.ExecuteWrite<long>(CreatePlaylistPostParams(playlist), Account);

			return rpcr;
		}
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
		public RPCResponse<long> CreatePlaylist<CustomFieldType>(BCPlaylist<CustomFieldType> playlist) {

			//Get the JSon reader returned from the APIRequest
			RPCResponse<long> rpcr = BCAPIRequest.ExecuteWrite<long>(CreatePlaylistPostParams<CustomFieldType>(playlist), Account);

			return rpcr;
		}

		private Dictionary<string, object> CreatePlaylistPostParams<CustomFieldType>(BCPlaylist<CustomFieldType> playlist) {
			
			// Generate post objects
			Dictionary<string, object> postParams = new Dictionary<string, object>();

			//add video to the post params
			RPCRequest rpc = new RPCRequest();
			rpc.method = "create_playlist";
			rpc.parameters = "\"playlist\": " + playlist.ToCreateJSON() + " ,\"token\": \"" + Account.WriteToken.Value + "\"";
			postParams.Add("json", rpc.ToJSON());
			return postParams;
		}

		#endregion Create Playlist

		#region Update Playlist

		public RPCResponse<BCPlaylist> UpdatePlaylist(BCPlaylist playlist) {

			//Get the JSon reader returned from the APIRequest
			RPCResponse<BCPlaylist> rpcr = BCAPIRequest.ExecuteWrite<BCPlaylist>(UpdatePlaylistPostParams(playlist), Account);

			return rpcr;
		}
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
		public RPCResponse<BCPlaylist<CustomFieldType>> UpdatePlaylist<CustomFieldType>(BCPlaylist<CustomFieldType> playlist) {

			//Get the JSon reader returned from the APIRequest
			RPCResponse<BCPlaylist<CustomFieldType>> rpcr = BCAPIRequest.ExecuteWrite<BCPlaylist<CustomFieldType>>(UpdatePlaylistPostParams<CustomFieldType>(playlist), Account);
			
			return rpcr;
		}

		private Dictionary<string, object> UpdatePlaylistPostParams<CustomFieldType>(BCPlaylist<CustomFieldType> playlist) {

			// Generate post objects
			Dictionary<string, object> postParams = new Dictionary<string, object>();

			//add video to the post params
			RPCRequest rpc = new RPCRequest();
			rpc.method = "update_playlist";
			rpc.parameters = "\"playlist\": " + playlist.ToUpdateJSON() + " ,\"token\": \"" + Account.WriteToken.Value + "\"";
			postParams.Add("json", rpc.ToJSON());
			return postParams;
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
