using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Mikoto.Core.ViewModels.TranslatorSetting;



namespace Mikoto.Fluent.TranslatorSettingPages;


public sealed partial class TranslatorSettingPage : Page
{
    TranslatorSettingViewModel ViewModel { get; set; } = App.Services.GetRequiredService<TranslatorSettingViewModel>();
    public TranslatorSettingPage()
    {
        InitializeComponent();
    }

    private void Expander_Expanding(Expander sender, ExpanderExpandingEventArgs args)
    {
        if (sender is FrameworkElement fe && fe.Tag is string providerName)
        {
            // 这里的 ViewModel 是你 Page 的 DataContext
            if (ViewModel.InitProviderCommand.CanExecute(providerName))
            {
                ViewModel.InitProviderCommand.Execute(providerName);
            }
        }
    }
}
