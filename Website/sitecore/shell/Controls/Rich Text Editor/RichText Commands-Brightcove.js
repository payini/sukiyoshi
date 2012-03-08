/* This file is shared between older developer center rich text editor and the new EditorPage, that is used exclusively by Content Editor */
RadEditorCommandList["InsertBrightcoveVideo"] = function(commandName, editor, args) {
	
	var html = editor.getSelectionHtml();
	var videoid = scItemID;
	var playerid = scItemID;
	var playlistids = scItemID;
	var wmode = "window";
	var bgcolor = "#ffffff";
	var autostart = "false";
	var selectedText = "";
	
	if (html) {
		selectedText = editor.getSelection().getText();
		html = unescape(getVidAttrVal(html, 'href'));
		html = html.split('/BrightcoveVideo.ashx').join('').split('&amp;').join('&');
		videoid = getVidQStringVal(html, 'video');
		playerid = getVidQStringVal(html, 'player');
		playlistids = getVidQStringVal(html, 'playlists');
		wmode = getVidQStringVal(html, 'wmode');
		bgcolor = getVidQStringVal(html, 'bgcolor');
		autostart = getVidQStringVal(html, 'autoStart');
	}

	scEditor = editor;

	editor.showExternalDialog(
		"/sitecore/shell/default.aspx?xmlcontrol=RichText.InsertVideo&la=" + scLanguage + "&video=" + videoid + "&player=" + playerid + "&playlists=" + playlistids + "&wmode=" + wmode + "&bgcolor=" + escape(bgcolor) + "&autostart=" + autostart + "&selectedText=" + selectedText,
		null, //argument
		600, //width
		500, //height
		scInsertBrightcoveVideo, //callback
		null, // callback args
		"Insert Video",
		true, //modal
		Telerik.Web.UI.WindowBehaviors.Close, // behaviors
		false, //showStatusBar
		false //showTitleBar
	);
};

function scInsertBrightcoveVideo(sender, returnValue) {
	if (!returnValue) {
		return;
	}
	
	var d = scEditor.getSelection().getParentElement();

	if ($telerik.isFirefox && d.tagName == "A") {
		d.parentNode.removeChild(d);
	} else {
		scEditor.fire("Unlink");
	}
		
	var text = scEditor.getSelectionHtml();

	// if selected string is a full paragraph, we want to insert the link inside the paragraph, and not the other way around.
	var regex = /^[\s]*<p>(.+)<\/p>[\s]*$/i;
	var match = regex.exec(text);
	if (match && match.length >= 2) {
		scEditor.pasteHtml("<p>" + returnValue.URL + "</p>", "DocumentManager");
		return;
	}

	scEditor.pasteHtml(returnValue.URL, "DocumentManager");
}

function getVidQStringVal(html, name) {

	var value = "";

	var reg = new Array();

	reg[0] = '.*?(\\?)(';
	reg[1] = '.*?(&amp;)(';
	reg[2] = '.*?(&)(';
	var reEnd = ')(=)([a-zA-Z0-9,#]*)';

	//check if it's 
	for (var i = 0; i < reg.length; i++) {
		var pval = new RegExp(reg[i] + name + reEnd, ["i"]);
		var mval = pval.exec(html);
		if (mval != null) {
			value = mval[4];
			break;
		}
	}

	return value;
}

RadEditorCommandList["EmbedBrightcoveVideo"] = function(commandName, editor, args) {

	var html = editor.getSelectionHtml();
	
	var videoid = scItemID;
	var playerid = scItemID;
	var playlisttabs = "";
	var playlistcombo = "";
	var videolist = "";
	var wmode = "window";
	var bgcolor = "#ffffff";
	var autostart = "false";

	if (html) {
		videoid = getVidAttrVal(html, 'video');
		playerid = getVidAttrVal(html, 'player');
		playlisttabs = getVidAttrVal(html, 'playlisttabs');
		playlistcombo = getVidAttrVal(html, 'playlistcombo');
		videolist = getVidAttrVal(html, 'videolist');
		wmode = getVidAttrVal(html, 'wmode');
		bgcolor = getVidAttrVal(html, 'bgcolor');
		autostart = getVidAttrVal(html, 'autostart');
	}

	scEditor = editor;
	
	editor.showExternalDialog(
		"/sitecore/shell/default.aspx?xmlcontrol=RichText.EmbedVideo&la=" + scLanguage + "&video=" + videoid + "&player=" + playerid + "&playlisttabs=" + playlisttabs + "&playlistcombo=" + playlistcombo + "&videolist=" + videolist + "&wmode=" + wmode + "&bgcolor=" + escape(bgcolor) + "&autostart=" + autostart,
		null, //argument
		600, //width
		500, //height
		scEmbedBrightcoveVideo, //callback
		null, // callback args
		"Embed Video",
		true, //modal
		Telerik.Web.UI.WindowBehaviors.Close, // behaviors
		false, //showStatusBar
		false //showTitleBar
	);
};

function scEmbedBrightcoveVideo(sender, returnValue) {
	if (!returnValue) {
		return;
	}

	scEditor.pasteHtml(returnValue.EmbedTag);
}

function getVidAttrVal(html, name) {

	var value = "";

	reg = '.*?(' + name + ')(=)(".*?")';

	//check if it's 
	var pval = new RegExp(reg, ["i"]); //241
	var mval = pval.exec(html);
	if (mval != null) {
		value = mval[3];
		value = value.split('"').join('');
	}

	return value;
}