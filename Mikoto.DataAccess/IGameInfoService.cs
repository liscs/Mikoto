
namespace Mikoto.DataAccess
{
    public interface IGameInfoService
    {
        Dictionary<Guid, GameInfo> AllCompletedGamesIdDict { get; set; }
        Dictionary<string, GameInfo> AllCompletedGamesPathDict { get; set; }
        bool DeleteGameByID(Guid gameID);
        void GetAllCompletedGames();
        GameInfo GetGameByPath(string gamePath);
        void SaveGameInfo(GameInfo gameInfo);
        bool UpdateGameInfoByID(Guid gameID, string key, object value);
        GameInfo? GetRunningGame();
        void OpenGameInfoFile(GameInfo gameInfo);
    }
}