using Mikoto.DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;

namespace Mikoto.Fluent;

public class AppEnvironment
{
    public IFileService FileService { get; } = new FileService();
    public IGameInfoService GameInfoService { get; } = new GameInfoService();

    public AppEnvironment()
    {
    }
}
