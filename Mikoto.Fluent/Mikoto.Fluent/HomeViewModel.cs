using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mikoto.DataAccess;
using Mikoto.Fluent.AddGamePages;
using Mikoto.ProcessInterop;
using System.Collections.ObjectModel;
using System.Diagnostics;
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


        [RelayCommand]
        private void StartGame(GameModel game) // 添加参数
        {
            if (game == null) return;

            string hookPath = game.ExePath; // 直接从参数获取，不再依赖 SelectedGame

            if (Path.GetExtension(hookPath) == ".log")
            {
                hookPath = Path.ChangeExtension(hookPath, ".exe");
            }

            Process.Start(hookPath);
        }
    }
}
