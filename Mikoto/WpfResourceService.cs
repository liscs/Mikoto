using Mikoto.Core;
using System.Windows;

namespace Mikoto;

public class WpfResourceService : IResourceService
{
    public string Get(string key)
    {
        if (Application.Current == null)
        {
            return $"[{key}]";
        }

        // 检查资源字典是否包含该键
        if (Application.Current.Resources.Contains(key))
        {
            // 键存在，进行安全类型检查
            if (Application.Current.Resources[key] is string strResource)
            {
                return strResource;
            }
            else
            {
                // 键存在但不是字符串
                return $"[{key}] is {Application.Current.Resources[key].GetType().FullName}";
            }
        }
        else
        {
            // 键不存在
            return $"[Resource: {key} - Not Found]";
        }
    }
}
