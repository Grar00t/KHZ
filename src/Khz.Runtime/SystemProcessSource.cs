using System.Diagnostics;

namespace Khz.Runtime;

public sealed class SystemProcessSource : IProcessSource
{
    public IEnumerable<KhzProcess> GetProcesses()
    {
        foreach (var process in Process.GetProcesses())
        {
            using (process)
            {
                KhzProcess item;

                try
                {
                    item = new KhzProcess(
                        process.Id,
                        process.ProcessName,
                        process.TotalProcessorTime.TotalSeconds,
                        process.WorkingSet64);
                }
                catch
                {
                    continue;
                }

                yield return item;
            }
        }
    }
}
