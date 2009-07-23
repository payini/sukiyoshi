using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OTP.Web.BrightcoveAPI
{
	public static class BCAPIConfig
	{
		public static string ReadToken {
			get {
				try {
					return ConfigurationManager.AppSettings["BC_ReadToken"].ToString();
				}
				catch(Exception ex)  {
					throw new Exception("Application Setting string missing from the web.config - 'BC_ReadToken'", ex);
				}
			}
		}

		public static string WriteToken {
			get {
				try {
					return ConfigurationManager.AppSettings["BC_WriteToken"].ToString();
				}
				catch(Exception ex) {
					throw new Exception("Application Setting string missing from the web.config - 'BC_WriteToken'", ex);
				}
			}
		}

		public static string ServiceURL {
			get {
				try {
					return ConfigurationManager.AppSettings["BC_ServiceURL"].ToString();
				}
				catch(Exception ex){
					throw new Exception("Application Setting string missing from the web.config - 'BC_ServiceURL'", ex);
				}
			}
		}
	}
}
