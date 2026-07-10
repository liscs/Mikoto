using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Mikoto.Fluent.Converters
{
    public partial class StringToImageSourceConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string url && !string.IsNullOrWhiteSpace(url))
            {
                try
                {
                    // 只有合法的 URI 才会返回 BitmapImage
                    return new BitmapImage(new Uri(url));
                }
                catch
                {
                    return null;
                }
            }
            // 如果是 null 或 empty，直接返回 null
            // 这样生成的代码会直接 Set_Source(null)，不会报错
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
