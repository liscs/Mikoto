using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace MisakaTranslator.Helpers
{
    public static class ImageHelper
    {
        private static PixelColor[,] GetPixels(BitmapSource? source)
        {
            if (source == null || source.Format != PixelFormats.Bgra32)
            {
                source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
            }

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            PixelColor[,] result = new PixelColor[width, height];

            source.CopyPixelColors(result, width * 4, 0);
            return result;
        }
        private static PixelColor[,] GetPixels(System.Drawing.Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            PixelColor[,] result = new PixelColor[width, height];
            System.Drawing.Imaging.BitmapData bmpData =
            bmp.LockBits(new System.Drawing.Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadWrite,
            bmp.PixelFormat);

            var length = bmpData.Stride * bmpData.Height;
            byte[] bytes = new byte[length];
            Marshal.Copy(bmpData.Scan0, bytes, 0, length);
            bmp.UnlockBits(bmpData);


            int k = 0;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    result[i, j].ColorBGRA = BitConverter.ToUInt32(bytes, k);
                    k += 4;
                }
            }
            return result;
        }
        public static Image GetGameIcon(string path)
        {
            Image ico = new()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(43, 0, 43, 0),
            };

            if (!File.Exists(path))
            {
                return ico;
            }

            BitmapImage? bitmapImage = ImageHelper.ImageToBitmapImage(GetAppIcon(path)!);
            ico.Source = bitmapImage;
            string[] icoPaths = Directory.GetFiles(Path.GetDirectoryName(path)!, "*icon.ico");
            if (icoPaths.Length > 0)
            {
                bitmapImage = new BitmapImage(new Uri(icoPaths[0]));
                ico.Source = bitmapImage;
            }
            return ico;
        }

        public static System.Drawing.Bitmap GetGameDrawingBitmapIcon(string path)
        {
            System.Drawing.Bitmap ico = new(64, 64);

            if (!File.Exists(path))
            {
                return ico;
            }

            ico = GetAppIcon(path) ?? ico;

            string[] icoPaths = Directory.GetFiles(Path.GetDirectoryName(path)!, "*icon.ico");
            if (icoPaths.Length > 0)
            {
                ico = new System.Drawing.Bitmap(icoPaths[0]);
            }
            return ico;
        }

        public static Brush GetMajorBrush(BitmapSource? bitmapSource)
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
                if (hue == 0)
                {
                    continue;
                }

                if (dict.TryGetValue(hue, out _))
                {
                    dict[hue]++;
                }
                else
                {
                    dict.Add(hue, 1);
                }
            }
            IOrderedEnumerable<KeyValuePair<int, int>> sortedDict = from entry in dict orderby entry.Value descending select entry;

            if (!sortedDict.Any())
            {
                //纯黑白图标
                LinearGradientBrush result = new()
                {
                    StartPoint = new Point(0, 1),
                    EndPoint = new Point(1, 0),
                };
                result.GradientStops.Add(new GradientStop(Color.FromRgb(255, 255, 255), 0.0));
                result.GradientStops.Add(new GradientStop(Color.FromRgb(200, 200, 200), 1.0));
                return result;
            }
            var majorColors = sortedDict.First().Key;


            switch (Common.CurrentTheme)
            {
                case Theme.Light:
                    {
                        LinearGradientBrush result = new()
                        {
                            StartPoint = new Point(0, 1),
                            EndPoint = new Point(1, 0),
                        };
                        result.GradientStops.Add(new GradientStop(ColorFromHSV(majorColors - 20, 0.6, 1), 0.0));
                        result.GradientStops.Add(new GradientStop(ColorFromHSV(majorColors, 1, 1), 1.0));
                        return result;
                    }
                default:
                    {
                        SolidColorBrush result = new(Color.FromRgb(46, 46, 46));
                        return result;
                    }
            }

        }

        private static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value *= 255;
            byte v = Convert.ToByte(value);
            byte p = Convert.ToByte(value * (1 - saturation));
            byte q = Convert.ToByte(value * (1 - f * saturation));
            byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            return hi switch
            {
                0 => Color.FromArgb(255, v, t, p),
                1 => Color.FromArgb(255, q, v, p),
                2 => Color.FromArgb(255, p, v, t),
                3 => Color.FromArgb(255, p, q, v),
                4 => Color.FromArgb(255, t, p, v),
                _ => Color.FromArgb(255, v, p, q)
            };
        }

        public static BitmapImage? GetBlurImage(System.Drawing.Bitmap bitmap)
        {
            PixelColor[,] pixels = GetPixels(bitmap);
            pixels = Normalize(pixels);
            pixels = CropRorate(pixels);
            var colors = pixels.Cast<PixelColor>().ToArray();
            GaussianBlur gaussianBlur = new(colors);
            return ImageHelper.ImageToBitmapImage(gaussianBlur.Process(3));

        }

        private static PixelColor[,] Normalize(PixelColor[,] origin)
        {
            const int NORMAL_LENGTH = 32;
            int width = origin.GetLength(0);
            double scale = NORMAL_LENGTH / (double)width;
            PixelColor[,] result = new PixelColor[NORMAL_LENGTH, NORMAL_LENGTH];
            for (int i = 0; i < NORMAL_LENGTH; i++)
            {
                for (int j = 0; j < NORMAL_LENGTH; j++)
                {
                    result[i, j] = origin[(int)(i / scale), (int)(j / scale)];
                }
            }
            return result;
        }

        private static double Next(this Random random, double minimum, double maximum)
        {
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        private static PixelColor[,] CropRorate(PixelColor[,] pixels)
        {
            int width = pixels.GetLength(0);
            Random random = new Random();
            double angle = random.Next(-Math.PI / 2, Math.PI / 2);

            PixelColor[,] crop1 = Rorate(pixels, width, angle);
            angle = random.Next(-Math.PI / 2, Math.PI / 2);
            PixelColor[,] crop2 = Rorate(pixels, width, angle);

            PixelColor[,] merge1 = Merge(pixels, width, crop1, crop2);
            angle = random.Next(-Math.PI / 2, Math.PI / 2);
            PixelColor[,] merge2 = Rorate(merge1, width, angle);
            merge2 = Mirror(merge2, width);

            PixelColor[,] result = Merge(pixels, width, merge1, merge2);
            return result;

        }

        private static PixelColor[,] Mirror(PixelColor[,] image, int length)
        {
            PixelColor[,] result = new PixelColor[length, length];
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    result[i, j] = image[i, length - j - 1];
                }
            }
            return result;
        }

        private static PixelColor[,] Merge(PixelColor[,] baseImage, int length, params object[] images)
        {
            PixelColor[,] result = (PixelColor[,])baseImage.Clone();
            foreach (var image in images)
            {
                for (int i = 0; i < length; i++)
                {
                    for (int j = 0; j < length; j++)
                    {
                        if (((PixelColor[,])image)[i, j].ColorBGRA != 0 && ((PixelColor[,])image)[i, j].ColorBGRA != 0xFFFFFFFF)
                        {
                            result[i, j] = ((PixelColor[,])image)[i, j];
                        }
                    }
                }
            }
            return result;
        }

        private static PixelColor[,] Rorate(PixelColor[,] pixels, int length, double angle)
        {
            PixelColor[,] crop = new PixelColor[length, length];
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    var x = (int)(i * Math.Cos(angle) - j * Math.Sin(angle));
                    var y = (int)(j * Math.Cos(angle) + i * Math.Sin(angle));
                    if (x > 0 && y > 0 && x < length && y < length)
                    {
                        crop[i, j] = pixels[x, y];
                    }
                }
            }
            return crop;
        }

        //Since BitmapSource.CopyPixels doesn't accept a two-dimensional array it is
        //necessary to convert the array between one-dimensional and two-dimensional.
        private static unsafe void CopyPixelColors(this BitmapSource source, PixelColor[,] pixels, int stride, int offset)
        {
            fixed (PixelColor* buffer = &pixels[0, 0])
            {
                source.CopyPixels(
                  new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight),
                  (IntPtr)(buffer + offset),
                  pixels.GetLength(0) * pixels.GetLength(1) * sizeof(PixelColor),
                  stride);
            }
        }

        /// <summary>
        /// 获取文件图标
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        private static unsafe System.Drawing.Bitmap? GetAppIcon(string filepath)
        {
            //选中文件中的图标总数
            var iconTotalCount = PInvoke.PrivateExtractIcons(filepath, 0, 0, 0, null, null, 0, 0);

            //用于接收获取到的图标指针
            Span<HICON> hIcons = stackalloc HICON[(int)iconTotalCount];

            uint result = 0xFFFFFFFF;
            fixed (HICON* p = hIcons)
            {
                //成功获取到的图标个数
                result = PInvoke.PrivateExtractIcons(filepath, 0, 256, 256, p, null, iconTotalCount, (uint)(IMAGE_FLAGS.LR_DEFAULTCOLOR));
            }

            System.Drawing.Bitmap? myIcon = null;
            if (result > 0 && result != 0xFFFFFFFF)
            {
                using System.Drawing.Icon ico = System.Drawing.Icon.FromHandle(hIcons[0]);
                myIcon = ico.ToBitmap();
            }

            //遍历并保存图标
            for (var i = 0; i < result; i++)
            {
                //指针为空，跳过
                if (hIcons[i] == HICON.Null) continue;
                //内存回收
                PInvoke.DestroyIcon(hIcons[i]);
            }

            return myIcon;
        }
        /// <summary>
        /// 将System.Drawing.Image转换成System.Windows.Media.Imaging.BitmapImage
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private static BitmapImage? ImageToBitmapImage(System.Drawing.Image bitmap)
        {
            if (bitmap == null)
            {
                return null;
            }

            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png); //格式选Bmp时，不带透明度

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                // Force the bitmap to load right now so we can dispose the stream.
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }

        }
    }


}
