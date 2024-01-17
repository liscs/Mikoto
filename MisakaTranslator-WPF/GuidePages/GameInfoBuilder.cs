using DataAccessLibrary;

namespace MisakaTranslator.GuidePages
{
    internal static class GameInfoBuilder
    {
        static GameInfoBuilder()
        {
            GameInfo = new GameInfo();
        }
        public static GameInfo GameInfo { get; set; }
    }
}
