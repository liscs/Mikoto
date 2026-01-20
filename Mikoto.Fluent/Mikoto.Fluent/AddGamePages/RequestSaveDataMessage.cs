using Mikoto.DataAccess;

namespace Mikoto.Fluent.AddGamePages
{
    // 指令：请存数据
    public record RequestSaveDataMessage(GameInfo Config, Action<bool> OnResult);
}