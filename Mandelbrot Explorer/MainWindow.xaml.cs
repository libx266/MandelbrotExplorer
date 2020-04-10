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
            Init();
        }
        private Bitmap fractalBitmap;
        private Mandelbrot mandelbrot;

        private void Init()
        {
            List<ControlPoint> list = new List<ControlPoint>() {
                new ControlPoint(0.0, System.Drawing.Color.FromArgb(255, 255, 255)),
                new ControlPoint(0.125, System.Drawing.Color.FromArgb(255, 0, 0)),
                new ControlPoint(0.25, System.Drawing.Color.FromArgb(21, 0, 255)),
                new ControlPoint(0.375, System.Drawing.Color.FromArgb(153, 218, 255)),
                new ControlPoint(0.50, System.Drawing.Color.FromArgb(0, 251, 255)),
                new ControlPoint(0.675, System.Drawing.Color.FromArgb(0, 255, 17)),
                new ControlPoint(0.75, System.Drawing.Color.FromArgb(251, 255, 0)),
                new ControlPoint(1, System.Drawing.Color.FromArgb(0, 0, 0))
            };
            foreach (var point in list)
            {
                AddControlPoint(point.Position, new System.Windows.Media.Color
                {
                    A = 255,
                    R = point.Color.R,
                    G = point.Color.G,
                    B = point.Color.B
                });
            }

            Button_Start(new object(), new RoutedEventArgs());


        }       
        private async void Button_Start(object sender, RoutedEventArgs e)
        {

            try
            {
                List<ControlPoint> list = GetControlPoints();
                ColorGradient colorGradient = new ColorGradient(list);
                
                
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
                    iterations,
                    colorGradient);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
                
        }
        private async void GoMandelbrot(double x, double y, double width, int RESOLUTION, int ITERATIONS, ColorGradient colorGradient)
        {
           
            if (mandelbrot == null)
            {
                mandelbrot = new Mandelbrot(x, y, width, RESOLUTION, ITERATIONS);
                mandelbrot.ColorGradient = colorGradient;
            }
            else
            {
                mandelbrot.XCenter = x;
                mandelbrot.YCenter = y;
                mandelbrot.ImageWidth = width;
                mandelbrot.Resolution = RESOLUTION;
                mandelbrot.MaxIter = ITERATIONS;
                mandelbrot.ColorGradient = colorGradient;
            }
            //Bitmap bitmap = new Bitmap(RESOLUTION, RESOLUTION);
            mandelbrot.Calculate();

            try
            {
                Bitmap canvas = mandelbrot.MakeBitmap(Slider_Shift.Value, Convert.ToInt32(TextBox_IterCycle.Text));
                fractalBitmap = canvas;
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
                mandelbrot.ColorGradient = new ColorGradient(GetControlPoints());
                fractalBitmap = mandelbrot.MakeBitmap(Slider_Shift.Value,
                                                      Convert.ToInt32(TextBox_IterCycle.Text));
                FractalImage.Source = BitmapToImage(fractalBitmap);
            } 
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
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
        private void Button_AddCP_Click(object sender, RoutedEventArgs e)
        {
            AddControlPoint(0.0, new System.Windows.Media.Color() { A = 255, R = 255, G = 255, B = 255 });
        }
        private void AddControlPoint(double pos, System.Windows.Media.Color color)
        {
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;

            TextBox text = new TextBox();
            text.Text = pos.ToString();
            text.MinWidth = 50;
            text.VerticalAlignment = VerticalAlignment.Center;
            text.Margin = new Thickness(5, 2, 5, 2);
            text.Name = $"TextBox_CP{ListBox_ControlPoints.Items.Count}";


            Xceed.Wpf.Toolkit.ColorPicker colorPicker = new Xceed.Wpf.Toolkit.ColorPicker();
            colorPicker.MinWidth = 150;
            colorPicker.Margin = new Thickness(5, 2, 5, 2);
            colorPicker.SelectedColor = color;
            colorPicker.Closed += ColorPicker_Closed;

            Button destroyer = new Button();
            destroyer.Name = $"Button_CP{ListBox_ControlPoints.Items.Count}";
            destroyer.Content = "X";
            destroyer.Width = 20;
            destroyer.Margin = new Thickness(5, 2, 5, 2);
            destroyer.Click += Button_DeleteCP_Click;

            stack.Children.Add(text);
            stack.Children.Add(colorPicker);
            stack.Children.Add(destroyer);

            stack.Name = $"StackPanel_CP{ListBox_ControlPoints.Items.Count}";

            ListBox_ControlPoints.Items.Add(stack);

            SetRectGradient();
        }
        private void ColorPicker_Closed(object sender, RoutedEventArgs e)
        {
            SetRectGradient();
            ReColoring();
        }
        private void Button_DeleteCP_Click(object sender, RoutedEventArgs e)
        {
            
            foreach(StackPanel panel in ListBox_ControlPoints.Items)
            {
                if (panel.Children.IndexOf((Button)sender) != -1)
                {
                    ListBox_ControlPoints.Items.Remove(panel);
                    break;
                }
            }
            SetRectGradient();
            if(ListBox_ControlPoints.Items.Count>=2) ReColoring();
        }
        private void Button_ClearCP_Click(object sender, RoutedEventArgs e)
        {
            ListBox_ControlPoints.Items.Clear();
            AddControlPoint(0, new System.Windows.Media.Color { A = 255, R = 255, G = 255, B = 255 });
            SetRectGradient();
            //AddControlPoint(1, new System.Windows.Media.Color { A = 255, R = 0, G = 0, B = 0 });
        }
        private List<ControlPoint> GetControlPoints()
        {
            List<ControlPoint> points = new List<ControlPoint>();
            if (ListBox_ControlPoints.Items.Count< 2) throw new Exception("Должны быть как минимум 2 контрольные точки");
            foreach (StackPanel panel in ListBox_ControlPoints.Items)
            {
                double pos = Convert.ToDouble(panel.Children.OfType<TextBox>().ToList()[0].Text.Replace('.',','));
                System.Windows.Media.Color badColor = (System.Windows.Media.Color)panel.Children.OfType<Xceed.Wpf.Toolkit.ColorPicker>().ToList()[0].SelectedColor;
                System.Drawing.Color color = System.Drawing.Color.FromArgb(badColor.R, badColor.G, badColor.B);
                points.Add(new ControlPoint(pos, color));
            }
            var points2 = from p in points
                          orderby p.Position
                          select p;
            points = points2.ToList<ControlPoint>();
            if (points[0].Position > 0) points[0].Position = 0;
            if (points[points.Count - 1].Position < 1) points[points.Count - 1].Position = 1;
            

            return points;
        }
        private void Button_RandomCP_Click(object sender, RoutedEventArgs e)
        {
            Random random = new Random(DateTime.Now.Millisecond);
            if (true)
            {
                ListBox_ControlPoints.Items.Clear();
                int point = random.Next(1, 5);
                for (int i = 0; i <= point; i++)
                {
                    AddControlPoint(Math.Round((double)i/point,4), new System.Windows.Media.Color()
                    {
                        A = 255,
                        R = (byte)random.Next(0, 255),
                        G = (byte)random.Next(0, 255),
                        B = (byte)random.Next(0, 255)
                    });
                }
                Slider_Shift.Value = random.NextDouble()* Slider_Shift.Maximum % Slider_Shift.Maximum;
                ReColoring();
            }
            else
            {
                for (int i = 0; i < ListBox_ControlPoints.Items.Count; i++)
                {
                    ((StackPanel)ListBox_ControlPoints.Items[i]).Children.OfType<Xceed.Wpf.Toolkit.ColorPicker>().ToList()[0].SelectedColor = new System.Windows.Media.Color()
                    {
                        A = 255,
                        R = (byte)random.Next(0, 255),
                        G = (byte)random.Next(0, 255),
                        B = (byte)random.Next(0, 255)
                    };
                }
            }
        }
        private void Slider_Shift_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ReColoring();
        }
        private void SetRectGradient()
        {

            GradientStopCollection gradients = new GradientStopCollection();
            List<ControlPoint> controlPoints = new List<ControlPoint>();
            if (ListBox_ControlPoints.Items.Count >= 2)
            {
                controlPoints = GetControlPoints();
                foreach (var point in controlPoints)
                {
                    gradients.Add(new GradientStop(new System.Windows.Media.Color()
                    {
                        A = 255,
                        R = point.Color.R,
                        G = point.Color.G,
                        B = point.Color.B
                    }, point.Position));
                }
            }
            else if (ListBox_ControlPoints.Items.Count == 1)
            {
                System.Windows.Media.Color color = (System.Windows.Media.Color)((StackPanel)ListBox_ControlPoints.Items[0]).Children.OfType<Xceed.Wpf.Toolkit.ColorPicker>().ToList()[0].SelectedColor;
                Rect_Graient.Fill = new SolidColorBrush(color);
            }
            else if (ListBox_ControlPoints.Items.Count == 0)
            {
                Rect_Graient.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
            }

            LinearGradientBrush brush = new LinearGradientBrush(gradients);
            Rect_Graient.Fill = brush;
        }
        private void SetRectGradient(List<ControlPoint> controlPoints)
        {

            GradientStopCollection gradients = new GradientStopCollection();
            if (controlPoints.Count >= 2)
            {
                
                foreach (var point in controlPoints)
                {
                    gradients.Add(new GradientStop(new System.Windows.Media.Color()
                    {
                        A = 255,
                        R = point.Color.R,
                        G = point.Color.G,
                        B = point.Color.B
                    }, point.Position));
                }
            }
            else if (controlPoints.Count == 1)
            {
                System.Windows.Media.Color color = new System.Windows.Media.Color() { A = 255,
                    R = controlPoints[0].Color.R,
                    G = controlPoints[0].Color.G,
                    B = controlPoints[0].Color.B
                };
                Rect_Graient.Fill = new SolidColorBrush(color);
            }
            else if (controlPoints.Count == 0)
            {
                Rect_Graient.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
            }

            LinearGradientBrush brush = new LinearGradientBrush(gradients);
            Rect_Graient.Fill = brush;
        }
        private void FractalImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.Image image = (System.Windows.Controls.Image)sender;
            System.Windows.Point mousePos = Mouse.GetPosition(image);
            mousePos.Y = 0.5*image.ActualHeight - mousePos.Y;
            mousePos.X = mousePos.X - 0.5 * image.ActualHeight;
            double newX, newY;
            newX = Convert.ToDouble(textbox_xpos.Text.Replace('.',',')) + mousePos.X * Convert.ToDouble(textbox_width.Text.Replace('.', ',')) / image.ActualWidth;
            newY = Convert.ToDouble(textbox_ypos.Text.Replace('.', ',')) + mousePos.Y * Convert.ToDouble(textbox_width.Text.Replace('.', ',')) / image.ActualHeight;
            Console.WriteLine(newY.ToString());
            try
            {
                List<ControlPoint> list = GetControlPoints();
                ColorGradient colorGradient = new ColorGradient(list);


                int iterations;
                if (((TextBlock)ComboBox_Iter.SelectedItem).Text == "Custom")
                {
                    iterations = Convert.ToInt32(TextBox_CustomIter.Text);
                }
                else iterations = Convert.ToInt32(((TextBlock)ComboBox_Iter.SelectedItem).Text);
                textbox_xpos.Text = newX.ToString();
                textbox_ypos.Text = newY.ToString();
                textbox_width.Text = (Convert.ToDouble(textbox_width.Text.Replace('.', ',')) / 2).ToString();
                GoMandelbrot(
                    Convert.ToDouble(textbox_xpos.Text.Replace('.', ',')),
                    Convert.ToDouble(textbox_ypos.Text.Replace('.', ',')),
                    Convert.ToDouble(textbox_width.Text.Replace('.', ',')),
                    Convert.ToInt32(textbox_res.Text),
                    iterations,
                    colorGradient);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        private void RenderingState(double done)
        {
            progressBar.Value = done;
            if (done == 1) progressBar.Visibility = Visibility.Collapsed;
            else progressBar.Visibility = Visibility.Visible;
        }
    }
}
