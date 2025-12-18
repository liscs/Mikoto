using Mikoto.ProcessInterop;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Mikoto.DataAccess
{
    public class GameInfoService : IGameInfoService
    {
        //游戏信息文件夹
        private readonly DirectoryInfo _gameInfoDirectory = Directory.CreateDirectory($"{DataFolder.Path}\\games\\");
        public Dictionary<Guid, GameInfo> AllCompletedGamesIdDict { get; set; } = new();
        public Dictionary<string, GameInfo> AllCompletedGamesPathDict { get; set; } = new();

        /// <summary>
        /// 得到一个游戏的GameInfo
        /// 如果游戏已经存在，则直接返回GameInfo，否则追加新游戏路径并返回新GameInfo
        /// </summary>
        public GameInfo GetGameByPath(string gamePath)
        {
            if (AllCompletedGamesPathDict.TryGetValue(gamePath, out GameInfo? value))
            {
                return value;
            }
            else
            {
                string name = Path.GetFileName(Path.GetDirectoryName(gamePath))!;
                Guid id = Guid.NewGuid();
                GameInfo gameInfo = new()
                {
                    GameID = id,
                    GameName = name,
                    FilePath = gamePath,
                };
                return gameInfo;
            }
        }


        /// <summary>
        /// 得到游戏库中所有有效游戏的信息。同时删除无效游戏信息。
        /// </summary>
        public void GetAllCompletedGames()
        {
            AllCompletedGamesIdDict.Clear();
            AllCompletedGamesPathDict.Clear();
            foreach (FileInfo fileInfo in _gameInfoDirectory.GetFiles())
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
        public bool DeleteGameByID(Guid gameID)
        {
            try
            {
                GameInfo gameInfo = AllCompletedGamesIdDict[gameID];
                File.Delete(GetGameInfoPath(gameInfo));
                return true;
            }
            catch (IOException ex)
            {
                Log.Error(ex, "删除游戏信息失败，GameID：{GameID}", gameID);
                return false;
            }
        }

        private string GetGameInfoPath(GameInfo gameInfo)
        {
            return $"{_gameInfoDirectory.FullName}\\{gameInfo.GameID}.json";
        }

        /// <summary>
        /// 修改游戏信息
        /// </summary>
        /// <param name="property">属性名</param>
        /// <param name="value">属性值</param>
        public bool UpdateGameInfoByID(Guid gameID, string property, object value)
        {
            if (AllCompletedGamesIdDict.TryGetValue(gameID, out GameInfo? gameInfo))
            {
                File.Delete(GetGameInfoPath(gameInfo));
                PropertyInfo? pinfo = typeof(GameInfo).GetProperty(property);
                if (pinfo == null)
                {
                    Log.Warning("更新游戏信息失败，未找到需要修改的属性：{Property}", property);
                    return false;
                }
                pinfo.SetValue(gameInfo, value);
                SaveGameInfo(gameInfo);
                return true;
            }
            else
            {
                Log.Warning("更新游戏信息失败，未找到对应的 GameID：{GameID}", gameID);
                return false;
            }
        }

        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        /// <summary>
        /// 保存GameInfo
        /// </summary>
        /// <param name="gameInfo"></param>
        /// <returns></returns>
        public void SaveGameInfo(GameInfo gameInfo)
        {
            string fileName = GetGameInfoPath(gameInfo);
            string jsonString = JsonSerializer.Serialize(gameInfo, options);
            File.WriteAllText(fileName, jsonString);
        }

        private bool TryLoadGameInfo(string path, [NotNullWhen(true)] out GameInfo? gameInfo)
        {
            gameInfo = null;
            try
            {
                string jsonString = File.ReadAllText(path);
                gameInfo = JsonSerializer.Deserialize<GameInfo>(jsonString, options);
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
