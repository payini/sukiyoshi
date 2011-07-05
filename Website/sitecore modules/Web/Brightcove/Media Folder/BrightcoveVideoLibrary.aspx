<%@ Page Language="C#" MasterPageFile="~/sitecore modules/Web/Brightcove/Media Folder/BCLibrary.master" AutoEventWireup="true" CodeFile="BrightcoveVideoLibrary.aspx.cs" Inherits="BrightcoveSCUtil.EditorTabs.BrightcoveVideoLibraryTab" %>
<%@ Register TagPrefix="bc" Namespace="BrightcoveSDK.UI" Assembly= "BrightcoveSDK" %>

<asp:Content ID="cntHead" ContentPlaceHolderID="Head" runat="server">

</asp:Content>

<asp:Content ID="cntMain" ContentPlaceHolderID="MainContent" runat="server">
	<div id="VideoLibrary">
		<div class="LibraryInfo">
			<div class="Videos">
				<div class="SectionTitle">Video Library</div>
				<div class="Details">
					Total Videos: <asp:Literal ID="ltlTotalVideos" runat="server"></asp:Literal>
				</div>
			</div>
		</div>
		<div class="UpdatePanel">				
			<div class="SectionTitle">Update Videos</div>
			<asp:RadioButtonList ID="radUpdate" runat="server">
				<asp:ListItem Text="Add new items" Value="new" Selected="True"></asp:ListItem>
				<asp:ListItem Text="Update existing items" Value="update"></asp:ListItem>
				<asp:ListItem Text="Add new and update exising items" Value="both"></asp:ListItem>
			</asp:RadioButtonList>
			<asp:Button ID="btnUpdate" CssClass="Submit" runat="server" OnClick="btnUpdate_Click" Text="Update Library"></asp:Button>					
			<asp:Panel ID="pnlNewMessage" CssClass="Message" Visible="false" runat="server">
				There were <asp:Literal ID="ltlNewItem" runat="server"></asp:Literal> new videos imported.<br/>
			</asp:Panel>
			<asp:Panel ID="pnlUpdateMessage" CssClass="Message" Visible="false" runat="server">
				There were <asp:Literal ID="ltlUpdatedItems" runat="server"></asp:Literal> videos updated.<br/>
			</asp:Panel>
		</div>
		<asp:Panel ID="pnlUpload" runat="server" CssClass="VideoUpload">
			<div class="SectionTitle">Add Video</div>
			<bc:VideoAddUpdate ID="uvVideo" runat="server" />
			<asp:Button ID="btnSubmit" CssClass="Submit" OnClick="btnSubmit_Click" Text="Add Video" runat="server" />
			<br /><br />
			<asp:Literal ID="ltlMessage" runat="server"></asp:Literal>
		</asp:Panel>
		<asp:Literal ID="ltlError" runat="server"></asp:Literal>
	</div>
</asp:Content>