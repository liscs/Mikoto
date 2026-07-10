using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Mikoto.Core.ViewModels;


namespace Mikoto.Fluent
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; } = App.Services.GetRequiredService<SettingsViewModel>();
        public SettingsPage()
        {
            InitializeComponent();
        }
    }
}
