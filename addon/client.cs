/// ===========================================================================================	 
///			 
///	 DiscordBL
///	 By c4rr0t
///	 This project is open source @ https://github.com/carrot512/DiscordBL
///	 
/// ===========================================================================================
	
if (!isWindows() || isMacintosh() || !isUnlocked()) // !isFunction(DiscordBL::Initialize)
{
	echo("DiscordBL | User does not meet criteria to run DiscordBL, quitting...");
	return;
}

/// Declarations
$DiscordBL::PARTY_ID = getNonsense();
$DiscordBL::PLAYER_NAME = $Pref::Player::NetName SPC "(BL_ID:" SPC getNumKeyId() @ ")";
$DiscordBL::CURRENT_GAME = "";
$DiscordBL::SERVER_NAME = "";

/// Preferences
if ($Pref::DiscordBL::IgnoreAllInvites $= "")
{
	$Pref::DiscordBL::IgnoreAllInvites = false;
}

if ($Pref::DiscordBL::Debugging $= "")
{
	$Pref::DiscordBL::Debugging = false;
}

/// Master Server
// These functions were also made to reduce headaches. Have fun.
function explode(%string, %delimiter)
{
	// https://www.garagegames.com/community/forums/viewthread/29295/1#comment-231125
	if (isObject(explode))
	{
		explode.delete();
	}
	
	new ScriptObject(explode);
	
	%explodeCount = 0;
	%lastFound = 0;
	
	%endChar = strLen(%string);
	for (%i = 0; %i < %endChar; %i++)
	{
		%charToCheck = getSubStr(%string, %i, 1);
		if (%charToCheck $= %delimiter)
		{
			explode.contents[%explodeCount] = getSubStr(%string, %lastFound, (%i - %lastFound));
			%lastFound = %i + 1;
			%explodeCount++;
		}
	}
	
	explode.contents[%explodeCount] = getSubStr(%string, %lastFound, (%i-%lastFound));
	explode.count = %explodeCount + 1;
	return explode;
}

function endsWith(%str, %suffix)
{
	return getSubStr(%str, strlen(%str) - 1, 1) $= %suffix;
}

function isCleanNumber(%string)
{
	// https://www.garagegames.com/community/forums/viewthread/133985/1#comment-843804
	
	%dot = 0;  
	for(%i = 0; (%char = getSubStr(%string, %i, 1)) !$= ""; %i++)  
	{  
		switch$(%char)  
		{  
			case "0" or "1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9":  
				continue;  
  
			case ".":  
				if(%dot > 1)  
					return false;  
  
				%dot++;  
				continue;  
  
			case "-":  
				if(%i) // only valid as first character  
					return false;  
  
				continue;  
  
			case "+":  
				if(%i) // only valid as first character  
					return false;  
  
				continue;  
  
			default:  
				return false;  
		}  
	}  
	// %text passed the test  
	return true;  
}

function isValidIP(%ip)
{
	// https://stackoverflow.com/questions/4581877/validating-ipv4-string-in-java
	
	%ip = explode(%ip, ".");
	
	if (%ip.count !$= 4)
	{
		return false;
	}
	
	for (%i = 0; %i < %ip.count; %i++)
	{
		if (!isCleanNumber(%ip.contents[%i]) || %ip.contents[%i] < 0 || %ip.contents[%i] > 255)
		{
			return false;
		}
	}
	 
	if (endsWith(%ip, "."))
	{
		return false;
	}
	
	return true;
}

function isValidPort(%port)
{
	if (!isCleanNumber(%port) || %port > 65535)
	{
		return false;
	}
	
	return true;
}

function MasterServerTCP::onConnected(%this)
{
	%this.buffer = "";
	%this.started = false;
	MasterServerTCP.send("GET / HTTP/1.0\r\nHost: master2.blockland.us\r\n\r\n");
}

function MasterServerTCP::onDisconnect(%this)
{
	// TorqueScript is the worst language in the history of ever.
	// Whoever made this language wanted to see the world burn.
	
	while (%this.buffer !$= "")
	{
		%this.buffer = nextToken(%this.buffer, "server", "><>"); // FISHY
		%server = explode(%server, "\t"); // or ^ or actual tabulation character
		%ip = %server.contents[0];
		%port = %server.contents[1];
		%name = %server.contents[4];
		
		%currentGame = explode($DiscordBL::CURRENT_GAME, ":");
		%currentIp = %currentGame.contents[0];
		%currentPort = %currentGame.contents[1];
		if (!isValidIp(%currentIP) || !isValidIp(%ip) || !isValidPort(%port) || !isValidPort(%currentPort))
		{
			return;
		}
		
		if (%currentIp $= %ip && %currentPort $= %port)
		{
			$DiscordBL::SERVER_NAME = %name;
		}
	}
}

function MasterServerTCP::onLine(%this, %line)
{
	%line = trim(%line);
	
	if (!%this.started || %line $= "END")
	{
		if (%line $= "START")
		{
			%this.started = true;
		}
		return;
	}
	
	%this.buffer = %this.buffer @ %line @ "><>"; // fishy
}

/// Functions
function GetMissionStatus()
{
	if ($missionRunning $= 1)
	{
		if ($Server::Port $= 0 && $Server::LAN $= 1)
		{
			return "singleplayer";
		}
		return "hosting";
	}
	
	if (ServerConnection.isLocal() $= 1 && ServerConnection.isLan() $= 1 && ServerConnection.getPort() $= 0)
	{
		return "lan";
	}
	
	return "multiplayer";
}

function DiscordBL::Output(%message, %debug)
{
	if (%debug)
	{
		if ($Pref::DiscordBL::Debugging)
		{
			echo("DiscordBL $ " @ %message);
		}
		return;
	}
	echo("DiscordBL | " @ %message);
}


function DiscordBL::JoinServer(%key)
{
	connectToServer(strreplace(strreplace(%key, "-", "."), "|", ":"), "", "1", 1);
}

function DiscordBL::DecideJoinRequest(%username, %id)
{
	%status = GetMissionStatus();
	
	if (%status $= "localplay" || %status $= "singleplayer")
	{
		return;
	}
	
	%message = %username SPC "wants to join" SPC (%status $= "hosting" ? "your" : "the") SPC "server";
	if (%status $= "hosting")
	{
		%message = %message SPC "\n\nDo you agree to let" SPC %username SPC "join your server?";
	}
	else
	{
		%message = %message SPC "you're currently playing.";
	}
	
	if ($Pref::DiscordBL::IgnoreAllInvites)
	{
		DiscordBL::RequestReply(%id, 2);
		return;
	}
	
	messageBoxYesNo("DiscordBL | Join Request", %message, "DiscordBL::RequestReply(" @ %id @ ", 0);", "DiscordBL::RequestReply(" @ %id @ ", 1);");
}	

function DiscordBL::UpdateGame()
{
	if (!isObject(ServerConnection))
	{
		DiscordBL::Output("ServerConnection doesn't exist-- exiting...", false);
		return;
	}
	
	DiscordBL::RunCallbacks();
	
	%status = GetMissionStatus();
	%playerCount = NPL_LIST.rowCount() >= 1 ? NPL_LIST.rowCount() : 1; // Cannot make parties with 1 player
	%maxPlayers = $ServerInfo::MaxPlayers; 
	%serverAddress = ServerConnection.getAddress();
	%partyKey = strreplace(strreplace(ServerConnection.getAddress(), ".", "-"), ":", "|");
	%serverInfo = ServerInfoGroup.getObject($ServerSOFromIP[%serverAddress]);
	%gameMode = %serverInfo.map;
	%passworded = $ServerInfo::Password $= 1;
	
	// Rather crude way of getting server name WITH host name
	if (isObject(MasterServerTCP))
	{
		MasterServerTCP.delete();
	}
	%masterserver = new TCPObject(MasterServerTCP);
	%masterserver.connect("master2.blockland.us:80");
	%serverName = $DiscordBL::SERVER_NAME;
	
	if ($Pref::DiscordBL::Debugging)
	{
		DiscordBL::Output("Status:" SPC %status, true);
		DiscordBL::Output("ServerName:" SPC %serverName, true);
		DiscordBL::Output("PlayerName:" SPC $DiscordBL::PLAYER_NAME, true);
		DiscordBL::Output("AdditionalDetails: TRUE", true);
		DiscordBL::Output("GameMode:" SPC %gameMode, true);
		DiscordBL::Output("PlayerCount:" SPC %playerCount, true);
		DiscordBL::Output("MaxPlayers:" SPC %maxPlayers, true);
		DiscordBL::Output("PartyKey:" SPC %partyKey, true);
		DiscordBL::Output("PartyId:" SPC $DiscordBL::PARTY_ID, true);
		DiscordBL::Output("Details: In Game", true);
	}
	
	if (%status $= "lan" || %status $= "singleplayer")
	{
		DiscordBL::UpdatePresence("In Game", $DiscordBL::PLAYER_NAME, true, %gameMode, %playerCount, %maxPlayers, %status, %partyKey, $DiscordBL::PARTY_ID);
		return;
	}
	
	DiscordBL::UpdatePresence("In Game", $DiscordBL::PLAYER_NAME, true, %serverName, %playerCount, %maxPlayers, %status, %partyKey, $DiscordBL::PARTY_ID);
}

/// Package
package DiscordBL
{
	function MM_UpdateDemoDisplay()
	{
		Parent::MM_UpdateDemoDisplay();
		DiscordBL::RunCallbacks();
		DiscordBL::UpdatePresence("In Main Menu", $DiscordBL::PLAYER_NAME, false);
	}
	
	function JoinServerGui::onWake()
	{
		Parent::onWake();
		DiscordBL::RunCallbacks();
		DiscordBL::UpdatePresence("In Lobby", $DiscordBL::PLAYER_NAME, false);
	}
	
	function JoinServerGui::ClickBack()
	{
		Parent::ClickBack();
		DiscordBL::RunCallbacks();
		DiscordBL::UpdatePresence("In Main Menu", $DiscordBL::PLAYER_NAME, false);
	}
	
	function connectToServer(%ip, %a, %b, %c)
	{
		Parent::connectToServer(%ip, %a, %b, %c);
		$DiscordBL::CURRENT_GAME = %ip;
		DiscordBL::RunCallbacks();
		DiscordBL::UpdateGame();
	}
	
	function NewPlayerListGui::update(%this, %clientId, %clientName, %bl_id, %isSuperAdmin, %isAdmin, %score)
	{
		Parent::update(%this, %clientId, %clientName, %bl_id, %isSuperAdmin, %isAdmin, %score);
		if ($Version >= 21) // we use UpdateWindowTitle instead, that is a better method. Plus, we do not want to execute the same thing twice.
		{
			return;
		}
		DiscordBL::RunCallbacks();
		DiscordBL::UpdateGame();
	}
	
	function NewPlayerListGui::UpdateWindowTitle(%this)
	{
		if ($Version < 21)
		{
			return;
		}
		Parent::UpdateWindowTitle(%this);
		DiscordBL::RunCallbacks();
		DiscordBL::UpdateGame();
	}
	
	function NewPlayerListGui::onWake()
	{
		Parent::onWake();
		DiscordBL::RunCallbacks();
		DiscordBL::UpdateGame();
	}
	
	function doQuitGame()
	{
		DiscordBL::Shutdown();
		Parent::doQuitGame();
	}
	
	function quit()
	{
		DiscordBL::Shutdown();
		Parent::quit();
	}
};

/// Initialize
DiscordBL::Initialize();
DiscordBL::RunCallbacks();

activatePackage(DiscordBL);

DiscordBL::Output("PlayerFullName:" SPC $DiscordBL::PLAYER_NAME, true);
DiscordBL::Output("PartyId:" SPC $DiscordBL::PARTY_ID, true);

DiscordBL::Output("v1.1 by c4rr0t loaded", false);

if ($Pref::DiscordBL::GameRegistered $= "" || $Pref::DiscordBL::GameRegistered $= false || $Pref::DiscordBL::GameRegistered $= 0)
{
	DiscordBL::Register();
}
