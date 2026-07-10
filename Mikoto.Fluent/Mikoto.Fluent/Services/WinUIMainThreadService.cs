using Microsoft.UI.Dispatching;
using Mikoto.Core.Interfaces;

namespace Mikoto.Fluent.Services;

public class WinUIMainThreadService : IMainThreadService
{
    private readonly DispatcherQueue _queue = DispatcherQueue.GetForCurrentThread();
    public void RunOnMainThread(Action action) => _queue.TryEnqueue(() => action());
}
