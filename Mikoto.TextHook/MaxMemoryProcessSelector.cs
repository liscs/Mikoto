using System.Diagnostics;

namespace Mikoto.TextHook
{
    public class MaxMemoryProcessSelector : IProcessSelector
    {
        public Process SelectMainProcess(List<Process> processes)
        {
            return processes.OrderByDescending(p => p.WorkingSet64).First();
        }
    }
}
