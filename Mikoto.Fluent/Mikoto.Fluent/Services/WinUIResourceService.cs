
using Microsoft.Windows.ApplicationModel.Resources;
using Mikoto.Core;

namespace Mikoto.Fluent.Services
{
    internal class WinUIResourceService : IResourceService
    {
        // 关键点：ResourceLoader 必须在包含 Strings 文件夹的项目中运行
        private readonly ResourceLoader loader = new ResourceLoader();
        public string Get(string key)
        {
            return loader.GetString(key);
        }
    }
}