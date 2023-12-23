using HandyControl.Controls;
using OCRLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MisakaTranslator_WPF.GuidePages.OCR
{
    /// <summary>
    /// ChooseHandleFuncPage.xaml 的交互逻辑
    /// </summary>
    public partial class ChooseHandleFuncPage : Page
    {
        public List<string> ImageProcFunclist;
        public List<string> Langlist;

        public ChooseHandleFuncPage()
        {
            InitializeComponent();

            ImageProcFunclist = ImageProcFunc.lstHandleFun.Keys.ToList();
            HandleFuncCombox.ItemsSource = ImageProcFunclist;
            HandleFuncCombox.SelectedIndex = 0;

            Langlist = ImageProcFunc.lstOCRLang.Keys.ToList();
            OCRLangCombox.ItemsSource = Langlist;
            OCRLangCombox.SelectedIndex = 1;

        }

        private void RenewAreaBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Drawing.Bitmap img = Common.Ocr.GetOCRAreaCap();

            SrcImg.Source = ImageProcFunc.ImageToBitmapImage(img);

            DstImg.Source = ImageProcFunc.ImageToBitmapImage(ImageProcFunc.Auto_Thresholding(img,
                ImageProcFunc.lstHandleFun[ImageProcFunclist[HandleFuncCombox.SelectedIndex]]));

            GC.Collect();
        }

        private async void OCRTestBtn_Click(object sender, RoutedEventArgs e)
        {

            if (Common.AppSettings.OCRsource == "TesseractOCR")
            {
                if (Common.Ocr.OCR_Init("", "") != false)
                {
                    string? res = await Common.Ocr.OCRProcessAsync();

                    if (res != null)
                    {
                        HandyControl.Controls.MessageBox.Show(res, Application.Current.Resources["MessageBox_Result"].ToString());
                    }
                    else
                    {
                        Growl.Error($"TesseractOCR {Application.Current.Resources["APITest_Error_Hint"]}\n{Common.Ocr.GetLastError()}");
                    }
                }
                else
                {
                    Growl.Error($"TesseractOCR {Application.Current.Resources["APITest_Error_Hint"]}\n{Common.Ocr.GetLastError()}");
                }
            }
            else if (Common.AppSettings.OCRsource == "BaiduOCR")
            {
                if (Common.Ocr.OCR_Init(Common.AppSettings.BDOCR_APIKEY, Common.AppSettings.BDOCR_SecretKey))
                {
                    string? res = await Common.Ocr.OCRProcessAsync();

                    if (res != null)
                    {
                        HandyControl.Controls.MessageBox.Show(res, Application.Current.Resources["MessageBox_Result"].ToString());
                    }
                    else
                    {
                        Growl.Error($"百度智能云OCR {Application.Current.Resources["APITest_Error_Hint"]}\n{Common.Ocr.GetLastError()}");
                    }
                }
                else
                {
                    Growl.Error($"百度智能云OCR {Application.Current.Resources["APITest_Error_Hint"]}\n{Common.Ocr.GetLastError()}");
                }
            }
            else if (Common.AppSettings.OCRsource == "BaiduFanyiOCR")
            {
                if (Common.Ocr.OCR_Init(Common.AppSettings.BDappID, Common.AppSettings.BDsecretKey))
                {
                    string? res = await Common.Ocr.OCRProcessAsync();

                    if (res != null)
                        HandyControl.Controls.MessageBox.Show(res, Application.Current.Resources["MessageBox_Result"].ToString());
                    else
                        Growl.Error($"百度翻译OCR {Application.Current.Resources["APITest_Error_Hint"]}\n{Common.Ocr.GetLastError()}");
                }
                else
                    Growl.Error($"百度翻译OCR {Application.Current.Resources["APITest_Error_Hint"]}\n{Common.Ocr.GetLastError()}");
            }
            else if (Common.AppSettings.OCRsource == "TencentOCR")
            {
                if (Common.Ocr.OCR_Init(Common.AppSettings.TXOSecretId, Common.AppSettings.TXOSecretKey))
                {
                    string? res = await Common.Ocr.OCRProcessAsync();

                    if (res != null)
                        HandyControl.Controls.MessageBox.Show(res, Application.Current.Resources["MessageBox_Result"].ToString());
                    else
                        Growl.Error($"腾讯图片翻译 {Application.Current.Resources["APITest_Error_Hint"]}\n{Common.Ocr.GetLastError()}");
                }
                else
                    Growl.Error($"腾讯图片翻译 {Application.Current.Resources["APITest_Error_Hint"]}\n{Common.Ocr.GetLastError()}");
            }
            else if (Common.AppSettings.OCRsource == "TesseractCli")
            {
                if (Common.Ocr.OCR_Init(Common.AppSettings.TesseractCli_Path, Common.AppSettings.TesseractCli_Args))
                {
                    string? res = await Common.Ocr.OCRProcessAsync();

                    if (res != null)
                    {
                        HandyControl.Controls.MessageBox.Show(res, Application.Current.Resources["MessageBox_Result"].ToString());
                    }
                    else
                    {
                        Growl.Error($"TesseractCli {Application.Current.Resources["APITest_Error_Hint"]}\n{Common.Ocr.GetLastError()}");
                    }
                }
                else
                {
                    Growl.Error($"TesseractCli {Application.Current.Resources["APITest_Error_Hint"]}\n{Common.Ocr.GetLastError()}");
                }
            }
            else if (Common.AppSettings.OCRsource == "WindowsOCR")
            {
                if (Common.Ocr.OCR_Init("", "") != false)
                {
                    string? res = await Common.Ocr.OCRProcessAsync();

                    if (res != null)
                    {
                        HandyControl.Controls.MessageBox.Show(res, Application.Current.Resources["MessageBox_Result"].ToString());
                    }
                    else
                    {
                        Growl.Error($"Windows OCR {Application.Current.Resources["APITest_Error_Hint"]}\n{Common.Ocr.GetLastError()}");
                    }
                }
                else
                {
                    Growl.Error($"Windows OCR {Application.Current.Resources["APITest_Error_Hint"]}\n{Common.Ocr.GetLastError()}");
                }
            }

            ConfirmBtn.IsEnabled = true;
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            Common.GameID = Guid.Empty;//OCR暂时还不支持保存,因此强制给值Empty

            //使用路由事件机制通知窗口来完成下一步操作
            PageChangeRoutedEventArgs args = new PageChangeRoutedEventArgs(PageChange.PageChangeRoutedEvent, this);
            args.XamlPath = "GuidePages/OCR/ChooseHotKeyPage.xaml";
            this.RaiseEvent(args);
        }

        private void OCRLangCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Common.Ocr.SetOCRSourceLang(ImageProcFunc.lstOCRLang[Langlist[OCRLangCombox.SelectedIndex]]);
        }

        private void HandleFuncCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Drawing.Image img = Common.Ocr.GetOCRAreaCap();

            SrcImg.Source = ImageProcFunc.ImageToBitmapImage(img);
            try
            {
                string imgProc = ImageProcFunc.lstHandleFun[ImageProcFunclist[HandleFuncCombox.SelectedIndex]];
                DstImg.Source = ImageProcFunc.ImageToBitmapImage(ImageProcFunc.Auto_Thresholding(new System.Drawing.Bitmap(img), imgProc));
                Common.Ocr.SetOCRSourceImgProc(imgProc);
            }
            catch (NullReferenceException)
            {
                Growl.Error(Application.Current.Resources["ChooseOCRAreaPage_RenewErrorHint"].ToString());
                return;
            }

            GC.Collect();
        }
    }
}
