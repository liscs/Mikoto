using Mikoto.DataAccess;
using Mikoto.Helpers.File;
using Mikoto.Windows.Logger;
using System.IO;

namespace Mikoto.Helpers.Text.ScriptInfos
{
    internal abstract class ScriptInfo
    {
        protected ScriptInfo() { }

        private string scriptsPath = Path.Combine(DataFolder.Path, "custom scripts");

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
        }


        //事件可能会在短时间内引发多次，需要添加防抖机制
        private DateTime lastRead = DateTime.MinValue;
        private string lastFile = string.Empty;
        //避免被回收的引用
        private static readonly List<FileSystemWatcher> fileSystemWatchers = new List<FileSystemWatcher>();
        private void AddFileSystemListener(string path)
        {
            // 创建一个FileSystemWatcher对象并设置要监控的目录
            FileSystemWatcher watcher = new(path, $"*.{FileExtension}");
            fileSystemWatchers.Add(watcher);

            // 监听文件创建、删除、修改和重命名事件
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Changed += OnChanged;
            watcher.Renamed += OnChanged;

            // 开始监控
            watcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            DateTime currentRead = DateTime.Now;
            const int DEBOUNCING_TIME = 500;
            if ((currentRead - lastRead).TotalMilliseconds > DEBOUNCING_TIME ||
                lastFile != e.FullPath)
            {
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
                        string scriptFile = ((RenamedEventArgs)e).OldFullPath;
                        //卸载旧文件
                        RemoveMethod(scriptFile);
                        AddMethod(e.FullPath);
                        break;
                }

                TextRepair.RefreshListOrder();
                lastRead = currentRead;
                lastFile = e.FullPath;
            }
        }
        private void AddMethod(string scriptFile)
        {
            //需要先加载依赖
            WaitFileHelper.WaitUntilReadable(scriptFile);
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

        protected abstract TextPreProcessFunction? GetMethod(string scriptFile);
    }
}
