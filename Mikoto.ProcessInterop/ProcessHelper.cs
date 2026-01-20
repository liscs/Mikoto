using System.ComponentModel;
using System.Diagnostics;

namespace Mikoto.ProcessInterop
{
    public static class ProcessHelper
    {
        /// <summary>
        /// 获得当前系统进程列表 形式：直接用于显示的字串和进程PID
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, int> GetAppNamePidDict()
        {
            Dictionary<string, int> result = new();

            //获取系统进程列表
            foreach (Process p in Process.GetProcesses())
            {
                if (p.MainWindowHandle != nint.Zero)
                {
                    string info = string.IsNullOrEmpty(p.MainWindowTitle)
                                    ? $"{p.ProcessName} (PID:{p.Id})"
                                    : $"{p.ProcessName} - {p.MainWindowTitle} (PID:{p.Id})";
                    result[info] = p.Id;
                }
                p.Dispose();
            }
            return result;
        }


        /// <summary>
        /// 查找同名进程并返回一个进程PID列表
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static List<Process> FindSameNameProcess(int pid)
        {
            string DesProcessName = Process.GetProcessById(pid).ProcessName;
            return Process.GetProcessesByName(DesProcessName).ToList();
        }

        /// <summary>
        /// 根据进程PID找到程序所在路径
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static string FindProcessPath(int pid)
        {
            try
            {
                Process p = Process.GetProcessById(pid);
                return p.MainModule!.FileName;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // Win32Exception:“A 32 bit processes cannot access modules of a 64 bit process.”
                // 通过调用外部64位程序，使主程序在32位下获取其它64位程序的路径。外部程序不存在或不是此错误时保持原有逻辑返回""
                throw;
            }
        }

        /// <summary>
        /// 返回 pid,绝对路径 的列表
        /// </summary>
        public static List<string> GetAppPaths()
        {
            var result = new List<string>();
            foreach (Process p in Process.GetProcesses().Where(p => p.MainWindowHandle != nint.Zero))
            {
                using (p)
                {
                    try { result.Add(p.MainModule!.FileName); }
                    catch (System.ComponentModel.Win32Exception) { } // 无权限
                    catch (InvalidOperationException) { } // 进程已退出
                }
            }
            return result;
        }


        public static bool IsProcessRunning(int processId)
        {
            try
            {
                Process process = Process.GetProcessById(processId);
                return !process.HasExited;
            }
            catch (ArgumentException)
            {
                // Process doesn't exist or has already exited
                return false;
            }
            catch (InvalidOperationException)
            {
                // Access issues or process has exited
                return false;
            }
            catch (Win32Exception)
            {
                // Access denied
                return false;
            }
        }

        public static bool StartProcessAsAdmin(string fileName)
        {
            ProcessStartInfo processStartInfo = new()
            {
                FileName = fileName,
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                var p = Process.Start(processStartInfo);
            }
            catch (Win32Exception)
            {
                return false;
            }
            return true;
        }

        public static Process? ShellStart(string filename)
        {
            return Process.Start(new ProcessStartInfo(filename) { UseShellExecute = true });
        }

        public static bool Is64BitProcess(int pid)
        {
            Windows.Win32.PInvoke.IsWow64Process(Process.GetProcessById(pid).SafeHandle, out Windows.Win32.Foundation.BOOL result);
            return !result;
        }
    }
}
