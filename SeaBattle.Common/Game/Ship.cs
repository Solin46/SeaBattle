using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattle.Common.Game
{
    public class Ship
    {
        public List<(int X, int Y)> Decks { get; }
        private HashSet<(int X, int Y)> _hits = new HashSet<(int, int)>();

        public Ship(List<(int X, int Y)> decks)
        {
            Decks = decks;
        }

        public bool Contains(int x, int y)
        {
            return Decks.Any(d => d.X == x && d.Y == y);
        }

        public bool Hit(int x, int y)
        {
            if (!Contains(x, y))
                return false;

            _hits.Add((x, y));
            return true;
        }

        public bool IsSunk()
        {
            return _hits.Count == Decks.Count;
        }
    }
}

