function GetDialogArguments() {
  if (window.radWindow) {
    return window.radWindow.Argument;
  }
  else {
    return null;
  }
}

var isRadWindow = true;

var radWindow = GetEditorRadWindowManager().GetCurrentRadWindow(window);

if (radWindow) { 
  if (window.dialogArguments) { 
    radWindow.Window = window;
  } 
  
  radWindow.OnLoad(); 
}

function scClose(url, text) {
	var returnValue = {
		url:url,
		text:text
	};
	
	CloseDlg(returnValue);
}

function scCloseWebEdit(url) {
  window.returnValue = url;
  window.close();
}

if (window.focus && Prototype.Browser.Gecko) {
  window.focus();
}
