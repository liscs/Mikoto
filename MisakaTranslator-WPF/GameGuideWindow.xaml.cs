using System;
using System.Collections.Generic;
using System.Windows;

namespace MisakaTranslator_WPF
{
    /// <summary>
    /// GameGuideWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GameGuideWindow : Window
    {
        GuideMode GuideMode;
        bool isComplete;//是否是在完成状态下退出的，作为检验，默认为假

        public GameGuideWindow(GuideMode Mode)
        {
            InitializeComponent();

            this.AddHandler(GuidePages.PageChange.PageChangeRoutedEvent, new RoutedEventHandler(Next_Click));

            isComplete = false;
            GuideMode = Mode;
            if (Mode == GuideMode.Hook)
            {
                //Hook模式
                List<string> lstStep = new List<string>()
                {
                Application.Current.Resources["GameGuideWin_Hook_Step_1"].ToString(),
                Application.Current.Resources["GameGuideWin_Hook_Step_2"].ToString(),
                Application.Current.Resources["GameGuideWin_Hook_Step_3"].ToString(),
                Application.Current.Resources["GameGuideWin_Step_4"].ToString(),
                Application.Current.Resources["GameGuideWin_Step_5"].ToString()
                };

                GuideStepBar.ItemsSource = lstStep;
                FuncHint.Text = Application.Current.Resources["GameGuideWin_FuncHint_Hook"].ToString();
                GuidePageFrame.Navigate(new Uri("GuidePages/Hook/ChooseGamePage.xaml", UriKind.Relative));
            }
            else if (Mode == GuideMode.Ocr)
            {
                //OCR模式
                List<string> lstStep = new List<string>()
                {
                Application.Current.Resources["GameGuideWin_OCR_Step_1"].ToString(),
                Application.Current.Resources["GameGuideWin_OCR_Step_2"].ToString(),
                Application.Current.Resources["GameGuideWin_OCR_Step_3"].ToString(),
                Application.Current.Resources["GameGuideWin_Step_4"].ToString(),
                Application.Current.Resources["GameGuideWin_Step_5"].ToString()
                };

                GuideStepBar.ItemsSource = lstStep;
                FuncHint.Text = Application.Current.Resources["GameGuideWin_FuncHint_OCR"].ToString();
                GuidePageFrame.Navigate(new Uri("GuidePages/OCR/ChooseOCRAreaPage.xaml", UriKind.Relative));
            }
            else if (Mode == GuideMode.Rehook)
            {
                //重新选择Hook方法
                List<string> lstStep = new List<string>()
                {
                Application.Current.Resources["GameGuideWin_ReHook_Step_1"].ToString(),
                Application.Current.Resources["GameGuideWin_Step_5"].ToString()
                };

                GuideStepBar.ItemsSource = lstStep;
                FuncHint.Text = Application.Current.Resources["GameGuideWin_FuncHint_ReHook"].ToString();
                GuidePageFrame.Navigate(new Uri("GuidePages/Hook/ReChooseHookFuncPage.xaml", UriKind.Relative));
            }
            else if (Mode == GuideMode.Clipboard)
            {
                //剪贴板监控
                List<string> lstStep = new List<string>()
                {
                Application.Current.Resources["GameGuideWin_Hook_Step_3"].ToString(),
                Application.Current.Resources["GameGuideWin_Step_4"].ToString(),
                Application.Current.Resources["GameGuideWin_Step_5"].ToString()
                };

                GuideStepBar.ItemsSource = lstStep;
                FuncHint.Text = Application.Current.Resources["GameGuideWin_FuncHint_ClipBoard"].ToString();
                GuidePageFrame.Navigate(new Uri("GuidePages/Hook/ChooseTextRepairFuncPage.xaml", UriKind.Relative));
            }
        }

        /// <summary>
        /// 触发进入下一步的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            GuidePages.PageChangeRoutedEventArgs args = e as GuidePages.PageChangeRoutedEventArgs;

            if (args.XamlPath == "1")
            {
                if (GuideMode == GuideMode.Hook)
                {
                    //Hook方式设置 完成
                    Common.TransMode = TransMode.Hook;
                    TranslateWindow translateWindow = new TranslateWindow();
                    translateWindow.Show();
                    isComplete = true;
                    this.Close();
                }
                else if (GuideMode == GuideMode.Ocr)
                {
                    //OCR方式设置 完成
                    Common.TransMode = TransMode.Ocr;
                    TranslateWindow translateWindow = new TranslateWindow();
                    translateWindow.Show();
                    isComplete = true;
                    this.Close();
                }
                else if (GuideMode == GuideMode.Rehook)
                {
                    //Hook方式设置 完成
                    Common.TransMode = TransMode.Hook;
                    TranslateWindow translateWindow = new TranslateWindow();
                    translateWindow.Show();
                    isComplete = true;
                    this.Close();
                }
                else if (GuideMode == GuideMode.Clipboard)
                {
                    //剪贴板监控方式设置 完成
                    Common.TransMode = TransMode.Hook;
                    TranslateWindow translateWindow = new TranslateWindow();
                    translateWindow.Show();
                    isComplete = true;
                    this.Close();
                }
            }
            else
            {
                //其他情况就跳转指定页面
                GuidePageFrame.Navigate(new Uri(args.XamlPath, UriKind.Relative));
                GuideStepBar.Next();
            }

        }

        private void GuideWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isComplete == false)
            {
                //确保是在未完成的情况下退出再检查
                if (GuideMode == GuideMode.Hook || GuideMode == GuideMode.Rehook || GuideMode == GuideMode.Clipboard)
                {
                    if (Common.textHooker != null)
                    {
                        Common.textHooker.StopHook();
                        Common.textHooker = null;
                    }
                }
            }

        }
    }
}