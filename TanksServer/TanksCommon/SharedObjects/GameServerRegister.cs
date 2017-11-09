using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon.SharedObjects
{
    [Serializable]
    public class GameServerRegister : IMessage
    {
        public short Id { get => 100; }
        public List<OpenGame> OpenGames { get; set; }

        public string OpenGamesString { get => OpenGames.ToString(); }
    }
}
