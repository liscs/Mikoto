using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mikoto.DataAccess;
using Mikoto.Fluent.AddGamePages;
using System.Collections.ObjectModel;
namespace Mikoto.Fluent
{
    public partial class HomeViewModel : ObservableObject
    {
        public ObservableCollection<GameModel> Games { get; } = new();

        [RelayCommand]
        private void AddGame()
        {
            // 收件处：MainWindow.xaml.cs (用于切换内容 Frame)
            WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(AddGamePage)));
        }
    }
}
