Steam Voice Driver - Version 0.60 - Bryan Holmes
Requirements : .net 4.5 framework. Microphone. Windows. Steam. Patience.
Additional language packs: http://www.microsoft.com/en-us/download/details.aspx?id=27224

Improvements:
	Added voice chat (see below for usage).

	Added audio feedback configuration drop down.

	Added manual toggle for Voice Keys

	Added localizations. Create a shortcut to steamdriver.exe then edit the target in the shortcut properties
	like this : "C:\Users\FallenWyvern\Documents\Visual Studio 2012\Projects\SteamDriver\SteamThing\bin\Debug\SteamThing.exe" pt-BR
	Please note the quotations are supposed to be there, that's on purpose. At the end is 5 characters denoting where you
	want this to be located. Obviously, this is hard for me to test so please provide feedback!

	From: http://msdn.microsoft.com/en-us/library/hh378476(v=office.14).aspx These are the codes to use.
	Catalan - Spain ca-ES, Danish - Denmark da-DK, German - Germany de-DE, English - Australia en-AU, English - Canada en-CA,
	English - Great Britain en-GB, English - Indian en-IN, English - United States en-US, Spanish - Spain es-ES, Spanish - Mexico es-MX,
	Finnish - Finland fi-FI, French - Canada fr-CA, French - France fr-FR, Italian - Italy it-IT, Japanese - Japan ja-JP, 
	Korean - Korea ko-KR, Norwegian - Norway nb-NO, Dutch - Netherlands nl-NL, Polish - Poland pl-PL, Portuguese - Brazil pt-BR,
	Portuguese - Portugal pt-PT, Russian - Russia ru-RU, Swedish - Sweden sv-SE, Chinese - China zh-CN, Chinese - Hong Kong zh-HK,
	Chinese - Taiwan zh-TW

	Added ding (beta). I know it sounds stupid to make that a beta note, but my computer has no
	working speakers so I don't know if it works.

	Now external files load the voice commands. All written in json.
	overrideCommandsschema.json - Stores commands for steam/games. If the steam protocol command
	needs an APP id, the type is "game" otherwise use "Steam.

	overrideGameschema.json	- Stores alternate names for games. Each line has an AppID and game name.
	The game name is the new thing to say when you want that game (although the old ones still work) and
	appid is used to find what it is in Steam's data file.

	overrideVoiceKeyschema.json - Stores voice keys. When voice keys are enabled, saying the trigger
	performs the appropriate keyboard shortcut. This is as outlined by Microsoft here:
	http://msdn.microsoft.com/en-us/library/system.windows.forms.sendkeys.aspx

	Keyboard input (voice keys) are stil semi-working. Specifically alt-tab and shift-tab (overlay). This
	is because of security in windows since vista.

To Do:
	Release source code
	Include .net 4.5 redistributable.
	Make a video tutorial.
	Make an installer

Troubleshooting:
	So far, there are two types of errors. The first just needs you to install the .net 4.5 framework.
	The other one is related to localizations. If you're region isn't one that's compatible with the
	default speech framework (which is to be implemented, so everyone can use it) that's probably your
	problem.

Adding a non-steam game:
	On Reddit, /u/WakeUp_SmellTheAshes asked how to launch non-steam games. He had dolphin setup to launch Skyward Sword in steam.
	This is the solution I provided:

	Right click the non-steam game and hit "Create Shortcut". Then, on your desktop, right click the shortcut and hit properties.
	In the target you should have something like "steam://rungameid/11588296607635865600" (This was for Baldurs Gate 2, I 
	dont know if the hashes are static or unique).

	Open "Overridecommandschema.json" in notepad (or whatever) and add this to it:

	{
		"type":"Steam",
		"trigger": "(Whatever Trigger You Want)",
		"command": "(Pase the rungameid line from shortcut into here)"
	},

	Then you can use "Steam" "Open" "(Trigger)" to launch it. The reason it has to be a steam command, is because the main app list 
	doesn't have that app ID (it's not part of the steam database, obviously) and a game override has to match an APP ID in the steam database.
	And you can delete the desktop shortcut afterwards.

How to use:
	Decompress to any folder.
	Run the exe (SteamThing.exe)
	Say one of the Base commands:
		Lock			Stops voice commands from working.
		Unlock			Resumes accepting voice commands.
		Toggle Voice Keys	Acts as a keyboard. Toggles on/off
		Steam			It starts listening for commands.

	If Voice Keys are toggled, commands from that json file act as keyboard
	input, even if the application isn't in focus.

	If you say "Steam" you have these options:
		Exit			Closes the application.
		Hide			Hides the application.
		Show			Shows the application.
		Open			Listens for steam commands.
		Game			Listens for game commands.
		Start Voice Chat	Starts voice chatting

	Open/Game have to work this way for now, because they pull from their
	command json file. If they weren't, everything would be hard coded and
	that's boring.

	If you say "Open" or "Game" you then can use any of the triggers in the
	command file as actions. For example "Open" then "Friends" performs the
	action found under the "Friends" trigger in the JSON file.

	If you start voice chat, nothing functions EXCEPT for voice chat. You can say
	"end voice chat" to close voice chat. You can use "send chat" to send the
	stuff you said. You can use "clear chat" to empty the chat box. Note that
	voice chat is really designed for big picture mode and will type your words
	into whatever process is in focus. Until I can force focus on the chat window,
	we're stuck with that.

Now one might wonder "Why can't I use any command in there that would work in a run box?". The answer is you can.
I wanted to put in more security to prevent people from performing actions that weren't related to steam but
then I thought people might WANT to be able to do whatever they want. So go ahead. I don't mind!