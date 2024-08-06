using Microsoft.ClearScript.V8;
using System.IO;

namespace Mikoto.Helpers.Text
{
    internal class JsScriptInfo : ScriptInfo
    {
        protected override string Name { get => "JavaScript"; }
        protected override string FileExtension { get => "js"; }
        protected override string FolderName { get => "js"; }


        protected override TextPreProcessFunction? GetMethod(string scriptFile)
        {
            V8ScriptEngine engine = new();
            string script = File.ReadAllText(scriptFile);
            try
            {
                engine.Execute(scriptFile, script);
                string? functionName = (engine.Script.PropertyNames as string[])?.First(p => p != "gc");
                if (functionName != null)
                {
                    TextPreProcessFunction method = p => engine.Script[functionName](p);
                    return method;
                }
                else
                {
                    Error = $"{scriptFile} contains no function";
                    return null;
                }
            }
            catch (Exception ex)
            {
                Error = $"{scriptFile} Execute Error{Environment.NewLine}{ex}";
                return null;
            }


        }
    }


}

