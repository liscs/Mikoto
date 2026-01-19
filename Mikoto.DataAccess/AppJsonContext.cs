using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Mikoto.DataAccess;


[JsonSourceGenerationOptions(
    WriteIndented = true, // 对应 options.WriteIndented = true
    ReadCommentHandling = JsonCommentHandling.Skip
)]
[JsonSerializable(typeof(GameInfo))] // 显式告诉 AOT 编译器：这个类要支持 JSON
[JsonSerializable(typeof(List<GameInfo>))]
internal partial class AppJsonContext : JsonSerializerContext
{
    // 定义一个私有静态变量来缓存实例
    private static AppJsonContext? _aotSafeContext;

    public static AppJsonContext AotSafeContext => _aotSafeContext ??= new AppJsonContext(new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        ReadCommentHandling = JsonCommentHandling.Skip
    });
}