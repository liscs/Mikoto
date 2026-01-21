using Mikoto.Config;
using Mikoto.DataAccess;
using Mikoto.TextHook;

namespace Mikoto.Core.Interfaces;

public interface IAppEnvironment
{
    public IGameInfoService GameInfoService { get; }
    public ITextHookService TextHookService { get; set; }
    public IAppSettings AppSettings { get; }
}
