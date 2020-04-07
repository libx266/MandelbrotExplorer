using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Remoting.Messaging;
using System.Numerics;

namespace ConsoleMandelBrot
{
    class Program
    {
        static void pon(string[] args)
        {
            const int FRAMES = 1;
            const int RESOLUTION = 1000;
            const int ITERATIONS = 350;
            double x =-1.1935,
                   y =-0.1145,
                   width = 0.001;

            Mandelbrot mandelbrot = new Mandelbrot(x, y, width, RESOLUTION, ITERATIONS);
            for (int i = 0; i < FRAMES; i++)
            {
                Bitmap canvas = mandelbrot.MakeBitmap();
                try
                {
                    canvas.Save("Mandelbrot" + i + ".png", ImageFormat.Png);
                    Console.WriteLine("Image {0} already rendered", i);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                mandelbrot.maxIter += 0;
                mandelbrot.imageWidth *= 0.7;
            }
            Process.Start(Environment.CurrentDirectory);

        }
    }
}
