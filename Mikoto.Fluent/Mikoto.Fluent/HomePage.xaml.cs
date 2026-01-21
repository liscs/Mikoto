using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Mikoto.Core;
using Mikoto.Core.Interfaces;
using Mikoto.Core.ViewModels;
using Windows.Storage.Streams;

namespace Mikoto.Fluent;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
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
        // 1. 获取原始数据 (Service 层)
        App.Services.GetRequiredService<IAppEnvironment>().GameInfoService.GetAllCompletedGames();
        var savedData = App.Services.GetRequiredService<IAppEnvironment>().GameInfoService.AllCompletedGamesIdDict.Values;
        // 2. 并行创建所有任务（此时图标提取已经开始并发执行）
        var loadTasks = savedData.Select(async data =>
        {
            // 这里的异步提取会在多个线程上并行执行
            var icon = await IconHelper.GetIconFromExeAsync(data.FilePath);

            return new GameModel
            {
                GameName = data.GameName,
                ExePath = data.FilePath,
                GameIcon = icon,
                LastPlayAt = data.LastPlayAt,
                Parent = ViewModel,
            };
        });

        // 3. 等待所有任务完成（等待最慢的那个图标提取完）
        var loadedModels = await Task.WhenAll(loadTasks);

        // 4. 一次性推送到 UI 集合中
        foreach (var model in loadedModels)
        {
            ViewModel.Games.Add(model);
        }
    }


    public async Task<BitmapImage?> ConvertBytesToImage(byte[]? bytes)
    {
        if (bytes == null || bytes.Length == 0) return null;

        var image = new BitmapImage();
        using (var stream = new InMemoryRandomAccessStream())
        {
            using (var writer = new DataWriter(stream.GetOutputStreamAt(0)))
            {
                writer.WriteBytes(bytes);
                await writer.StoreAsync();
            }
            await image.SetSourceAsync(stream);
        }
        return image;
    }
}
