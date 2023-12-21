using OCRLibrary;
using System.Windows;
using TranslatorLibrary;
using TranslatorLibrary.Translator;

namespace MisakaTranslator_WPF
{
    /// <summary>
    /// GlobalOCRWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GlobalOCRWindow : Window
    {
        System.Drawing.Bitmap img;

        public GlobalOCRWindow(System.Drawing.Bitmap i)
        {
            InitializeComponent();
            img = i;
        }

        private async void dataInit()
        {
            OCREngine ocr;
            string res = null;
            if (Common.AppSettings.OCRsource == "TesseractOCR")
            {
                ocr = new TesseractOCREngine();
                if (ocr.OCR_Init("", "") != false)
                {
                    ocr.SetOCRSourceLang(Common.AppSettings.GlobalOCRLang);
                    res = await ocr.OCRProcessAsync(img);

                    if (res != null)
                    {
                        sourceText.Text = res;
                    }
                    else
                    {
                        HandyControl.Controls.Growl.WarningGlobal($"TesseractOCR {Application.Current.Resources["APITest_Error_Hint"]}\n{ocr.GetLastError()}");
                    }
                }
                else
                {
                    HandyControl.Controls.Growl.ErrorGlobal($"TesseractOCR {Application.Current.Resources["APITest_Error_Hint"]}\n{ocr.GetLastError()}");
                }
            }
            else if (Common.AppSettings.OCRsource == "TesseractCli")
            {
                ocr = new TesseractCommandLineEngine();
                if (ocr.OCR_Init(Common.AppSettings.TesseractCli_Path, Common.AppSettings.TesseractCli_Args))
                {
                    ocr.SetOCRSourceLang(Common.AppSettings.GlobalOCRLang);
                    res = await ocr.OCRProcessAsync(img);

                    if (res != null)
                    {
                        sourceText.Text = res;
                    }
                    else
                    {
                        HandyControl.Controls.Growl.WarningGlobal($"TesseractCli {Application.Current.Resources["APITest_Error_Hint"]}\n{ocr.GetLastError()}");
                    }
                }
                else
                {
                    HandyControl.Controls.Growl.ErrorGlobal($"TesseractCli {Application.Current.Resources["APITest_Error_Hint"]}\n{ocr.GetLastError()}");
                }
            }
            else if (Common.AppSettings.OCRsource == "BaiduOCR")
            {
                ocr = new BaiduGeneralOCREngine();
                if (ocr.OCR_Init(Common.AppSettings.BDOCR_APIKEY, Common.AppSettings.BDOCR_SecretKey))
                {
                    ocr.SetOCRSourceLang(Common.AppSettings.GlobalOCRLang);
                    res = await ocr.OCRProcessAsync(img);

                    if (res != null)
                    {
                        sourceText.Text = res;
                    }
                    else
                    {
                        HandyControl.Controls.Growl.WarningGlobal($"百度智能云OCR {Application.Current.Resources["APITest_Error_Hint"]}\n{ocr.GetLastError()}");
                    }
                }
                else
                {
                    HandyControl.Controls.Growl.ErrorGlobal($"百度智能云OCR {Application.Current.Resources["APITest_Error_Hint"]}\n{ocr.GetLastError()}");
                }
            }
            else if (Common.AppSettings.OCRsource == "BaiduFanyiOCR")
            {
                ocr = new BaiduFanyiOCREngine();
                if (ocr.OCR_Init(Common.AppSettings.BDappID, Common.AppSettings.BDsecretKey))
                {
                    ocr.SetOCRSourceLang(Common.AppSettings.GlobalOCRLang);
                    res = await ocr.OCRProcessAsync(img);

                    if (res != null)
                        FirstTransText.Text = res;
                    else
                        HandyControl.Controls.Growl.WarningGlobal($"百度翻译OCR {Application.Current.Resources["APITest_Error_Hint"]}\n{ocr.GetLastError()}");
                }
                else
                    HandyControl.Controls.Growl.ErrorGlobal($"百度翻译OCR {Application.Current.Resources["APITest_Error_Hint"]}\n{ocr.GetLastError()}");
            }
            else if (Common.AppSettings.OCRsource == "TencentOCR")
            {
                ocr = new TencentOCR();
                if (ocr.OCR_Init(Common.AppSettings.TXOSecretId, Common.AppSettings.TXOSecretKey))
                {
                    ocr.SetOCRSourceLang(Common.AppSettings.GlobalOCRLang);
                    res = await ocr.OCRProcessAsync(new System.Drawing.Bitmap(img));

                    if (res != null)
                        FirstTransText.Text = res;
                    else
                        HandyControl.Controls.Growl.WarningGlobal($"腾讯云图片翻译 {Application.Current.Resources["APITest_Error_Hint"]}\n{ocr.GetLastError()}");
                }
                else
                    HandyControl.Controls.Growl.ErrorGlobal($"腾讯云图片翻译 {Application.Current.Resources["APITest_Error_Hint"]}\n{ocr.GetLastError()}");
            }
            else if (Common.AppSettings.OCRsource == "WindowsOCR")
            {
                ocr = new WindowsOCREngine();
                if (ocr.OCR_Init("", "") != false)
                {
                    ocr.SetOCRSourceLang(Common.AppSettings.GlobalOCRLang);
                    res = await ocr.OCRProcessAsync(img);

                    if (res != null)
                    {
                        sourceText.Text = res;
                    }
                    else
                    {
                        HandyControl.Controls.Growl.WarningGlobal($"Windows OCR {Application.Current.Resources["APITest_Error_Hint"]}\n{ocr.GetLastError()}");
                    }
                }
                else
                {
                    HandyControl.Controls.Growl.ErrorGlobal($"Windows OCR {Application.Current.Resources["APITest_Error_Hint"]}\n{ocr.GetLastError()}");
                }
            }

            if (res == null)
            {
                FirstTransText.Text = "OCR ERROR";
            }
            else if (!(Common.AppSettings.OCRsource == "BaiduFanyiOCR" || Common.AppSettings.OCRsource == "TencentOCR"))
            {
                // 因为历史原因，OCR的源语言用的是三个字母的，如eng和jpn。而翻译的API即Common.UsingSrcLang用的两个字母，如en和jp
                string srclang;
                switch (Common.AppSettings.GlobalOCRLang)
                {
                    case "eng":
                        srclang = "en";
                        break;
                    case "jpn":
                        srclang = "ja";
                        break;
                    default:
                        srclang = Common.AppSettings.GlobalOCRLang;
                        break;
                }

                if (!Common.AppSettings.EachRowTrans)
                    if (srclang == "en")
                        res = res.Replace("\n", " ").Replace("\r", " ");
                    else
                        res = res.Replace("\n", "").Replace("\r", "");

                if (Common.AppSettings.HttpProxy != "")
                {
                    TranslatorCommon.SetHttpProxiedClient(Common.AppSettings.HttpProxy);
                }
                ITranslator translator1 = TranslateWindow.TranslatorAuto(Common.AppSettings.FirstTranslator);
                ITranslator translator2 = TranslateWindow.TranslatorAuto(Common.AppSettings.SecondTranslator);
                //5.提交翻译
                string transRes1 = "";
                string transRes2 = "";
                if (translator1 != null)
                {
                    transRes1 = await translator1.TranslateAsync(res, Common.UsingDstLang, srclang);
                }
                if (translator2 != null)
                {
                    transRes2 = await translator2.TranslateAsync(res, Common.UsingDstLang, srclang);
                }

                FirstTransText.Text = transRes1;
                SecondTransText.Text = transRes2;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dataInit();
        }
        private void Window_Closing(object sender, object e)
        {
            img.Dispose();
        }
    }
}
