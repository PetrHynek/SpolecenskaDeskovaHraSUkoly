using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpolecenskaDeskovaHraSUkoly.Models
{
    public class GameState
    {
        public List<Player> Players { get; set; } = new List<Player>();
        public int CurrentPlayerIndex { get; set; }

        public string SelectedBoardType { get; set; }
        public string SelectedBoardSize { get; set; }
        public bool UseBonusTiles { get; set; }
        public bool UsePenaltyTiles { get; set; }
        public bool UseEmptyTiles { get; set; }
        public int WinningScore { get; set; }
        public int TaskTimer { get; set; }

        public List<BoardTile> Board { get; set; } = new List<BoardTile>();
    }
}
