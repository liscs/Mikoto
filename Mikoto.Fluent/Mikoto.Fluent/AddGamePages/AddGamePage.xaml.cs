using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Mikoto.Core.ViewModels.AddGamePages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mikoto.Fluent.AddGamePages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AddGamePage : Page
{
    public AddGameViewModel ViewModel { get; } = App.Services.GetRequiredService<AddGameViewModel>();

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


    }


    private int _lastStepIndex;
    private void NavigateWithTransition(bool useSlide)
    {
        var currentIndex = ViewModel.CurrentStepIndex;
        var step = ViewModel.Steps[currentIndex];

        NavigationTransitionInfo transition;

        if (!useSlide)
        {
            transition = new EntranceNavigationTransitionInfo();
        }
        else
        {
            // 动态判断方向：如果当前索引大于上次，说明是向后走
            var effect = currentIndex > _lastStepIndex
                ? SlideNavigationTransitionEffect.FromRight
                : SlideNavigationTransitionEffect.FromLeft;

            transition = new SlideNavigationTransitionInfo { Effect = effect };
        }

        _lastStepIndex = currentIndex;

        ContentFrame.Navigate(App.GetViewFromViewModel(step.ViewModelType), ViewModel, transition);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        // 2. 关键：初始化时加载第一步
        NavigateWithTransition(false);
    }
}