using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Navigation;
using Mikoto.Core.Interfaces;
using Mikoto.Core.ViewModels.AddGame;
using Mikoto.DataAccess;
using Mikoto.Helpers.Async;


namespace Mikoto.Fluent.AddGamePages
{
    public sealed partial class HookSettingsPage
    {
        public HookSettingsViewModel ViewModel { get; private set; } = default!;
        public HookSettingsPage()
        {
            InitializeComponent();
        }

        protected override bool SaveData(GameInfo config)
        {
            if (ViewModel.SelectedFunction != null)
            {
                config.HookCode = ViewModel.SelectedFunction.HookCode;
                config.MisakaHookCode = ViewModel.SelectedFunction.MisakaHookCode;
                return true;
            }

            ViewModel.ShowError =true;
            return false;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel = App.Services.GetRequiredService<HookSettingsViewModel>();
            ViewModel.StartHookingAsync(BaseViewModel.DraftConfig).FireAndForget();
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            App.Services.GetRequiredService<IAppEnvironment>().TextHookService.CloseTextractor();
        }
    }
}
