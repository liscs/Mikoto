using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace DataAccessLibrary
{
    public class GameInfo
    {
        /// <summary>
        /// 游戏名（非进程名，但在游戏名未知的情况下先使用进程所在的文件夹名替代）
        /// </summary>
        public string GameName { get; set; } = string.Empty;

        /// <summary>
        /// 游戏文件路径
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// 游戏ID
        /// </summary>
        public Guid GameID { get; set; }

        /// <summary>
        /// 翻译模式,1=hook 2=ocr
        /// </summary>
        public int TransMode { get; set; }

        /// <summary>
        /// 源语言代码，同翻译API中语言代码
        /// </summary>
        public string SrcLang { get; set; } = string.Empty;

        /// <summary>
        /// 目标语言代码，同翻译API中语言代码
        /// </summary>
        public string DstLang { get; set; } = string.Empty;

        /// <summary>
        /// 去重方法，仅在hook模式有效
        /// </summary>
        public string? RepairFunc { get; set; }

        /// <summary>
        /// 去重方法所需参数1，仅在hook模式有效
        /// </summary>
        public string? RepairParamA { get; set; }

        /// <summary>
        /// 去重方法所需参数2，仅在hook模式有效
        /// </summary>
        public string? RepairParamB { get; set; }

        /// <summary>
        /// 特殊码值，仅在hook模式有效
        /// </summary>
        public string? HookCode { get; set; }

        /// <summary>
        /// 用户自定的特殊码值，如果用户这一项不是自定义的，那么应该为'NULL'，仅在hook模式有效，注意下次开启游戏时这里就需要注入一下
        /// </summary>
        public string? HookCodeCustom { get; set; }

        /// <summary>
        /// 检查是否是64位应用程序
        /// </summary>
        public bool Isx64 { get; set; }

        /// <summary>
        /// 包含hook地址信息的本软件特有的MisakaHookCode
        /// </summary>
        public string? MisakaHookCode { get; set; }

        public bool Cleared { get; set; } = false;
    }

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
            try
            {
                return AllCompletedGamesPathDict[gamePath];
            }
            catch (KeyNotFoundException)
            {
                return AddGame(gamePath);
            }
        }

        private static GameInfo AddGame(string gamePath)
        {
            string name = Path.GetFileName(Path.GetDirectoryName(gamePath)) ?? string.Empty;
            Guid id = Guid.NewGuid();
            GameInfo gameInfo = new GameInfo()
            {
                GameID = id,
                GameName = name,
                FilePath = gamePath,
            };
            return gameInfo;
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
                GameInfo gameInfo = LoadGameInfo(fileInfo.FullName);
                if (string.IsNullOrEmpty(gameInfo.RepairFunc)
                    || string.IsNullOrEmpty(gameInfo.HookCode)
                    )
                {
                    File.Delete(fileInfo.FullName);
                    continue;
                }
                list.Add(gameInfo);
                AllCompletedGamesIdDict.Add(gameInfo.GameID, gameInfo);
                AllCompletedGamesPathDict.Add(gameInfo.FilePath, gameInfo);
            }
            return list.OrderByDescending(p => p.Cleared).ThenBy(p => p.GameName).ToList();
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
            try
            {
                GameInfo gameInfo = AllCompletedGamesIdDict[gameID];
                File.Delete($"{_gameInfoDirectory.FullName}\\{gameInfo.GameID}.json");
                PropertyInfo? pinfo = typeof(GameInfo).GetProperty(key);
                if (pinfo == null) return false;
                pinfo.SetValue(gameInfo, value);
                SaveGameInfo(gameInfo);
                return true;
            }
            catch (KeyNotFoundException)
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

        private static GameInfo LoadGameInfo(string path)
        {
            string jsonString = File.ReadAllText(path);
            GameInfo gameInfo = JsonSerializer.Deserialize<GameInfo>(jsonString, options)!;
            return gameInfo;
        }
    }
}
