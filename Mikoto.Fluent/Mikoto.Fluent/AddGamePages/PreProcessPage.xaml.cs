using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Navigation;
using Mikoto.Core.Interfaces;
using Mikoto.Core.ViewModels.AddGame;
using Mikoto.DataAccess;
using Mikoto.Helpers.Async;



namespace Mikoto.Fluent.AddGamePages;

public sealed partial class PreProcessPage
{
    public PreProcessViewModel ViewModel { get; private set; } = default!;
    public PreProcessPage()
    {
        InitializeComponent();
    }

    protected override bool SaveData(GameInfo config)
    {
        config.RepairFunc= ViewModel.RepairFunctionViewModel.SelectedRepairFunction.MethodName;
        config.RepairParamA= ViewModel.ParamA;
        config.RepairParamB  = ViewModel.ParamB;
        return true;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel = App.Services.GetRequiredService<PreProcessViewModel>();
        ViewModel.StartHookingAsync(BaseViewModel.DraftConfig).FireAndForget();
    }


    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        App.Services.GetRequiredService<IAppEnvironment>().TextHookService.CloseTextractor();
    }
}
