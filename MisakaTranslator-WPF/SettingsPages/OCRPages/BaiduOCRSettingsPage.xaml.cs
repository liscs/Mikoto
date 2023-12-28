using OCRLibrary;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace MisakaTranslator.SettingsPages.OCRPages
{
    /// <summary>
    /// BaiduOCRPage.xaml 的交互逻辑
    /// </summary>
    public partial class BaiduOCRPage : Page
    {
        public BaiduOCRPage()
        {
            InitializeComponent();
            APIKEYBox.Text = Common.AppSettings.BDOCR_APIKEY;
            SecretKeyBox.Text = Common.AppSettings.BDOCR_SecretKey;
        }

        private void AuthTestBtn_Click(object sender, RoutedEventArgs e)
        {
            Common.AppSettings.BDOCR_APIKEY = APIKEYBox.Text;
            Common.AppSettings.BDOCR_SecretKey = SecretKeyBox.Text;

            if (APIKEYBox.Text.Length == 17)
            {
                HandyControl.Controls.Growl.Error($"百度智能云OCR {Application.Current.Resources["APITest_Error_Hint"]}\nDo not use fanyi.baidu.com endpoint.");
                return;
            }

            BaiduGeneralOCREngine bgocr = new BaiduGeneralOCREngine();

            bool ret = bgocr.OCR_Init(APIKEYBox.Text, SecretKeyBox.Text);

            if (ret == true)
            {
                HandyControl.Controls.Growl.Success($"百度智能云OCR {Application.Current.Resources["APITest_Success_Hint"]}");
            }
            else
            {
                HandyControl.Controls.Growl.Error($"百度智能云OCR {Application.Current.Resources["APITest_Error_Hint"]}\n{bgocr.GetLastError()}");
            }
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(BaiduGeneralOCREngine.GetUrl_API()) { UseShellExecute = true });
        }

        private void DocBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(BaiduGeneralOCREngine.GetUrl_Doc()) { UseShellExecute = true });
        }

        private void BillBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(BaiduGeneralOCREngine.GetUrl_Bill()) { UseShellExecute = true });
        }
    }
}