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
using System.IO;

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
        private float[,] dataAbs;
        private int[] lineDataIter;
        private float[] lineDataAbs;



        public Mandelbrot(double xCenter, double yCenter, double width, int resolution, int maxIterations)
        {
            this.XCenter = xCenter;
            this.YCenter = yCenter;
            ImageWidth = width;
            this.Resolution = resolution;
            this.MaxIter = maxIterations;
            dataIter = new int[resolution, resolution];
            dataAbs = new float[resolution, resolution];
        }

        public Mandelbrot(double xCenter, double yCenter, double width, int resolution, int maxIterations, ColorGradient colorGradient)
        {
            this.XCenter = xCenter; // х
            this.YCenter = yCenter; // у
            ImageWidth = width; //хз
            this.Resolution = resolution; //масштаб
            this.MaxIter = maxIterations; // макс итераций
            this.ColorGradient = colorGradient; // градиент
            dataIter = new int[resolution, resolution];
            dataAbs = new float[resolution, resolution];
            lineDataAbs = new float[resolution * resolution];
            lineDataIter = new int[resolution * resolution];
    }
        /*
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
        */
        /*
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
        */
        public Bitmap MakeBitmapOpenCL(double shift, int iterCycle)
        {



            string programStr = @"
            
__kernel void
            program(__write_only image2d_t bitmap, __constant int *res,
                   __constant int *shift, __constant int *cycle,
                   __constant int *iter, __constant int *maxiter,
                   __constant float *abs, __constant float *pos,
                   __constant int *R, __constant int *G, __constant int *B, __constant int *n)
            {
               int2 coor=(int2)(get_global_id(0),get_global_id(1));
               int i =*(iter+mad_sat(coor.x,*res,coor.y));
               float mag=*(abs+mad_sat(coor.x,*res,coor.y));
               uint4 color = (uint4)(0,0,0,255);
               int red,green,blue;
               if(i!=-1)
               {
                   float smooth=i-log2(log2(mag))+4+*shift;
                   while(smooth>=0)
                   {
                       smooth-=*cycle;
                   }
                   smooth+=*cycle;
                   
                   float cpos=smooth / (float)(*cycle);
                   for(int i = 1; i<(*n); i++)
                   {
                       if(cpos<(*(pos+i)))
                       {
                           red = round(R[i - 1] + (cpos - pos[i - 1])  / (pos[i] - pos[i - 1])* (R[i] - R[i - 1]));
                           green = round(G[i - 1] + (cpos - pos[i - 1])  / (pos[i] - pos[i - 1])* (G[i] - G[i - 1]));
                           blue = round(B[i - 1] + (cpos - pos[i - 1])  / (pos[i] - pos[i - 1])* (B[i] - B[i - 1]));
                           break;
                       }  
                    }
                    color=(uint4)(blue,green,red,255);                 
               }              
               write_imageui(bitmap, (int2)(coor.y,*res-1-coor.x), color);
               
            }
            ";

            OpenCLTemplate.CLCalc.InitCL();
            List<Cloo.ComputeDevice> L = OpenCLTemplate.CLCalc.CLDevices;
            //Команда устанавливает устройство с которым будет вестись работа
            OpenCLTemplate.CLCalc.Program.DefaultCQ = 0;
            //Компиляция программы vecSum
            OpenCLTemplate.CLCalc.Program.Compile(new string[] { programStr });

            //Присовоение названия скомпилированной программе, её загрузка.
            OpenCLTemplate.CLCalc.Program.Kernel program = new OpenCLTemplate.CLCalc.Program.Kernel("program");

            byte[] matrix = new byte[Resolution * Resolution * 4];

            float[] pos = new float[ColorGradient.controlPoints.Count];
            int[] R = new int[ColorGradient.controlPoints.Count];
            int[] G = new int[ColorGradient.controlPoints.Count];
            int[] B = new int[ColorGradient.controlPoints.Count];

            for (int i = 0; i < ColorGradient.controlPoints.Count; i++)
            {
                pos[i] = float.Parse(ColorGradient.controlPoints[i].Position.ToString());
                R[i] = ColorGradient.controlPoints[i].Color.R;
                G[i] = ColorGradient.controlPoints[i].Color.G;
                B[i] = ColorGradient.controlPoints[i].Color.B;
            }

            OpenCLTemplate.CLCalc.Program.Image2D var_bitmap = new OpenCLTemplate.CLCalc.Program.Image2D(matrix, Resolution, Resolution);
            OpenCLTemplate.CLCalc.Program.Variable var_res = new OpenCLTemplate.CLCalc.Program.Variable(new int[] { Resolution });
            OpenCLTemplate.CLCalc.Program.Variable var_maxiter = new OpenCLTemplate.CLCalc.Program.Variable(new int[] { MaxIter });
            OpenCLTemplate.CLCalc.Program.Variable var_shift = new OpenCLTemplate.CLCalc.Program.Variable(new int[] { Convert.ToInt32(Math.Round(shift)) });
            OpenCLTemplate.CLCalc.Program.Variable var_cycle = new OpenCLTemplate.CLCalc.Program.Variable(new int[] { iterCycle });
            OpenCLTemplate.CLCalc.Program.Variable var_iter = new OpenCLTemplate.CLCalc.Program.Variable(lineDataIter);
            OpenCLTemplate.CLCalc.Program.Variable var_abs = new OpenCLTemplate.CLCalc.Program.Variable(lineDataAbs);
            OpenCLTemplate.CLCalc.Program.Variable var_pos = new OpenCLTemplate.CLCalc.Program.Variable(pos);
            OpenCLTemplate.CLCalc.Program.Variable var_R = new OpenCLTemplate.CLCalc.Program.Variable(R);
            OpenCLTemplate.CLCalc.Program.Variable var_G = new OpenCLTemplate.CLCalc.Program.Variable(G);
            OpenCLTemplate.CLCalc.Program.Variable var_B = new OpenCLTemplate.CLCalc.Program.Variable(B);
            OpenCLTemplate.CLCalc.Program.Variable var_n = new OpenCLTemplate.CLCalc.Program.Variable(new int[] { ColorGradient.controlPoints.Count });
            //var_bitmap.WriteToDevice(matrix);
            OpenCLTemplate.CLCalc.Program.MemoryObject[] args = new OpenCLTemplate.CLCalc.Program.MemoryObject[] { var_bitmap, var_res,
                var_shift,var_cycle,var_iter,var_maxiter, var_abs, var_pos,var_R,var_G,var_B,var_n};
            
            program.Execute(args, new int[] { Resolution, Resolution });


            var_bitmap.ReadFromDeviceTo(matrix);
            Stopwatch stop = new Stopwatch();
            stop.Start();

            /*
            for (int i = 0; i < matrix.Length; i += 4)
            {
                bitmap.SetPixel(i / 4 / Resolution,  (i / 4) % Resolution, Color.FromArgb(matrix[i + 2], matrix[i + 1], matrix[i + 0]));
            }
            */
            Bitmap bmp = new Bitmap(Resolution, Resolution, PixelFormat.Format32bppPArgb);

            //Create a BitmapData and Lock all pixels to be written 
            BitmapData bmpData = bmp.LockBits(
                                 new Rectangle(0, 0, bmp.Width, bmp.Height),
                                 ImageLockMode.WriteOnly, bmp.PixelFormat);
            //Copy the data from the byte array into BitmapData.Scan0
            Marshal.Copy(matrix, 0, bmpData.Scan0, matrix.Length);
            //Unlock the pixels
            bmp.UnlockBits(bmpData);


            stop.Stop();
            //MessageBox.Show(stop.ElapsedMilliseconds.ToString());





            return bmp;
        }
        /*public void Calculate()
        {
            System.Diagnostics.Stopwatch myStopwatch = new System.Diagnostics.Stopwatch();
            myStopwatch.Start();
            dataIter = new int[Resolution, Resolution];
            dataAbs = new float[Resolution, Resolution];

            double x = XCenter - ImageWidth / 2.0d;
            double y = ImageWidth / 2.0d - YCenter;
            object locker = new object();

            for (int xpix = 0; xpix < Resolution; xpix++)
            {
                Parallel.For(0, Resolution, ypix =>
                {

                    (int, float) tuple;
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
        */
        public void CalculateOpenCL()
        {
            
            dataIter = new int[Resolution, Resolution];
            dataAbs = new float[Resolution, Resolution];
            string vecSum = @"
            __kernel void
            mandelbrot(__global    int * iter,
                   __global    float * abs, 
                   __constant double *xc,__constant double *yc,__constant double *width,__constant int *res,__constant int *maxiter)
            {
                int xpix=get_global_id(0);
                int ypix=get_global_id(1);


                double x = *xc - *width/2 + *width/(*res)*xpix;
                double y = *yc - *width/2 + *width/(*res)*ypix;
                double Re = 0;
                double Im = 0;
                int i;
                float mag = 0;
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
            dataAbs = new float[Resolution, Resolution];
            lineDataIter = new int[Resolution * Resolution];
            lineDataAbs = new float[Resolution * Resolution];

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
            /*
            for (int i = 0; i < Resolution; i++)
            {
                for (int j = 0; j < Resolution; j++)
                {
                    dataIter[i, j] = lineDataIter[Resolution * j + i];
                    dataAbs[i, j] = lineDataAbs[Resolution * j + i];
                }
            }
            */
            
            
            
        }
        /*
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
        private (int,float) CalculateIters(double x, double y, int maxIter)
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
        private double Smooth(double n, float abs)
        {
            double pon = n - Math.Log(Math.Log(abs, 2), 2) + 4;
            return pon;
        }*/

        public object Clone()
        {
            return new Mandelbrot(XCenter, YCenter, ImageWidth, Resolution, MaxIter, ColorGradient);
        }
    }
}
