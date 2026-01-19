using Config.Net;
using Mikoto.Config;
using Mikoto.DataAccess;
using Mikoto.TextHook;

namespace Mikoto.Fluent;

public class AppEnvironment
{
    public IGameInfoService GameInfoService { get; } = new GameInfoService();
    public ITextHookService TextHookService { get; set; } = new TextHookService();
    public IAppSettings AppSettings { get; } = new ConfigurationBuilder<IAppSettings>().UseIniFile(Path.Combine(DataFolder.Path, "settings", "settings.ini")).Build();

    public AppEnvironment()
    {
    }
}
