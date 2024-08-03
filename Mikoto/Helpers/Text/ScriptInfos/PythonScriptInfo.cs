using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;
using System.IO;

namespace Mikoto.Helpers.Text
{
    internal class PythonScriptInfo : ScriptInfo
    {
        protected override string Name { get => "Python"; }
        protected override string FileExtension { get => "py"; }
        protected override string FolderName { get => "python"; }

        private ScriptEngine _engine = default!;
        private ScriptScope _scope = default!;

        protected override void InitEngine()
        {
            _engine = Python.CreateEngine();
            _scope = _engine.CreateScope();
        }

        protected override TextPreProcesFunction? GetMethod(string scriptFile)
        {
            string script = File.ReadAllText(scriptFile);
            try
            {
                _engine.Execute(script, _scope);
                dynamic pythonFunction = _scope.GetItems().Select(p => p.Value).First(p => p is PythonFunction);
                TextPreProcesFunction method = p => pythonFunction(p);
                return method;
            }
            catch (Exception ex)
            {
                Error = $"{scriptFile} Execute Error{Environment.NewLine}{ex}";
                return null;
            }

        }

        protected override void ReleaseResourse()
        {
        }
    }
}
