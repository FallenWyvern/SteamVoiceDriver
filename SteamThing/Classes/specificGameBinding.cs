using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamThing
{
    class specificGameBinding
    {
        public Bindings bindings { get; set; }
    }

    public class BindingCmd
    {
        public string command { get; set; }
        public string bind { get; set; }
    }

    public class Bindings
    {
        public List<BindingCmd> bindingCmds { get; set; }
    }    
}
