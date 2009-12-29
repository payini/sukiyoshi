using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;

namespace BrightcoveAPI.UI
{
	[DefaultProperty("VideoID")]
	[ParseChildren(true)]
	[ControlBuilderAttribute(typeof(VideoPlayerControlBuilder))]
	[ToolboxData("<{0}:VideoPlayer runat=server></{0}:VideoPlayer>")]
	public class VideoPlayer : WebControl
	{
		#region Properties

		[Bindable(true), Category("Appearance"), DefaultValue(-1), Localizable(true)]
		public long VideoID {
			get {
				return (ViewState["VideoID"] == null) ? -1 : (long)ViewState["VideoID"];
			}set {
				ViewState["VideoID"] = value;
			}
		}

		[Bindable(true), Category("Appearance"), DefaultValue(-1), Localizable(true)]
		public long PlayerID {
			get {
				return (ViewState["PlayerID"] == null) ? -1 : (long)ViewState["PlayerID"];
			}set {
				ViewState["PlayerID"] = value;
			}
		}

		[Bindable(true), Category("Appearance"), DefaultValue("#000000"), Localizable(true)]
		public string BackColor {
			get {
				return (ViewState["BackColor"] == null) ? "#000000" : (String)ViewState["BackColor"];
			}set {
				ViewState["BackColor"] = value;
			}
		}

		[Bindable(true), Category("Appearance"), DefaultValue(0), Localizable(true)]
		public int Width {
			get {
				return (ViewState["Width"] == null) ? 0 : (int)ViewState["Width"];
			}set {
				ViewState["Width"] = value;
			}
		}

		[Bindable(true), Category("Appearance"), DefaultValue(0), Localizable(true)]
		public int Height {
			get {
				return (ViewState["Height"] == null) ? 0 : (int)ViewState["Height"];
			}set {
				ViewState["Height"] = value;
			}
		}

		[Bindable(true), Category("Appearance"), DefaultValue("_Player"), Localizable(true)]
		public string PlayerName {
			get {
				return (ViewState["PlayerName"] == null) ? this.ClientID + "_Player" : (String)ViewState["PlayerName"];
			}set {
				ViewState["PlayerName"] = value;
			}
		}

		[Bindable(true), Category("Appearance"), DefaultValue(false), Localizable(true)]
		public bool AutoStart {
			get {
				return (ViewState["AutoStart"] == null) ? false : (bool)ViewState["AutoStart"];
			}set {
				ViewState["AutoStart"] = value;
			}
		}

		[Bindable(true), Category("Appearance"), DefaultValue(true), Localizable(true)]
		public bool IsVid {
			get {
				return (ViewState["IsVid"] == null) ? true : (bool)ViewState["IsVid"];
			}
			set {
				ViewState["IsVid"] = value;
			}
		}

		[Bindable(true), Category("Appearance"), DefaultValue("transparent"), Localizable(true)]
		public string WMode {
			get {
				return (ViewState["WMode"] == null) ? String.Empty : (String)ViewState["WMode"];
			}set {
				ViewState["WMode"] = value;
			}
		}

		[Bindable(true), Category("Appearance"), DefaultValue(-1), Localizable(true)]
		public long VideoList {
			get {
				return (ViewState["VideoList"] == null) ? -1 : (long)ViewState["VideoList"];
			}set {
				ViewState["VideoList"] = value;
			}
		}

		private List<PlaylistTab> _PlaylistTabs = new List<PlaylistTab>();

		protected string PlaylistTabString {
			get {
				StringBuilder sb = new StringBuilder();
				foreach (PlaylistTab p in PlaylistTabs) {
					if (sb.Length > 0) {
						sb.Append(",");
					}
					sb.Append(p.Value.ToString());
				}
				return sb.ToString();
			}
		}

		/// <summary>
		/// A collection of Playlist IDs for Tabs
		/// </summary>
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public List<PlaylistTab> PlaylistTabs {
			get {
				return _PlaylistTabs;
			}
		}

		private List<PlaylistCombo> _PlaylistCombos = new List<PlaylistCombo>();
	
		protected string PlaylistComboString {
			get {
				StringBuilder sb = new StringBuilder();
				char[] comma = { ',' };
				foreach (PlaylistCombo p in PlaylistCombos) {
					if (sb.Length > 0) {
						sb.Append(",");
					}
					sb.Append(p.Value.ToString());
				}
				return sb.ToString();
			}
		}
		
		/// <summary>
		/// A collection of Playlist IDs for the Combo Box
		/// </summary>
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public List<PlaylistCombo> PlaylistCombos {
			get {
				return _PlaylistCombos;
			}
		}

		#endregion Properties

		protected override void OnPreRender(EventArgs e) {
            
			//Add Brightcove experiences javascript
			Page.ClientScript.RegisterClientScriptInclude("BCExperiences", "http://admin.brightcove.com/js/BrightcoveExperiences.js");
			
			//Add Brightcove API Modules javascript
			Page.ClientScript.RegisterClientScriptInclude("BCAPIModules", "http://admin.brightcove.com/js/APIModules_all.js");
						
			//Add the Add/Remove player js from the page
			Page.ClientScript.RegisterClientScriptInclude("AddRemovePlayer", Page.ClientScript.GetWebResourceUrl(this.GetType(), "BrightcoveAPI.UI.Resources.AddRemovePlayer.js"));
			base.OnPreRender(e);
        }

		/// <summary>
		/// Modifies the container to 
		/// </summary>
		/// <param name="writer"></param>
		public override void RenderBeginTag(HtmlTextWriter writer) {

			//Add writer attributes
			writer.AddAttribute("id", "VideoPlayer_" + this.ClientID);

			//Write writer content
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
		}

		/// <summary>
		/// Creates the JavaScript block and Flash div controls and adds them to the page
		/// </summary>
		protected override void CreateChildControls() {

			//Add the flash content panel to the page
			this.Controls.Clear();

			Panel vp = new Panel();
			vp.ID = this.ClientID;
			this.Controls.Add(vp);

			if (!VideoID.Equals(-1) && !PlayerID.Equals(-1)) {

				//Create the SWFObjectCustoms script block
				Literal litScript = new Literal();
				litScript.Text = "<script type=\"text/javascript\">\n";
				litScript.Text += "addPlayer(" + VideoID + ", " + PlayerID + ", '" + PlayerName + "', " + AutoStart.ToString().ToLower() + ", '" + BackColor + "', " + Width + ", " + Height + ", " + IsVid.ToString().ToLower() + ", '" + WMode + "', '" + vp.ClientID + "', '" + PlaylistTabString + "', '" + PlaylistComboString + "', '" + VideoList.ToString() + "'); \n";
				litScript.Text += "</script>";

				//Add the SWFObjectCustoms script block to the page
				//Page.ClientScript.RegisterStartupScript(this.GetType(), "addPlayer_" + this.ID.ToString(), litScript.Text);
				Controls.Add(litScript);
			}
		}
	}

	public class VideoPlayerControlBuilder : ControlBuilder
	{
		public override Type GetChildControlType(String tagName, IDictionary attributes) {

			if (String.Compare(tagName, "PlaylistTab", true) == 0) {
				return typeof(PlaylistTab);
			}
			else if (String.Compare(tagName, "PlaylistCombo", true) == 0) {
				return typeof(PlaylistCombo);
			}

			return null;
		}
	}
	
	public class PlaylistTab
	{
		private long _value;
		public long Value {
			get {
				return _value;
			}
			set {
				_value = value;
			}
		}

		public PlaylistTab(long Val) {
			_value = Val;
		}
	}

	public class PlaylistCombo
	{
		private long _value;
		public long Value {
			get {
				return _value;
			}
			set {
				_value = value;
			}
		}

		public PlaylistCombo(long Val) {
			_value = Val;
		}
	}
}
