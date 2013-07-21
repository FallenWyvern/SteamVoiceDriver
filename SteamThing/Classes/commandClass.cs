using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamThing
{
    class commandClass
    {
        public CommandOverride commandOverride { get; set; }
    }
    public class cCommand
    {
        public string type { get; set; }
        public string trigger { get; set; }
        public string command { get; set; }
    }

    public class CommandOverride
    {
        public List<cCommand> command { get; set; }
    }
}
