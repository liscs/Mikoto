namespace Mikoto.Core.Interfaces;

public interface IMainThreadService
{
    void RunOnMainThread(Action action);
}
