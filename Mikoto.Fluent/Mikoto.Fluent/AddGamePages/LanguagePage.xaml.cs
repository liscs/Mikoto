using Mikoto.Core.ViewModels.AddGamePages;
using Mikoto.DataAccess;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Mikoto.Fluent.AddGamePages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
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
