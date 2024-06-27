using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace DataAccessLibrary
{
    public static class GameHelper
    {
        //游戏信息文件夹
        private static readonly DirectoryInfo _gameInfoDirectory = Directory.CreateDirectory($"{Environment.CurrentDirectory}\\data\\games\\");
        public static Dictionary<Guid, GameInfo> AllCompletedGamesIdDict { get; set; } = new();
        public static Dictionary<string, GameInfo> AllCompletedGamesPathDict { get; set; } = new();

        /// <summary>
        /// 得到一个游戏的GameInfo
        /// 如果游戏已经存在，则直接返回GameInfo，否则追加新游戏路径并返回新GameInfo
        /// </summary>
        public static GameInfo GetGameByPath(string gamePath)
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
        /// 得到游戏库中所有有效游戏的信息
        /// </summary>
        public static List<GameInfo> GetAllCompletedGames()
        {
            List<GameInfo> list = new();
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
                        continue;
                    }
                    list.Add(gameInfo);
                    AllCompletedGamesIdDict.Add(gameInfo.GameID, gameInfo);
                    AllCompletedGamesPathDict.Add(gameInfo.FilePath, gameInfo);
                }
            }
            return list.OrderByDescending(p => p.LastPlayAt).ThenByDescending(p => p.GameName).ToList();
        }



        /// <summary>
        /// 删除游戏信息
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public static bool DeleteGameByID(Guid gameID)
        {
            try
            {
                GameInfo gameInfo = AllCompletedGamesIdDict[gameID];
                File.Delete($"{_gameInfoDirectory.FullName}\\{gameInfo.GameID}.json");
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        /// <summary>
        /// 修改游戏信息
        /// </summary>
        /// <param name="key">属性名</param>
        /// <param name="value">属性值</param>
        public static bool UpdateGameInfoByID(Guid gameID, string key, object value)
        {
            if (AllCompletedGamesIdDict.TryGetValue(gameID, out GameInfo? gameInfo))
            {
                File.Delete($"{_gameInfoDirectory.FullName}\\{gameInfo.GameID}.json");
                PropertyInfo? pinfo = typeof(GameInfo).GetProperty(key);
                if (pinfo == null) return false;
                pinfo.SetValue(gameInfo, value);
                SaveGameInfo(gameInfo);
                return true;
            }
            else
            {
                return false;
            }
        }

        private static readonly JsonSerializerOptions options = new JsonSerializerOptions
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
        public static void SaveGameInfo(GameInfo gameInfo)
        {
            string fileName = $"{_gameInfoDirectory.FullName}\\{gameInfo.GameID}.json";
            string jsonString = JsonSerializer.Serialize(gameInfo, options);
            File.WriteAllText(fileName, jsonString);
        }

        private static bool TryLoadGameInfo(string path, [NotNullWhen(true)] out GameInfo? gameInfo)
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
                    return false;
                }
            }
            catch (Exception ex) when (ex is IOException or JsonException)
            {
                return false;
            }
        }
    }
}
