using System.IO;

namespace Mikoto.Helpers.File
{
    public static class HookFileHelper
    {
        /// <summary>
        /// hook文件与启动文件的路径转换
        /// </summary>
        /// <param name="hookPath">hook文件路径</param>
        /// <returns>启动文件路径</returns>
        public static string ToEntranceFilePath(string hookPath)
        {
            string entranceExe = ToEntranceExeIfCircus(hookPath);
            return entranceExe;
        }

        /// <summary>
        /// 如果是Circus运行时的临时log文件，返回启动文件路径，否则直接返回入参
        /// </summary>
        private static string ToEntranceExeIfCircus(string hookPath)
        {
            if (Path.GetExtension(hookPath) == ".log")
            {
                //对CIRCUS的特殊处理，因其hook的文件是运行时的一个临时log文件
                hookPath = Path.ChangeExtension(hookPath, ".exe");
            }

            return hookPath;
        }
    }
}