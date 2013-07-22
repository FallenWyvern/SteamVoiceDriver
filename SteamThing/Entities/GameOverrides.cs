using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamThing
{
    class GameOverrides
    {
        public GameOverride gameOverride { get; set; }
    }
    public class Game
    {
        public int appid { get; set; }
        public string name { get; set; }
    }

    public class GameOverride
    {
        public List<Game> game { get; set; }
    }
}