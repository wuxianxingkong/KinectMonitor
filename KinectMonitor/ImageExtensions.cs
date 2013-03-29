using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.VideoSurveillance;
using Emgu.CV.Structure;
using Microsoft.Kinect;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
namespace KinectMonitor
{
   public static class ImageExtensions
    {
        public static Bitmap ToBitmap(this byte[] data, int width, int height
            , System.Drawing.Imaging.PixelFormat format)
        {
            var bitmap = new Bitmap(width, height, format);

            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly,
                bitmap.PixelFormat);
            System.Runtime.InteropServices.Marshal.Copy(data, 0, bitmapData.Scan0, data.Length);
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

        public static Bitmap ToBitmap(this short[] data, int width, int height
            , System.Drawing.Imaging.PixelFormat format)
        {
            var bitmap = new Bitmap(width, height, format);

            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly,
                bitmap.PixelFormat);
            System.Runtime.InteropServices.Marshal.Copy(data, 0, bitmapData.Scan0, data.Length);
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

        public static System.Windows.Media.Imaging.BitmapSource ToBitmapSource(this byte[] data
            , System.Windows.Media.PixelFormat format, int width, int height)
        {
            return System.Windows.Media.Imaging.BitmapSource.Create(width, height, 96, 96
                , format, null, data, width * format.BitsPerPixel / 8);
        }

        public static System.Windows.Media.Imaging.BitmapSource ToBitmapSource(this short[] data
        , System.Windows.Media.PixelFormat format, int width, int height)
        {
            return System.Windows.Media.Imaging.BitmapSource.Create(width, height, 96, 96
                , format, null, data, width * format.BitsPerPixel / 8);
        }
        // bitmap methods
        public static Bitmap ToBitmap(this ColorImageFrame image, System.Drawing.Imaging.PixelFormat format)
        {
            if (image == null || image.PixelDataLength == 0)
                return null;
            var data = new byte[image.PixelDataLength];
            image.CopyPixelDataTo(data);
            return data.ToBitmap(image.Width, image.Height
                , format);
        }

        public static Bitmap ToBitmap(this DepthImageFrame image, System.Drawing.Imaging.PixelFormat format)
        {
            if (image == null || image.PixelDataLength == 0)
                return null;
            var data = new short[image.PixelDataLength];
            image.CopyPixelDataTo(data);
            return data.ToBitmap(image.Width, image.Height
                , format);
        }

        public static Bitmap ToBitmap(this ColorImageFrame image)
        {
            return image.ToBitmap(System.Drawing.Imaging.PixelFormat.Format32bppRgb);
        }

        public static Bitmap ToBitmap(this DepthImageFrame image)
        {
            return image.ToBitmap(System.Drawing.Imaging.PixelFormat.Format16bppRgb565);
        }

        // bitmapsource methods

        public static System.Windows.Media.Imaging.BitmapSource ToBitmapSource(this ColorImageFrame image)
        {
            if (image == null || image.PixelDataLength == 0)
                return null;
            var data = new byte[image.PixelDataLength];
            image.CopyPixelDataTo(data);
            return data.ToBitmapSource(System.Windows.Media.PixelFormats.Bgr32, image.Width, image.Height);
        }

        public static System.Windows.Media.Imaging.BitmapSource ToBitmapSource(this DepthImageFrame image)
        {
            if (image == null || image.PixelDataLength == 0)
                return null;
            var data = new short[image.PixelDataLength];
            image.CopyPixelDataTo(data);
            return data.ToBitmapSource(System.Windows.Media.PixelFormats.Bgr555, image.Width, image.Height);
        }

        public static System.Windows.Media.Imaging.BitmapSource ToTransparentBitmapSource(this byte[] data
            , int width, int height)
        {
            return data.ToBitmapSource(System.Windows.Media.PixelFormats.Bgra32, width, height);
        }
    }
}
