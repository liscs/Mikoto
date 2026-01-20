using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Mikoto.DataAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

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
