using Mikoto.Helpers.Graphics;
using Mikoto.Helpers.ViewModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;

namespace Mikoto.Windows
{
    internal class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<Border> GamePanelCollection { get; set; } = new();

        private Visibility _LEEnabled;
        public Visibility LEEnabled
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

        private Effect? _gameCollectionEffect = null;
        public Effect? GameCollectionEffect { get => _gameCollectionEffect; set => SetProperty(ref _gameCollectionEffect, value); }

        BlurEffect _blur = new BlurEffect();

        private bool gameInfoDrawerIsOpen;
        public bool GameInfoDrawerIsOpen
        {
            get => gameInfoDrawerIsOpen; set
            {
                if (value)
                {
                    GameCollectionEffect = _blur;
                }
                else
                {
                    GameCollectionEffect = null;
                }
                SetProperty(ref gameInfoDrawerIsOpen, value);
            }
        }
    }

}