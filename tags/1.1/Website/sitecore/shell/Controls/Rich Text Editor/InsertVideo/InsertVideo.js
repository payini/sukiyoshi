function GetDialogArguments() {
    return getRadWindow().ClientParameters;
}

function getRadWindow() {
  if (window.radWindow) {
        return window.radWindow;
  }
    
    if (window.frameElement && window.frameElement.radWindow) {
        return window.frameElement.radWindow;
    }
    
    return null;
}

var isRadWindow = true;

var radWindow = getRadWindow();

if (radWindow) { 
  if (window.dialogArguments) { 
    radWindow.Window = window;
  } 
}

function scCloseEmbed(embedTag) {
	var returnValue = {
		EmbedTag:embedTag
	};
	
	getRadWindow().close(returnValue);
}
function scCloseLink(url) {
	var returnValue = {
		URL:url
	};
	
	getRadWindow().close(returnValue);
}

function scCancel() {

  getRadWindow().close();
}

function scCloseWebEdit(embedTag) {
  window.returnValue = embedTag;
  window.close();
}

if (window.focus && Prototype.Browser.Gecko) {
  window.focus();
}