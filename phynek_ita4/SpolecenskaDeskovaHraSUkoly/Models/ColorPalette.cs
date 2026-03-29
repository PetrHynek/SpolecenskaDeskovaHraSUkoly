using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SpolecenskaDeskovaHraSUkoly.Models
{
    public static class ColorPalette
    {
        public static readonly Dictionary<string, Brush> AvailableBrushes = new Dictionary<string, Brush>
        {
            { "Červená", Brushes.Red },
            { "Modrá", Brushes.DodgerBlue },
            { "Zelená", Brushes.LimeGreen },
            { "Žlutá", Brushes.Gold },
            { "Oranžová", Brushes.Orange },
            { "Fialová", Brushes.MediumPurple },
            { "Růžová", Brushes.HotPink },
            { "Tyrkysová", Brushes.DarkCyan },
            { "Hnědá", Brushes.SaddleBrown },
            { "Šedá", Brushes.SlateGray }
        };
    }
}
