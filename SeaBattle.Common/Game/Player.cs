using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattle.Common.Game
{
    public class Player
    {
        public GameBoard Board { get; }

        public Player()
        {
            Board = new GameBoard();
        }
    }
}
