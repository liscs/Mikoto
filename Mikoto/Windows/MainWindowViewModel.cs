using Mikoto.Helpers.ViewModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Mikoto.Windows
{
    internal class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<Border> GamePanelCollection { get; set; } = new();

        private System.Windows.Visibility _LEEnabled;
        public System.Windows.Visibility LEEnabled
        {
            get
            {
                return _LEEnabled;
            }

            set
            {
                SetProperty(ref _LEEnabled, value);
            }
        }

        private string lastStartTime = string.Empty;

        public string LastStartTime { get => lastStartTime; set => SetProperty(ref lastStartTime, value); }

        private Visibility gameInfoFileButtonVisibility = Visibility.Collapsed;

        public Visibility GameInfoFileButtonVisibility { get => gameInfoFileButtonVisibility; set => SetProperty(ref gameInfoFileButtonVisibility, value); }
    }

}