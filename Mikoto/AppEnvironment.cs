using Mikoto.Core;
using Mikoto.DataAccess;
using Mikoto.TextHook;

namespace Mikoto;

public class AppEnvironment
{
    public TranslationContext Context { get; }
    public ITextHookService TextHookService { get; set; }
    public IResourceService ResourceService { get; }
    public IFileService FileService { get; }
    public HistoryExporter HistoryExporter { get; }
    public IGameInfoService GameInfoService { get; } = new GameInfoService();

    public AppEnvironment()
    {
        Context = new TranslationContext();
        TextHookService = new TextHookService();
        ResourceService = new WpfResourceService();
        FileService = new FileService();

        HistoryExporter = new HistoryExporter(
            TextHookService,
            FileService,
            ResourceService);
    }
}
