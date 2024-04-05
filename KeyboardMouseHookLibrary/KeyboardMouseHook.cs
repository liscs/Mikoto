using System.Diagnostics;
using System.Drawing;

namespace KeyboardMouseHookLibrary
{
    /// <summary>
    /// 鼠标动作事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void MouseButtonEventHandler(object sender, Point e);

    /// <summary>
    /// 键盘动作事件
    /// </summary>
    /// <param name="sender"></param>
    public delegate void KeyboardEventHandler(object sender);


    public class KeyboardMouseHook
    {
        private Process? processMonitor;

        public event MouseButtonEventHandler? OnMouseActivity;
        public event KeyboardEventHandler? OnKeyboardActivity;


        public KeyboardMouseHook()
        {

        }

        ~KeyboardMouseHook()
        {
            Stop();
        }

        public void Stop()
        {
            if (processMonitor != null)
            {
                processMonitor.Kill();
                processMonitor.Close();
                processMonitor = null;
            }
        }

        /// <summary>
        /// 开启外部进程，进行键鼠hook
        /// </summary>
        /// <param name="isMouse">是否是鼠标hook</param>
        /// <param name="keyCode">要捕获动作的键值，当捕获鼠标时，1代表左键，2代表右键</param>
        /// <returns></returns>
        public bool Start(bool isMouse, int keyCode)
        {
            processMonitor = new Process();
            processMonitor.StartInfo.FileName = "KeyboardMouseMonitor.exe";
            //加额外参数
            if (isMouse)
            {
                processMonitor.StartInfo.Arguments = "1 " + keyCode;
            }
            else
            {
                processMonitor.StartInfo.Arguments = "2 " + keyCode;
            }

            processMonitor.StartInfo.CreateNoWindow = true;
            processMonitor.StartInfo.UseShellExecute = false;
            processMonitor.StartInfo.RedirectStandardInput = true;
            processMonitor.StartInfo.RedirectStandardOutput = true;

            processMonitor.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);


            try
            {
                bool res = processMonitor.Start();
                processMonitor.BeginOutputReadLine();
                return res;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                return false;
            }
        }

        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            string? output = outLine.Data;

            Console.WriteLine(output);

            if (output == "hookFailed")
            {
                processMonitor?.Kill();
                processMonitor?.Close();
                processMonitor = null;
                throw new Exception("Keyboard Mouse Hook Failed");
            }
            else if (output == "KeyboardAction")
            {
                OnKeyboardActivity?.Invoke(this);
            }
            else
            {
                if (output != null)
                {
                    string[] res = output.Split(' ');

                    if (res.Length == 3)
                    {

                        Point pt = new()
                        {
                            X = int.Parse(res[1]),
                            Y = int.Parse(res[2])
                        };

                        OnMouseActivity?.Invoke(this, pt);
                    }
                }


            }

        }

    }
}
