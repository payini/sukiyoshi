using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OTP.Web.BrightcoveAPI.UI
{
	[DefaultProperty("Text")]
	[ToolboxData("<{0}:UploadVideo runat=server></{0}:UploadVideo>")]
	public class UploadVideo : FileUpload
	{
		#region Properties

		[Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
		public string VideoName {
			get {
				return (ViewState["VideoName"] == null) ? string.Empty : (String)ViewState["VideoName"];
			}set {
				ViewState["VideoName"] = value;
			}
		}

		[Bindable(true), Category("Appearance"), DefaultValue(false), Localizable(true)]
		public bool ShowShortDescriptionForm {
			get {
				return (ViewState["ShowShortDescriptionForm"] == null) ? false : (bool)ViewState["ShowShortDescriptionForm"];
			}
			set {
				ViewState["ShowShortDescriptionForm"] = value;
			}
		}

		[Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
		public string ShortDescription {
			get {
				return (ViewState["ShortDescription"] == null) ? string.Empty : (String)ViewState["ShortDescription"];
			}set {
				ViewState["ShortDescription"] = value;
			}
		}

		[Bindable(true), Category("Appearance"), DefaultValue(false), Localizable(true)]
		public bool ShowLongDescriptionForm {
			get {
				return (ViewState["ShowLongDescriptionForm"] == null) ? false : (bool)ViewState["ShowLongDescriptionForm"];
			}
			set {
				ViewState["ShowLongDescriptionForm"] = value;
			}
		}

		[Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
		public string LongDescription {
			get {
				return (ViewState["LongDescription"] == null) ? string.Empty : (String)ViewState["LongDescription"];
			}set {
				ViewState["LongDescription"] = value;
			}
		}

		[Bindable(true), Category("Appearance"), DefaultValue(false), Localizable(true)]
		public bool ShowTagsForm {
			get {
				return (ViewState["ShowTagsForm"] == null) ? false : (bool)ViewState["ShowTagsForm"];
			}
			set {
				ViewState["ShowTagsForm"] = value;
			}
		}

		[Bindable(true), Category("Appearance"), DefaultValue(null), Localizable(true)]
		public List<string> Tags {
			get {
				return (ViewState["Tags"] == null) ? null : (List<String>)ViewState["Tags"];
			}set {
				ViewState["Tags"] = value;
			}
		}

		[Bindable(true), Category("Appearance"), DefaultValue(false), Localizable(true)]
		public bool ShowEconomicsForm {
			get {
				return (ViewState["ShowEconomicsForm"] == null) ? false : (bool)ViewState["ShowEconomicsForm"];
			}
			set {
				ViewState["ShowEconomicsForm"] = value;
			}
		}

		[Bindable(true), Category("Appearance"), DefaultValue(BCVideoEconomics.AD_SUPPORTED), Localizable(true)]
		public BCVideoEconomics Economics {
			get {
				return (ViewState["Economics"] == null) ? BCVideoEconomics.AD_SUPPORTED : (BCVideoEconomics)ViewState["Economics"];
			}set {
				ViewState["Economics"] = value;
			}
		}
		
		#endregion Properties

		/// <summary>
		/// Modifies the container to 
		/// </summary>
		/// <param name="writer"></param>
		public override void RenderBeginTag(HtmlTextWriter writer) {

			//Add writer attributes
			writer.AddAttribute("class", "VideoUpload_" + this.ClientID);

			//Write writer content
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
		}

		protected override void RenderContents(HtmlTextWriter output) {

			output.Write("<div class=\"VideoName\">");
			output.Write("<label for=\"VideoName_" + this.ClientID + "\" >Video Name</label");
			output.Write("<input type=\"text\" id=\"VideoName_" + this.ClientID + "\" name=\"VideoName_" + this.ClientID + "\"/>");
			output.Write("</div>");
			
			if(ShowShortDescriptionForm){
				output.Write("<div class=\"ShortDescription\">");
				output.Write("<label for=\"ShortDescription_" + this.ClientID + "\" >Short Description</label");
				output.Write("<textarea id=\"ShortDescription_" + this.ClientID + "\" name=\"ShortDescription_" + this.ClientID + "\"></textarea>");
				output.Write("</div>");
			}
			if(ShowLongDescriptionForm){
				output.Write("<div class=\"LongDescription\">");
				output.Write("<label for=\"LongDescription_" + this.ClientID + "\" >Long Description</label");
				output.Write("<textarea id=\"LongDescription_" + this.ClientID + "\" name=\"LongDescription_" + this.ClientID + "\"></textarea>");
				output.Write("</div>");
			}
			if(ShowTagsForm){
				output.Write("<div class=\"Tags\">");
				output.Write("<label for=\"Tags_" + this.ClientID + "\" >Tags</label");
				output.Write("<textarea id=\"Tags_" + this.ClientID + "\" name=\"Tags_" + this.ClientID + "\"></textarea>");
				output.Write("<label for=\"Tags_" + this.ClientID + "\">(Comma Delimited List)</label>");
				output.Write("</div>");
			}
				
			if(ShowEconomicsForm){
				output.Write("<div class=\"Economics\">");
				output.Write("<label for=\"Economics_" + this.ClientID + "\" >Economics</label"); 
				output.Write("<select id=\"Economics_" + this.ClientID + "\" name=\"Economics_" + this.ClientID + "\">");
				foreach (string s in Enum.GetNames(typeof(BCVideoEconomics))) {
					output.Write("<option value=\"" + s + "\">" + s + "</option>");
				}
				output.Write("</select>");
				output.Write("</div>");
			}

			output.Write("<div class=\"FileUploader\"><input id=\"" + this.ClientID + "\" type=\"file\" name=\"" + this.ClientID + "\"/></div>");
			
		}
	}
}
