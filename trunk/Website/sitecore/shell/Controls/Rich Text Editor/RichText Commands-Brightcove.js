RadEditorCommandList["InsertBrightcoveVideo"] = function(commandName, editor, tool) {
	var args = editor.GetDialogParameters(commandName);

	var html = editor.GetSelectionHtml();
	var videoid = scItemID;
	var playerid = scItemID;
	var playlistids = scItemID;
	var wmode = "window";
	var bgcolor = "#ffffff";
	var autostart = "false";

	if (html) {
		videoid = getVidQStringVal(html, 'video');
		playerid = getVidQStringVal(html, 'player');
		playlistids = getVidQStringVal(html, 'playlists');
		wmode = getVidQStringVal(html, 'wmode');
		bgcolor = getVidQStringVal(html, 'bgcolor');
		autostart = getVidQStringVal(html, 'autostart');
	}

	scEditor = editor;

	editor.ShowDialog(
		"/sitecore/shell/default.aspx?xmlcontrol=RichText.InsertVideo&la=" + scLanguage + "&video=" + videoid + "&player=" + playerid + "&playlists=" + playlistids + "&selectedText=" + escape(html) + "&wmode=" + wmode + "&bgcolor=" + bgcolor + "&autostart=" + autostart,
		null, //argument
		600, //width
		500, //height
		scInsertBrightcoveVideo,
		null,
		"Insert Video");
};

function scInsertBrightcoveVideo(returnValue) {
	if (returnValue) {
		var text = scEditor.GetSelectionHtml();

		if (text != "" && text != null) {
			// if selected string is a full paragraph, we want to insert the link inside the paragraph, and not the other way around.
			var regex = new RegExp("^[\s]*<p>(.+)<\/p>[\s]*$", ["i"]);
			var match = regex.exec(text);
			if (match && match.length >= 2) {
				scEditor.PasteHtml("<p>" + returnValue.url + "</p>");
				return;
			}
		}

		scEditor.PasteHtml(returnValue.url);
	}
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

RadEditorCommandList["EmbedBrightcoveVideo"] = function(commandName, editor, tool) {

	var args = editor.GetDialogParameters(commandName);

	var html = editor.GetSelectionHtml();
	var videoid = scItemID;
	var playerid = scItemID;
	var playlisttabs = "";
	var playlistcombo = "";
	var videolist = "";
	var wmode = "window";
	var bgcolor = "#ffffff";
	var autostart = "false";

	if (html) {
		html = unescape(getVidAttrVal(html, 'src'));
		html = html.split('parameters=').join('').split('&amp;').join('&');
		videoid = getVidQStringVal(html, 'video');
		playerid = getVidQStringVal(html, 'player');
		playlisttabs = getVidQStringVal(html, 'playlisttabs');
		playlistcombo = getVidQStringVal(html, 'playlistcombo');
		videolist = getVidQStringVal(html, 'videolist');
		wmode = getVidQStringVal(html, 'wmode');
		bgcolor = getVidQStringVal(html, 'bgcolor');
		autostart = getVidQStringVal(html, 'autostart');
	}

	scEditor = editor;

	editor.ShowDialog(
		"/sitecore/shell/default.aspx?xmlcontrol=RichText.EmbedVideo&la=" + scLanguage + "&video=" + videoid + "&player=" + playerid + "&playlisttabs=" + playlisttabs + "&playlistcombo=" + playlistcombo + "&videolist=" + videolist + "&wmode=" + wmode + "&bgcolor=" + escape(bgcolor) + "&autostart=" + autostart,
		null, //argument
		600, //width
		600, //height
		scEmbedBrightcoveVideo,
		null,
		"Embed Video");
};

function scEmbedBrightcoveVideo(returnValue) {
	if (returnValue) {
		var text = scConvertWebControl(returnValue.url);
		scEditor.PasteHtml(text);
	}
}

function scConvertWebControl(html) {
	try {
		var win = window.dialogArguments[0];
	} catch (e) { win = window; }

	var form = win.scForm;
	if (form != null) {
		var request = new win.scRequest();
		request.form = "html=" + encodeURIComponent(html);
		request.build("", "", "", 'Convert(\"' + scMode + '\")', true);
		var url = "/sitecore/shell/Applications/Content Manager/Execute.aspx?cmd=Convert&mode=" + scMode;
		request.url = url;
		request.send();
		var r = "";
		if (request.httpRequest != null && request.httpRequest.status == "200") {
			r = request.httpRequest.responseText;
		}
		return r;
	}
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