using OCRLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MisakaTranslator.Utils
{

    [StructLayout(LayoutKind.Explicit)]
    public struct PixelColor
    {
        // 32 bit BGRA 
        [FieldOffset(0)] public UInt32 ColorBGRA;
        // 8 bit components
        [FieldOffset(0)] public byte Blue;
        [FieldOffset(1)] public byte Green;
        [FieldOffset(2)] public byte Red;
        [FieldOffset(3)] public byte Alpha;
    }

    public static class ImageHelper
    {
        public static PixelColor[,] GetPixels(BitmapSource source)
        {
            if (source.Format != PixelFormats.Bgra32)
                source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            PixelColor[,] result = new PixelColor[width, height];

            source.CopyPixelColors(result, width * 4, 0);
            return result;
        }

        public static Image GetGameIcon(string path)
        {
            Image ico = new()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Height = 64,
                Width = 64,
            };

            if (!File.Exists(path))
            {
                return ico;
            }

            BitmapImage? bitmapImage = ImageProcFunc.ImageToBitmapImage(ImageProcFunc.GetAppIcon(path)!);
            ico.Source = bitmapImage;
            string[] icoPaths = Directory.GetFiles(Path.GetDirectoryName(path)!, "*.ico");
            if (icoPaths.Length > 0 && ico.Source is null)
            {
                bitmapImage = new BitmapImage(new Uri(icoPaths[0]));
                ico.Source = bitmapImage;
            }
            return ico;
        }

        public static SolidColorBrush GetMajorBrush(BitmapSource? bitmapSource)
        {
            if (bitmapSource == null)
            {
                return new SolidColorBrush(Color.FromRgb(51, 51, 51));
            }

            PixelColor[,] pixels = GetPixels(bitmapSource);
            Dictionary<int, int> dict = new();
            foreach (PixelColor pixelColor in pixels)
            {
                int hue = (int)System.Drawing.Color.FromArgb(pixelColor.Alpha, pixelColor.Red, pixelColor.Green, pixelColor.Blue).GetHue();
                //跳过透明
                if (hue == 0) continue;
                if (dict.TryGetValue(hue, out _))
                    dict[hue]++;
                else
                {
                    dict.Add(hue, 1);
                }
            }
            IOrderedEnumerable<KeyValuePair<int, int>> sortedDict = from entry in dict orderby entry.Value descending select entry;
            return new SolidColorBrush(ColorFromHSV(sortedDict.First().Key, 1, 1));
        }

        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value *= 255;
            byte v = Convert.ToByte(value);
            byte p = Convert.ToByte(value * (1 - saturation));
            byte q = Convert.ToByte(value * (1 - f * saturation));
            byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

    }

    //Since BitmapSource.CopyPixels doesn't accept a two-dimensional array it is
    //necessary to convert the array between one-dimensional and two-dimensional.
    public static class BitmapSourceExtension
    {
        public unsafe static void CopyPixelColors(this BitmapSource source, PixelColor[,] pixels, int stride, int offset)
        {
            fixed (PixelColor* buffer = &pixels[0, 0])
                source.CopyPixels(
                  new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight),
                  (IntPtr)(buffer + offset),
                  pixels.GetLength(0) * pixels.GetLength(1) * sizeof(PixelColor),
                  stride);
        }
    }
}
