<%@ Page Language="C#" MasterPageFile="~/sitecore modules/Web/Brightcove/Media Folder/BCLibrary.master" AutoEventWireup="true" CodeFile="BrightcoveVideoImages.aspx.cs" Inherits="BrightcoveSCUtil.EditorTabs.BrightcoveVideoImagesTab" %>
<%@ Register TagPrefix="bc" Namespace="BrightcoveSDK.UI" Assembly= "BrightcoveSDK" %>

<asp:Content ID="cntHead" ContentPlaceHolderID="Head" runat="server">

</asp:Content>

<asp:Content ID="cntMain" ContentPlaceHolderID="MainContent" runat="server">
	<div id="Video">
		<div class="AddImage">
			<div class="SectionTitle">Add Image to this Video</div>
			<bc:ImageAdd ID="iaAdd" runat="server"></bc:ImageAdd>
			<asp:Button ID="btnAddImage" CssClass="Submit" OnClick="btnAddImage_Click" Text="Add Image" runat="server" />
			<br /><br />
			<asp:Panel ID="pnlImageMessage" CssClass="Message" Visible="false" runat="server">
				<asp:Literal ID="ltlImageMessage" runat="server"></asp:Literal>
			</asp:Panel>
		</div>
		<br />
		<div class="VideoInfo">
			<div class="SectionTitle">Video Images</div>
			<asp:Panel ID="pnlThumb" Visible="false" CssClass="Detail" runat="server">
				<div class="DetailTitle">Thumbnail</div>
				<asp:Image ID="imgThumb" runat="server" />
			</asp:Panel>
			<asp:Panel ID="pnlStill" Visible="false" CssClass="Detail" runat="server">
				<div class="DetailTitle">Video Still</div>
				<asp:Image ID="imgStill" runat="server" />
			</asp:Panel>
		</div>
		<asp:Literal ID="ltlError" runat="server"></asp:Literal>
	</div>
</asp:Content>