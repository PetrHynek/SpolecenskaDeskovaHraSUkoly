using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SpolecenskaDeskovaHraSUkoly.Models
{
    public class Player
    {
        public string Name { get; set; }
        public string ColorName { get; set; }
        public int Score { get; set; }
        public int Position { get; set; }

        [JsonIgnore]
        public Ellipse Figure { get; set; }

        [JsonIgnore]
        public Brush PlayerColor
        {
            get
            {
                if (ColorPalette.AvailableBrushes.ContainsKey(ColorName))
                {
                    return ColorPalette.AvailableBrushes[ColorName];
                }
                return Brushes.Black;
            }
        }
    }
}
