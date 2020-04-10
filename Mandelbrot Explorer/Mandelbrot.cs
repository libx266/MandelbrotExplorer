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
using System.Threading;
using System.Windows;

namespace ConsoleMandelBrot
{
    class Mandelbrot : ICloneable
    {
        public int Resolution { get; set; }
        public int MaxIter { get; set; }
             
        public double ImageWidth { get; set; }
        public double XCenter { get; set; }
        public double YCenter { get; set; }
        public ColorGradient ColorGradient { get; set; } = new ColorGradient(new List<ControlPoint>() {
                new ControlPoint(0.0,Color.FromArgb(255,255,255)),
                new ControlPoint(0.125,Color.FromArgb(255,0,0)),
                new ControlPoint(0.25,Color.FromArgb(21,0,255)),
                new ControlPoint(0.375,Color.FromArgb(153,218,255)),
                new ControlPoint(0.50,Color.FromArgb(0,251,255)),
                new ControlPoint(0.675,Color.FromArgb(0,255,17)),
                new ControlPoint(0.75,Color.FromArgb(251,255,0)),
                new ControlPoint(1,Color.FromArgb(0,0,0))
            });

        private int[,] dataIter;
        private double[,] dataAbs;



        public Mandelbrot(double xCenter, double yCenter, double width, int resolution, int maxIterations)
        {
            this.XCenter = xCenter;
            this.YCenter = yCenter;
            ImageWidth = width;
            this.Resolution = resolution;
            this.MaxIter = maxIterations;
        }

        public Mandelbrot(double xCenter, double yCenter, double width, int resolution, int maxIterations, ColorGradient colorGradient)
        {
            this.XCenter = xCenter;
            this.YCenter = yCenter;
            ImageWidth = width;
            this.Resolution = resolution;
            this.MaxIter = maxIterations;
            this.ColorGradient = colorGradient;
        }

        public Bitmap MakeBitmap()
        {
            Bitmap canvas = new Bitmap(Resolution, Resolution);

            for (int x = 0; x < Resolution; x++)
            {
                for (int y = 0; y < Resolution; y++)
                {
                    canvas.SetPixel(x, y, MandelbrotColor(dataIter[x, y], dataAbs[x, y], MaxIter));
                }
            }

            return canvas;
        }

        public Bitmap MakeBitmap(double shift, int iterCycle)
        {
            Bitmap canvas = new Bitmap(Resolution, Resolution);

            for (int x = 0; x < Resolution; x++)
            {
                for (int y = 0; y < Resolution; y++)
                {
                    canvas.SetPixel(x, y, MandelbrotColor(dataIter[x, y], dataAbs[x, y], MaxIter, shift, iterCycle));
                }
            }

            return canvas;
        }
        public void Calculate()
        {
            System.Diagnostics.Stopwatch myStopwatch = new System.Diagnostics.Stopwatch();
            myStopwatch.Start();
            dataIter = new int[Resolution, Resolution];
            dataAbs = new double[Resolution, Resolution];
            
            double x = XCenter - ImageWidth / 2.0d;
            double y = YCenter - ImageWidth / 2.0d;
            object locker = new object();
  
            for (int xpix = 0; xpix < Resolution; xpix++)
            {
                Parallel.For(0, Resolution, ypix =>
                {
                    
                    (int,double) tuple;
                    lock (locker)
                    {
                        y = YCenter - ImageWidth / 2 + ImageWidth / Resolution * ypix;
                        tuple = CalculateIters(x, y, MaxIter);
                    }
                    lock (dataIter)
                        dataIter[xpix, ypix] = tuple.Item1;
                    lock (dataAbs)
                        dataAbs[xpix, ypix] = tuple.Item2;
                    

                });
                x += ImageWidth / Resolution;
                Console.WriteLine($"{100 * xpix / Resolution}%");
            }
            myStopwatch.Stop();
            //MessageBox.Show(myStopwatch.ElapsedMilliseconds.ToString());
            /*
            for (int xpix = 0; xpix < resolution; xpix++)
            {
                for(int ypix=0; ypix<resolution; ypix++)
                {

                    y = yCenter - imageWidth / 2 + imageWidth / resolution * ypix;
                    //canvas.SetPixel(xpix, ypix, MandelbrotColor(x, y, maxIter));
                    var tuple = CalculateIters(x, y, maxIter);
                    dataIter[xpix, ypix] = tuple.Item1;
                    dataAbs[xpix, ypix] = tuple.Item2;
                }
                x += imageWidth / resolution;
            }
            */
        }        
        public async void CalculateAsync()
        {
            await Task.Run(() => Calculate());
        }
        private Color MandelbrotColor(int iter, double abs, int maxIter)
        {
            if (iter == -1) return Color.FromArgb(0, 0, 0);
            //return ColorGradient.GetColor(Smooth(iter, abs) / maxIter);
            if (abs > 10e3) abs = 10e3;
            //Console.WriteLine(abs);
            return ColorGradient.GetColor(Smooth(iter, abs) % 400 / 400);
        }
        private Color MandelbrotColor(int iter, double abs, int maxIter, double shift, int iterCycle)
        {
            if (iter == -1) return Color.FromArgb(0, 0, 0);
            //return ColorGradient.GetColor(Smooth(iter, abs) / maxIter);

            if (abs > 10e3) abs = 10e3;
            //Console.WriteLine(abs);
            return ColorGradient.GetColor((Smooth(iter, abs) + shift) % iterCycle / iterCycle);
        }
        private (int,double) CalculateIters(double x, double y, int maxIter)
        {
            double Re = 0;
            double Im = 0;
            int i;
            double abs = 0;
            for (i = 0; i < maxIter; i++)
            {

                double buff = Re;
                Re = Re * Re - Im * Im + x;
                Im = 2 * buff * Im + y;

                abs = Re * Re + Im * Im;
                if (abs >= 4) break;

            }
            if (i == maxIter) i = -1;
            return (i, abs);
        }
        private double Smooth(double n, double abs)
        {
            double pon = n - Math.Log(Math.Log(abs, 2), 2) + 4;
            return pon;
        }

        public object Clone()
        {
            return new Mandelbrot(XCenter, YCenter, ImageWidth, Resolution, MaxIter, ColorGradient);
        }
    }
}
