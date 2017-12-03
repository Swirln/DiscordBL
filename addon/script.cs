// ===========================================================================================
//                  
//      By TRASHPROVIDER56
//      Some code from https://github.com/blocklandana/blockland-discord
//      This project is open source @ https://github.com/trashprovider56/DiscordBL
//
// ===========================================================================================
	
// During testing I noticed that not using Windows would mean that the DLL would be unusable, meaning the Add-On will be unusable as well.
// Sorry to all Macintosh users :(
if(!isWindows() || isMacintosh()) {
	echo("DiscordBL | User runs a non Windows device, quitting...");
	return;
}
function getPartyKey() {
	%letters = "a b c d e f g h i j k l m n o p q r s t u v w x y z 1 2 3 4 5 6 7 8 9 0 A B C D E F G H I J K L M N O P Q R S T U V W X Y Z";
	for(%i = 0; %i < 32; %i ++) {
		for(%a = 0; %a < 4; %a ++) {
			%Randomchar = getRandom(0, 35);
		}
		%char = getWord(%letters, %Randomchar);
		%string = %string @ %char;
	}
	if(%string $= "")
		return -1;
	return %string;
}

$partyId = getPartyKey();
// Set (default) preferences
if($Pref::DiscordBL::IgnoreAllInvites $= "")
	$Pref::DiscordBL::IgnoreAllInvites = false;
if($Pref::DiscordBL::Debugging $= "")
	$Pref::DiscordBL::Debugging = false;
// Keybind
$remapDivision[$remapCount] = "DiscordBL";
$remapName[$remapCount] = "Open Options Menu";
$remapCmd[$remapCount] = "DiscordBL::OpenOptions()";
$remapCount++;

// These are all the DiscordBL functions, and global variables.
// They're not included in the package for a reason. Don't add it back in.

if($Pref::DiscordBL::Debugging $= 1)
	%debugging = true;
else
	%debugging = false;

if (!isUnlocked())
	$playerName = "Demo";
else {
	%playerName = $Pref::Player::NetName;
	%playerBLID = getNumKeyId();
	$playerName = %playerName @ " (BL_ID: " @ %playerBLID @ ")";
}

function DiscordBL::Output(%msg, %debugmsg) {
	if(%debugging $= true && %debugmsg $= true)
		echo("DiscordBL DEBUG | " @ %msg);
	else
		echo("DiscordBL | " @ %msg);
}

// WIP Matchmaking module

function DiscordBL::JoinServer(%key) {
	// decrypt the party key
	%serverAddress = strreplace(strreplace(%key, "-", "."), "|", ":");
	// join server
	connectToServer(%serverAddress, "", "1", 1);
}

function DiscordBL::DecideJoinRequest(%username) {
	if($Pref::DiscordBL::IgnoreAllInvites $= false) {
		if($missionRunning $= 1)
			messageBoxYesNo("DiscordBL | Join Request", %username @ " wants to join your server. \n\n Do you agree to let " @ %username @ " join your server?", "replyDiscordPresence(1);", "replyDiscordPresence(0);");
		else
			messageBoxYesNo("DiscordBL | Join Request", %username @ " wants to join the server you're currently playing.", "replyDiscordPresence(1);", "replyDiscordPresence(0);");
	}
	else
		replyDiscordPresence(2);
}	
// Options

function DiscordBL::OpenOptions() {
	exec("./options.gui");
	canvas.pushDialog(DiscordBLOptions);
}

function DiscordBL::ManualUpdateGame() {
	// Kill if ServerConnection doesn't exist
	if(!isObject(ServerConnection)) {
		DiscordBL::Output("ServerConnection doesn't exist-- exiting...", false);
		return;
	}
	%status = "default";
	%serverName = $ServerInfo::Name;
	%playerCount = NPL_LIST.rowCount();
	%maxPlayers = $ServerInfo::MaxPlayers; 
	// create party key
	%serverAddress = ServerConnection.getAddress();
	%partyKey = strreplace(strreplace(ServerConnection.getAddress(), ".", "-"), ":", "|");
	// set state
	if($missionRunning $= 1 && $Server::Port $= 0 && $Server::LAN $= 1)
		%status = "singleplayer";
	else if($missionRunning $= 1)
		%status = "hosting";
	else if(ServerConnection.isLocal() $= 1 && ServerConnection.isLan() $= 1 && ServerConnection.getPort() $= 0)
		%status = "localplay";
	else
		%status = "multiplayer";
	// get gamemode
	%serverInfo = ServerInfoGroup.getObject($ServerSOFromIP[%partyKey]);
	%gameMode = %serverInfo.map;
	%passworded = 0; 
	// declare passworded
	if ($ServerInfo::Password $= 1)
		%passworded = 1;
	
	DiscordBL::Output("Status: " @ %status, true);
	DiscordBL::Output("ServerName: " @ %serverName, true);
	DiscordBL::Output("PlayerName: " @ $playerName, true);
	DiscordBL::Output("AdditionalDetails: TRUE", true);
	DiscordBL::Output("GameMode: " @ %gameMode, true);
	DiscordBL::Output("PlayerCount: " @ %playerCount, true);
	DiscordBL::Output("MaxPlayers: " @ %maxPlayers, true);
	DiscordBL::Output("PartyKey: " @ %partyKey, true);
	DiscordBL::Output("PartyId: " @ $partyId, true);
	DiscordBL::Output("Details: In Game", true);
	
	if($missionRunning $= 1 && $Server::Port $= 0 && $Server::LAN $= 1)
		updateDiscordPresence("In Game", $playerName, 1, %gameMode, %playerCount, %maxPlayers, %partyKey, %status, $partyId);
	else
		updateDiscordPresence("In Game", $playerName, 1, %serverName, %playerCount, %maxPlayers, %partyKey, %status, $partyId);
}

package DiscordBL {
	// Functions
	function MM_UpdateDemoDisplay() {
		Parent::MM_UpdateDemoDisplay();
		updateDiscordPresence("In Main Menu", $playerName, 0);
	}
	function JoinServerGui::onWake() {
		if (isUnlocked()) {
			updateDiscordPresence("In Lobby", $playerName, 0);
		}
		JoinServerGui::queryWebMaster();
	}
	function JoinServerGui::ClickBack() {
		canvas.popDialog(JoinServerGui);
		MainMenuGui::showButtons(MainMenuGui);
		updateDiscordPresence("In Main Menu", $playerName, 0);
	}
	function NewPlayerListGui::UpdateWindowTitle(%this) {
		Parent::UpdateWindowTitle(%this);
		// Kill if ServerConnection doesn't exist
		if(!isObject(ServerConnection)) {
			DiscordBL::Output("ServerConnection doesn't exist-- exiting...", false);
			return;
		}
		%status = "default";
		%serverName = $ServerInfo::Name;
		%playerCount = NPL_LIST.rowCount();
		%maxPlayers = $ServerInfo::MaxPlayers; 
		// create party key
		%serverAddress = ServerConnection.getAddress();
		%partyKey = strreplace(strreplace(ServerConnection.getAddress(), ".", "-"), ":", "|");
		// set state
		if($missionRunning $= 1 && $Server::Port $= 0 && $Server::LAN $= 1) {
			%status = "singleplayer";
		}
		else if(ServerConnection.isLocal() $= 1 && ServerConnection.isLan() $= 1) { // && ServerConnection.getPort() == 28500
			%status = "localplay";
		}
		else
		{
			%status = "multiplayer";
		}
		// get gamemode
		%serverInfo = ServerInfoGroup.getObject($ServerSOFromIP[%partyKey]);
		%gameMode = %serverInfo.map;
		%passworded = 0; 
		// declare passworded
		if ($ServerInfo::Password $= 1) {
			%passworded = 1;
		}
	
		DiscordBL::Output("Status: " @ %status, true);
		DiscordBL::Output("ServerName: " @ %serverName, true);
		DiscordBL::Output("PlayerName: " @ $playerName, true);
		DiscordBL::Output("AdditionalDetails: TRUE", true);
		DiscordBL::Output("GameMode: " @ %gameMode, true);
		DiscordBL::Output("PlayerCount: " @ %playerCount, true);
		DiscordBL::Output("MaxPlayers: " @ %maxPlayers, true);
		DiscordBL::Output("PartyKey: " @ %partyKey, true);
		DiscordBL::Output("PartyId: " @ $partyId, true);
		DiscordBL::Output("Details: In Game", true);
		
		if($missionRunning $= 1 && $Server::Port $= 0 && $Server::LAN $= 1) {
			updateDiscordPresence("In Game", $playerName, 1, %gameMode, %playerCount, %maxPlayers, %partyKey, %status, $partyId);
		}
		else {
			updateDiscordPresence("In Game", $playerName, 1, %serverName, %playerCount, %maxPlayers, %partyKey, %status, $partyId);
		}
	}
}; 

activatePackage(DiscordBL);

// Start
replyBackWeLoaded();
doDiscordBLRegister();
discordInitalize();
DiscordBL::Output("PlayerName: " @ %playerName, true);
DiscordBL::Output("PlayerBLID: " @ %playerBLID, true);
DiscordBL::Output("PlayerFullName: " @ $playerName, true);
DiscordBL::Output("PartyId: " @ $partyId, true);
DiscordBL::Output("Add-On successfully loaded", false);
