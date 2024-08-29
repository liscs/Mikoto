using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mikoto.Helpers.File
{
    public class WaitFileHelper
    {
        public static void WaitUntilReadable(string filePath)
        {
            Thread.Sleep(1);
            const int maxAttempts = 100;
            const int delay = 1; // 等待 500 毫秒

            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                    // 成功打开文件，文件可读
                    return;
                }
                catch (IOException)
                {
                    // 文件正在被使用或不可访问
                    Thread.Sleep(delay);
                }
            }

            throw new IOException("无法读取文件，该文件可能正在被另一个进程占用。");
        }
    }
}
