using DateTimeA = System.DateTime;
namespace Mikoto
{
    public class LogEntry : ViewModelBase
    {
        private static int index = 0;
        public string DateTime { get; set; } = DateTimeA.Now.ToString();

        public int Index
        {
            get
            {
                index++;
                return index;
            }
        }

        public string Message { get; set; } = string.Empty;
    }
}
