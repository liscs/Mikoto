using System.IO;

namespace Mikoto.Helpers.Text
{
    internal abstract class ScriptInfo
    {
        protected ScriptInfo() { }

        private const string SCRIPTS_PATH = "data\\custom scripts\\";

        protected abstract string Name { get; }
        protected abstract string FileExtension { get; }
        protected abstract string FolderName { get; }
        protected string Error { get; set; } = string.Empty;

        public void Init()
        {
            InitEngine();
            string csPath = Path.Combine(SCRIPTS_PATH, FolderName);
            Directory.CreateDirectory(csPath);
            string[] cSharpScriptFiles = Directory.GetFiles(csPath, $"*.{FileExtension}");
            foreach (var scriptFile in cSharpScriptFiles)
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
                    TextRepair.RepairFunctionNameDict.Value[$"{Name} {filename}"] = $"{Name} {filename}";
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
