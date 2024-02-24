using ConsoleMandelBrot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.Primitives;

namespace Mandelbrot_Explorer
{
    public class Fractal
    {
        
        public double XCenter { get; set; } // x
        public double YCenter { get; set; } // y
        public double width { get; set; }
        public int Resolution { get; set; } // масштаб
        public int MaxIter { get; set; } // кол-во итераций
        public double ImageWidth { get; set; } // ширина
        public double ImageHeight { get; set; } // высота
        public string linkPalette { get; set; } // ссылка на палитру

        /*
         * 1) позиция по x
         *  2) позиция по у
         *  3) масштаб
         *  4) количество итераций
         *  5) разрешение
         *  7) ссылка на палитру
         */
    }
}
