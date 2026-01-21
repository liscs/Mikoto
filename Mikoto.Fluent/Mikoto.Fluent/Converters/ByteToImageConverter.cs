using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Storage.Streams;

namespace Mikoto.Fluent.Converters
{
    public partial class ByteToImageConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is byte[] bytes && bytes.Length > 0)
            {
                var image = new BitmapImage();
                using var stream = new InMemoryRandomAccessStream();
                // 使用扩展方法直接同步写入（UI 线程安全）
                stream.WriteAsync(bytes.AsBuffer()).AsTask().Wait();
                stream.Seek(0);
                image.SetSource(stream);
                return image;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
