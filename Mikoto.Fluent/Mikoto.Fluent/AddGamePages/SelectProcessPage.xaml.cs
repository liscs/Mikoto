using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Navigation;
using Mikoto.Core.ViewModels.AddGamePages;
using Mikoto.DataAccess;
using Mikoto.Helpers.Async;


namespace Mikoto.Fluent.AddGamePages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SelectProcessPage : BaseStepPage
{
    public SelectProcessViewModel ViewModel { get; } = App.Services.GetRequiredService<SelectProcessViewModel>();
    public SelectProcessPage()
    {
        InitializeComponent();
    }

    protected override bool SaveData(GameInfo config)
    {
        //获取到选择的进程路径
        if (ViewModel.SelectedProcess!=null)
        {
            string filePath = ViewModel.SelectedProcess.ImagePath;
            config.GameID = Guid.NewGuid();
            config.FilePath = filePath;
            config.GameName = Path.GetFileName(Path.GetDirectoryName(filePath))??Path.GetFileNameWithoutExtension(filePath);
            config.Isx64 = ProcessInterop.ProcessHelper.Is64BitProcess(ViewModel.SelectedProcess.Id);
            return true;
        }

        ViewModel.ShowError =true;
        return false;
    }



    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        // 页面进入时自动刷新列表
        ViewModel.RefreshProcessesAsync().FireAndForget();
    }
}
