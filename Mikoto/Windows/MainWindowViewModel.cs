using Mikoto.DataAccess;
using Mikoto.Helpers.ViewModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;

namespace Mikoto.Windows
{
    internal class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<Border> GamePanelCollection { get; set; } = new();

        private Visibility _leStartVisibility;
        public Visibility LEStartVisibility
        {
            get
            {
                return _leStartVisibility;
            }

            set
            {
                SetProperty(ref _leStartVisibility, value);
            }
        }

        private GameInfo gameInfo = new();
        public GameInfo GameInfo
        {
            get => gameInfo;
            set => SetProperty(ref gameInfo, value);
        }
        public void RefreshGameInfo() => RaisePropertyChanged(nameof(GameInfo));

        private Visibility gameInfoFileButtonVisibility = Visibility.Collapsed;

        public Visibility GameInfoFileButtonVisibility { get => gameInfoFileButtonVisibility; set => SetProperty(ref gameInfoFileButtonVisibility, value); }

        private Effect? _gameCollectionEffect = null;
        public Effect? GameCollectionEffect { get => _gameCollectionEffect; set => SetProperty(ref _gameCollectionEffect, value); }

        private readonly BlurEffect _blur = new();

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

        public ICommand CloseDrawer => new RelayCommand(() =>
        {
            GameInfoDrawerIsOpen = false;
        });

        private RelayCommand? searchCmd;
        public ICommand SearchCmd => searchCmd ??= new RelayCommand(PerformSearchCmd);
        private void PerformSearchCmd(object? commandParameter)
        {
            if (commandParameter is string searchText)
            {
                foreach (var border in GamePanelCollection)
                {
                    if (((GameInfo)border.Tag).GameName.Contains(searchText))
                    {
                        border.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        border.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
    }

}