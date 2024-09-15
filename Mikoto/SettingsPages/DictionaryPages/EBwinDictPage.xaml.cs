using Microsoft.Win32;
using Mikoto.Dictionary;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using Cursors = System.Windows.Input.Cursors;

namespace Mikoto.SettingsPages.DictionaryPages
{
    /// <summary>
    /// Page1.xaml 的交互逻辑
    /// </summary>
    public partial class ManageDictionariesPage : Page
    {
        private EbwinHelper _ebwinHelper = new EbwinHelper();

        private HashSet<Dict> allDicts = new HashSet<Dict>();

        public ManageDictionariesPage()
        {
            InitializeComponent();
            UpdateDictionariesDisplay();
        }

        private void UpdateDictionariesDisplay()
        {
            allDicts = EbwinHelper.GetAllDicts();
            List<DockPanel> resultDockPanels = new();

            foreach (Dict dict in allDicts)
            {
                DockPanel dockPanel = new();
                CheckBox checkBox = new()
                {
                    Width = 20,
                    IsChecked = dict.GetActive(),
                    Margin = new Thickness(10),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                };

                Button buttonIncreasePriority = new()
                {
                    Width = 25,
                    Content = '↑',
                    Margin = new Thickness(1),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                };

                Button buttonDecreasePriority = new()
                {
                    Width = 25,
                    Content = '↓',
                    Margin = new Thickness(1),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                };

                TextBlock priority = new()
                {
                    Name = "priority",
                    Width = 0,
                    Text = dict.Priority.ToString(CultureInfo.InvariantCulture),
                    Visibility = Visibility.Collapsed
                };

                TextBlock dictTypeDisplay = new()
                {
                    Width = 100,
                    Text = dict.Name,
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10)
                };

                string fullPath = Path.GetFullPath(dict.DictPath);
                bool invalidPath = !Directory.Exists(fullPath) && !File.Exists(fullPath);
                TextBlock dictPathValidityDisplay = new()
                {
                    Width = 13,
                    Text = invalidPath ? "❌" : "",
                    ToolTip = invalidPath ? "Invalid Path" : null,
                    Foreground = Brushes.Crimson,
                    Margin = new Thickness(1),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                TextBlock dictPathDisplay = new()
                {
                    Width = 150,
                    Text = dict.DictPath,
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10),
                    Cursor = Cursors.Hand
                };
                dictPathDisplay.PreviewMouseLeftButtonUp += PathTextBox_PreviewMouseLeftButtonUp;
                dictPathDisplay.MouseEnter += (_, _) => dictPathDisplay.TextDecorations = TextDecorations.Underline;
                dictPathDisplay.MouseLeave += (_, _) => dictPathDisplay.TextDecorations = null;

                Button buttonRemove = new()
                {
                    Height = 30,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.White,
                    Background = Brushes.Red,
                    BorderThickness = new Thickness(1),
                    Visibility = Visibility.Visible
                };
                buttonRemove.SetResourceReference(Button.ContentProperty, "EBwinDictPage_RemoveDictionary");

                Button buttonEdit = new()
                {
                    Height = 30,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.White,
                    Background = Brushes.DodgerBlue,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(0, 0, 5, 0)
                };
                buttonEdit.SetResourceReference(Button.ContentProperty, "EBwinDictPage_EditDictionary");


                checkBox.Unchecked += (_, _) =>
                {
                    dict.SetActive(false);
                    EbwinHelper.AddOrUpdateDictionary(dict);
                };

                checkBox.Checked += (_, _) =>
                {
                    dict.SetActive(true);
                    EbwinHelper.AddOrUpdateDictionary(dict);
                };

                buttonIncreasePriority.Click += (_, _) =>
                {
                    PrioritizeDict(dict);
                    UpdateDictionariesDisplay();
                };
                buttonDecreasePriority.Click += (_, _) =>
                {
                    DeprioritizeDict(dict);
                    UpdateDictionariesDisplay();
                };
                buttonRemove.Click += (_, _) =>
                {
                    var askResult = HandyControl.Controls.MessageBox.Ask(Application.Current.Resources["EBwinDictPage_RemoveDictAsk"].ToString(), Application.Current.Resources["MessageBox_Ask"].ToString());
                    if (askResult == MessageBoxResult.OK)
                    {
                        _ = allDicts.Remove(dict);
                        EbwinHelper.RemoveDictionary(dict);
                        int priorityOfDeletedDict = dict.Priority;

                        foreach (Dict d in allDicts)
                        {
                            if (d.Priority > priorityOfDeletedDict)
                            {
                                d.Priority -= 1;
                            }
                            EbwinHelper.AddOrUpdateDictionary(d);
                        }
                        UpdateDictionariesDisplay();
                    }
                };
                buttonEdit.Click += (_, _) =>
                {
                    string path = BrowseForDictionaryFile("Dictionary Files|CATALOG*;*.dic;*.idx;*.ebd;*.ifo;*.mdx;*.dsl;*.dz");
                    if (path != string.Empty)
                    {
                        Dict d = new Dict(path)
                        {
                            Priority = allDicts.Count + 1
                        };
                        d.SetActive(true);

                        EbwinHelper.AddOrUpdateDictionary(d);
                    }
                    UpdateDictionariesDisplay();
                };

                resultDockPanels.Add(dockPanel);

                _ = dockPanel.Children.Add(checkBox);
                _ = dockPanel.Children.Add(buttonIncreasePriority);
                _ = dockPanel.Children.Add(buttonDecreasePriority);
                _ = dockPanel.Children.Add(priority);
                _ = dockPanel.Children.Add(dictTypeDisplay);
                _ = dockPanel.Children.Add(dictPathValidityDisplay);
                _ = dockPanel.Children.Add(dictPathDisplay);
                _ = dockPanel.Children.Add(buttonEdit);
                _ = dockPanel.Children.Add(buttonRemove);
            }

            DictionariesDisplay.ItemsSource = resultDockPanels.OrderBy(static dockPanel =>
                dockPanel.Children
                    .OfType<TextBlock>()
                    .Where(static textBlock => textBlock.Name is "priority")
                    .Select(static textBlockPriority => Convert.ToInt32(textBlockPriority.Text, CultureInfo.InvariantCulture)).First());
        }


        private void PathTextBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string fullPath = Path.GetFullPath(((TextBlock)sender).Text);
            if (File.Exists(fullPath) || Directory.Exists(fullPath))
            {
                if (File.Exists(fullPath))
                {
                    fullPath = Path.GetDirectoryName(fullPath) ?? fullPath;
                }
                Process.Start(new ProcessStartInfo
                {
                    FileName = fullPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
        }

        private void PrioritizeDict(Dict dict)
        {
            if (dict.Priority == 1)
            {
                return;
            }

            foreach (var item in allDicts)
            {
                if (item.Priority == dict.Priority - 1)
                {
                    Dict neighborDict = item;
                    neighborDict.Priority += 1;
                    dict.Priority -= 1;
                    EbwinHelper.AddOrUpdateDictionary(neighborDict);
                    EbwinHelper.AddOrUpdateDictionary(dict);
                    break;
                }
            }
        }

        private void DeprioritizeDict(Dict dict)
        {
            if (dict.Priority == allDicts.Count)
            {
                return;
            }
            foreach (var item in allDicts)
            {
                if (item.Priority == dict.Priority + 1)
                {
                    Dict neighborDict = item;
                    neighborDict.Priority -= 1;
                    dict.Priority += 1;
                    EbwinHelper.AddOrUpdateDictionary(neighborDict);
                    EbwinHelper.AddOrUpdateDictionary(dict);
                }
            }
        }

        private void ButtonAddDictionary_OnClick(object sender, RoutedEventArgs e)
        {
            string path = BrowseForDictionaryFile("Dictionary Files|CATALOG*;*.dic;*.idx;*.ebd;*.ifo;*.mdx;*.dsl;*.dz");
            if (path != string.Empty)
            {
                Dict d = new Dict(path)
                {
                    Priority = allDicts.Count + 1
                };
                EbwinHelper.AddOrUpdateDictionary(d);
                d.SetActive(true);

            }
            UpdateDictionariesDisplay();
        }

        private string BrowseForDictionaryFile(string filter)
        {
            OpenFileDialog openFileDialog = new() { Filter = filter };

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }
            return string.Empty;
        }
    }
}
