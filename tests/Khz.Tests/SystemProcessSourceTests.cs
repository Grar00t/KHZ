using System.Diagnostics;
using Khz.Runtime;

namespace Khz.Tests;

public sealed class SystemProcessSourceTests
{
    [Fact]
    public void GetProcesses_IncludesCurrentProcess()
    {
        using var current = Process.GetCurrentProcess();

        var source = new SystemProcessSource();
        var processes = source.GetProcesses().ToList();

        Assert.NotEmpty(processes);

        var actual = Assert.Single(
            processes,
            process => process.Id == current.Id);

        Assert.Equal(current.ProcessName, actual.Name);
        Assert.True(actual.CpuSeconds >= 0);
        Assert.True(actual.WorkingSetBytes > 0);
    }
}


