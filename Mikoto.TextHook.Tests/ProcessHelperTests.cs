namespace Mikoto.TextHook.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

[TestClass]
public class ProcessHelperTests
{
    [TestMethod]
    public void GetAppNamePidDict_ShouldReturnDictionary_WhenHasWindowedProcesses()
    {
        var dict = ProcessHelper.GetAppNamePidDict();

        Assert.IsNotNull(dict);
        // 不能保证当前一定有带窗口的进程，但至少字典结构不能是 null
        Assert.IsInstanceOfType<Dictionary<string, int>>(dict);
    }

    [TestMethod]
    public void FindSameNameProcess_ShouldReturnList()
    {
        var current = Process.GetCurrentProcess();
        var list = ProcessHelper.FindSameNameProcess(current.Id);

        Assert.IsNotNull(list);
        Assert.IsNotEmpty(list);
        Assert.IsTrue(list.Any(p => p.ProcessName == current.ProcessName));
    }

    [TestMethod]
    public void FindProcessPath_ShouldReturnValidPath_ForCurrentProcess()
    {
        var current = Process.GetCurrentProcess();
        var path = ProcessHelper.FindProcessPath(current.Id);

        Assert.IsFalse(string.IsNullOrEmpty(path));
        Assert.IsTrue(System.IO.File.Exists(path));
    }

    [TestMethod]
    public void FindProcessPath_ShouldThrow_Exception()
    {
        try
        {
            ProcessHelper.FindProcessPath(-1);
            Assert.Fail("应该抛出异常，但没有抛。");
        }
        catch (ArgumentException)
        {
            // success
        }
    }

    [TestMethod]
    public void GetAppPaths_ShouldReturnList()
    {
        var result = ProcessHelper.GetAppPaths();

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<List<string>>(result);
    }

    [TestMethod]
    public void IsProcessRunning_ShouldReturnTrue_ForCurrentProcess()
    {
        var current = Process.GetCurrentProcess();
        Assert.IsTrue(ProcessHelper.IsProcessRunning(current.Id));
    }

    [TestMethod]
    public void IsProcessRunning_ShouldReturnFalse_ForInvalidPid()
    {
        Assert.IsFalse(ProcessHelper.IsProcessRunning(-12345));
    }
}



