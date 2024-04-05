using System.Diagnostics;

namespace TextHookLibrary
{
    public static class ProcessHelper
    {
        private const string ExtPath = "lib\\ProcessHelperExt.exe";

        /// <summary>
        /// 获得当前系统进程列表 形式：直接用于显示的字串和进程PID
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, int> GetProcessList_Name_PID()
        {
            Dictionary<string, int> ret = new();

            //获取系统进程列表
            foreach (Process p in Process.GetProcesses())
            {
                if (p.MainWindowHandle != IntPtr.Zero)
                {
                    string info = p.ProcessName + ": ";
                    if (!string.IsNullOrEmpty(p.MainWindowTitle))
                    {
                        info += "【" + p.MainWindowTitle + "】: ";
                    }
                    info += p.Id;
                    ret.Add(info, p.Id);
                }
                p.Dispose();
            }
            return ret;
        }

        /// <summary>
        /// 查找同名进程并返回一个进程PID列表
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static List<Process> FindSameNameProcess(int pid)
        {
            string DesProcessName = Process.GetProcessById(pid).ProcessName;

            List<Process> res = new List<Process>();
            foreach (Process p in Process.GetProcessesByName(DesProcessName))
                res.Add(p);
            return res;
        }

        /// <summary>
        /// 根据进程PID找到程序所在路径
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static string FindProcessPath(int pid, bool isx64game = false)
        {
            try
            {
                Process p = Process.GetProcessById(pid);
                return p.MainModule!.FileName;
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                if (!(isx64game && e.NativeErrorCode == 299 && System.IO.File.Exists(ExtPath)))
                    return "";

                // Win32Exception:“A 32 bit processes cannot access modules of a 64 bit process.”
                // 通过调用外部64位程序，使主程序在32位下获取其它64位程序的路径。外部程序不存在或不是此错误时保持原有逻辑返回""
                var p = Process.Start(new ProcessStartInfo(ExtPath, pid.ToString())
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                });
                if (p == null)
                {
                    throw new InvalidOperationException("Failed to execute ProcessHelper.exe");
                }
                string path = p.StandardOutput.ReadToEnd().TrimEnd();
                if (p.ExitCode == 3) // 不存在此pid对应的进程
                    return "";
                else if (p.ExitCode != 0)
                    throw new InvalidOperationException("Failed to execute ProcessHelper.exe");
                return path;
            }
        }

        /// <summary>
        /// 返回 pid,绝对路径 的列表
        /// </summary>
        public static List<(int, string)> GetProcessesData()
        {
            var result = new List<(int, string)>();
            foreach (Process p in Process.GetProcesses().Where(p => p.MainWindowHandle != IntPtr.Zero))
            {
                using (p)
                {
                    try { result.Add((p.Id, p.MainModule!.FileName)); }
                    catch (System.ComponentModel.Win32Exception) { } // 无权限
                    catch (InvalidOperationException) { } // 进程已退出
                }
            }
            return result;
        }

        /// <summary>
        /// internal bool System.Diagnostics.ProcessManager.IsProcessRunning(int pid)
        /// </summary>
        public static Func<int, bool> IsProcessRunning = (Func<int, bool>)typeof(Process).Assembly.GetType("System.Diagnostics.ProcessManager")!.GetMethod("IsProcessRunning", new[] { typeof(int) })!.CreateDelegate(typeof(Func<int, bool>));
    }
}
