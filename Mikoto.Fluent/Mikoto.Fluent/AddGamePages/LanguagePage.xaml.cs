using Mikoto.Core.ViewModels.AddGame;
using Mikoto.DataAccess;



namespace Mikoto.Fluent.AddGamePages
{
    public sealed partial class LanguagePage
    {
        public LanguageViewModel ViewModel { get; } = new LanguageViewModel();
        public LanguagePage()
        {
            InitializeComponent();
        }

        protected override bool SaveData(GameInfo config)
        {
            config.SrcLang = ViewModel.SelectedSourceLanguage.LanguageCode;
            config.DstLang = ViewModel.SelectedTargetLanguage.LanguageCode;
            return true;
        }
    }
}
