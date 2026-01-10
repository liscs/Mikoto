using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mikoto.Windows
{
    public class IntToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int weight)
            {
                return FontWeight.FromOpenTypeWeight(weight);
            }
            return FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FontWeight fw)
            {
                return fw.ToOpenTypeWeight();
            }

            return FontWeights.Normal.ToOpenTypeWeight();
        }
    }

}
