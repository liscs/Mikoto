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
                    TextRepair.CustomMethodsDict[$"{Name} " + filename] = method;
                    TextRepair.LstRepairFun.Value[$"{Name} " + filename] = $"{Name} " + filename;
                }


            }
            ReleaseResourse();
        }

        protected abstract void InitEngine();

        protected abstract void ReleaseResourse();

        protected abstract TextPreProcesFunction? GetMethod(string scriptFile);
    }
}
