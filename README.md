# DiscordBL

DiscordBL is a DLL that adds Discord's Rich Presence integration in Blockland.<br>
This code is based off of (Ana's DiscordDLL.)[https://github.com/blocklandana/blockland-discord]<br>
DiscordBL is not finished. Do not treat it as a finished product. As DiscordBL is constantly under development, there will be some bugs in the GitHub repository.<br>
DiscordBL is licensed under the AGPLv3.

## Compiling

To compile, you first need to download Discord's Rich Presence libraries.<br>
You can download Discord's Rich Presence libraries (here)[https://github.com/discordapp/discord-rpc/releases].<br>
You also need Visual Studio Community 2017 to compile DiscordBL. If you do not already have it installed, (install it here.)[https://www.visualstudio.com/thank-you-downloading-visual-studio/?sku=Community&rel=15)
After you have done that, follow these steps:<br>
* Make a new folder named `rpc` in the project folder
* Get all the folders that end with `static` (ex: `win32-static`)
* Move or copy all the static folders to the `rpc` folder
* Compile the DLL in the `dll` folder
You have now successfully compiled the DLL. You may want to now install it, which is covered in the `Installing` section (below).<br>
Please note that you may have to edit the .vcxproj for the library paths.

## Installing
* Run `autozip.bat`
* Place System_DiscordBL in the `Add-Ons` folder
* Download BlocklandLoader and install it
* Place DiscordBLL (from your compilation) into the `modules` folder
* Launch Blockland