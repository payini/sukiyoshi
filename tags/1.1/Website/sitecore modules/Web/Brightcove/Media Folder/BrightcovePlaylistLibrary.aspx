<%@ Page Language="C#" MasterPageFile="~/sitecore modules/Web/Brightcove/Media Folder/BCLibrary.master" AutoEventWireup="true" CodeFile="BrightcovePlaylistLibrary.aspx.cs" Inherits="BrightcoveSCUtil.EditorTabs.BrightcovePlaylistLibraryTab" %>
<%@ Register TagPrefix="bc" Namespace="BrightcoveSDK.UI" Assembly= "BrightcoveSDK" %>

<asp:Content ID="cntHead" ContentPlaceHolderID="Head" runat="server">

</asp:Content>

<asp:Content ID="cntMain" ContentPlaceHolderID="MainContent" runat="server">
	<div id="PlaylistLibrary">
		<div class="LibraryInfo">
			<div class="Playlists">
				<div class="SectionTitle">Playlist Library</div>
				<div class="Details">
					Total Playlists: <asp:Literal ID="ltlTotalPlaylists" runat="server"></asp:Literal>
				</div>
			</div>
		</div>
		<div class="UpdatePanel">				
			<div class="SectionTitle">Update Playlists</div>
			<asp:RadioButtonList ID="radUpdate" runat="server">
				<asp:ListItem Text="Add new items" Value="new" Selected="True"></asp:ListItem>
				<asp:ListItem Text="Update existing items" Value="update"></asp:ListItem>
				<asp:ListItem Text="Add new and update exising items" Value="both"></asp:ListItem>
			</asp:RadioButtonList>
			<asp:Button ID="btnUpdate" CssClass="Submit" runat="server" OnClick="btnUpdate_Click" Text="Update Library"></asp:Button>					
			<asp:Panel ID="pnlNewMessage" CssClass="Message" Visible="false" runat="server">
				There were <asp:Literal ID="ltlNewPlaylists" runat="server"></asp:Literal> new playlists imported.<br/>
			</asp:Panel>
			<asp:Panel ID="pnlUpdateMessage" CssClass="Message" Visible="false" runat="server">
				There were <asp:Literal ID="ltlUpdatedPlaylists" runat="server"></asp:Literal> playlists updated.<br/>
			</asp:Panel>
		</div>
		<asp:Panel ID="pnlCreate" runat="server" CssClass="PlaylistCreate">
			<div class="SectionTitle">Add Playlist</div>
			<bc:PlaylistAddUpdate id="plPlaylist" runat="server" />
			<asp:Button ID="btnSubmit" CssClass="Submit" OnClick="btnSubmit_Click" Text="Add Playlist" runat="server" />
			<br /><br />
			<asp:Literal ID="ltlMessage" runat="server"></asp:Literal>
		</asp:Panel>
		<asp:Literal ID="ltlError" runat="server"></asp:Literal>
	</div>
</asp:Content>