<%@ Page Language="C#" MasterPageFile="~/sitecore modules/Web/Brightcove/Media Folder/BCLibrary.master" AutoEventWireup="true" CodeFile="BrightcoveAccount.aspx.cs" Inherits="BrightcoveSCUtil.EditorTabs.BrightcoveAccountTab" %>

<asp:Content ID="cntHead" ContentPlaceHolderID="Head" runat="server">

</asp:Content>

<asp:Content ID="cntMain" ContentPlaceHolderID="MainContent" runat="server">
	<div id="Account">
		<div class="LibraryInfo">
			<div class="Videos">
				<div class="SectionTitle">Videos</div>
				<div class="Details">
					Total Videos: <asp:Literal ID="ltlTotalVideos" runat="server"></asp:Literal>
				</div>
			</div>
			<div class="Playlists">
				<div class="SectionTitle">Playlists</div>
				<div class="Details">
					Total Playlists: <asp:Literal ID="ltlTotalPlaylists" runat="server"></asp:Literal>
				</div>
			</div>
		</div>
		<div class="UpdatePanel">				
			<div class="SectionTitle">Update Videos and Playlists</div>
			<asp:RadioButtonList ID="radUpdate" runat="server">
				<asp:ListItem Text="Add new items" Value="new" Selected="True"></asp:ListItem>
				<asp:ListItem Text="Update existing items" Value="update"></asp:ListItem>
				<asp:ListItem Text="Add new and update exising items" Value="both"></asp:ListItem>
			</asp:RadioButtonList>
			<asp:Button ID="btnUpdate" CssClass="Submit" runat="server" OnClick="btnUpdate_Click" Text="Update Library"></asp:Button>					
			<asp:Panel ID="pnlNewMessage" CssClass="Message" Visible="false" runat="server">
				There were <asp:Literal ID="ltlNewItem" runat="server"></asp:Literal> new videos imported.<br/>
				There were <asp:Literal ID="ltlNewPlaylists" runat="server"></asp:Literal> new playlists imported.<br/>
			</asp:Panel>
			<asp:Panel ID="pnlUpdateMessage" CssClass="Message" Visible="false" runat="server">	
				There were <asp:Literal ID="ltlUpdatedPlaylists" runat="server"></asp:Literal> playlists updated.<br/>
				There were <asp:Literal ID="ltlUpdatedItems" runat="server"></asp:Literal> videos updated.<br/>
			</asp:Panel>
			
		</div>
		<asp:Literal ID="ltlError" runat="server"></asp:Literal>
	</div>
</asp:Content>
