using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mikoto.Fluent.AddGamePages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AddGamePage : Page
{
    public AddGameViewModel ViewModel { get; } = new AddGameViewModel();

    public AddGamePage()
    {
        InitializeComponent();

        // 1. 订阅属性变化以处理后续导航
        ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.CurrentStepIndex))
            {
                NavigateWithTransition(true);
            }
        };

        // 2. 关键：初始化时加载第一步
        NavigateWithTransition(false);
    }

    private void NavigateWithTransition(bool useSlide)
    {
        var step = ViewModel.Steps[ViewModel.CurrentStepIndex];

        // 建议：传递 ViewModel 实例给子页面，实现数据共享
        NavigationTransitionInfo transition = useSlide
            ? new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight }
            : new EntranceNavigationTransitionInfo();

        ContentFrame.Navigate(step.PageType, ViewModel, transition);
    }
}