using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mikoto.Fluent.AddGamePages
{
    public partial class HookFuncItem : ObservableObject
    {
        public int GamePID { get; set; }
        public string HookFunc { get; set; } = string.Empty;
        public string MisakaHookCode { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
    }
}
