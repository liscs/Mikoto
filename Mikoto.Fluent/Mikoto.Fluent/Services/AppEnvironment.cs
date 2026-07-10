using Microsoft.Extensions.Configuration;
using Mikoto.Config;
using Mikoto.Core.Interfaces;
using Mikoto.DataAccess;
using Mikoto.Resource;
using Mikoto.TextHook;
using Serilog;
using System.Text;

namespace Mikoto.Fluent.Services;

public class AppEnvironment : IAppEnvironment
{
    public IGameInfoService GameInfoService { get; } = new GameInfoService();
    public ITextHookService TextHookService { get; } = new TextHookService() { HandleMode = 1 };
    public IAppSettings AppSettings { get; }

    public IResourceService ResourceService { get; } = new WinUIResourceService();

    public IMainThreadService MainThreadService { get; } = new WinUIMainThreadService();

    public AppEnvironment()
    {
        // 1. 确定路径
        string iniPath = Path.Combine(DataFolder.Path, "settings", "settings.ini");

        // 1. 自动修复并获取结构化数据
        Dictionary<string, string?> sanitizedData = FixAndLoadIni(iniPath);

        // 2. 构建配置源
        var config = new ConfigurationBuilder()
                .AddInMemoryCollection(sanitizedData)
                .Build();

        // 3. 传入构造函数
        AppSettings = new AotSettings(config, iniPath);
    }

    private static Dictionary<string, string?> FixAndLoadIni(string path)
    {
        // Dictionary<SectionName, Dictionary<Key, Value>>
        // 外层和内层字典都使用 OrdinalIgnoreCase 来忽略大小写冲突
        var sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        var flatData = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        if (!File.Exists(path)) return flatData;

        // --- 步骤 1: 读取并去重 ---
        string currentSection = "Default"; // 默认 Section
        foreach (var line in File.ReadAllLines(path))
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(';') || trimmed.StartsWith('#')) continue;

            if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
            {
                currentSection = trimmed.Substring(1, trimmed.Length - 2).Trim();
            }
            else
            {
                int idx = trimmed.IndexOf('=');
                if (idx > 0)
                {
                    string key = trimmed.Substring(0, idx).Trim();
                    string val = trimmed.Substring(idx + 1).Trim();

                    if (!sections.ContainsKey(currentSection))
                        sections[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    // 重点：如果大小写不同，后面的会覆盖前面的，达到去重效果
                    sections[currentSection][key] = val;

                    // 同时填充给 ConfigurationBuilder 使用的扁平化字典
                    flatData[$"{currentSection}:{key}"] = val;
                }
            }
        }

        // --- 步骤 2: 规范化写回文件 (自动修复) ---
        try
        {
            StringBuilder sb = new StringBuilder();
            foreach (var section in sections)
            {
                sb.AppendLine($"[{section.Key}]");
                foreach (var kvp in section.Value)
                {
                    sb.AppendLine($"{kvp.Key}={kvp.Value}");
                }
                sb.AppendLine(); // Section 之间留空行
            }

            // 只有内容真的发生变化时（或文件包含乱七八糟的重复时）才写入
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }
        catch (Exception ex)
        {
            // 记录日志，但不应阻断程序启动
            Log.Error(ex, "INI Fix Failed: {Message}", ex.Message);
        }

        return flatData;
    }
}
