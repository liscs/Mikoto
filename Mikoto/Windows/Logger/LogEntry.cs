using Mikoto.Helpers.ViewModel;
using System.Diagnostics;
using System.Windows.Media;
namespace Mikoto.Windows.Logger
{
    internal class LogEntry : ViewModelBase
    {
        private static int index = 0;
        public string DateTime { get; set; } = System.DateTime.Now.ToString();

        public int Index
        {
            get
            {
                index++;
                return index;
            }
            set
            {
                throw new UnreachableException();
            }
        }

        public string Message { get; set; } = string.Empty;

        public SolidColorBrush Color { get; set; } = Brushes.AliceBlue;
    }
}
