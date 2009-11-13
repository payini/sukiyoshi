﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OTP.Web.BrightcoveAPI.Media;
using Sitecore.Data.Items;

namespace OTP.Web.BrightcoveAPI.SitecoreUtil
{
	public class UpdateInsertPair<T>
	{
		public List<T> NewItems = new List<T>();
		public List<T> UpdatedItems = new List<T>();
	}
}
