using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mikoto.Fluent.AddGamePages
{
    public partial class BaseStepPage : Page
    {
        public AddGameViewModel ViewModel { get; private set; } = default!;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is AddGameViewModel vm)
            {
                this.ViewModel = vm;
            }
        }
    }
}
