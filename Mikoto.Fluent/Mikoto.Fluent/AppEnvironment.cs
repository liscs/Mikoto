using Microsoft.Extensions.Configuration;
using Mikoto.Config;
using Mikoto.DataAccess;
using Mikoto.TextHook;

namespace Mikoto.Fluent;

public class AppEnvironment
{
    public IGameInfoService GameInfoService { get; } = new GameInfoService();
    public ITextHookService TextHookService { get; set; } = new TextHookService();
    public IAppSettings AppSettings { get; }

    public AppEnvironment()
    {
        // 1. 确定路径
        string iniPath = Path.Combine(DataFolder.Path, "settings", "settings.ini");

        // 2. 构建配置源
        var config = new ConfigurationBuilder()
            .AddIniFile(iniPath, optional: true)
            .Build();

        // 3. 传入构造函数
        AppSettings = new AotSettings(config, iniPath);
    }
}
