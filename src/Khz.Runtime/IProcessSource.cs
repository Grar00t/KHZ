namespace Khz.Runtime;

public interface IProcessSource
{
    IEnumerable<KhzProcess> GetProcesses();
}
