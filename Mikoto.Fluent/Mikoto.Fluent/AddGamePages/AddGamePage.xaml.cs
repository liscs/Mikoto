using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Mikoto.Core.ViewModels.AddGame;

namespace Mikoto.Fluent.AddGamePages;

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

        NavigateWithTransition(false);
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

}