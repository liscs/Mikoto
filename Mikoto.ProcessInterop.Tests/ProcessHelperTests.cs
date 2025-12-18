using System.Diagnostics;
using Xunit;

namespace Mikoto.ProcessInterop.Tests;

public class ProcessHelperTests
{
    [Fact]
    public void GetAppNamePidDict_ShouldReturnDictionary_WhenHasWindowedProcesses()
    {
        var dict = ProcessHelper.GetAppNamePidDict();

        Assert.NotNull(dict);
        Assert.IsType<Dictionary<string, int>>(dict);
    }

    [Fact]
    public void FindSameNameProcess_ShouldReturnList()
    {
        var current = Process.GetCurrentProcess();
        var list = ProcessHelper.FindSameNameProcess(current.Id);

        Assert.NotNull(list);
        Assert.NotEmpty(list);
        Assert.Contains(list, p => p.ProcessName == current.ProcessName);
    }

    [Fact]
    public void FindProcessPath_ShouldReturnValidPath_ForCurrentProcess()
    {
        var current = Process.GetCurrentProcess();
        var path = ProcessHelper.FindProcessPath(current.Id);

        Assert.False(string.IsNullOrEmpty(path));
        Assert.True(File.Exists(path));
    }

    [Fact]
    public void FindProcessPath_ShouldThrow_Exception()
    {
        Assert.Throws<ArgumentException>(() => ProcessHelper.FindProcessPath(-1));
    }

    [Fact]
    public void GetAppPaths_ShouldReturnList()
    {
        var result = ProcessHelper.GetAppPaths();

        Assert.NotNull(result);
        Assert.IsType<List<string>>(result);
    }

    [Fact]
    public void IsProcessRunning_ShouldReturnTrue_ForCurrentProcess()
    {
        var current = Process.GetCurrentProcess();
        Assert.True(ProcessHelper.IsProcessRunning(current.Id));
    }

    [Fact]
    public void IsProcessRunning_ShouldReturnFalse_ForInvalidPid()
    {
        Assert.False(ProcessHelper.IsProcessRunning(-12345));
    }
}