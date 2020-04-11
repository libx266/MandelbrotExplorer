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
            dataIter = new int[resolution, resolution];
            dataAbs = new double[resolution, resolution];
        }

        public Mandelbrot(double xCenter, double yCenter, double width, int resolution, int maxIterations, ColorGradient colorGradient)
        {
            this.XCenter = xCenter;
            this.YCenter = yCenter;
            ImageWidth = width;
            this.Resolution = resolution;
            this.MaxIter = maxIterations;
            this.ColorGradient = colorGradient;
            dataIter = new int[resolution, resolution];
            dataAbs = new double[resolution, resolution];
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
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            for (int x = 0; x < Resolution; x++)
            {
                
                for (int y = 0; y < Resolution; y++)
                {
                    canvas.SetPixel(x, Resolution - y - 1, MandelbrotColor(dataIter[x, y], dataAbs[x, y], MaxIter, shift, iterCycle));
                }
                
            }

            stopwatch.Stop();
            
            return canvas;
        }
        public void Calculate()
        {
            System.Diagnostics.Stopwatch myStopwatch = new System.Diagnostics.Stopwatch();
            myStopwatch.Start();
            dataIter = new int[Resolution, Resolution];
            dataAbs = new double[Resolution, Resolution];

            double x = XCenter - ImageWidth / 2.0d;
            double y = ImageWidth / 2.0d - YCenter;
            object locker = new object();

            for (int xpix = 0; xpix < Resolution; xpix++)
            {
                Parallel.For(0, Resolution, ypix =>
                {

                    (int, double) tuple;
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
            
            
        }

        public void CalculateOpenCL()
        {
            Stopwatch myStopwatch = new Stopwatch();
            myStopwatch.Start();
            dataIter = new int[Resolution, Resolution];
            dataAbs = new double[Resolution, Resolution];
            string vecSum = @"
            __kernel void
            mandelbrot(__global    int * iter,
                   __global    double * abs, 
                   __global double *xc,__global double *yc,__global double *width,__global int *res,__global int *maxiter)
            {
                int xpix=get_global_id(0);
                int ypix=get_global_id(1);


                double x = *xc - *width/2 + *width/(*res)*xpix;
                double y = *yc - *width/2 + *width/(*res)*ypix;
                double Re = 0;
                double Im = 0;
                int i;
                double mag = 0;
                for (i = 0; i < *maxiter; i++)
                {

                    double buff = Re;
                    Re = Re * Re - Im * Im + x;
                    Im = 2 * buff * Im + y;

                    mag = Re * Re + Im * Im;
                    if (mag >= 4) break;
                }
                if (i == *maxiter) i = -1;
                iter[mad_sat(ypix,*res,xpix)]=i;
                abs[mad_sat(ypix,*res,xpix)]=mag;
            }
            ";
            
            //Инициализация платформы. В скобках можно задавать параметры. По умолчанию инициализируются только GPU.
            //OpenCLTemplate.CLCalc.InitCL(Cloo.ComputeDeviceTypes.All) позволяет инициализировать не только
            //GPU но и CPU.
            OpenCLTemplate.CLCalc.InitCL();
            //Команда выдаёт список проинициализированных устройств.
            List<Cloo.ComputeDevice> L = OpenCLTemplate.CLCalc.CLDevices;
            //Команда устанавливает устройство с которым будет вестись работа
            OpenCLTemplate.CLCalc.Program.DefaultCQ = 0;
            //Компиляция программы vecSum
            OpenCLTemplate.CLCalc.Program.Compile(new string[] { vecSum });
            //Присовоение названия скомпилированной программе, её загрузка.
            OpenCLTemplate.CLCalc.Program.Kernel Mandelbrot = new OpenCLTemplate.CLCalc.Program.Kernel("mandelbrot");
            int n = Resolution;
            dataIter = new int[Resolution, Resolution];
            dataAbs = new double[Resolution, Resolution];
            int[] lineDataIter = new int[Resolution * Resolution];
            double[] lineDataAbs = new double[Resolution * Resolution];

            //Загружаем вектора в память устройства
            OpenCLTemplate.CLCalc.Program.Variable var_iter = new OpenCLTemplate.CLCalc.Program.Variable(lineDataIter);
            OpenCLTemplate.CLCalc.Program.Variable var_abs = new OpenCLTemplate.CLCalc.Program.Variable(lineDataAbs);
            OpenCLTemplate.CLCalc.Program.Variable var_xc = new OpenCLTemplate.CLCalc.Program.Variable(new double[] { XCenter });
            OpenCLTemplate.CLCalc.Program.Variable var_yc = new OpenCLTemplate.CLCalc.Program.Variable(new double[] { YCenter });
            OpenCLTemplate.CLCalc.Program.Variable var_width = new OpenCLTemplate.CLCalc.Program.Variable(new double[] { ImageWidth});
            OpenCLTemplate.CLCalc.Program.Variable var_res = new OpenCLTemplate.CLCalc.Program.Variable(new int[] { Resolution });
            OpenCLTemplate.CLCalc.Program.Variable var_maxiter = new OpenCLTemplate.CLCalc.Program.Variable(new int[] { MaxIter});

            //Объявление того, кто из векторов кем является
            OpenCLTemplate.CLCalc.Program.Variable[] args = new OpenCLTemplate.CLCalc.Program.Variable[] { var_iter, var_abs, var_xc,var_yc,
                                                                                                           var_width, var_res, var_maxiter };

            //Сколько потоков будет запущенно
            int[] workers = new int[2] { n, n };

            //Исполняем ядро VectorSum с аргументами args и колличеством потоков workers
            Mandelbrot.Execute(args, workers);

            //выгружаем из памяти
            var_iter.ReadFromDeviceTo(lineDataIter);
            var_abs.ReadFromDeviceTo(lineDataAbs);
            for (int i = 0; i < Resolution; i++)
            {
                for (int j = 0; j < Resolution; j++)
                {
                    dataIter[i, j] = lineDataIter[Resolution * j + i];
                    dataAbs[i, j] = lineDataAbs[Resolution * j + i];
                }
            }

            myStopwatch.Stop();
            
            
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
