<%@ Page Language="C#" MasterPageFile="~/sitecore modules/Web/Brightcove/Media Folder/BCLibrary.master" AutoEventWireup="true" CodeFile="BrightcoveVideoPreview.aspx.cs" Inherits="BrightcoveSCUtil.EditorTabs.BrightcoveVideoPreviewTab" %>
<%@ Register TagPrefix="bc" Namespace="BrightcoveSDK.UI" Assembly="BrightcoveSDK" %>

<asp:Content ID="cntHead" ContentPlaceHolderID="Head" runat="server">

</asp:Content>

<asp:Content ID="cntMain" ContentPlaceHolderID="MainContent" runat="server">
	<div id="Video">
		<div class="VideoInfo">
			<div class="SectionTitle">Video Preview</div>
			<asp:Panel ID="pnlVideo" CssClass="fieldRow" runat="server">
				<div class="fieldLabel">Video :</div>
				<div>
					<asp:DropDownList CssClass="VideoList" ID="ddlVideo" runat="server"></asp:DropDownList>
				</div>
			</asp:Panel>
			<asp:Panel ID="pnlPlaylist" CssClass="fieldRow" runat="server">
				<div class="fieldLabel">Playlist :</div>
				<div class="CheckList">
					<asp:CheckBoxList ID="cblPlaylist" RepeatColumns="3" runat="server"></asp:CheckBoxList>
				</div>
			</asp:Panel>
			<div class="fieldRow">
				<div class="fieldLabel">Auto Start:</div>
				<asp:CheckBox ID="chkAutoStart" runat="server" />
			</div>
			<div class="fieldRow">
				<div class="fieldLabel">Background Color:</div>
				<asp:TextBox ID="txtBGColor" runat="server" Text="#FFFFFF"></asp:TextBox>
			</div>
			<div class="fieldRow">
				<div class="fieldLabel">WMode:</div>
				<asp:DropDownList ID="ddlWMode" runat="server"></asp:DropDownList>
			</div>
			<div class="fieldRow">
				<div class="fieldLabel">Preview Video :</div>
				<asp:Button ID="btnPreview" CssClass="Submit" OnClick="btnPreview_Click" Text="Preview" runat="server" />
				<div class="PreviewBox">
					<bc:VideoPlayer ID="vpPlayer" WMode="transparent" runat="server" />
					<asp:Literal ID="ltlMessage" runat="server"></asp:Literal>
				</div>
			</div>
		</div>
		<asp:Literal ID="ltlError" runat="server"></asp:Literal>
	</div>
</asp:Content>