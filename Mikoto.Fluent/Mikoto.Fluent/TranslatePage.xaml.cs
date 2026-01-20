using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Mikoto.DataAccess;



namespace Mikoto.Fluent;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class TranslatePage : Page
{
    public TranslateViewModel ViewModel { get; } = new();

    public TranslatePage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // 1. 这里的 e.Parameter 是从 HomeViewModel 发送过来的 GameModel
        if (e.Parameter is GameInfo game)
        {
            // 2. 将数据交给 ViewModel 处理
            ViewModel.CurrentGame = game;

            ViewModel.InitializeTranslationCommand.Execute(null);
        }
    }
}
