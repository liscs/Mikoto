using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Mikoto.Core.ViewModels;
using Mikoto.Core.ViewModels.AddGame;


namespace Mikoto.Fluent
{
    public sealed partial class EditGamePage : Page
    {
        EditGameViewModel ViewModel = App.Services.GetRequiredService<EditGameViewModel>();
        public EditGamePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is GameItemViewModel vm)
            {
                ViewModel.GameItem = vm;
                ViewModel.RepairFunctionViewModel = App.Services.GetRequiredService<RepairFunctionViewModel>();
                ViewModel.RepairFunctionViewModel.SelectedRepairFunction = ViewModel.RepairFunctionViewModel.FunctionList.FirstOrDefault(x => x.MethodName == vm.GameInfo.RepairFunc)??ViewModel.RepairFunctionViewModel.FunctionList.First();
                ViewModel.LanguageViewModel = new LanguageViewModel();
                ViewModel.LanguageViewModel.SelectedSourceLanguage = ViewModel.LanguageViewModel.LangList.FirstOrDefault(x => x.LanguageCode == vm.GameInfo.SrcLang)??ViewModel.LanguageViewModel.LangList.First();
                ViewModel.LanguageViewModel.SelectedTargetLanguage = ViewModel.LanguageViewModel.LangList.FirstOrDefault(x => x.LanguageCode == vm.GameInfo.DstLang)??ViewModel.LanguageViewModel.LangList.First();

            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
