using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeaBattle.Common.Game;
using SeaBattle.Common.Enums;

namespace SeaBattle.Client.Models
{
    public class ClientGameState
    {
        public GameBoard MyBoard { get; set; }
        public GameBoard EnemyBoard { get; set; }
        public bool IsMyTurn { get; set; }
        public GamePhase Phase { get; set; } = GamePhase.Placement;

        public ClientGameState()
        {
            MyBoard = new GameBoard();
            EnemyBoard = new GameBoard();
            IsMyTurn = false;
        }
    }
}
