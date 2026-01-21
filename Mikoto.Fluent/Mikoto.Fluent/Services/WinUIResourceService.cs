using Microsoft.Windows.ApplicationModel.Resources;
using Mikoto.Core;

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
            catch
            {
                // 资源不存在时 ResourceLoader 可能抛出异常或返回空
            }

            return $"[{key}]";
        }
    }
}