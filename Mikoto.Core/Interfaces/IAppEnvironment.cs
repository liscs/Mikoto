using Mikoto.Config;
using Mikoto.DataAccess;
using Mikoto.Resource;
using Mikoto.TextHook;

namespace Mikoto.Core.Interfaces;

public interface IAppEnvironment
{
    public IGameInfoService GameInfoService { get; }
    public ITextHookService TextHookService { get; }
    public IAppSettings AppSettings { get; }
    public IResourceService ResourceService { get; }
    public IMainThreadService MainThreadService { get; }
}
