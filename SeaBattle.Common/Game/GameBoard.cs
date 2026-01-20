using System.Collections.Generic;
using System.Linq;
using SeaBattle.Common.Enums;

namespace SeaBattle.Common.Game
{
    public class GameBoard
    {
        public const int Size = 10;
        public CellState[,] Cells { get; } = new CellState[Size, Size];
        public List<Ship> Ships { get; } = new List<Ship>();

        // Лимиты кораблей: размер → количество
        private readonly Dictionary<int, int> _shipLimits = new Dictionary<int, int>
        {
            {4, 1},  // 1 четырёхпалубный
            {3, 2},  // 2 трёхпалубных
            {2, 3},  // 3 двухпалубных
            {1, 4}   // 4 однопалубных
        };

        public IReadOnlyDictionary<int, int> ShipLimits => _shipLimits;


        // Проверка, можно ли поставить корабль
        public bool CanPlaceShip(Ship ship)
        {
            int size = ship.Decks.Count;

            if (!_shipLimits.ContainsKey(size))
                return false;

            // проверка лимита по размеру
            if (Ships.Count(s => s.Decks.Count == size) >= _shipLimits[size])
                return false;

            foreach (var (x, y) in ship.Decks)
            {
                // выход за пределы поля
                if (x < 0 || x >= Size || y < 0 || y >= Size)
                    return false;

                // клетка занята
                if (Cells[x, y] != CellState.Empty)
                    return false;

                // проверка периметра
                if (!IsPerimeterFree(x, y))
                    return false;
            }

            return true;
        }

        // Поставить корабль на доску
        public bool PlaceShip(Ship ship)
        {
            if (!CanPlaceShip(ship))
                return false;

            Ships.Add(ship);
            foreach (var (x, y) in ship.Decks)
                Cells[x, y] = CellState.Ship;

            return true;
        }

        // Удалить корабль по координате клетки
        public void RemoveShipAt(int x, int y)
        {
            var ship = Ships.FirstOrDefault(s => s.Contains(x, y));
            if (ship == null)
                return;

            Ships.Remove(ship);
            foreach (var (sx, sy) in ship.Decks)
                Cells[sx, sy] = CellState.Empty;
        }

        // Проверка периметра вокруг клетки
        private bool IsPerimeterFree(int x, int y)
        {
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx < 0 || nx >= Size || ny < 0 || ny >= Size)
                        continue;

                    if (Cells[nx, ny] == CellState.Ship)
                        return false;
                }
            return true;
        }

        // Проверка, расставлены ли все корабли
        public bool AllShipsPlaced()
        {
            return Ships.Count == _shipLimits.Values.Sum();
        }

        // Выстрел по клетке
        public ShotResult Shoot(int x, int y)
        {
            if (Cells[x, y] == CellState.Hit || Cells[x, y] == CellState.Miss)
                return ShotResult.AlreadyShot;

            foreach (var ship in Ships)
            {
                if (ship.Hit(x, y))
                {
                    Cells[x, y] = CellState.Hit;
                    return ship.IsSunk() ? ShotResult.Sunk : ShotResult.Hit;
                }
            }

            Cells[x, y] = CellState.Miss;
            return ShotResult.Miss;
        }

        // Проверка, все ли корабли потоплены
        public bool AllShipsSunk()
        {
            return Ships.All(s => s.IsSunk());
        }
    }
}
