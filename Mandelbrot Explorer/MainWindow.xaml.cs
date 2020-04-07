using ConsoleMandelBrot;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        private Mandelbrot mandelbrot;

     
        
        private void Button_Start(object sender, RoutedEventArgs e)
        {

            try
            {
                int iterations;
                if (((TextBlock)ComboBox_Iter.SelectedItem).Text == "Custom")
                {
                    iterations = Convert.ToInt32(TextBox_CustomIter.Text);
                }
                else iterations = Convert.ToInt32(((TextBlock)ComboBox_Iter.SelectedItem).Text);

                GoMandelbrot(
                    Convert.ToDouble(textbox_xpos.Text.Replace('.', ',')),
                    Convert.ToDouble(textbox_ypos.Text.Replace('.', ',')),
                    Convert.ToDouble(textbox_width.Text.Replace('.', ',')),
                    Convert.ToInt32(textbox_res.Text),
                    iterations);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
                
        }

        private void GoMandelbrot(double x, double y, double width, int RESOLUTION, int ITERATIONS)
        {
            if (mandelbrot == null)
            {
                mandelbrot = new Mandelbrot(x, y, width, RESOLUTION, ITERATIONS);
            }
            else
            {
                mandelbrot.XCenter = x;
                mandelbrot.YCenter = y;
                mandelbrot.ImageWidth = width;
                mandelbrot.Resolution = RESOLUTION;
                mandelbrot.MaxIter = ITERATIONS;
            }
            mandelbrot.Calculate();
            
            try
            {
                Bitmap canvas = mandelbrot.MakeBitmap(Slider_Shift.Value, Convert.ToInt32(TextBox_IterCycle.Text));
                FractalImage.Source = BitmapToImage(canvas);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            

            
            
            
            
        }

        private ImageSource BitmapToImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
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

        private void ComboBox_Iter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (TextBox_CustomIter == null) return;
            if (((TextBlock)ComboBox_Iter.SelectedItem).Text == "Custom")
            {
                TextBox_CustomIter.Visibility = Visibility.Visible;
                
            }
            else
            {
                TextBox_CustomIter.Visibility = Visibility.Collapsed;
                //Slider_Shift.Maximum = Convert.ToDouble(((TextBlock)ComboBox_Iter.SelectedItem).Text);
            }
        }

        private void ReColoring()
        {
            if (mandelbrot == null) return;
            try
            {
                fractalBitmap = mandelbrot.MakeBitmap(Slider_Shift.Value,
                                                      Convert.ToInt32(TextBox_IterCycle.Text));
                FractalImage.Source = BitmapToImage(fractalBitmap);
            } 
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
    

        private void TextBox_CustomIter_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Slider_Shift.Maximum = Convert.ToDouble(TextBox_CustomIter.Text);
        }


        private void Slider_Shift_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
        private void TextBox_IterCycle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(Slider_Shift!=null)
                try
                {
                    Slider_Shift.Maximum = Convert.ToInt32(TextBox_IterCycle.Text);
                }
                catch(Exception ex)
                {
                    
                }
                
            
        }

        private void Button_Recoloring(object sender, RoutedEventArgs e)
        {
            if(mandelbrot!=null)
                ReColoring();
        }

        
    }
}
