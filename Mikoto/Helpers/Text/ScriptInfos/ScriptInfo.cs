using Mikoto.Windows.Logger;
using System.IO;

namespace Mikoto.Helpers.Text.ScriptInfos
{
    internal abstract class ScriptInfo
    {
        protected ScriptInfo() { }

        private string scriptsPath = Path.Combine(Common.DataFolder, "custom scripts");

        protected abstract string Name { get; }
        protected abstract string FileExtension { get; }
        protected abstract string FolderName { get; }
        protected string Error { get; set; } = string.Empty;

        public void Init()
        {
            InitEngine();
            string path = Path.Combine(scriptsPath, FolderName);
            Directory.CreateDirectory(path);
            string[] scriptFiles = Directory.GetFiles(path, $"*.{FileExtension}");
            foreach (var scriptFile in scriptFiles)
            {
                var method = GetMethod(scriptFile);
                if (method == null)
                {
                    Logger.Warn(Error);
                }
                else
                {
                    string filename = Path.GetFileName(scriptFile);
                    TextRepair.CustomMethodsDict[$"{Name} {filename}"] = method;
                }
            }
            ReleaseInitResources();
        }

        /// <summary>
        /// 初始化可重用资源
        /// </summary>
        protected virtual void InitEngine() { }

        /// <summary>
        /// 结束初始化，释放初始化使用的临时资源
        /// </summary>
        protected virtual void ReleaseInitResources() { }

        protected abstract TextPreProcessFunction? GetMethod(string scriptFile);
    }
}
