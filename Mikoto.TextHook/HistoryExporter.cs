using Mikoto.DataAccess;
using Mikoto.Resource;

namespace Mikoto.TextHook;

public class HistoryExporter(
    ITextHookService hook,
    IFileService file,
    IResourceService res)
{
    public bool Export(string path = "TextractorOutPutHistory.txt")
    {
        try
        {
            var lines = new List<string>
            {
                res.Get("Common_TextractorHistory")
            };

            lines.AddRange(hook.TextractorOutPutHistory);

            file.WriteAllLines(path, lines);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
