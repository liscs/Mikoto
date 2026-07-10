using Microsoft.Windows.ApplicationModel.Resources;
using Mikoto.Resource;
using Serilog;

namespace Mikoto.Fluent.Services
{
    internal class WinUIResourceService : IResourceService
    {
        // 使用单例加载器，避免重复创建
        private readonly ResourceLoader _loader = new();

        public string Get(string key)
        {
            // 1. 优先尝试从 .resw 文件读取 (标准本地化路径)
            try
            {
                string value = _loader.GetString(key);
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }
            catch(Exception ex)
            {
                Log.Warning(ex, "读取资源失败，键：{Key}", key);
                // 资源不存在时 ResourceLoader 可能抛出异常或返回空
            }

            return $"[{key}]";
        }
    }
}