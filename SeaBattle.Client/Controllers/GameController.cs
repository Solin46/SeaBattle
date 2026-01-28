using SeaBattle.Client.Models;
using SeaBattle.Common.Enums;
using SeaBattle.Common.Game;
using System;
using System.Linq;

namespace SeaBattle.Client.Controllers
{
    public class GameController
    {
        public ClientGameState State { get; private set; }

        public event Action GameFinished; // UI подписывается

        public GameController()
        {
            State = new ClientGameState();
        }

        // Расстановка корабля
        public bool PlaceShip(Ship ship)
        {
            return State.MyBoard.PlaceShip(ship);
        }

        public void RemoveShipAt(int x, int y)
        {
            State.MyBoard.RemoveShipAt(x, y);
        }

        public bool AllShipsPlaced()
        {
            return State.MyBoard.AllShipsPlaced();
        }

        public void FinishPlacement()
        {
            if (!AllShipsPlaced())
                return;

            State.Phase = GamePhase.Battle;
            State.IsMyTurn = true; // первый ход за игроком
        }

        
        // Выстрел по противнику
        public ShotResult ShootAtEnemy(int x, int y)
        {
            if (!State.IsMyTurn)
                return ShotResult.Miss;

            var result = State.EnemyBoard.Shoot(x, y);

            // Если все корабли противника уничтожены
            if (State.EnemyBoard.AllShipsSunk())
            {
                FinishGame();
            }

            if (result == ShotResult.Miss)
                State.IsMyTurn = false;

            return result;
        }

        public void SwitchTurn()
        {
            State.IsMyTurn = !State.IsMyTurn;
        }

        public void ResetBoards()
        {
            State.MyBoard = new GameBoard();
            State.EnemyBoard = new GameBoard();
            State.Phase = GamePhase.Placement;
            State.IsMyTurn = false;
        }

        public void FinishGame()
        {
            State.Phase = GamePhase.Finished;
            GameFinished?.Invoke();
        }

        public void RestartGame()
        {
            State = new ClientGameState();
        }
    }
}
