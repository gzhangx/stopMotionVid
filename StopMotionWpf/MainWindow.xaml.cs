using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VideCaptureLib;

namespace StopMotionWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly VidCaptureBmpAry capture;
        static byte[] emptyFrame = new byte[0];
        byte[] curFrame = emptyFrame;
        int curImageSaveIndex = 1;
        List<byte[]> prevImages = new List<byte[]>();
        const int MAXIMGS = 20;

        int curImageOverLayInd = -1;

        System.Windows.Controls.Image[] prevImageControls = new System.Windows.Controls.Image[MAXIMGS];
        Grid[] prevGridControls = new Grid[MAXIMGS];
        Border[] prevImageBorders= new Border[MAXIMGS];

        private string? saveLocation = null;
        public MainWindow()
        {
            InitializeComponent();            

            this.Closing += MainWindow_Closing;
            capture = new VidCaptureBmpAry(this.onVideFrame);
            if (!ensureDir()) return;

            for (int i = 0; i < MAXIMGS; i++)
            {
                prevImageControls[i] = new System.Windows.Controls.Image();
                Grid grid = new Grid();
                prevGridControls[i] = grid;
                Border border = new Border();
                border.Child = prevImageControls[i];
                grid.Children.Add(border);
                prevImageBorders[i] = border;
                border.BorderThickness = new Thickness(0);
                border.Visibility= Visibility.Visible;
                border.BorderBrush = System.Windows.Media.Brushes.Yellow;
                //border.Background = System.Windows.Media.Brushes.Yellow;
                panImages.Children.Add(grid);
            }

            loadOldFiles();
            ShowPrevImages();
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            capture.Stop();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            capture.Start();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            capture.Stop();
        }

        void onVideFrame(byte[] data)
        {
            lock (emptyFrame)
            {
                curFrame = data;
            }
            this.Dispatcher.Invoke(new Action(() =>
            {
                imgMain.Source = byteToDspImg(data);
            }));            
        }

        BitmapImage byteToDspImg(byte[] data)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            MemoryStream ms = new MemoryStream(data);
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        private bool ensureDir()
        {
            try
            {
                saveLocation = txtSaveLocation.Text;

                if (!File.Exists(saveLocation))
                {
                    Directory.CreateDirectory(saveLocation);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't create dir " + saveLocation + ex.Message);
                return false;
            }
        }
        private void btnCapture_Click(object sender, RoutedEventArgs e)
        {
            if (!ensureDir()) return;
            if (!Directory.Exists(saveLocation))
            {
                MessageBox.Show("not a dir " + saveLocation);
                return;
            }

            if (curFrame.Length > 0)
            {
                lock (emptyFrame)
                {
                    curImageSaveIndex++;
                    File.WriteAllBytes(System.IO.Path.Combine(saveLocation, "IMG_"+ curImageSaveIndex.ToString().PadLeft(3,'0') + ".jpeg"), curFrame);
                    prevImages.Add(curFrame);
                    curFrame = emptyFrame;
                }
                while (prevImages.Count > MAXIMGS)
                {
                    prevImages.RemoveAt(0);
                }
                ShowPrevImages();
            } else
            {
                MessageBox.Show("No frame yet");
            }
        }

        private void ShowPrevImages()
        {
            for (int i = 0; i < prevImages.Count; i++)
            {
                if (prevImages[i] != null)
                {
                    prevImageControls[i].Source = byteToDspImg(prevImages[i]);
                }
            }
        }

        private void loadOldFiles()
        {
            var dirInfo = new DirectoryInfo(txtSaveLocation.Text);
            var fInfos = from files in dirInfo.EnumerateFiles("*.jpeg")
                         orderby files.FullName ascending
                         select new
                         {
                             fileName = files.FullName,
                             name = files.Name,
                         };
            int i = 0;
            int maxInd = 0;
            foreach (var f in fInfos.Take(MAXIMGS))
            {                
                var matched = new System.Text.RegularExpressions.Regex("IMG_(?<Num>[0-9])+.jpeg", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Match("IMG_111.jpeg");
                if (matched.Success)
                {
                    var val = matched.Groups["Num"].Value;
                    try
                    {
                        var intVal = int.Parse(val);
                        curImageOverLayInd = i;
                        prevImages.Add(File.ReadAllBytes(f.fileName));
                        if (maxInd < intVal) maxInd = intVal;
                    } catch
                    {
                        Console.WriteLine("Error parse file " + f.fileName);
                    }
                }                
            }
            curImageSaveIndex = maxInd;            
        }

        void showOverLay()
        {
            if (curImageOverLayInd >= 0 && curImageOverLayInd < prevImages.Count)
            {
                var data = prevImages[curImageOverLayInd];
                if (data == null) return;
                imgOverlay.Source = byteToDspImg(data);
            }
        }


        void hightLightSelectedPrevImage()
        {
            foreach( var border in prevImageBorders)
            {
                if (border == null) continue;
                border.BorderThickness = new Thickness(0);
            }

            if (curImageOverLayInd >= 0)
            {
                var border = prevImageBorders[curImageOverLayInd];
                if (border == null) return;
                border.BorderThickness = new Thickness(3);
            }
        }
        private void btnPrevOverlay_Click(object sender, RoutedEventArgs e)
        {
            curImageOverLayInd--;
            if (curImageOverLayInd < 0)
            {
                curImageOverLayInd = 0;
                btnPrevOverlay.IsEnabled = false;                
            }
            btnNextOverlay.IsEnabled = true;
            showOverLay();
            hightLightSelectedPrevImage();
        }

        private void btnNextOverlay_Click(object sender, RoutedEventArgs e)
        {
            curImageOverLayInd++;
            if (curImageOverLayInd >= prevImages.Count || prevImages[curImageOverLayInd] == null)
            {
                curImageOverLayInd--;
                btnPrevOverlay.IsEnabled = true;
                btnNextOverlay.IsEnabled = false;
            }
            showOverLay();
            hightLightSelectedPrevImage();
        }

        private void txtMainImageAlpha_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var txt = txtMainImageAlpha.Text;
            try
            {
                var alpha = float.Parse(txt);
                imgMain.Opacity = alpha;
            } catch(Exception exc)
            {
                MessageBox.Show("Error change alpha " + exc.Message);
            }
        }
    }
}
