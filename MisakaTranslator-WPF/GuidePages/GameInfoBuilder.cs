using DataAccessLibrary;

namespace MisakaTranslator.GuidePages
{
    internal static class GameInfoBuilder
    {
        public static void Reset()
        {
            GameInfo = new GameInfo();
            GameProcessId = -1;
        }
        public static GameInfo GameInfo { get; set; } = new GameInfo();
        public static int GameProcessId { get; set; }
    }
}
