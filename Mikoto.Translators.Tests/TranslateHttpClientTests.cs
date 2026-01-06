using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;
using Xunit;

namespace Mikoto.Translators.Tests;

public class TranslateHttpClientTests
{
    [Fact]
    public void Instance_ShouldReturnSameObject()
    {
        // Assert: 验证单例
        var instance1 = TranslateHttpClient.Instance;
        var instance2 = TranslateHttpClient.Instance;
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void Instance_ShouldHaveCorrectUserAgent()
    {
        // Act
        var client = TranslateHttpClient.Instance;

        // Assert
        Assert.Contains("Mikoto", client.DefaultRequestHeaders.UserAgent.ToString());
    }

    [Fact]
    public void SetProxy_ShouldUpdateDynamicProxyUri()
    {
        // Arrange
        Uri testTarget = new Uri("https://www.google.com");
        string proxy1 = "http://127.0.0.1:8888";
        string proxy2 = "http://192.168.1.100:1080";

        // Act 1: 设置第一个代理
        TranslateHttpClient.SetProxy(proxy1);
        var uriAfterFirstSet = TranslateHttpClient.GetCurrentProxyUri(testTarget);

        // Act 2: 更换代理地址
        TranslateHttpClient.SetProxy(proxy2);
        var uriAfterSecondSet = TranslateHttpClient.GetCurrentProxyUri(testTarget);

        // Assert
        Assert.Equal(uriAfterFirstSet, new Uri(proxy1));
        Assert.Equal(uriAfterSecondSet, new Uri(proxy2));
    }

    [Fact]
    public void SetProxy_WithEmptyString_ShouldFallbackToSystemProxy()
    {
        // Arrange
        // 1. 获取系统对于目标地址预期的代理（可能是 null，也可能是某个地址）
        Uri testTarget = new Uri("https://www.google.com");
        Uri? expectedSystemProxy = HttpClient.DefaultProxy.GetProxy(testTarget);

        // Act
        // 2. 传入空字符串，触发回退逻辑
        TranslateHttpClient.SetProxy("");

        // 3. 获取当前实际生效的代理 URI
        // 注意：GetCurrentProxyUri 内部应该调用 _dynamicProxy.GetProxy(testTarget)
        Uri? actualProxy = TranslateHttpClient.GetCurrentProxyUri(testTarget);

        // Assert
        Assert.Equal(expectedSystemProxy, actualProxy);
    }
}
