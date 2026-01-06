namespace Mikoto.Translators;

public class TranslateHttpClient
{
    private static HttpClient? _httpClient;
    private static readonly Lock _syncLock = new();
    private static readonly DynamicProxy _dynamicProxy = new();
    private static readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(12);

    public static HttpClient Instance
    {
        get
        {
            if (_httpClient == null) Initialize(null);
            return _httpClient!;
        }
    }

    public static Uri? GetCurrentProxyUri(Uri target)
    {
        return _dynamicProxy.GetProxy(target);
    }

    /// <summary>
    /// 动态设置或更换代理地址，无需重启 HttpClient
    /// </summary>
    public static void SetProxy(string? uriString)
    {
        // 如果还没初始化，先初始化
        if (_httpClient == null)
        {
            Initialize(uriString);
        }

        // 更新代理地址（即便已经初始化，这一步也会对后续请求生效）
        _dynamicProxy.ProxyUri = string.IsNullOrEmpty(uriString) ? null : new Uri(uriString);
    }

    private static void Initialize(string? initialProxy)
    {
        lock (_syncLock)
        {
            if (_httpClient != null) return;

            if (!string.IsNullOrEmpty(initialProxy))
            {
                _dynamicProxy.ProxyUri = new Uri(initialProxy);
            }

            var handler = new SocketsHttpHandler
            {
                Proxy = _dynamicProxy,
                UseProxy = true,
                PooledConnectionLifetime = TimeSpan.FromMinutes(2)
            };

            _httpClient = new HttpClient(handler) { Timeout = _defaultTimeout };
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mikoto");
        }
    }
}
