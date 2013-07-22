using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamThing
{
    class voiceKeyClass
    {
        public VoiceKey voiceKey { get; set; }
    }
    public class vCommand
    {
        public string trigger { get; set; }
        public string key { get; set; }
    }

    public class VoiceKey
    {
        public List<vCommand> command { get; set; }
    }
}
