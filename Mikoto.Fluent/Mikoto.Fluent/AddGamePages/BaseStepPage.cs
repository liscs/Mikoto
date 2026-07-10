using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Mikoto.Core.ViewModels.AddGame;
using Mikoto.DataAccess;

namespace Mikoto.Fluent.AddGamePages;

public abstract partial class BaseStepPage : Page
{
    public AddGameViewModel BaseViewModel { get; private set; } = default!;

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is AddGameViewModel vm)
        {
            this.BaseViewModel = vm;
        }

        WeakReferenceMessenger.Default.Register<RequestSaveDataMessage>(this, (r, m) =>
        {
            m.OnResult?.Invoke(SaveData(m.Config));
        });
    }



    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);

        // 离开页面时统一注销，防止内存泄漏和消息重复触发
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    // 实现接口：子类实现具体的保存逻辑
    protected abstract bool SaveData(GameInfo config);
}
