using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Mikoto.Core.Models.Setting;
using Mikoto.Core.ViewModels.TranslatorSetting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mikoto.Fluent.TranslatorSettingPages
{
    public sealed partial class TranslatorConfigControl : UserControl
    {
        // 提供给 XAML x:Bind 使用的强类型属性
        public CommonTranslatorSettingsViewModel? ViewModel => DataContext as CommonTranslatorSettingsViewModel;

        public TranslatorConfigControl()
        {
            this.InitializeComponent();

            // 重要：当 DataContext 变化时（比如从外部赋值），通知 XAML 刷新绑定
            this.DataContextChanged += (s, e) =>
            {
                this.Bindings.Update();
            };
        }

        private void TransTestBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LangCodeBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DocBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BillingBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
