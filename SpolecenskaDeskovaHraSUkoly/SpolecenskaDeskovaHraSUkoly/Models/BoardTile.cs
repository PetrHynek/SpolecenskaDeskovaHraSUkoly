using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpolecenskaDeskovaHraSUkoly.Models
{
    internal class BoardTile
    {
        public int Index { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public TileType? Type { get; set; }

        public Direction From { get; set; }
        public Direction To { get; set; }
    }

    enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    enum TileType
    {
        Task,
        Bonus,
        Penalty,
        Empty
    }
}
