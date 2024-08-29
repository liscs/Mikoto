namespace Mikoto.Helpers.Text.ScriptInfos
{
    internal class CSharpScriptInfo : ScriptInfo
    {
        protected override string Name { get => "C#"; }
        protected override string FileExtension { get => "cs"; }
        protected override string FolderName { get => "csharp"; }

        protected override TextPreProcessFunction? GetMethod(string scriptFile)
        {
            try
            {
                string script = System.IO.File.ReadAllText(scriptFile);
                return CSharpCompilerHelper.GetProcessFunction(script);
            }
            catch (Exception ex)
            {
                Error = $"{scriptFile} Execute Error{Environment.NewLine}{ex}";
                return null;
            }
        }
    }
}
