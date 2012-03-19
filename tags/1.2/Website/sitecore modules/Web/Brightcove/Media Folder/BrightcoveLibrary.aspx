<%@ Page Language="C#" MasterPageFile="~/sitecore modules/Web/Brightcove/Media Folder/BCLibrary.master" AutoEventWireup="true" CodeFile="BrightcoveLibrary.aspx.cs" Inherits="BrightcoveSCUtil.EditorTabs.BrightcoveLibraryTab" %>

<asp:Content ID="cntHead" ContentPlaceHolderID="Head" runat="server">

</asp:Content>

<asp:Content ID="cntMain" ContentPlaceHolderID="MainContent" runat="server">
	<div id="Library">
		Welcome to the Brightcove Media Module.
		<br /><br />
		If you have the brightcove settings in your web.config file you can setup an account. 
		
		<ol>
			<li>right click on the video section</li>
			<li>go to insert</li>
			<li>select "Account Folder"</li>
			<li>adjust the settings</li>
		</ol>
	</div>
</asp:Content>