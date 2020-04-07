using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleMandelBrot
{
    class ControlPoint
    {
        public double Position { get; set; }
        public Color Color { get; set; }

        public ControlPoint (double position, Color color)
        {
            if (position < 0 || position > 1) throw new Exception("Position must be in range [0, 1]");
            Position = position;
            Color = color;
        }
    }
}
