
namespace Mikoto.DataAccess
{
    public interface IGameInfoService
    {
        Dictionary<Guid, GameInfo> AllCompletedGamesIdDict { get; set; }
        Dictionary<string, GameInfo> AllCompletedGamesPathDict { get; set; }
        bool DeleteGameInfo(GameInfo gameInfo);
        void GetAllCompletedGames();
        void SaveGameInfo(GameInfo gameInfo);
        GameInfo? GetRunningGame();
        void OpenGameInfoFile(GameInfo gameInfo);
    }
}