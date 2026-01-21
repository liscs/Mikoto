using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Mikoto.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mikoto.Fluent.Converters
{
    public partial class NotifySeverityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // 强制将 int 转为 WinUI 的枚举
            if (value is InfoSeverity intValue)
            {
                return (InfoBarSeverity)intValue;
            }
            return InfoBarSeverity.Informational;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
