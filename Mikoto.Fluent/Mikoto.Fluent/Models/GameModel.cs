using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Mikoto.DataAccess;

namespace Mikoto.Fluent
{
    public partial class GameModel : ObservableObject
    {
        // 在加载游戏列表时，把 ViewModel 的引用塞给每个模型
        public HomeViewModel? Parent { get; init; }

        [ObservableProperty]
        public partial string GameName { get; set; }

        [ObservableProperty]
        public partial ImageSource? GameIcon { get; set; }

        [ObservableProperty]
        public partial DateTime LastPlayAt { get; set; }

        [ObservableProperty]
        public partial string ExePath { get; set; }


        public GameInfo ToEntity()
        {
            // 1. 尝试从全局字典中获取现有的实体对象
            if (!App.Env.GameInfoService.AllCompletedGamesPathDict.TryGetValue(ExePath, out GameInfo? game))
            {
                // 如果字典里没有，说明是新游戏，创建一个新实例
                game = new GameInfo();
            }

            // 2. 将 UI 层（Model）的最新修改同步到实体（Entity）中
            game.GameName = this.GameName;
            game.FilePath = this.ExePath;
            game.LastPlayAt = this.LastPlayAt;

            // 如果有其他 UI 特有的属性，比如翻译名称，也在这里同步
            // game.TranslationName = this.TranslatedName;

            return game;
        }
    }
}