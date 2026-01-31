using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Mikoto.Fluent.Converters
{
    public partial class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // 如果 VndbId 不为空，则显示；否则隐藏
            return (value is string s && !string.IsNullOrWhiteSpace(s))
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
