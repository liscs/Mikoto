using Mikoto.ProcessInterop;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Mikoto.DataAccess
{
    public class GameInfoService : IGameInfoService
    {
        //游戏信息文件夹
        private static readonly DirectoryInfo gameInfoDirectory = Directory.CreateDirectory($"{DataFolder.Path}\\games\\");
        public Dictionary<Guid, GameInfo> AllCompletedGamesIdDict { get; set; } = new();
        public Dictionary<string, GameInfo> AllCompletedGamesPathDict { get; set; } = new();

        /// <summary>
        /// 得到游戏库中所有有效游戏的信息。同时删除无效游戏信息。
        /// </summary>
        public void GetAllCompletedGames()
        {
            AllCompletedGamesIdDict.Clear();
            AllCompletedGamesPathDict.Clear();
            foreach (FileInfo fileInfo in gameInfoDirectory.GetFiles())
            {
                if (TryLoadGameInfo(fileInfo.FullName, out GameInfo? gameInfo))
                {
                    if (string.IsNullOrEmpty(gameInfo.RepairFunc)
                        || string.IsNullOrEmpty(gameInfo.HookCode))
                    {
                        File.Delete(fileInfo.FullName);
                        Log.Warning("删除无效游戏信息，路径：{FullName}", fileInfo.FullName);
                        continue;
                    }
                    AllCompletedGamesIdDict.Add(gameInfo.GameID, gameInfo);
                    AllCompletedGamesPathDict.Add(gameInfo.FilePath, gameInfo);
                }
            }
        }

        /// <summary>
        /// 删除游戏信息
        /// </summary>
        public bool DeleteGameInfo(GameInfo gameInfo)
        {
            var path = GetGameInfoPath(gameInfo);
            try
            {
                File.Delete(path);
                return true;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Log.Error(ex, "删除游戏信息失败。GameName: {GameName}, Path: {Path}", gameInfo.GameName, path);
                return false;
            }
        }

        private static string GetGameInfoPath(GameInfo gameInfo)
        {
            return $"{gameInfoDirectory.FullName}\\{gameInfo.GameID}.json";
        }

        public void OpenGameInfoFile(GameInfo gameInfo)
        {
            string gameInfoFilePath = GetGameInfoPath(gameInfo);
            ProcessHelper.ShellStart(gameInfoFilePath);
        }

        /// <summary>
        /// 保存GameInfo
        /// </summary>
        /// <param name="gameInfo"></param>
        /// <returns></returns>
        public void SaveGameInfo(GameInfo gameInfo)
        {
            string fileName = GetGameInfoPath(gameInfo);
            string jsonString = JsonSerializer.Serialize(gameInfo, AppJsonContext.Default.GameInfo);
            File.WriteAllText(fileName, jsonString);
        }

        private bool TryLoadGameInfo(string path, [NotNullWhen(true)] out GameInfo? gameInfo)
        {
            gameInfo = null;
            try
            {
                string jsonString = File.ReadAllText(path);
                gameInfo = JsonSerializer.Deserialize(jsonString, AppJsonContext.Default.GameInfo);
                if (gameInfo != null)
                {
                    return true;
                }
                else
                {
                    Log.Warning("从 '{Path}' 反序列化 GameInfo 得到 null，可能文件为空或所有字段都不匹配", path);
                    return false;
                }
            }
            catch (Exception ex) when (ex is IOException or JsonException)
            {
                Log.Warning(ex, "加载 GameInfo 失败，路径：{Path}", path);
                return false;
            }
        }

        /// <summary>
        /// 寻找任何正在运行中的之前已保存过的游戏
        /// </summary>
        public GameInfo? GetRunningGame()
        {
            foreach (string path in ProcessHelper.GetAppPaths())
            {
                if (AllCompletedGamesPathDict.TryGetValue(path, out var result))
                {
                    return result;
                }
            }
            Log.Information("未找到任何正在运行的已保存游戏");
            return null;
        }
    }
}
