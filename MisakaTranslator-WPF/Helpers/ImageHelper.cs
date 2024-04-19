using OCRLibrary;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MisakaTranslator.Helpers
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
            string[] icoPaths = Directory.GetFiles(Path.GetDirectoryName(path)!, "*icon.ico");
            if (icoPaths.Length > 0)
            {
                bitmapImage = new BitmapImage(new Uri(icoPaths[0]));
                ico.Source = bitmapImage;
            }
            return ico;
        }


        public static Brush GetMajorBrush(BitmapSource? bitmapSource, Theme theme = Theme.Light)
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
                return new SolidColorBrush(Color.FromRgb(51, 51, 51));
            }
            var majorColors = sortedDict.ElementAt(0).Key;


            switch (Application.Current.Resources.MergedDictionaries[4].Source.OriginalString)
            {
                case "Themes/LightTheme.xaml":
                    {
                        LinearGradientBrush result = new()
                        {
                            StartPoint = new Point(0, 1),
                            EndPoint = new Point(1, 0)
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

        public static Brush GetBlurBrush(BitmapSource? bitmapSource)
        {
            PixelColor[,] pixels = GetPixels(bitmapSource);
            pixels = Normalize(pixels);
            pixels = CropRorate(pixels);
            var colors = pixels.Cast<PixelColor>().ToArray();
            GaussianBlur gaussianBlur = new(colors);
            return new ImageBrush(ImageProcFunc.ImageToBitmapImage(gaussianBlur.Process(3)));

        }

        private static PixelColor[,] Normalize(PixelColor[,] origin)
        {
            const int NORMAL_WIDTH = 32;
            int width = origin.GetLength(0);
            double scale = NORMAL_WIDTH / (double)width;
            PixelColor[,] result = new PixelColor[NORMAL_WIDTH, NORMAL_WIDTH];

            Parallel.For(0, NORMAL_WIDTH, i =>
            {
                for (int j = 0; j < NORMAL_WIDTH; j++)
                {
                    result[i, j] = origin[(int)(i / scale), (int)(j / scale)];
                }
            });
            return result;
        }

        public static double Next(this Random random, double minimum, double maximum)
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

        private static PixelColor[,] Mirror(PixelColor[,] image, int width)
        {
            PixelColor[,] result = new PixelColor[width, width];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    result[i, j] = image[i, width - j - 1];
                }
            }
            return result;
        }

        private static PixelColor[,] Merge(PixelColor[,] baseImage, int width, params object[] images)
        {
            PixelColor[,] result = (PixelColor[,])baseImage.Clone();
            Parallel.ForEach(images, image =>
            {
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        if (((PixelColor[,])image)[i, j].ColorBGRA != 0 && ((PixelColor[,])image)[i, j].ColorBGRA != 0xFFFFFFFF)
                        {
                            result[i, j] = ((PixelColor[,])image)[i, j];
                        }
                    }
                }
            });
            return result;
        }

        private static PixelColor[,] Rorate(PixelColor[,] pixels, int width, double angle)
        {
            PixelColor[,] crop = new PixelColor[width, width];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    var x = (int)(i * Math.Cos(angle) - j * Math.Sin(angle));
                    var y = (int)(j * Math.Cos(angle) + i * Math.Sin(angle));
                    if (x > 0 && y > 0 && x < width && y < width)
                    {
                        crop[i, j] = pixels[x, y];
                    }
                }
            }
            return crop;
        }

        //Since BitmapSource.CopyPixels doesn't accept a two-dimensional array it is
        //necessary to convert the array between one-dimensional and two-dimensional.
        public static unsafe void CopyPixelColors(this BitmapSource source, PixelColor[,] pixels, int stride, int offset)
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
    }


}
