using Mikoto.Core;
using System.Windows;

namespace Mikoto;

public class WpfResourceService : IResourceService
{
    public string Get(string key)
    {
        if (Application.Current == null)
            return $"[{key}]";

        return (string)Application.Current.Resources[key];
    }
}
