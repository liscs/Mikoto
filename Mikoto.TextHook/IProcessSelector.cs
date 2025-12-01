using System.Diagnostics;

namespace Mikoto.TextHook
{
    public interface IProcessSelector
    {
        Process? SelectMainProcess(List<Process> gameProcesses);
    }
}
