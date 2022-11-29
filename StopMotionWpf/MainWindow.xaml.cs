using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using VideCaptureLib;

namespace StopMotionWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        VidCaptureBmpAry capture;
        static byte[] emptyFrame = new byte[0];
        byte[] curFrame = emptyFrame;
        int curImageIndex = 1;
        List<byte[]> prevImages = new List<byte[]>();
        const int MAXIMGS = 20;

        System.Windows.Controls.Image[] prevImageControls = new System.Windows.Controls.Image[MAXIMGS];

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_Closing;
            capture = new VidCaptureBmpAry(this.onVideFrame);
            for(int i = 0; i < MAXIMGS; i++)
            {
                prevImageControls[i] = new System.Windows.Controls.Image();
                panImages.Children.Add(prevImageControls[i]);
            }
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

        private void btnCapture_Click(object sender, RoutedEventArgs e)
        {
            var saveLocation = txtSaveLocation.Text;
            try
            {
                if (!File.Exists(saveLocation))
                {
                    Directory.CreateDirectory(saveLocation);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't create dir " + saveLocation + ex.Message);
                return;
            }
            if (!Directory.Exists(saveLocation))
            {
                MessageBox.Show("not a dir " + saveLocation);
                return;
            }

            if (curFrame.Length > 0)
            {
                lock (emptyFrame)
                {
                    curImageIndex++;
                    File.WriteAllBytes(System.IO.Path.Combine(saveLocation, "IMG_"+ curImageIndex.ToString().PadLeft(3,'0') + ".jpeg"), curFrame);
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

        private void btnPrevOverlay_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnNextOverlay_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
