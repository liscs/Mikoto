using Mikoto.DataAccess;

namespace Mikoto.Core.ViewModels.AddGame
{
    // 指令：请存数据
    public record RequestSaveDataMessage(GameInfo Config, Action<bool> OnResult);
}