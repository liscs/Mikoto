using OCRLibrary;
using System.ComponentModel;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Win32;

namespace MisakaTranslator
{
    /// <summary>
    /// ScreenCaptureWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ScreenCaptureWindow : Window
    {
        private BitmapImage img;
        private Point iniP;
        private ViewModel viewModel;
        private Rect selectRect;
        private double scale;

        public static System.Drawing.Rectangle OCRArea;
        private int capMode;

        public ScreenCaptureWindow(BitmapImage i, int mode = 1)
        {
            img = i;
            scale = Common.GetScale();
            capMode = mode;
            InitializeComponent();

            imgMeasure.Source = img;

            DrawingAttributes drawingAttributes = new DrawingAttributes
            {
                Color = Colors.Red,
                Width = 2,
                Height = 2,
                StylusTip = StylusTip.Rectangle,
                //FitToCurve = true,
                IsHighlighter = false,
                IgnorePressure = true,
            };
            inkCanvasMeasure.DefaultDrawingAttributes = drawingAttributes;

            viewModel = new ViewModel
            {
                MeaInfo = Application.Current.Resources["ScreenCaptureWindow_Area"].ToString()!,
                InkStrokes = new StrokeCollection(),
            };

            DataContext = viewModel;

        }

        private void InkCanvasMeasure_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                iniP = e.GetPosition(inkCanvasMeasure);
            }

            if (e.ChangedButton == MouseButton.Right)
            {
                e.Handled = true;
                Capture();
                Dispatcher.Invoke(async () => { await System.Threading.Tasks.Task.Delay(100); Close(); }); // 等待一毫秒再关闭否则右键会被下层窗口接收到
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (capMode == 2 && e.Key == Key.Escape)
            {
                e.Handled = true;
                this.Close();
            }
        }

        private void InkCanvasMeasure_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Draw square
                Point endP = e.GetPosition(inkCanvasMeasure);
                Point[] pointList = new Point[]
                    {
                        new Point(iniP.X, iniP.Y),
                        new Point(iniP.X, endP.Y),
                        new Point(endP.X, endP.Y),
                        new Point(endP.X, iniP.Y),
                        new Point(iniP.X, iniP.Y),
                    };
                StylusPointCollection point = new StylusPointCollection(pointList);
                Stroke stroke = new Stroke(point)
                {
                    DrawingAttributes = inkCanvasMeasure.DefaultDrawingAttributes.Clone()
                };
                viewModel.InkStrokes.Clear();
                viewModel.InkStrokes.Add(stroke);

                selectRect = new Rect(new Point(iniP.X * scale, iniP.Y * scale), new Point(endP.X * scale, endP.Y * scale));
            }
        }

        private void Capture()
        {
            OCRArea = new System.Drawing.Rectangle((int)selectRect.X, (int)selectRect.Y, (int)selectRect.Width, (int)selectRect.Height);

            if (capMode == 2)
            {
                //全局OCR截图，直接打开结果页面
                System.Drawing.Bitmap img = ScreenCapture.GetWindowRectCapture(System.IntPtr.Zero, OCRArea, true);
                if (img.Width == 0 && img.Height == 0) // 没有框选范围
                    return;

                var reswin = new GlobalOCRWindow(img);
                PInvoke.GetCursorPos(out System.Drawing.Point mousestart);
                reswin.Left = mousestart.X;
                reswin.Top = mousestart.Y;

                reswin.Show();
            }

        }

    }

    internal class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string meaInfo = string.Empty;
        public string MeaInfo
        {
            get => meaInfo;
            set
            {
                meaInfo = value;
                OnPropertyChanged("MeaInfo");
            }
        }

        private StrokeCollection inkStrokes = new();
        public StrokeCollection InkStrokes
        {
            get
            {
                return inkStrokes;
            }
            set
            {
                inkStrokes = value;
                OnPropertyChanged("InkStrokes");
            }
        }
    }

}
