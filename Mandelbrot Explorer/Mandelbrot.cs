using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
namespace ConsoleMandelBrot
{
    class Mandelbrot
    {
        public int resolution { get; set; }
        public int maxIter { get; set; }
        public int iterDone = 0;
        public int iterCycle = 400;
        public double imageWidth { get; set; }
        public double xCenter { get; set; }
        public double yCenter { get; set; }


        public Mandelbrot(double xCenter, double yCenter, double width, int resolution, int maxIterations)
        {
            this.xCenter = xCenter;
            this.yCenter = yCenter;
            imageWidth = width;
            this.resolution = resolution;
            this.maxIter = maxIterations;
        }


        static List<ControlPoint> controlPoints = new List<ControlPoint>() {
                new ControlPoint(0.0,Color.FromArgb(0,7,100)),
                new ControlPoint(0.16,Color.FromArgb(32,107,203)),
                new ControlPoint(0.42,Color.FromArgb(237,255,255)),
                new ControlPoint(0.6425,Color.FromArgb(255,170,0)),
                new ControlPoint(0.8575,Color.FromArgb(0,2,0)),
                new ControlPoint(1,Color.FromArgb(0,0,0))
            };
        static ColorGradient colorGradient = new ColorGradient(controlPoints);
        static List<ControlPoint> controlPoints2 = new List<ControlPoint>() {
                new ControlPoint(0.0,Color.FromArgb(255,255,255)),
                new ControlPoint(0.125,Color.FromArgb(255,0,0)),
                new ControlPoint(0.25,Color.FromArgb(21,0,255)),
                new ControlPoint(0.375,Color.FromArgb(153,218,255)),
                new ControlPoint(0.50,Color.FromArgb(0,251,255)),
                new ControlPoint(0.675,Color.FromArgb(0,255,17)),
                new ControlPoint(0.75,Color.FromArgb(251,255,0)),
                new ControlPoint(1,Color.FromArgb(0,0,0))
            };
        static ColorGradient colorGradient_Rainbow = new ColorGradient(controlPoints2);

        public Bitmap MakeBitmap()
        {
            Bitmap canvas = new Bitmap(resolution, resolution);

            double x = xCenter - imageWidth / 2.0d;
            double y = yCenter - imageWidth / 2.0d;

            for (int xpix = 0; xpix < resolution; xpix++)
            {
                Parallel.For(0, resolution, ypix =>
                {
                    lock (canvas)
                    {
                        y = yCenter - imageWidth / 2 + imageWidth / resolution * ypix;
                        canvas.SetPixel(xpix, ypix, MandelbrotColor(x, y, maxIter));
                    }
                });
                x += imageWidth / resolution;
            }

            return canvas;
        }

        
        public Color MandelbrotColor(double x, double y, int maxIter)
        {
            Complex z0 = new Complex(0, 0);
            double Re = 0;
            double Im = 0;
            int i;
            double abs = 0;
            for (i = 0; i < maxIter; i++)
            {
                /*
                z0 = Complex.Pow(z0, 2) + new Complex(x, y);
                abs = Complex.Abs(z0);
                if (abs >= 4) break;
                */
                
                double buff = Re;
                Re = Re * Re - Im * Im + x;
                Im = 2 * buff * Im + y;      

                abs = Re * Re + Im * Im;
                if (abs >= 4) break;

            }
            if (i == maxIter) return Color.FromArgb(0, 0, 0);
            //Console.WriteLine(Smooth(i, abs));
            return colorGradient_Rainbow.GetColor(Smooth(i, abs) / maxIter);
            iterDone++;
            if (abs > 10e3) abs = 10e3;
            //Console.WriteLine(abs);
            //return colorGradient.GetColor(Smooth(i, abs) % iterCycle / iterCycle);
        }


        public static double Smooth(double n, double abs)
        {
            double pon = n - Math.Log(Math.Log(abs, 2), 2) + 4;
            return pon;
        }


    }
}
