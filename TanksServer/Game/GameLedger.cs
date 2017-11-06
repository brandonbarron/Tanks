using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class GameLedger
    {
        private static GameLedger instance;

        private GameLedger() { }

        public static GameLedger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameLedger();
                }
                return instance;
            }
        }

        public List<TanksCommon.SharedObjects.ListOfOpenGames> ListOfOpenGames { get; set; }
    }
}
