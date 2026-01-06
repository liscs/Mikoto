using System.Net;

namespace Mikoto.Translators;

public class DynamicProxy : IWebProxy
{
    private Uri? _proxyUri;
    private readonly Lock _lock = new();

    // 可以在程序运行期间随时修改这个属性
    public Uri? ProxyUri
    {
        get { lock (_lock) return _proxyUri; }
        set { lock (_lock) _proxyUri = value; }
    }

    public ICredentials? Credentials { get; set; }

    public Uri? GetProxy(Uri destination)
    {
        return ProxyUri ?? HttpClient.DefaultProxy.GetProxy(destination);
    }

    // 是否对特定地址绕过代理
    public bool IsBypassed(Uri host)
    {
        return host.IsLoopback;
    }
}