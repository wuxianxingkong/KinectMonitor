using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.VideoSurveillance;
using Emgu.CV.Structure;
using Microsoft.Kinect;
using System.Windows.Media.Imaging;
using System.Drawing;
namespace KinectMonitor
{
    public static class EmguImageExtensions
    {
        public static Image<TColor, TDepth> ToOpenCVImage<TColor, TDepth>(this ColorImageFrame image)
            where TColor : struct, IColor
            where TDepth : new()
        {
            //WriteableBitmap ColorImageBitmap= new WriteableBitmap(image.FrameWidth, image.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
            var bitmap = image.ToBitmap();
            return new Image<TColor, TDepth>(bitmap);
        }

        public static Image<TColor, TDepth> ToOpenCVImage<TColor, TDepth>(this Bitmap bitmap)
            where TColor : struct, IColor
            where TDepth : new()
        {
            return new Image<TColor, TDepth>(bitmap);
        }

        //public static System.Windows.Media.Imaging.BitmapSource ToBitmapSource(this IImage image)
        //{
        //    var source = image.Bitmap.ToBitmapSource();
        //    return source;
        //}
    }
}
