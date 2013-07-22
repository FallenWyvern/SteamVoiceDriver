using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Recognition;

namespace SteamThing
{
    public partial class voiceChat : Form
    {        
        public SpeechRecognitionEngine recog;               // Speech Recognition Engine. If it wasn't "Engine" it would use the windows implementation.        

        public voiceChat(string locale)
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            recog = new SpeechRecognitionEngine(new System.Globalization.CultureInfo(locale));  // Loads the localisation information.
            recog.RequestRecognizerUpdate();                                                    // Required to update the voice recognition.

            recog.LoadGrammar(new Grammar(new GrammarBuilder("close voice chat")));
            recog.LoadGrammar(new Grammar(new GrammarBuilder("clear chat")));
            recog.LoadGrammar(new Grammar(new GrammarBuilder("send chat")));
            recog.LoadGrammar(new DictationGrammar());

            recog.SpeechDetected += recog_SpeechDetected;                                        // Event when speech is detected. Not recgonized, just detected.
            recog.SpeechRecognized += recog_SpeechRecognized;                                   // Event when a word is found. Any processing is over there.
            recog.SetInputToDefaultAudioDevice();                                               // This sets us to the default microphone set in the Windows audio panel.
            recog.RecognizeAsync(RecognizeMode.Multiple);                                       // This just makes voice recognition work beautifully. Just leave it alone.
        }

        void recog_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            switch (e.Result.Text)
            {                    
                case "send chat":
                    this.Hide();
                    for (int i = 0; i < textBox1.Text.Length; i++)
                    {
                        SendKeys.Send(textBox1.Text[i].ToString());
                    }
                    SendKeys.Send("{ENTER}");
                    textBox1.Text = "";
                    this.Show();
                    break;
                case "clear chat":
                    textBox1.Text = "";
                    break;
                case "close voice chat":
                    textBox1.Text = "";
                    this.Close();
                    break;
                default:
                    textBox1.Text += e.Result.Text;
                    break;
            }
        }

        void recog_SpeechDetected(object sender, SpeechDetectedEventArgs e)
        {            
        }

        private void voiceChat_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
        }

        private void voiceChat_FormClosing(object sender, FormClosingEventArgs e)
        {
            recog.RecognizeAsyncStop();
            recog.Dispose();
            textBox1.Text = "";
        }
    }
}
