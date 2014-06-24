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

			int defaultPageSize = 100;
			if (!reqparams.ContainsKey("page_size")) { // if page size is not set than set it
				reqparams.Add("page_size", defaultPageSize.ToString());
				qr.MaxToGet = -1;
			} else { // else parse it 
				qr.MaxToGet = Convert.ToInt32(reqparams["page_size"]);
				defaultPageSize = qr.MaxToGet;
			}

			
			MakeRequest(qr, reqparams, itemType, account);
			if (!reqparams.ContainsKey("page_number")) { // if page number is not set then pass make recursive calls
				reqparams.Add("page_number", "0");

				//make sure you get the correct page num
				int modifier = (qr.MaxToGet.Equals(-1)) ? qr.TotalCount : qr.MaxToGet;
				double maxPageNum = (qr.TotalCount > 0) ? Math.Ceiling((double)(modifier / defaultPageSize)) : 0;

				//if there are more to get move to next page and keep getting them
				for (int pageNum = 1; pageNum <= maxPageNum; pageNum++) {
					//update page each iteration
					reqparams["page_number"] = pageNum.ToString();
					MakeRequest(qr, reqparams, itemType, account);
				}

				if (itemType.Equals(BCObjectType.videos)) {
					//trim if specified
					if (qr.Videos.Count > qr.MaxToGet && !qr.MaxToGet.Equals(-1) && qr.MaxToGet < qr.TotalCount) {
						List<BCVideo> vidTemp = qr.Videos.GetRange(0, Convert.ToInt32(qr.MaxToGet));
						qr.Videos.Clear();
						qr.Videos.AddRange(vidTemp);
					}
				}
			}

			return qr;
		}

		private static void MakeRequest(BCQueryResult qr, Dictionary<string, string> reqparams, BCObjectType itemType, BCAccount account) {
			QueryResultPair qrp = BCAPIRequest.ExecuteRead(reqparams, account);
			qrp.JsonResult = qrp.JsonResult.Replace("\"items\":", "\"" + itemType.ToString() + "\":");
			qr.QueryResults.Add(qrp);
			qr.Merge(JSON.Converter.Deserialize<BCQueryResult>(qrp.JsonResult));
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
