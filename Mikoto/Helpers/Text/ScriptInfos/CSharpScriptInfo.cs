namespace Mikoto.Helpers.Text
{
    internal class CSharpScriptInfo : ScriptInfo
    {
        protected override string Name { get => "C#"; }
        protected override string FileExtension { get => "cs"; }
        protected override string FolderName { get => "csharp"; }

        protected override TextPreProcesFunction? GetMethod(string scriptFile)
        {
            try
            {
                return CSharpCompilerHelper.GetProcessFunction(scriptFile);

            }
            catch (Exception ex)
            {
                Error = $"{scriptFile} Execute Error{Environment.NewLine}{ex}";
                return null;
            }
        }

        protected override void InitEngine()
        {
        }

        protected override void ReleaseResourse()
        {
            CSharpCompilerHelper.References.Clear();
        }
    }
}
