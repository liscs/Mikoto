using System.IO;

namespace Mikoto.Helpers.Files
{
    internal static class HookFileHelper
    {
        internal static string ToCircusEntranceExe(string path)
        {
            if (Path.GetExtension(path) == ".log")
            {
                //对CIRCUS的特殊处理，因其hook的文件是运行时的一个临时log文件
                path = Path.ChangeExtension(path, ".exe");
            }

            return path;
        }
    }
}