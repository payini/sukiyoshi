<%@ Page Language="C#" MasterPageFile="~/sitecore modules/Web/Brightcove/Media Folder/BCLibrary.master" AutoEventWireup="true" CodeFile="BrightcovePlaylist.aspx.cs" Inherits="BrightcoveSCUtil.EditorTabs.BrightcovePlaylistTab" %>
<%@ Register TagPrefix="bc" Namespace="BrightcoveSDK.UI" Assembly= "BrightcoveSDK" %>

<asp:Content ID="cntHead" ContentPlaceHolderID="Head" runat="server">

</asp:Content>

<asp:Content ID="cntMain" ContentPlaceHolderID="MainContent" runat="server">
	<div id="Playlist">
		<div class="PlaylistCreate">
			<div class="SectionTitle">Save Playlist to Brightcove</div>
			<bc:PlaylistAddUpdate id="plPlaylist" runat="server" />
			<asp:Button ID="btnSave" CssClass="Submit" OnClick="btnSave_Click" Text="Save Playlist" runat="server" />
			<br />
			<asp:Panel ID="pnlSaveMessage" CssClass="Message" Visible="false" runat="server">
				<asp:Literal ID="ltlSaveMessage" runat="server"></asp:Literal>
			</asp:Panel>
		</div>
		<div class="UpdatePanel">	
			<div class="SectionTitle">Playlist Info</div>
			<div class="fieldRow">
				<div class="fieldLabel">Playlist ID :</div>
				<asp:Literal ID="ltlPlaylistID" runat="server"></asp:Literal>
			</div>
			<div class="fieldRow">
				<div class="fieldLabel">Videos :</div>
				<asp:Literal ID="ltlVideos" runat="server"></asp:Literal>
			</div>
			<div class="fieldRow">
				<div class="fieldLabel">Account ID :</div>
				<asp:Literal ID="ltlAccount" runat="server"></asp:Literal>
			</div>
			<div class="fieldRow">
				<div class="fieldLabel">Update from Brightcove :</div>
				<asp:Button ID="btnUpdate" CssClass="Submit" runat="server" OnClick="btnUpdate_Click" Text="Update Playlist"></asp:Button>					
				<asp:Panel ID="pnlUpdateMessage" CssClass="Message" Visible="false" runat="server">
					<asp:Literal ID="ltlUpdateMessage" runat="server"></asp:Literal>
				</asp:Panel>
			</div>
			<br />
			<div class="fieldRow">
				<div class="fieldLabel">Delete Video :</div>
				<asp:Button ID="btnDelete" CssClass="Submit" Text="Delete Playlist" runat="server" OnClick="btnDelete_Click" />
				(this will remove it from Sitecore and Brightcove)
				<asp:Panel ID="pnlDeleteMessage" CssClass="Message" Visible="false" runat="server">
					<asp:Literal ID="ltlDeleteMessage" runat="server"></asp:Literal>
				</asp:Panel>
			</div>
		</div>
		<asp:Literal ID="ltlError" runat="server"></asp:Literal>
	</div>
</asp:Content>