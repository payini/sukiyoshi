<%@ Page Language="C#" MasterPageFile="~/sitecore modules/Web/Brightcove/Media Folder/BCLibrary.master" AutoEventWireup="true" CodeFile="BrightcovePlaylistImages.aspx.cs" Inherits="BrightcoveSCUtil.EditorTabs.BrightcovePlaylistImagesTab" %>
<%@ Register TagPrefix="bc" Namespace="BrightcoveSDK.UI" Assembly= "BrightcoveSDK" %>

<asp:Content ID="cntHead" ContentPlaceHolderID="Head" runat="server">

</asp:Content>

<asp:Content ID="cntMain" ContentPlaceHolderID="MainContent" runat="server">
	<div id="Playlist">
		<div class="PlaylistInfo">
			<div class="SectionTitle">Playlist Images</div>
			<asp:Panel ID="pnlThumb" Visible="false" CssClass="Detail" runat="server">
				<div class="DetailTitle">Thumbnail</div>
				<asp:Image ID="imgThumb" runat="server" />
			</asp:Panel>
		</div>
		<asp:Literal ID="ltlError" runat="server"></asp:Literal>
	</div>
</asp:Content>
