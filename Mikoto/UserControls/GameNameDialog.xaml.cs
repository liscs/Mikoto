using Mikoto.DataAccess;
using Mikoto.UserControls;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Mikoto
{
    /// <summary>
    /// GameNameDialog.xaml 的交互逻辑
    /// </summary>
    public partial class GameNameDialog : UserControl
    {
        private readonly GameNameDialogViewModel _viewModel;

        public GameNameDialog(GameInfo gameInfo)
        {
            InitializeComponent();
            _viewModel = new GameNameDialogViewModel(gameInfo);
            DataContext = _viewModel;
        }

        private async void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _viewModel.SaveCommand.Execute(null);
            await MainWindow.Instance.RefreshAsync();
        }
    }
}