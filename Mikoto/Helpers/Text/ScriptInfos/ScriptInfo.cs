using Mikoto.Windows.Logger;
using System.IO;

namespace Mikoto.Helpers.Text.ScriptInfos
{
    internal abstract class ScriptInfo
    {
        protected ScriptInfo() { }

        private string scriptsPath = Path.Combine(Common.DataFolder, "custom scripts");

        protected abstract string Name { get; }
        protected abstract string FileExtension { get; }
        protected abstract string FolderName { get; }
        protected string Error { get; set; } = string.Empty;

        public void Init()
        {
            string path = Path.Combine(scriptsPath, FolderName);
            Directory.CreateDirectory(path);
            AddFileSystemListener(path);
            InitEngine();
            string[] scriptFiles = Directory.GetFiles(path, $"*.{FileExtension}");
            foreach (var scriptFile in scriptFiles)
            {
                AddMethod(scriptFile);
            }
            ReleaseInitResources();
        }

        private static List<FileSystemWatcher> fileSystemWatchers = new List<FileSystemWatcher>();
        private void AddFileSystemListener(string path)
        {
            // 创建一个FileSystemWatcher对象并设置要监控的目录
            FileSystemWatcher watcher = new FileSystemWatcher(path);
            fileSystemWatchers.Add(watcher);
            void OnChanged(object source, FileSystemEventArgs e)
            {
                Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");

                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Created:
                        AddMethod(e.FullPath);
                        break;
                    case WatcherChangeTypes.Deleted:
                        RemoveMethod(e.FullPath);
                        break;
                    case WatcherChangeTypes.Changed:
                        RemoveMethod(e.FullPath);
                        AddMethod(e.FullPath);
                        break;
                    case WatcherChangeTypes.Renamed:
                        string scriptFile = (e as RenamedEventArgs)!.OldFullPath;
                        //卸载旧文件
                        RemoveMethod(scriptFile);
                        AddMethod(e.FullPath);
                        break;
                }

                TextRepair.RefreshListOrder();
            }

            // 监听文件创建、删除、修改和重命名事件
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Changed += OnChanged;
            watcher.Renamed += OnChanged;

            // 开始监控
            watcher.EnableRaisingEvents = true;
        }
        private void AddMethod(string scriptFile)
        {
            //需要先加载依赖
            var method = GetMethod(scriptFile);
            if (method == null)
            {
                Logger.Warn(Error);
            }
            else
            {
                string filename = Path.GetFileName(scriptFile);
                TextRepair.CustomMethodsDict[$"{Name} {filename}"] = method;
            }
        }

        private void RemoveMethod(string scriptFile)
        {
            string filename = Path.GetFileName(scriptFile);
            TextRepair.CustomMethodsDict.Remove($"{Name} {filename}");
        }

        /// <summary>
        /// 初始化可重用资源
        /// </summary>
        protected virtual void InitEngine() { }

        /// <summary>
        /// 结束初始化，释放初始化使用的临时资源
        /// </summary>
        protected virtual void ReleaseInitResources() { }

        protected abstract TextPreProcessFunction? GetMethod(string scriptFile);
    }
}
