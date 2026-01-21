using Mikoto.DataAccess;
using System.Diagnostics;

namespace Mikoto.Core
{
    internal class GameProcessHelper
    {
        internal static int GetGamePid(GameInfo currentGame)
        {
            string name;
            if (Path.GetExtension(currentGame.FilePath).Equals(".exe", StringComparison.OrdinalIgnoreCase))
            {
                name = Path.GetFileNameWithoutExtension(currentGame.FilePath);
            }
            else
            {
                name = Path.GetFileName(currentGame.FilePath);
            }

            List<Process> gameProcessList = Process.GetProcessesByName(name).ToList();
            if (gameProcessList.Count == 0)
            {
                throw new Exception("Game process not found.");
            }
            return gameProcessList[0].Id;
        }
    }
}
