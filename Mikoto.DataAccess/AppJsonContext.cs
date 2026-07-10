using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Mikoto.DataAccess;



[JsonSerializable(typeof(GameInfo))]
[JsonSerializable(typeof(List<GameInfo>))]
internal partial class AppJsonContext : JsonSerializerContext
{
    private static AppJsonContext? _aotSafeContext;
    public static AppJsonContext AotSafeContext => _aotSafeContext ??= new AppJsonContext(new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        ReadCommentHandling = JsonCommentHandling.Skip
    });
}