using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightcoveSDK.Entities.Containers;
using BrightcoveSDK;
using System.Configuration;
using BrightcoveSDK.HTTP;
using BrightcoveSDK.Media;
using BrightcoveSDK.Entities;

namespace BrightcoveSDK {
	public partial class BCAPI {
		#region Properties

		protected AccountConfigElement AccountConfig;

		private BCAccount _Account;
		public BCAccount Account { get; set; }

		#endregion Properties

		#region Constructors

		/// <summary>
		/// RJE 10-16-2012
		/// Construct an API with a supplied configuration
		/// </summary>
		public BCAPI(BCAccount account) {
			Account = account;
		}

		public BCAPI(string accountName) {
			BrightcoveConfig bc = (BrightcoveConfig)ConfigurationManager.GetSection("brightcove");
			foreach (AccountConfigElement a in bc.Accounts) {
				if (a.Name.Equals(accountName)) {
					Account = new BCAccount(a);
				}
			}
		}

		public BCAPI(long publisherId) {
			BrightcoveConfig bc = (BrightcoveConfig)ConfigurationManager.GetSection("brightcove");
			foreach (AccountConfigElement a in bc.Accounts) {
				if (a.PublisherID.Equals(publisherId)) {
					Account = new BCAccount(a);
				}
			}
		}

		#endregion Constructors

		#region Main Helper Methods

		private static BCQueryResult MultipleQueryHandler(Dictionary<String, String> reqparams, BCObjectType itemType, BCAccount account) {

			//Get the JSon reader returned from the APIRequest
			BCQueryResult qr = new BCQueryResult();
			qr.TotalCount = 0;

			try {

				//set some global request paramameters
				if (!reqparams.ContainsKey("page_number"))
					reqparams.Add("page_number", "0");

				//determine what the page size should be
				qr.MaxToGet = (!reqparams.ContainsKey("page_size")) ? -1 : Convert.ToInt32(reqparams["page_size"]);
				int defaultPageSize = (qr.MaxToGet.Equals(-1)) ? 100 : qr.MaxToGet;
				if (qr.MaxToGet.Equals(-1))
					reqparams.Add("page_size", defaultPageSize.ToString());

				//get initial query
				QueryResultPair qrp = BCAPIRequest.ExecuteRead(reqparams, account);
				//convert the result for deserialization
				qrp.JsonResult = qrp.JsonResult.Replace("\"items\":", "\"" + itemType.ToString() + "\":");
				qr.QueryResults.Add(qrp);
				qr.Merge(JSON.Converter.Deserialize<BCQueryResult>(qrp.JsonResult));

				//make sure you get the correct page num
				double maxPageNum = 0;
				if (qr.TotalCount > 0) {
					//if you want all use the total count to calculate the number of pages
					if (qr.MaxToGet.Equals(-1)) {
						maxPageNum = Math.Ceiling((double)(qr.TotalCount / defaultPageSize));
					}
						//or just use the max you want to calculate the number of pages
					else {
						maxPageNum = Math.Ceiling((double)(qr.MaxToGet / defaultPageSize));
					}
				}

				//if there are more to get move to next page and keep getting them
				for (int pageNum = 1; pageNum <= maxPageNum; pageNum++) {

					//update page each iteration
					reqparams["page_number"] = pageNum.ToString();

					QueryResultPair qrp2 = BCAPIRequest.ExecuteRead(reqparams, account);
					//convert the result for deserialization
					qrp2.JsonResult = qrp2.JsonResult.Replace("\"items\":", "\"" + itemType.ToString() + "\":");
					qr.QueryResults.Add(qrp2);
					qr.Merge(JSON.Converter.Deserialize<BCQueryResult>(qrp2.JsonResult));
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
						//MODIFIED_DATE,
					else if (reqparams["sort_by"].Equals("MODIFIED_DATE")) {
						qr.Videos.Sort(BCVideo.ModifiedDateComparison);
					}
						//CREATION_DATE, 
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
			} catch (Exception ex) {
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
	}
}
