using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideCaptureLib
{
    public class VidCaptureBmpAry : VidCaptureMat
    {
        public VidCaptureBmpAry(Action<byte[]> grabAction, int ind = 0) : base(mat =>
        {
            MemoryStream ms = new MemoryStream();            
            Mat mat1 = mat.Clone();
            using (var bmp = mat.ToBitmap())
            {
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            }
            grabAction(ms.ToArray());
        }, ind)
        {
        }
    }
}
