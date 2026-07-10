using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Mikoto.Core.ViewModels;

namespace Mikoto.Fluent;

public sealed partial class HomePage : Page
{
    HomeViewModel ViewModel = App.Services.GetRequiredService<HomeViewModel>();

    public HomePage()
    {
        InitializeComponent();
    }

    // 2. 当页面加载完成后触发
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // 防止重复加载
        if (ViewModel.Games.Count > 0) return;

        // 3. 执行异步加载
        await LoadGamesAsync();
    }

    private async Task LoadGamesAsync()
    {
        await ViewModel.LoadGamesCommand.ExecuteAsync(async path =>
        {
            // 这里返回的是 Task<BitmapImage>
            var image = await IconHelper.GetIconFromExeAsync(path);

            // 关键点：将结果显式转为 object? 以匹配 Task<object?>
            return (object?)image;
        });
    }

}
