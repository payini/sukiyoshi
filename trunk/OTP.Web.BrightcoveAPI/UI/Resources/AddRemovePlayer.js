isPlayerAdded = false;

function addPlayer(videoID, playerID, playerName, autoStart, bgColor, playerWidth, playerHeight, isVid, playerWMode, htmlElementID) {

	if(isPlayerAdded == true) {
		removePlayer(playerName);	
	}
	var params = {};
	params.playerID = playerID;
	params.videoId = videoID;
	if(autoStart){
		params.autoStart = autoStart;
	}
	if(playerWMode != ""){
		params.bgcolor = bgColor;
	}
	params.width = playerWidth;
	params.height = playerHeight;
	params.isVid = isVid;
	if(playerWMode != ""){
		params.wmode = playerWMode;	
	}

	var player = brightcove.createElement("object");
	player.id = playerName;
	var parameter;
	for (var i in params) {
		parameter = brightcove.createElement("param");
		parameter.name = i;
		parameter.value = params[i];
		player.appendChild(parameter);
	}
	
	var playerContainer = document.getElementById(htmlElementID);
	brightcove.createExperience(player, playerContainer, true);
	isPlayerAdded = true;
		
	return false;
}

function removePlayer(playerName) {
	if(isPlayerAdded == true) {
		isPlayerAdded = false;
		brightcove.removeExperience(playerName);
	}
}