using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace DictionaryHelperLibrary
{
    public class EbwinHelper
    {
        //Ebwin命令行用的词典路径信息的设置文件
        private static string EBPOCKET = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EBWin4\\EBPOCKET.GRP";

        //行列表
        private static List<string> EBPOCKETlist = new()
        {
            "%%UTF-8=1"
        };

        //字典信息文件夹
        public static readonly DirectoryInfo directory = Directory.CreateDirectory($"{Environment.CurrentDirectory}\\data\\dictionaries");


        public EbwinHelper()
        {
            //先在%appdata%\EBWin4\添加空的alternate.ini文件以防止报错
            //建立词典路径
            //%appdata%\EBWin4\EBPOCKET.GRP
            //EBPOCKET.GRP内容格式如下，采用UTF-8编码
            //%%UTF-8=1
            //C:\Users\dream\Desktop\新世纪日汉双解\xsjrihanshuangjie.mdx|_|_|_|_|_|
            //
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EBWin4"))
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EBWin4");
            File.Create(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\EBWin4\\alternate.ini").Close();
            var dList = GetAllDicts().ToList().OrderBy(d => d.Priority);
            foreach (var d in dList)
            {
                if (d.GetActive())
                {
                    EBPOCKETlist.Add($"{d.DictPath}|_|_|_|_|_|");
                }
            }
            File.WriteAllText(EBPOCKET, string.Join(Environment.NewLine, EBPOCKETlist));

        }

        public static void EnActive(Dict d)
        {
            if (d.Priority == 0 || d.Name == null) return;
            try
            {
                EBPOCKETlist[d.Priority] = $"{d.DictPath}|_|_|_|_|_|";
            }
            catch (ArgumentOutOfRangeException)
            {
                EBPOCKETlist.Add($"{d.DictPath}|_|_|_|_|_|");
            }
            File.WriteAllText(EBPOCKET, string.Join(Environment.NewLine, EBPOCKETlist));
            string fileName = $"{directory.FullName}\\{d.Name}.json";
            string jsonString = JsonSerializer.Serialize(d, options);
            File.WriteAllText(fileName, jsonString);
        }
        public static void InActive(Dict d)
        {
            if (d.Priority == 0 || d.Name == null) return;
            EBPOCKETlist[d.Priority] = " ";
            File.WriteAllText(EBPOCKET, string.Join(Environment.NewLine, EBPOCKETlist));
            string fileName = $"{directory.FullName}\\{d.Name}.json";
            string jsonString = JsonSerializer.Serialize(d, options);
            File.WriteAllText(fileName, jsonString);
        }

        /// <summary>
        /// 搜索词条，指定词典名为空时搜索所有词典，词典名匹配包含指定名称的词典
        /// </summary>
        public static string Search(string entry, string dictionaryName = "")
        {
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = $"{Environment.CurrentDirectory}\\ebwin4\\ebwinc.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    RedirectStandardError = true
                }
            };
            if (dictionaryName == "")
            {
                process.StartInfo.Arguments = $"-C=1 -N=20 -M=a {entry}";
            }
            else
            {
                Dictionary<string, int> dictionaries = new();
                using (FileStream grp = new FileStream(EBPOCKET, FileMode.Open))
                {
                    using (StreamReader streamReader = new StreamReader(grp))
                    {
                        streamReader.ReadLine();
                        int id = 0;
                        while (!streamReader.EndOfStream)
                        {
                            string? fileString = streamReader.ReadLine();
                            if (!string.IsNullOrWhiteSpace(fileString))
                            {
                                dictionaries.Add(fileString, id);
                                id++;
                            }
                        }
                    }
                }
                foreach (var v in dictionaries.Keys)
                {
                    if (v.Contains(dictionaryName))
                    {
                        process.StartInfo.Arguments = $"-C=1 -N=20 -M=a -#={dictionaries[v]} {entry}";
                    }
                }
            }


            process.Start();
            string outputData = string.Empty;
            string errorData = string.Empty;
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.OutputDataReceived += (sender, e) => outputData += (e.Data + Environment.NewLine);
            process.ErrorDataReceived += (sender, e) => errorData += (e.Data + Environment.NewLine);

            //等待退出  
            process.WaitForExit();

            //关闭进程  
            process.Close();

            //返回流结果  
            string output = outputData;
            string error = errorData;
            return output;

        }


        private static JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        };

        //适用于以下格式词典
        //CATALOG*;*.dic;*.idx;*.ebd;*.ifo;*.mdx;*.dsl;*.dz
        public static bool AddOrUpdateDictionary(Dict dict)
        {
            if (dict.Name == null) { return false; }
            EBPOCKETlist.RemoveAll(s => s.Contains($"{dict.DictPath}|_|_|_|_|_|"));
            if (dict.GetActive())
            {
                try
                {
                    EBPOCKETlist.Insert(dict.Priority, $"{dict.DictPath}|_|_|_|_|_|");
                }
                catch
                {
                    EBPOCKETlist.Add($"{dict.DictPath}|_|_|_|_|_|");
                    dict.Priority = EBPOCKETlist.Count;
                }
            }
            File.WriteAllText(EBPOCKET, string.Join(Environment.NewLine, EBPOCKETlist));

            string fileName = $"{directory.FullName}\\{dict.Name}.json";
            string jsonString = JsonSerializer.Serialize(dict, options);
            File.WriteAllText(fileName, jsonString);

            return true;
        }

        public static bool RemoveDictionary(Dict dict)
        {
            EBPOCKETlist.RemoveAll(s => s.Contains($"{dict.DictPath}|_|_|_|_|_|"));
            File.WriteAllText(EBPOCKET, string.Join(Environment.NewLine, EBPOCKETlist));

            string fileName = $"{directory.FullName}\\{dict.Name}.json";
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            return true;
        }

        public static HashSet<Dict> GetAllDicts()
        {
            HashSet<Dict> allDicts = new HashSet<Dict>();
            string[] files = Directory.GetFiles(directory.FullName);
            foreach (string file in files)
            {
                Dict? dict = GetDictInfo(Path.GetFileNameWithoutExtension(file));
                if (dict != null)
                {
                    allDicts.Add(dict);
                }
            }
            return allDicts;
        }

        private static Dict? GetDictInfo(string dictName)
        {
            string fileName = $"{directory.FullName}\\{dictName}.json";
            string jsonString = File.ReadAllText(fileName);
            return JsonSerializer.Deserialize<Dict>(jsonString);
        }


    }
}
