using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon.SharedObjects
{
    [Serializable]
    public class ListOfOpenGames : IMessage
    {
        public ListOfOpenGames()
        {
            OpenGames = new List<OpenGame>();
        }
        public short Id { get => 8; }
        public List<OpenGame> OpenGames { get; set; }
    }
}
