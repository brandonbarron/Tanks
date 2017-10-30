using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TanksCommon.SharedObjects
{
    class ListOfOpenGames : IMessage
    {
        public int Id { get => 8; }
        public List<OpenGame> OpenGames { get; set; }
    }
}
