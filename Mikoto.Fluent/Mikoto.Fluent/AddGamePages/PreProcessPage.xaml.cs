using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Mikoto.DataAccess;
using Mikoto.Helpers.Async;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;



namespace Mikoto.Fluent.AddGamePages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PreProcessPage
    {
        public PreProcessViewModel ViewModel { get; private set; } = default!;
        public PreProcessPage()
        {
            InitializeComponent();
        }

        protected override bool SaveData(GameInfo config)
        {
            return true;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel = new PreProcessViewModel();
            ViewModel.StartHookingAsync(BaseViewModel.DraftConfig).FireAndForget();
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            App.Env.TextHookService.CloseTextractor();
        }
    }
}
