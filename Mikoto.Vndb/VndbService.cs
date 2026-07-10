using Mikoto.DataAccess;
using Serilog;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;


namespace Mikoto.Vndb;

// 定义 API 请求体类
public class VndbApiRequest
{
    public object[] filters { get; set; } = [];
    public string fields { get; set; } = string.Empty;
}

// 定义 JSON 上下文
[JsonSerializable(typeof(VndbApiRequest))]
[JsonSerializable(typeof(VndbApiResponse))]
internal partial class AppJsonContext : JsonSerializerContext
{
}

public class VndbService
{
    private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };

    public static async Task<GameMetadata> QueryDataAsync(GameInfo gameInfo)
    {
        // 构造备选搜索词列表
        var candidates = GetSearchCandidates(gameInfo);

        foreach (var term in candidates)
        {
            var result = await CallVndbApi(term);
            if (result != null) return result;

            await Task.Delay(300);
        }

        return new GameMetadata(); // 全部失败返回空对象
    }

    private static List<string> GetSearchCandidates(GameInfo info)
    {
        var list = new List<string>();
        list.Add(info.GameName);
        list.Add(CleanString(info.GameName));

        // 获取路径各级信息
        string fileName = Path.GetFileNameWithoutExtension(info.FilePath);
        var dirPath = Path.GetDirectoryName(info.FilePath);
        DirectoryInfo? dir = null;
        if (dirPath != null)
        {
            dir = new DirectoryInfo(dirPath);
        }
        string dirName = dir?.Name ?? "";
        string parentDirName = dir?.Parent?.Name ?? "";

        // 1. 优先处理文件名（清洗掉 _CRACK, _hd 等）
        string cleanFile = CleanString(fileName);
        if (!IsGenericName(cleanFile)) list.Add(cleanFile);

        // 2. 其次处理文件夹名
        string cleanDir = CleanString(dirName);
        if (!string.IsNullOrEmpty(cleanDir)) list.Add(cleanDir);

        // 4. 如果文件夹名是纯数字，把父目录加进来
        if (Regex.IsMatch(cleanDir, @"^\d+$") && !string.IsNullOrEmpty(parentDirName))
            list.Add(CleanString(parentDirName));

        return list.Distinct().ToList();
    }


    private static async Task<GameMetadata?> CallVndbApi(string term)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Length < 2) return null;

        // 建议在外部初始化一次，而不是每次调用都设置
        _httpClient.DefaultRequestHeaders.UserAgent.Clear();
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mikoto");

        var requestBody = new VndbApiRequest
        {
            filters = new object[] { "search", "=", term },
            // 增加：released (日期), developers.name (开发商), screens.url (截图)
            fields = "title, alttitle, image.url, rating, description, released, developers.name"
        };

        try
        {
            // 使用 Source Generation 的重载版本
            var response = await _httpClient.PostAsJsonAsync(
                "https://api.vndb.org/kana/vn",
                requestBody,
                AppJsonContext.Default.VndbApiRequest);

            if (response.IsSuccessStatusCode)
            {
                // 同样使用 Source Generation
                var data = await response.Content.ReadFromJsonAsync(
                    AppJsonContext.Default.VndbApiResponse);

                var res = data?.results?.FirstOrDefault();
                if (res != null)
                {
                    return new GameMetadata
                    {
                        VndbId = res.id,
                        Title = res.title,
                        NativeTitle = res.alttitle??res.title,
                        Score = res.rating ?? 0,
                        CoverUrl = res.image?.url,
                        Description = res.description,
                        ReleaseDate = res.released,
                        Developer = string.Join(", ", res.developers?.Select(p => p.name)?? [])
                    };
                }
            }
        }
        catch(Exception ex)
        {
            Log.Error(ex,"获取 VNDB 信息失败");
        }
        return null;
    }

    private static string CleanString(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";

        // 去掉 [组名], (日期), 【豪华版】等
        string s = Regex.Replace(input, @"[\[\(\{（【].*?[\]\)\}）】]", "");

        // 去掉常见后缀
        string[] noise = ["特典付き版", "DL版", "特典", "豪華版", "CHS", "汉化", "_hd", "_re", "_ar", "_CRACK",];
        foreach (var n in noise) s = s.Replace(n, "", StringComparison.OrdinalIgnoreCase);

        // 处理 .exe.xxx 这种双后缀
        if (s.Contains(".exe")) s = s.Split(".exe")[0];

        return s.Trim(' ', '-', '_', '.');
    }

    private static bool IsGenericName(string name)
    {
        string[] blacklist = { "BGI", "SiglusEngine", "rio", "start", "game", "Setup", "DC4PHDL" };
        return blacklist.Any(b => name.Equals(b, StringComparison.OrdinalIgnoreCase)) || name.Length < 3;
    }
}

// API 返回的最外层包装
internal class VndbApiResponse
{
    public List<VndbResult>? results { get; set; }
    public bool more { get; set; }
}

// 单条结果：使用 class 或 record 均可，但建议属性可空以防 API 返回缺失
internal record VndbResult(
    string id,
    string title,
    string? alttitle,
    float? rating,
    string? description,
    string? released,                 // "2024-05-15"
    List<VndbDeveloper>? developers,  // 开发商列表
    List<VndbScreenshot>? screens,    // 游戏截图列表
    VndbImage? image
);

// 图像信息
internal class VndbImage
{
    public string url { get; set; } = string.Empty;
}

internal record VndbDeveloper(string name);
internal record VndbScreenshot(string url);