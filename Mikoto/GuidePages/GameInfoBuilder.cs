using Mikoto.DataAccess;

namespace Mikoto.GuidePages
{
    public class GameInfoBuilder
    {
        public void Reset()
        {
            GameInfo = new GameInfo();
            GameProcessId = -1;
        }
        public GameInfo GameInfo { get; set; } = new GameInfo();
        public int GameProcessId { get; set; }
    }
}
