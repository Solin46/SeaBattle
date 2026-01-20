using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SeaBattle.Common.Enums;

namespace SeaBattle.Common.Game
{
    public class Game
    {
        public Player Player1 { get; }
        public Player Player2 { get; }

        private int _currentPlayerIndex = 0;

        public Game()
        {
            Player1 = new Player();
            Player2 = new Player();
        }

        public Player CurrentPlayer =>
            _currentPlayerIndex == 0 ? Player1 : Player2;

        public Player EnemyPlayer =>
            _currentPlayerIndex == 0 ? Player2 : Player1;

        public ShotResult Shoot(int x, int y)
        {
            var result = EnemyPlayer.Board.Shoot(x, y);

            if (result == ShotResult.Miss)
                SwitchTurn();

            return result;
        }

        private void SwitchTurn()
        {
            _currentPlayerIndex = _currentPlayerIndex == 0 ? 1 : 0;
        }
    }
}
