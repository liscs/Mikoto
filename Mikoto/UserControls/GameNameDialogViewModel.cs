using Mikoto.DataAccess;
using Mikoto.Helpers.ViewModel;
using Mikoto.Translators;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Mikoto.UserControls
{
    internal class GameNameDialogViewModel : ViewModelBase
    {
        private readonly GameInfo _game;
        public GameNameDialogViewModel(GameInfo gameInfo)
        {
            SaveCommand = new RelayCommand(Save);

            _game = gameInfo;
            GameName = gameInfo.GameName;
            FilePath = gameInfo.FilePath;
            HookCode = gameInfo.HookCode;
            MisakaHookCode = gameInfo.MisakaHookCode;
            IsX64 = gameInfo.Isx64;
            RepairParamA = gameInfo.RepairParamA ?? string.Empty;
            RepairParamB = gameInfo.RepairParamB ?? string.Empty;

            SrcLang = gameInfo.SrcLang;
            DstLang = gameInfo.DstLang;
            SelectedRepairFunc = gameInfo.RepairFunc?? string.Empty;
        }



        // Data Bind Properties
        public string GameName { get => _gameName; set => SetProperty(ref _gameName, value); }
        private string _gameName = string.Empty;

        public string FilePath { get => _filePath; set => SetProperty(ref _filePath, value); }
        private string _filePath = string.Empty;

        public string HookCode { get => _hookCode; set => SetProperty(ref _hookCode, value); }
        private string _hookCode = string.Empty;

        public string MisakaHookCode { get => _misakaHookCode; set => SetProperty(ref _misakaHookCode, value); }
        private string _misakaHookCode = string.Empty;

        public bool IsX64 { get => _isX64; set => SetProperty(ref _isX64, value); }
        private bool _isX64;

        public string RepairParamA { get => _repairParamA; set => SetProperty(ref _repairParamA, value); }
        private string _repairParamA = string.Empty;

        public string RepairParamB { get => _repairParamB; set => SetProperty(ref _repairParamB, value); }
        private string _repairParamB = string.Empty;

        public ObservableCollection<string> LangList { get; } = new(TranslatorCommon.LanguageDict.Values.ToList());
        public string DstLang { get => _dstLang; set => SetProperty(ref _dstLang, value); }
        private string _dstLang = string.Empty;

        public string SrcLang { get => _srcLang; set => SetProperty(ref _srcLang, value); }
        private string _srcLang = string.Empty;

        public ObservableCollection<string> RepairFuncList { get; } = new(TextRepair.DisplayNameToNameDict.Value.Values.ToList());
        public string SelectedRepairFunc { get => _selectedRepairFunc; set => SetProperty(ref _selectedRepairFunc, value); }
        private string _selectedRepairFunc = string.Empty;

        public ICommand SaveCommand { get; }

        private void Save()
        {
            _game.GameName = GameName;
            _game.FilePath = FilePath;
            _game.DstLang = DstLang;
            _game.SrcLang = SrcLang;
            _game.HookCode = HookCode;
            _game.MisakaHookCode = MisakaHookCode;
            _game.Isx64 = IsX64;
            _game.RepairFunc = SelectedRepairFunc;
            _game.RepairParamA = RepairParamA;
            _game.RepairParamB = RepairParamB;

            App.Env.GameInfoService.SaveGameInfo(_game);
            MainWindow.Instance.SetGameInfoModel(_game);
        }
    }

}
