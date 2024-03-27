using System;
using System.Windows.Forms;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace TextHookLibrary
{
    public class ClipboardNotification
    {
        private HWND winHandle;
        private HWND ClipboardViewerNext;

        public ClipboardNotification(IntPtr winH)
        {
            winHandle = (HWND)winH;
        }

        public void RegisterClipboardViewer()
        {
            ClipboardViewerNext = PInvoke.SetClipboardViewer(winHandle);
        }

        public void UnregisterClipboardViewer()
        {
            PInvoke.ChangeClipboardChain(winHandle, ClipboardViewerNext);
        }
    }


    /// <summary>
    /// 剪贴板更新事件
    /// </summary>
    /// <param name="ClipboardText">更新的文本</param>
    public delegate void ClipboardUpdateEventHandler(string ClipboardText);

    /// <summary>
    /// 使用一个隐藏窗口来接受窗口消息,对外就是剪贴板监视类
    /// </summary>
    public class ClipboardMonitor : Form
    {
        public event ClipboardUpdateEventHandler OnClipboardUpdate;
        private IntPtr _hwnd;

        public ClipboardNotification ClipboardNotification { get; set; }

        public ClipboardMonitor(ClipboardUpdateEventHandler onClipboardUpdate)
        {
            this.OnClipboardUpdate = onClipboardUpdate;
            this._hwnd = this.Handle;
            ClipboardNotification = new ClipboardNotification(_hwnd);
            ClipboardNotification.RegisterClipboardViewer();
        }

        ~ClipboardMonitor()
        {
            ClipboardNotification.UnregisterClipboardViewer();
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x308: //WM_DRAWCLIPBOARD
                    {
                        IDataObject? iData = Clipboard.GetDataObject();
                        if (iData != null)
                        {
                            string str = iData.GetData(DataFormats.UnicodeText) as string ?? "剪贴板更新失败 ClipBoard Update Failed";
                            this.OnClipboardUpdate(str);
                        }
                        else
                        {
                            this.OnClipboardUpdate("剪贴板更新失败 ClipBoard Update Failed");
                        }
                        break;
                    }
                default:
                    {
                        base.WndProc(ref m);
                        break;
                    }
            }
        }
    }
}
