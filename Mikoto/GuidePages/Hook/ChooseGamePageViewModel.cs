using Mikoto.Helpers.ViewModel;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace Mikoto.GuidePages.Hook
{
    internal class ChooseGamePageViewModel : ViewModelBase
    {
        private string _focusingProcess = string.Empty;

        public string FocusingProcess
        {
            get
            {
                return _focusingProcess;
            }
            set
            {
                SetProperty(ref _focusingProcess, value);
            }
        }

        private bool enableSelectFocusButton = true;

        public bool EnableSelectFocusButton { get => enableSelectFocusButton; set => SetProperty(ref enableSelectFocusButton, value); }


        public ObservableCollection<ProcessItem> ProcessList { get; set; } = new();

        private ProcessItem selectedProcess = new();

        public ProcessItem SelectedProcess { get => selectedProcess; set => SetProperty(ref selectedProcess, value); }

    }

    public class ProcessItem
    {
        public string DisplayName { get; set; } = string.Empty;
        public BitmapSource? Icon { get; set; }  // 绑定图标
        public int PID { get; set; } = -1;
    }
}