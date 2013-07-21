using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;                   // Used to create network client
using System.IO;                    // Access local files
using Newtonsoft.Json;              // JSON data is used to store information
using System.Speech.Recognition;    // Basic speech utilities
using System.Threading;             // Used to thread the application
using System.Diagnostics;           // Used to launch steam commands.

// The application is three fold. First, it loads the entire steam content library as JSON using the web API.
// Secondly, the application loads local override files to get the commands, alternate game names and voice keys.
// Thirdly, it activates the speech engine.

namespace SteamThing
{
    public partial class Form1 : Form
    {        
        WebClient client = new WebClient();                 // Network client used to download Steam data.
        steamGameList allGames = new steamGameList();       // This is an object filled with the Steam JSON
        Process steamProcess = new Process();               // This is used to perform steam protocol commands.
        overrideClass allOverrides = new overrideClass();   // JSON Game overrides are loaded into this.
        voiceKeyClass allVoiceKeys = new voiceKeyClass();   // JSON Voice Key data is loaded into this.
        commandClass allCommands = new commandClass();      // JSON Commands are loaded into this.
        specificGameBinding mainGameBindings = new specificGameBinding();   // Game specific bindings
       
        string lastBinds = "";

        // Note, you can really not use the game overrides or voice keys, but you should be using commands. It's here to mirror the structure.
        bool useOverides = false;                           // Are you using game overrides (if not, they're not loaded into the speech grammars).
        bool useVoiceKeys = false;                          // Are you using voice keys (if not, they're not loaded into the speech grammars).
        bool useCommands = false;                           // Are you using commands (if not, they're not loaded into the speech grammars).
        bool useChat = false;                               // Toggle for voice chat.

        string jsonString = "";                             // String used to load the Steam JSON, so it can later be parsed.
        string locale = "en-US";                            // Locale information, by default it's english-US
        string appTitle = "Steam Driver";                   // Name of the App. If run with a command line option, locale will be added to this.
        int loadingProgress = 0;                            // Used to animate network activity.
        
        public SpeechRecognitionEngine recog;               // Speech Recognition Engine. If it wasn't "Engine" it would use the windows implementation.        
        Choices triggers;                                   // List of voice commands that hard coded triggers are loaded into.
        Choices commands;                                   // Grammar list of commands, loaded from the command file.
        Choices voiceKeys;                                  // Grammar list of voice keys, loaded from the voice keys file.
        Choices overrides;                                  // Grammar list for game overrides, loaded from overrides file.
        Choices gameBinds;                                  // Grammar list for game specific keybinds that can be loaded/unloaded.

        bool ack = false;                                   // Switch used to track if we're listening for commands.
        bool game = false;                                  // Switch used to track if we're listening for game names.
        bool openKeys = false;                              // Swich used to track if voice keys are on.
        bool steamControl = false;                          // Switch used to track if we're listening for steam commands.
        int gameSelected = 0;                               // Index of which game is currently selected.
        bool lockspeech = false;                            // Is lock enabled or not.
                
        // Stage 1.
        public Form1()
        {
            InitializeComponent();
            string[] args = Environment.GetCommandLineArgs();   // Grab command line arguments.
            for (int i = 0; i < args.Length; i++)               
            {                                                   
                if (args[i].Length == 5)                        // If the argument is 5 characters...
                {
                    locale = args[i];                           
                    if (!checkLocale(locale))                   // Check its validity.
                    {
                        locale = "en-US";                       // If invalid, return to english US
                    }
                    appTitle += " " + locale;                   // Append to app title.
                }                
            }

            // This adds the sounds for audio feedback.
            comboBox2.Items.Add("Asterisk");
            comboBox2.Items.Add("Beep");            
            comboBox2.Items.Add("Exclamation");  
            comboBox2.Items.Add("Hand");  
            comboBox2.Items.Add("Question");
            comboBox2.SelectedIndex = 0;
        }

        private bool checkLocale(string localization) 
        {
            bool returnBool = false;                        // Return true/false for validity
            switch (localization)
            {
                case "ca-ES":
                    returnBool = true;
                    break;
                case "da-DK":
                    returnBool = true;
                    break;
                case "de-DE":
                    returnBool = true;
                    break;
                case "en-AU":
                    returnBool = true;
                    break;
                case "en-CA":
                    returnBool = true;
                    break;
                case "en-GB":
                    returnBool = true;
                    break;
                case "en-IN":
                    returnBool = true;
                    break;
                case "en-US":
                    returnBool = true;
                    break;
                case "es-ES":
                    returnBool = true;
                    break;
                case "es-MX":
                    returnBool = true;
                    break;
                case "fi-FI":
                    returnBool = true;
                    break;
                case "fr-CA":
                    returnBool = true;
                    break;
                case "fr-FR":
                    returnBool = true;
                    break;
                case "it-IT":
                    returnBool = true;
                    break;
                case "ja-JP":
                    returnBool = true;
                    break;
                case "ko-KR":
                    returnBool = true;
                    break;
                case "nb-NO":
                    returnBool = true;
                    break;
                case "nl-NL":
                    returnBool = true;
                    break;
                case "pl-PL":
                    returnBool = true;
                    break;
                case "pt-BR":
                    returnBool = true;
                    break;
                case "pt-PT":
                    returnBool = true;
                    break;
                case "ru-RU":
                    returnBool = true;
                    break;
                case "sv-SE":
                    returnBool = true;
                    break;
                case "zh-CN":
                    returnBool = true;
                    break;
                case "zh-HK":
                    returnBool = true;
                    break;
                case "zh-TW":
                    returnBool = true;
                    break;
                default:
                    break;
            }            
            return returnBool;
        }

        private void Form1_Load(object sender, EventArgs e)
        {                        
            // If we're adding a Cache of the JSON data, this is a good place to do it.
            // Because the download is async, even if we load from HDD you can still update
            // the cache.
            this.Text = appTitle;                                                           // Set app title 
            Uri url = new Uri(@"http://api.steampowered.com/ISteamApps/GetAppList/v0002/"); // URL for the major app list.           
            client.DownloadStringAsync(url);                                                // Grab data from the URL
            client.DownloadProgressChanged += (sender1, e1) => growLoad();                  // Animate spinner
            client.DownloadStringCompleted += client_DownloadStringCompleted;               // When finished, continue.
        }

        private void growLoad()
        {
            label1.Text = "Steam Licenses: Getting List ";
            if (loadingProgress > 3)
            {
                loadingProgress = 0;
            }

            switch (loadingProgress)
            {
                case 0:
                    label1.Text += @"\";
                    loadingProgress++;
                    break;
                case 1:
                    label1.Text += "|";
                    loadingProgress++;
                    break;
                case 2:
                    label1.Text += "/";
                    loadingProgress++;
                    break;
                case 3:
                    label1.Text += "-";
                    loadingProgress++;
                    break;
                default:
                    break;
            }
        }   // Spinner used to indicate network activity.
        
        private void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            jsonString = e.Result;                                                      // String of JSON to be converted...
            allGames = JsonConvert.DeserializeObject<steamGameList>(jsonString);        // Gets converted here
            loadLocalOverrides();                                                       // Load the JSON overrides
            label1.Text = "Steam Licenses: " + allGames.applist.apps.Count.ToString();  // Write out how many licenses exist.
            speech();                                                                   // Start Speech module.
        }

        // Stage 2
        private void loadLocalOverrides()
        {
            // This pattern works three times:
            // First fill temp strings with the JSON.
            // Then make sure the length is greater than 0 (so it's valid).
            // Then fill the master object (allCommands, allOverrides, allVoiceKeys)
            // And set the boolean. 
            string tempString = "";
            tempString = loadFromFile(@".\overrides\overrideCommandschema.json");
            if (tempString.Length > 0)
            {
                allCommands = JsonConvert.DeserializeObject<commandClass>(tempString);
                useCommands = true;
            }
            tempString = loadFromFile(@".\overrides\overrideGameschema.json");
            if (tempString.Length > 0)
            {
                allOverrides = JsonConvert.DeserializeObject<overrideClass>(tempString);
                useOverides = true;
            }
            tempString = loadFromFile(@".\overrides\overrideVoiceKeyschema.json");
            if (tempString.Length > 0)
            {
                allVoiceKeys = JsonConvert.DeserializeObject<voiceKeyClass>(tempString);
                useVoiceKeys = true;
            }
        }        

        private string loadFromFile(string filename)
        {
            // This just makes it faster to load files, rather than typing this three times.
            // Plus, it could easily be reused for other purposes later.
            string text = "";
            if (File.Exists(@".\" + filename))
            {
                StreamReader streamReader = new StreamReader(@".\" + filename);
                text = streamReader.ReadToEnd();
            }            
            return text;
        }

        // Stage 3
        private void speech()
        {            
            recog = new SpeechRecognitionEngine(new System.Globalization.CultureInfo(locale));  // Loads the localisation information.
            triggers = new Choices();                                                           // Initiates the grammar list for triggers
            commands = new Choices();                                                           // Initiates the grammar list for commands
            voiceKeys = new Choices();                                                          // Initiates the grammar list for voicekeys
            overrides = new Choices();                                                          // Initiates the grammar list for game overrides            
            comboBox1.Items.Clear();                                                            // Fills the comboBox with the total list of games.
            
            recog.RequestRecognizerUpdate();                                                    // Required to update the voice recognition.

            for (int i = 0; i < allGames.applist.apps.Count; i++)                               // then load the grammar into the recognizer for master game list.
            {
                triggers.Add(allGames.applist.apps[i].name);
                comboBox1.Items.Add(allGames.applist.apps[i].name + " (" + allGames.applist.apps[i].appid + ")");
            }

            recog.LoadGrammar(new Grammar(triggers));                                           // This loads it, once games are all added.

            if (useVoiceKeys)                                                                   // If we're using voice keys
            {                                                                                   // then add them to the choice list.
                for (int i = 0; i < allVoiceKeys.voiceKey.command.Count; i++)
                {
                    voiceKeys.Add(allVoiceKeys.voiceKey.command[i].trigger);
                }
                recog.LoadGrammar(new Grammar(voiceKeys));                                      // then load the grammar into the recognizer for voice keys.
            }

            if (useCommands)                                                                    // If we're using commands
            {                                                                                   // then add them to the choice list.
                for (int i = 0; i < allCommands.commandOverride.command.Count; i++)
                {
                    commands.Add(allCommands.commandOverride.command[i].trigger);
                }
                recog.LoadGrammar(new Grammar(commands));                                       // then load the grammar into the recognizer for commands.
            }

            recog.LoadGrammar(new Grammar(new GrammarBuilder("Steam")));                        // Each of these is a hard coded trigger
            recog.LoadGrammar(new Grammar(new GrammarBuilder("game")));                         // this ensures that the speech engine will always be
            recog.LoadGrammar(new Grammar(new GrammarBuilder("load game binding")));              // Used to load game specific keybinds
            recog.LoadGrammar(new Grammar(new GrammarBuilder("unload binds")));                 // Used to load game specific keybinds
            recog.LoadGrammar(new Grammar(new GrammarBuilder("open")));                         // able to recognize these words.
            recog.LoadGrammar(new Grammar(new GrammarBuilder("lock")));
            recog.LoadGrammar(new Grammar(new GrammarBuilder("exit")));
            recog.LoadGrammar(new Grammar(new GrammarBuilder("hide")));
            recog.LoadGrammar(new Grammar(new GrammarBuilder("show")));
            recog.LoadGrammar(new Grammar(new GrammarBuilder("unlock")));
            recog.LoadGrammar(new Grammar(new GrammarBuilder("start voice chat")));        
            recog.LoadGrammar(new Grammar(new GrammarBuilder("toggle voice keys")));
                        
            if (useOverides)                                                                    // Overrides are added last. They have to be added after
            {                                                                                   // the master list, as a good practice. It doesn't actually
                for (int i = 0; i < allOverrides.gameOverride.game.Count; i++)                  // affect anything, since they're kept separate.
                {
                    overrides.Add(allOverrides.gameOverride.game[i].name);
                }   
                recog.LoadGrammar(new Grammar(overrides));                                      // Adds the grammar to the recognizer for game overrides. 
            }
            
            recog.SpeechDetected +=recog_SpeechDetected;                                        // Event when speech is detected. Not recgonized, just detected.
            recog.SpeechRecognized += recog_SpeechRecognized;                                   // Event when a word is found. Any processing is over there.
            recog.SetInputToDefaultAudioDevice();                                               // This sets us to the default microphone set in the Windows audio panel.
            recog.RecognizeAsync(RecognizeMode.Multiple);                                       // This just makes voice recognition work beautifully. Just leave it alone.
            makeNoise();
        }

        private void recog_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            makeNoise();                                        // This plays feedback noise.

            if (!useChat)
            {
                label4.Text = e.Result.Text;                        // Set the "Voice Command" label to be the recognized word.
            }
            // This does *not* mean that the command was executed.

            if (e.Result.Text == "lock")                        // If the detected word was "Lock"
            {                                                   // Then flip the boolean and
                lockspeech = true;                              // Drop opacity.
                this.Opacity = 0.5;
            }

            if (e.Result.Text == "unlock")                      // If it's unlock, then reflip
            {                                                   // the boolean and raise opacity.
                lockspeech = false;
                this.Opacity = 1.0;
            }

            if (lockspeech)                                     // If we're locked, keep things locked down.
            {                                                   // Flip off ack, game, steamcontrol, voice keys
                ack = false;                                    // zero out the selected game and use the color red.
                game = false;                                   // Otherwise, go ahead and process the word.
                steamControl = false;                           // Note that lock and unlock work without ack.
                openKeys = false;                               // but all words still get recognized, updating the label.
                useChat = false;
                gameSelected = 0;
                label4.ForeColor = Color.Red;
            }
            else
            {
                if (!useChat)
                {
                    label4.ForeColor = Color.Black;                 // Flip the color back to black so we don't look locked.
                    if (!ack && e.Result.Text == "Steam")           // If we're not authenticated, authenticate.
                    {                                               // if we are authenticated, don't do anything. We're good.
                        ack = true;
                    }

                    if (ack && e.Result.Text == "start voice chat")
                    {
                        useChat = true;
                        voiceChat chatBox = new voiceChat(locale);
                        chatBox.Show();
                        chatBox.FormClosed += (Csender, Ce) => stopVoiceChat();
                    }

                    if (e.Result.Text == "toggle voice keys")       // Toggles voice keys on or off. There's probably a more
                    {                                               // code efficent way to do this, but it works for now.
                        if (openKeys)
                        {
                            openKeys = false;
                            label5.Text = "VoiceKeys Off";
                        }
                        else
                        {
                            openKeys = true;
                            label5.Text = "VoiceKeys On";
                        }
                    }

                    if (openKeys)           // This bit checks to see if the command matches a voice key trigger
                    {                       // then performs "SendKeys.send" for the voice key command.
                        string voiceCommand = e.Result.Text;
                        for (int i = 0; i < allVoiceKeys.voiceKey.command.Count; i++)
                        {
                            if (voiceCommand.Contains(allVoiceKeys.voiceKey.command[i].trigger))
                            {
                                SendKeys.Send(allVoiceKeys.voiceKey.command[i].key);
                            }
                        }
                    }

                    if (ack && e.Result.Text == "exit") //Closes the application.
                    {
                        Application.Exit();
                    }

                    if (ack && e.Result.Text == "hide") // Hides the application.
                    {                                   // Eventually, this should be a system tray app and not need this.
                        this.WindowState = FormWindowState.Minimized;
                    }

                    if (ack && e.Result.Text == "show") // Shows the application.
                    {
                        this.WindowState = FormWindowState.Normal;
                    }

                    if (ack && e.Result.Text == "open") // Opens up steam command listener.
                    {
                        steamControl = true;
                    }

                    if (ack && e.Result.Text == "game") // Opens up game command listener.
                    {
                        gameSelected = 0;
                        game = true;
                    }

                    if (steamControl)                           // This bit checks the recognized word against the list of commands
                    {                                           // but doesn't match unless it's ALSO a "Steam" type command.
                        string voiceCommand = e.Result.Text;    // these commands wont trigger unless you've used "Open"... ie "Open" "Friends".
                        for (int i = 0; i < allCommands.commandOverride.command.Count; i++)
                        {
                            if (voiceCommand.Contains(allCommands.commandOverride.command[i].trigger) && allCommands.commandOverride.command[i].type == "Steam")
                            {
                                steamProcess.StartInfo.FileName = allCommands.commandOverride.command[i].command;
                                steamProcess.Start();
                            }
                        }
                    }

                    if (lastBinds != "" && gameBinds != null)                                               // If lastbinds isn't null, we know that a bind list is loaded.
                    {
                        string voiceCommand = e.Result.Text;                                                // Get the result into a string for easy reading.
                        for (int i = 0; i < mainGameBindings.bindings.bindingCmds.Count; i++)               // Check if the result was in the list of game specific binds.
                        {                                                                                   // Then if so, send the keys specific to that command.
                            if (voiceCommand.Contains(mainGameBindings.bindings.bindingCmds[i].command))
                            {                                                                               // This otherwise works just like voice keys, we just don't have
                                SendKeys.Send(mainGameBindings.bindings.bindingCmds[i].bind);               // A boolean to check if we should use game specific bindings.
                            }                                                                               // So there's an extra step of logic.
                        }
                    }

                    if (e.Result.Text == "load game binding")                                                                               // Game specific bindings
                    {       
                        if (File.Exists(@".\overrides\" + allGames.applist.apps[gameSelected].appid + ".JSON") && lastBinds == "")          // Make sure the file exists.
                        {
                            richTextBox1.Text += "Found " + allGames.applist.apps[gameSelected].name + " bindings." + Environment.NewLine;
                            lastBinds = allGames.applist.apps[gameSelected].appid.ToString();                                               // Use last binds in case the
                        }                                                                                                                   // game changes or something weird.
                        else
                        {
                            richTextBox1.Text += allGames.applist.apps[gameSelected].name + " not found." + lastBinds + Environment.NewLine;
                        }
                        if (lastBinds != "")
                        {
                            try
                            {
                                richTextBox1.Text += "Bindings Loaded." + Environment.NewLine;          // This bit is in a try, in case the file
                                string tempString = "";                                                 // is malformed or has no commands.
                                tempString = loadFromFile(@".\overrides\" + lastBinds + ".json");       
                                richTextBox1.Text += lastBinds + ".json" + Environment.NewLine;
                                if (tempString.Length > 0)
                                {
                                    gameBinds = new Choices();                                                          // Initiates the grammar list for Game specific Bindings
                                    mainGameBindings = JsonConvert.DeserializeObject<specificGameBinding>(tempString);  // And then puts the JSON into the object.

                                    for (int i = 0; i < mainGameBindings.bindings.bindingCmds.Count; i++)
                                    {
                                        gameBinds.Add(mainGameBindings.bindings.bindingCmds[i].command);                // Iterate and add each voice command.
                                    }

                                    recog.LoadGrammar(new Grammar(gameBinds));
                                }
                                else
                                {
                                    richTextBox1.Text += "No Bindings in file." + Environment.NewLine;
                                    lastBinds = "";
                                }
                            }
                            catch
                            {
                                richTextBox1.Text += "Error: Unable to load bindings." + Environment.NewLine;
                            }
                        }
                    }

                    if (e.Result.Text == "unload binds")
                    {
                        if (lastBinds != "")                                                                            // Make sure that a bind is loaded.
                        {
                            lastBinds = "";                                                                             // Clear it
                            for (int i = 0; i < mainGameBindings.bindings.bindingCmds.Count; i++)                       // Remove each command from the recognizer.
                            {
                                recog.UnloadGrammar(new Grammar(mainGameBindings.bindings.bindingCmds[i].command));
                            }
                            mainGameBindings = null;                                                                    // finish nulling out so that we can re-fill
                            gameBinds = null;                                                                           // with other game specific commands.
                            // Then unload the grammar.
                        }
                    }

                    if (game)
                    {
                        string voiceCommand = e.Result.Text;                                        // This became complicated once I added loading overrides.
                        for (int i = 0; i < allGames.applist.apps.Count; i++)                       // So the first thing it checks is the main game list.
                        {                                                                           // EVEN if it finds something, it will check against the override list.
                            if (voiceCommand.Contains(allGames.applist.apps[i].name))
                            {
                                gameSelected = i;
                                comboBox1.SelectedIndex = gameSelected;
                            }
                        }

                        for (int i = 0; i < allOverrides.gameOverride.game.Count; i++)              // This bit then goes through the overrides. If it finds something,
                        {                                                                           // It sets "Selected Game" to whatever game in the main game list has
                            if (voiceCommand.Contains(allOverrides.gameOverride.game[i].name))      // The same APP ID as the override game APP ID.
                            {
                                int gameAppID = allOverrides.gameOverride.game[i].appid;
                                for (int s = 0; s < allGames.applist.apps.Count; s++)
                                {
                                    if (allGames.applist.apps[s].appid == gameAppID)
                                    {
                                        gameSelected = s;
                                        comboBox1.SelectedIndex = s;
                                    }
                                }
                            }
                        }

                        for (int i = 0; i < allCommands.commandOverride.command.Count; i++)         // This actually runs the game. Essentially, you said "Game",
                        {                                                                           // then you said the name of a game and if both those requirements have been met...
                            if (gameSelected > 0)
                            {                                                  // then when a command is spoken of the "game" type, it executes.
                                if (voiceCommand.Contains(allCommands.commandOverride.command[i].trigger) && allCommands.commandOverride.command[i].type == "game")
                                {
                                    ack = false;
                                    game = false;
                                    steamControl = false;
                                    steamProcess.StartInfo.FileName = allCommands.commandOverride.command[i].command + allGames.applist.apps[gameSelected].appid;
                                    richTextBox1.Text = steamProcess.StartInfo.FileName;
                                    steamProcess.Start();
                                    Thread.Sleep(1000);
                                    gameSelected = 0;
                                }
                            }
                        }
                    }                   
                }
            }
        }

        private void recog_SpeechDetected(object sender, SpeechDetectedEventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            gameSelected = comboBox1.SelectedIndex;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            openKeys = checkBox2.Checked;
            if (openKeys)
            {
                label5.Text = "VoiceKeys On";
            }
            else
            {
                label5.Text = "VoiceKeys Off";
            }
        }

        private void stopVoiceChat()
        {
            useChat = false;
            ack = false;
        }

        private void makeNoise()
        {
            if (checkBox1.Checked && !lockspeech)               // If the lock is off, and the checkbox for audio is checked...
            {
                switch (comboBox2.SelectedIndex)
                {
                    case 0:
                        System.Media.SystemSounds.Asterisk.Play();
                        break;
                    case 1:
                        System.Media.SystemSounds.Beep.Play();
                        break;
                    case 2:
                        System.Media.SystemSounds.Exclamation.Play();
                        break;
                    case 3:
                        System.Media.SystemSounds.Hand.Play();
                        break;
                    case 4:
                        System.Media.SystemSounds.Question.Play();
                        break;
                    default:
                        System.Media.SystemSounds.Beep.Play();
                        break;                        
                }
            }
        }
    }
}
