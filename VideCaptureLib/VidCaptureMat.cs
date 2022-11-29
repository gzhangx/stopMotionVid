using Emgu.CV;
using System;

namespace VideCaptureLib
{
    public class VidCaptureMat : IDisposable
    {
        private object vidLock = new object();
        protected VideoCapture vid;
        public int W { get; protected set; }
        public int H { get; protected set; }
        public VidCaptureMat(Action<Mat> grabAction, int ind = 0)
        {
            //DsDevice[] _SystemCamereas = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            //WebCams = new Video_Device[_SystemCamereas.Length];
            vid = new VideoCapture(ind);
            W = vid.Width; //(int)vid.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth);
            H = vid.Height; //(int)vid.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight);
            vid.ImageGrabbed += (sender, e) =>
            {
                if (vid != null)
                {
                    using (Mat mat = new Mat())
                    {
                        lock (vidLock)
                        {
                            if (vid != null) //mat = vid.QueryFrame();
                                vid.Retrieve(mat);
                        }
                        if (mat == null)
                        {
                            return;
                        }
                        grabAction(mat);
                    }
                }
            };            
        }

        public void Start()
        {
            vid.Start();
        }
        public void Stop()
        {
            vid.Stop();
        }

        public void Dispose()
        {
            if (vid != null)
            {
                lock (vidLock)
                {
                    vid.Stop();
                    vid.Dispose();
                }
            }
        }
    }

}