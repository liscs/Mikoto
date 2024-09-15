using Mikoto.DataAccess;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Mikoto
{
    /// <summary>
    /// GameNameDialog.xaml 的交互逻辑
    /// </summary>
    public partial class GameNameDialog : UserControl
    {
        private readonly List<GameInfo> gameInfolst;
        private readonly int gid; //当前选中的顺序，并非游戏ID
        private readonly MainWindow _mainWindow;

        public GameNameDialog(MainWindow mainWindow, List<GameInfo> gameInfo, int id)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            gameInfolst = gameInfo;
            gid = id;
            NameBox.Text = gameInfolst[gid].GameName;
            PathBox.Text = gameInfolst[gid].FilePath;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bool needRefresh = false;
            string newName = NameBox.Text;
            if (!string.IsNullOrWhiteSpace(newName) && newName != gameInfolst[gid].GameName)
            {
                GameHelper.UpdateGameInfoByID(gameInfolst[gid].GameID, nameof(GameInfo.GameName), newName);
                needRefresh = true;
            }
            string newPath = PathBox.Text;
            if (File.Exists(newPath) && newPath != gameInfolst[gid].FilePath)
            {
                GameHelper.UpdateGameInfoByID(gameInfolst[gid].GameID, nameof(GameInfo.FilePath), newPath);
                needRefresh = true;
            }
            if (needRefresh)
            {
                _mainWindow.Refresh();
            }
        }
    }
}