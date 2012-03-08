<%@ Page Language="C#" MasterPageFile="~/sitecore modules/Web/Brightcove/Media Folder/BCLibrary.master" AutoEventWireup="true" CodeFile="BrightcoveVideo.aspx.cs" Inherits="BrightcoveSCUtil.EditorTabs.BrightcoveVideoTab" %>
<%@ Register TagPrefix="bc" Namespace="BrightcoveSDK.UI" Assembly= "BrightcoveSDK" %>

<asp:Content ID="cntHead" ContentPlaceHolderID="Head" runat="server">

</asp:Content>

<asp:Content ID="cntMain" ContentPlaceHolderID="MainContent" runat="server">
	<div id="Video">
		<div class="VideoUpload">
			<div class="SectionTitle">Save Video to Brightcove</div>
			<bc:VideoAddUpdate ID="uvVideo" runat="server" ShowFileUpload="false" />
			<asp:Button ID="btnSave" CssClass="Submit" OnClick="btnSave_Click" Text="Save Video" runat="server" />
			<br /><br />
			<asp:Panel ID="pnlSaveMessage" CssClass="Message" Visible="false" runat="server">
				<asp:Literal ID="ltlSaveMessage" runat="server"></asp:Literal>
			</asp:Panel>
		</div>
		<br />
		<div class="VideoInfo">
			<div class="SectionTitle">Video Information</div>
			<div class="fieldRow">
				<div class="fieldLabel">Video ID :</div>
				<asp:Literal ID="ltlVideoID" runat="server"></asp:Literal>
			</div>
			<div class="fieldRow">
				<div class="fieldLabel">Video Status :</div>
				<asp:Literal ID="ltlStatus" runat="server"></asp:Literal>
			</div>
			<div class="fieldRow">
				<div class="fieldLabel">Creation Date :</div>
				<asp:Literal ID="ltlCreation" runat="server"></asp:Literal>
			</div>
			<div class="fieldRow">
				<div class="fieldLabel">Published Date :</div>
				<asp:Literal ID="ltlPublished" runat="server"></asp:Literal>
			</div>
			<div class="fieldRow">
				<div class="fieldLabel">Modified Date :</div>
				<asp:Literal ID="ltlModified" runat="server"></asp:Literal>
			</div>
			<div class="fieldRow">
				<div class="fieldLabel">Length :</div>
				<asp:Literal ID="ltlLength" runat="server"></asp:Literal>
			</div>
			<div class="fieldRow">
				<div class="fieldLabel">Plays Total :</div>
				<asp:Literal ID="ltlPlays" runat="server"></asp:Literal>
			</div>
			<div class="fieldRow">
				<div class="fieldLabel">Plays Trailing Week :</div>
				<asp:Literal ID="ltlPlaysTrailing" runat="server"></asp:Literal>
			</div>
			<div class="fieldRow">				
				<div class="fieldLabel">Update from Brightcove :</div>
				<asp:Button ID="btnUpdate" CssClass="Submit" runat="server" OnClick="btnUpdate_Click" Text="Update Video"></asp:Button>					
				<asp:Panel ID="pnlUpdateMessage" CssClass="Message" Visible="false" runat="server">
					<asp:Literal ID="ltlUpdateMessage" runat="server"></asp:Literal>
				</asp:Panel>
			</div>
			<br />
			<div class="fieldRow">
				<div class="fieldLabel">Delete Video :</div>
				<asp:Button ID="btnDelete" CssClass="Submit" Text="Delete Video" runat="server" OnClick="btnDelete_Click" />
				(this will remove it from Sitecore and Brightcove)
				<asp:Panel ID="pnlDeleteMessage" CssClass="Message" Visible="false" runat="server">
					<asp:Literal ID="ltlDeleteMessage" runat="server"></asp:Literal>
				</asp:Panel>
			</div>
			<div class="clear"></div>
		</div>
		<asp:Literal ID="ltlError" runat="server"></asp:Literal>
    </div>
</asp:Content>