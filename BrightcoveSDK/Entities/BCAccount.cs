using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightcoveSDK.Entities {
	public class BCAccount {

		public string Name { get; set; }

		public long PublisherID { get; set; }

		public AccountType Type { get; set; }

		public Token ReadToken { get; set; }

		public Token ReadTokenURL { get; set; }

		public Token ReadURL { get; set; }

		public Token WriteToken { get; set; }

		public Token WriteURL { get; set; }

		public BCAccount(AccountConfigElement AccountConfig) {

			Name = AccountConfig.Name;
			PublisherID = AccountConfig.PublisherID;
			Type = AccountConfig.Type;
			ReadToken = AccountConfig.ReadToken;
			ReadTokenURL = AccountConfig.ReadTokenURL;
			ReadURL = AccountConfig.ReadURL;
			WriteToken = AccountConfig.WriteToken;
			WriteURL = AccountConfig.WriteURL;
		}
	}
}
