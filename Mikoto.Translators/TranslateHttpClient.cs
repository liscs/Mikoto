using System.Net;

namespace Mikoto.Translators;

public class TranslateHttpClient
{
    private static HttpClient? _httpClient;
    private static readonly object _httpClientLock = new object();
    private static readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(12);
    /// <summary>
    /// 获得HttpClient单例，第一次调用自动初始化
    /// </summary>
    public static HttpClient Instance
    {
        get
        {
            if (_httpClient == null)
            {
                lock (_httpClientLock)
                {
                    if (_httpClient == null)
                    {
                        _httpClient = new HttpClient()
                        {
                            Timeout = _defaultTimeout
                        };
                        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mikoto");
                    }
                }
            }

            return _httpClient;
        }
    }
    public static void SetHttpProxiedClient(string uriString)
    {
        if (_httpClient == null)
        {
            HttpClientHandler handler = new()
            {
                Proxy = new WebProxy()
                {
                    Address = new Uri(uriString),
                    UseDefaultCredentials = true
                }
            };
            _httpClient = new HttpClient(handler)
            {
                Timeout = _defaultTimeout
            };
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mikoto");
        }
    }
}
