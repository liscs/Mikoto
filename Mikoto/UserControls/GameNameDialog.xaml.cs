using Mikoto.DataAccess;
using Mikoto.UserControls;
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
        private readonly GameNameDialogViewModel _viewModel;

        public GameNameDialog(MainWindow mainWindow, List<GameInfo> gameInfo, int id)
        {
            InitializeComponent();
            _viewModel = new GameNameDialogViewModel(gameInfo[id]);
            DataContext = _viewModel;
            _mainWindow = mainWindow;
            gid = id;
            gameInfolst = gameInfo;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _viewModel.SaveCommand.Execute(null);
        }
    }
}