using ConsoleMandelBrot;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Mandelbrot_Explorer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private Bitmap fractalBitmap;


        private void Button_Start(object sender, RoutedEventArgs e)
        {
            /*
            await Task.Run(() => GoMandelbrot(
                Convert.ToDouble(textbox_xpos.Text.Replace('.', ',')),
                Convert.ToDouble(textbox_ypos.Text.Replace('.', ',')),
                Convert.ToDouble(textbox_width.Text.Replace('.', ',')),
                Convert.ToInt32(textbox_res),
                Convert.ToInt32(textbox_iter))
            );    */
            Console.WriteLine(textbox_xpos.Text);
            GoMandelbrot(
                Convert.ToDouble(textbox_xpos.Text.Replace('.', ',')),
                Convert.ToDouble(textbox_ypos.Text.Replace('.', ',')),
                Convert.ToDouble(textbox_width.Text.Replace('.', ',')),
                Convert.ToInt32(textbox_res.Text),
                Convert.ToInt32(textbox_iter.Text));
        }

        private void GoMandelbrot(double x, double y, double width, int RESOLUTION, int ITERATIONS)
        {
            Mandelbrot mandelbrot = new Mandelbrot(x, y, width, RESOLUTION, ITERATIONS);
            Bitmap canvas = mandelbrot.MakeBitmap();
            fractalBitmap = canvas;

            using (MemoryStream memory = new MemoryStream())
            {
                canvas.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                FractalImage.Source = bitmapImage;
            }
            
        }

        private void Button_Save(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Bitmap (*.BMP)| *.BMP |" +
                "JPEG (*.jpg) | *.jpg |" +
                "PNG (*.png) | *.png |" +
                "TIFF (*.tiff) | *.tiff |" +
                "GIF (*.gif) | *.gif |" +
                "All files(*.*) | *.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                fractalBitmap.Save(saveFileDialog.FileName);
            }
        }
    }
}
